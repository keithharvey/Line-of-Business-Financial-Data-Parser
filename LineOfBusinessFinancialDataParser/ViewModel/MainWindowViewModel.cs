using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;
using System.Diagnostics;
using System.IO;
using System.Data.OleDb;
using System.Data;
using System.Linq;
using LineOfBusinessFinancialDataParser.Model;



namespace LineOfBusinessFinancialDataParser.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Fields

        CommandViewModel _openSpreadsheet;
        string _filename = null;
        string _statusText = "Please select a pay spreadsheet to begin.";
        Dictionary<string, Activity> _activities = new Dictionary<string,Activity>();
        SelectPricesViewModel _spvm = new SelectPricesViewModel();
        double _value = 0;
        /// <summary>
        /// Insantiated inside LoadSpreadSheet()
        /// Holds the dataset with ActivityNumber as the key, ActivityStruct as the value
        /// </summary>
        EnumerableRowCollection<ActivityStruct> _query;

        #endregion Fields

        public CommandViewModel OpenSpreadsheetCommand
        {
            get
            {
                if (_openSpreadsheet == null)
                {
                    _openSpreadsheet = new CommandViewModel(
                        "Open Spreadsheet",
                        new RelayCommand(param => this.OpenSpreadsheet()));
                }

                return _openSpreadsheet;
            }
        }

        public string StatusText
        {
            get
            {
                if (_filename != null)
                {
                    _statusText = "File " + _filename + " loaded with " + (_activities.Count) + " activities.";
                }
                if (_value != 0)
                {
                    _statusText = "Value of pay period: " + _value;
                }
                return _statusText;
            }
            set
            {
                if (value != _statusText)
                {
                    _statusText = value;
                    OnPropertyChanged("StatusText");
                }
            }
        }
        public SelectPricesViewModel Spvm
        {
            get
            {
                return _spvm;
            }
        }

        List<string> UniqueItems
        {
            get
            {
                // return a list of unique items derived from the items contained within the activity list
                return null;
            }
        }

        #region Private Helpers

        void OpenSpreadsheet()
        {
            // Configure save file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.DefaultExt = ".xls"; // Default file extension
            dlg.Filter = "Text documents (.xls)|*.xls"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                _filename = dlg.FileName;
                Debug.WriteLine("Filename loaded: " + _filename);
                LoadSpreadsheet();
            }
        }

        void LoadSpreadsheet()
        {
            var connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0; data source={0}; Extended Properties=Excel 8.0;", _filename);

            var adapter = new OleDbDataAdapter("SELECT * FROM [Sheet1$]", connectionString);
            var ds = new DataSet();

            // grab our spreadsheet data from the adapter
            adapter.Fill(ds, "Activity Number");

            // now, make it enumerable
            var data = ds.Tables["Activity Number"].AsEnumerable();
            // grab our data fromt he spreadsheet data with a LINQ query
            _query = data.Where(x => x.Field<string>("Activity Number") != string.Empty).Select(x => 
                new ActivityStruct
                {
                    activityID = x.Field<string>("Activity Number"),
                    lineItem = x.Field<string>("Product Line"),
                    install = x.Field<string>("Order Sub Type") == "New Install"
                });

            List<string> uniqueActivities = new List<string>();
            List<string> uniqueItems = new List<string>();
            // populate a list of unique activities
            // TODO: Figure out a way to do this and uniqueItems with one pass, leaving it the way it is now because the LINQ is readable
            uniqueActivities = (from x in _query
                                where x.activityID != null
                                select x.activityID).Distinct().ToList();
            uniqueItems = (from x in _query
                          where x.lineItem != null
                          select x.lineItem).Distinct().ToList();

            // TODO: This status text system doesn't seem consistent with the MVVM pattern, look into it
            OnPropertyChanged("StatusText");

            _spvm.AddItems(uniqueItems);
            _spvm.OkPressed += this.OnPricesFinalized;
        }

        void OnPricesFinalized(object sender, EventArgs e)
        {
            _value = 0;
            // if Activities is not populated, fill it with line items from the prices window
            if (_activities.Count == 0)
            {
                foreach (ActivityStruct line in _query)
                {
                    // TODO: Move _activities population to the spreadsheet load, then refresh after the user hits "OkCommand"
                    if (line.activityID != null)
                    {
                        // if the unique activity is not already in the list.
                        if (!_activities.Keys.Contains(line.activityID))
                        {
                            _activities.Add(line.activityID, new Activity(line.activityID, line.install));
                        }
                        var extractedLineItems = from LineItemViewModel livm in _spvm.Items
                                                 where livm.Name == line.lineItem
                                                 select livm.LineItem;
                        _activities[line.activityID].LineItems.Add(extractedLineItems.First());
                        Debug.WriteLine("Activity #" + line.activityID + " Line Item: " + line.lineItem);
                    }
                }
            }
            foreach (Activity activity in _activities.Values)
            {
                _value += activity.Value(_spvm.PriceOfInstall, _spvm.PriceOfUpgrade);
            }
            OnPropertyChanged("StatusText");
        }

        struct ActivityStruct
        {
            public string activityID;
            public string lineItem;
            public bool install;
        }


        #endregion
    }
}