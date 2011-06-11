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
        Dictionary<string, Activity> _activities = new Dictionary<string, Activity>();
        SelectPricesViewModel _spvm = new SelectPricesViewModel();
        double _value = 0;
        /// <summary>
        /// Insantiated inside LoadSpreadSheet()
        /// Holds the dataset with ActivityNumber as the key, ActivityStruct as the value
        /// </summary>

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

            Dictionary<string, LineItem> uniqueLineItems = new Dictionary<string, LineItem>();
            // iterate through every line of the excel table
            foreach (var x in data)
            {
                // get the raw strings from our data, used for dictionary keys and model unique IDs
                string lineItemID = x.Field<string>("Product Line");
                string activityID = x.Field<string>("Activity Number");

                // make sure our line item and activity string aren't null
                // if line item is null, we don't really care about the entry
                if (activityID != null && lineItemID != null)
                {
                    bool isInstall = x.Field<string>("Order Sub Type") == "New Install";


                    // see if the new line item is already in our unique list
                    if(!uniqueLineItems.Keys.Contains(lineItemID))
                    {
                        //instantiate the new line data we're going to add to our tree structure
                        LineItem newLineItem = new LineItem(lineItemID, 0);
                        // add it, referencing newLineItem.Name saves redundant data
                        uniqueLineItems.Add(newLineItem.Name, newLineItem);
                    }

                    if (!_activities.Keys.Contains(activityID))
                    {
                        Activity newActivity = new Activity(activityID, isInstall, uniqueLineItems);
                        // we add the new activity to the list using the same method we used earlier with lineItem
                        _activities.Add(newActivity.ActivityNumber, newActivity);
                        // passes a reference to the uniqueLineItems instance of our object
                        // that way, when it updates from the VM data bind our models auto-update
                        _activities[activityID].AddLineItem(uniqueLineItems[lineItemID]);
                    }
                    else
                    {
                        _activities[activityID].AddLineItem(uniqueLineItems[lineItemID]);
                    }
                }
            }
            
            _spvm.AddItems(uniqueLineItems);
            _spvm.OkPressed += this.OnPricesFinalized;

            // TODO: This status text system doesn't seem consistent with the MVVM pattern, look into it
            OnPropertyChanged("StatusText");
        }

        void OnPricesFinalized(object sender, EventArgs e)
        {
            foreach (Activity activity in _activities.Values)
            {
                _value += activity.Value(_spvm.PriceOfInstall, _spvm.PriceOfUpgrade);
            }
            OnPropertyChanged("StatusText");
        }
        #endregion
    }
}