using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using LineOfBusinessFinancialDataParser.Model;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace LineOfBusinessFinancialDataParser.ViewModel
{
    public class SelectPricesViewModel : ViewModelBase
    {
        #region Fields

        ObservableCollection<LineItemViewModel> _items;
        RelayCommand _okCommand;
        
        #endregion Fields

        #region Properties
        /// <summary>
        /// Holds a list of unique items and user input of values
        ///     A list of Items are extracted from the excel spreadsheet in MainWindowViewModel.cs
        ///     They are then used to enter values dynamically at runtime and programatically calculate bounds for the pay period.
        /// </summary>
        public ObservableCollection<LineItemViewModel> Items
        {
            // is never null because it is instantiated in the constructor
            get
            {
                if (_items == null)
                    _items = new ObservableCollection<LineItemViewModel>();
                return _items;
            }
            set
            {
                _items = value;
            }
        }
        /// <summary>
        /// Simply returns whether the list has been populated by spreadsheet data, yet.
        /// </summary>
        public bool IsPopulated
        {
            get
            {
                // It requires > 2 because our default values (install and upgrade) take up indices 0 and 1
                return Items.ToList<LineItemViewModel>().Count > 2;
            }
        }
        /// <summary>
        /// Simply returns the LineItemViewModel value for installs since I already had
        /// that class set up to accept user input, I just reused it to evalute
        /// the two extra fields I needed
        /// </summary>
        public double PriceOfInstall
        {
            get
            {
                return (from LineItemViewModel li in Items
                               where li.Name == "New Install"
                               select li.Value).First();
            }
        }
        /// <summary>
        /// Simply returns the LineItemViewModel value for upgrades since I already had
        /// that class set up to accept user input, I just reused it to evalute
        /// the two extra fields I needed
        /// </summary>
        public double PriceOfUpgrade
        {
            get
            {
                return (from LineItemViewModel li in Items
                               where li.Name == "Upgrade"
                               select li.Value).First();
            }
        }
        #endregion

        #region Constructors
        public SelectPricesViewModel()
        {
            SetDefaultItems();

        }
        #endregion


        #region OK Command


        /// <summary>
        /// Pressed after the user has concluded inputting prices
        /// </summary>
        public ICommand OkCommand
        {
            get
            {
                if (_okCommand == null)
                    _okCommand = new RelayCommand(
                        param => this.OnOkPressed()
                        );

                return _okCommand;
            }
        }

        #endregion // OK Command


        #region OnOkPressed [event]
        /// <summary>
        /// Delegate for the OK command, routed to through OnOkPressed
        /// </summary>
        public event EventHandler OkPressed;


        /// <summary>
        /// Method called by the OkPressed event
        ///     This method is called when the user presses the 'Ok' button
        ///     in the SelectPricesView
        /// </summary>
        void OnOkPressed()
        {
            // links the method with the delegate
            EventHandler handler = this.OkPressed;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        #endregion // OnOkPressed [event]

        #region Private Helpers

        /// <summary>
        /// Used to set the default items or if there are already items there, reset it after a spreadsheet reload.
        /// </summary>
        void SetDefaultItems()
        {
            // Observeable collection does not implement RemoveAll() so we have to iterate to clear it out.
            List<LineItemViewModel> removedItems = Items.ToList<LineItemViewModel>();
            foreach (LineItemViewModel removedItem in removedItems)
            {
                Items.Remove(removedItem);
            }

            // add our two default values into the list
            Items.Add(new LineItemViewModel(new LineItem("New Install", 0)));
            Items.Add(new LineItemViewModel(new LineItem("Upgrade", 0)));

            OnPropertyChanged("IsPopulated");
        }

        #endregion Private Helpers

        public void AddItems(List<string> uniqueItems)
        {
            // null out any previous items in the list
            SetDefaultItems();

            if (uniqueItems != null)
            {
                foreach (string item in uniqueItems)
                {
                    Items.Add(new LineItemViewModel(new LineItem(item, 0)));
                }

                OnPropertyChanged("IsPopulated");
            }
        }

    }
}
