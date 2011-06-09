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
        // obtained by evaluating "Product Line"
        List<LineItem> _lineItems;
        // obtained by evaluating "Order sub type"
        bool _isInstall;

        #endregion

        #region Properties
        public string ActivityNumber
        {
            get { return _activityNumber; }
        }

        public List<LineItem> LineItems
        {
            get
            {
                if (_lineItems == null)
                {
                    _lineItems = new List<LineItem>();
                }

                return _lineItems;
            }
            set
            {
                _lineItems = value;
            }
        }
        #endregion

        #region Constructors
        public Activity(string activityNumber, bool isInstall)
        {
            _activityNumber = activityNumber;
            _isInstall = isInstall;
        }

        public Activity(string activityNumber, string lineItemName)
        {
            _activityNumber = activityNumber;
            LineItems.Add(new LineItem(lineItemName, 0));
        }
        #endregion

        public double Value(double PriceOfInstall, double PriceOfUpgrade)
        {
            double value = 0;
            Debug.WriteLine("Activity #" + ActivityNumber);

            if (_isInstall)
            {
                Debug.WriteLine("Install - Value: " + PriceOfInstall);
                value += PriceOfInstall;
            }
            else
            {
                Debug.WriteLine("Upgrade - Value: " + PriceOfUpgrade);
                value += PriceOfUpgrade;
            }

            bool skippedOne = false;
            // Populate a dictionary to track one-offs, bool is true if the line item has already been counted
            Dictionary<LineItem, bool> OneOffTracker = new Dictionary<LineItem, bool>();
            foreach (LineItem li in OneOffLineItems)
            {
                OneOffTracker.Add(li, false);
            }
            //now we process the line items for value
            foreach (LineItem li in LineItems)
            {
                if (li.Value != 0)
                {
                    if (li.IsReceiver == false
                        && li.SkipDuplicates == false)
                    {
                        value += li.Value;
                        Debug.WriteLine(li.Name + " - Value: " + li.Value);
                    }
                    else if (li.SkipDuplicates == true &&
                        !OneOffTracker[li])
                    {
                        OneOffTracker[li] = true;
                        value += li.Value;
                        Debug.WriteLine(li.Name + " - Value: " + li.Value);
                    }
                    // if it's a SkipOnInstall item, check to see if one has already been skipped before adding the value
                    else if (_isInstall &&
                        !skippedOne)
                    {
                        skippedOne = true;
                    }
                    else
                    {
                        value += li.Value;
                        Debug.WriteLine(li.Name + " - Value: " + li.Value);
                    }
                }
            }
            Debug.WriteLine("TOTAL VALUE: $" + value);
            Debug.WriteLine("");

            return value;
        }

        /// <summary>
        /// returns a list of line items only meant to be counted once per activity
        /// </summary>
        List<LineItem> OneOffLineItems
        {
            get
            {
                return (from LineItem li in LineItems
                        where li.SkipDuplicates == true
                        select li).ToList();
            }
        }
    }
}
