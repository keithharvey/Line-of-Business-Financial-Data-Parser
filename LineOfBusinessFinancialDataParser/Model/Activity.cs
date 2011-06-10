using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace LineOfBusinessFinancialDataParser.Model
{
    public class Activity
    {
        #region Private Variables

        // obtained by evaluating "Activity Number"
        string _activityNumber;
        Dictionary<string, LineItem> _uniqueLineItems;
        // obtained by evaluating "Order sub type"
        bool _isInstall;
        
        Dictionary<LineItem, int> itemQuantities;


        #endregion

        #region Properties
        public string ActivityNumber
        {
            get { return _activityNumber; }
        }

        public Dictionary<LineItem, int> ItemQuantities
        {
            get
            {
                if (itemQuantities == null)
                    itemQuantities = new Dictionary<LineItem, int>();

                return itemQuantities;
            }
        }

        #endregion

        #region Constructors
        /// <summary>
        /// represents an individual activity, containing line items
        /// </summary>
        /// <param name="activityNumber">Unique activityID</param>
        /// <param name="isInstall">True if an install, false if an upgrade.</param>
        /// <param name="uniqueLineItems">Collection of line items, same list passed into the spvm.</param>
        public Activity(string activityNumber, bool isInstall, Dictionary<string, LineItem> uniqueLineItems)
        {
            _activityNumber = activityNumber;
            _isInstall = isInstall;
            _uniqueLineItems = uniqueLineItems;
        }

        #endregion

        public double Value(double priceOfInstall, double priceOfUpgrade)
        {
            // accumulates value across the method
            double value = 0;
            Debug.WriteLine("Activity #" + ActivityNumber);

            if (_isInstall)
            {
                Debug.WriteLine("Install - Value: " + priceOfInstall);
                value += priceOfInstall;
            }
            else
            {
                Debug.WriteLine("Uprade - Value: " + priceOfUpgrade);
                value += priceOfUpgrade;
            }
                        
            // client gets a free receiver install with the price of an install
            bool installReceiverNotPaid = true;
            //now we process the line items for value
            foreach (var item in itemQuantities)
            {
                // if there is a quantity associated with the item (should never happen)
                if (item.Key.Value != 0)
                {
                    // now we iterate however many times the item appears in the activity
                    for (int individualItem = 0; individualItem < item.Value; individualItem++)
                    {
                        // if activity is an install, item is a receiver, and we haven't already acknowledged the first receiver
                        if (_isInstall && installReceiverNotPaid && item.Key.IsReceiver)
                        {
                            Debug.WriteLine("First receiver accounted for.");
                            // value remains the same
                            installReceiverNotPaid = false;
                        }
                        // this catches everything else for both installs, upgrades, non-first receivers
                        else
                        {
                            Debug.WriteLine("Added " + item.Key.Name + ": " + item.Key.Value);
                            value += item.Key.Value;
                        }
                    }
                }
            }
            Debug.WriteLine("TOTAL VALUE: $" + value);
            Debug.WriteLine("");

            return value;
        }


        internal void AddLineItem(LineItem lineItem)
        {
            if (ItemQuantities.Keys.Contains(lineItem))
            {
                ItemQuantities[lineItem]++;
            }
            else
            {
                ItemQuantities.Add(lineItem, 1);
            }
        }

    }
}
