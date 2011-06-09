using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LineOfBusinessFinancialDataParser.Model;

namespace LineOfBusinessFinancialDataParser.ViewModel
{
    public class LineItemViewModel : ViewModelBase
    {
        LineItem _lineItem;

        public LineItem LineItem
        {
            get { return _lineItem; }
        }
        public string Name
        {
            get
            {
                return _lineItem.Name;
            }
        }
        public double Value
        {
            get
            {
                return _lineItem.Value;
            }
            set
            {
                _lineItem.Value = value;
                OnPropertyChanged("Value");
            }
        }
        public bool IsReceiver
        {
            get
            {
                return _lineItem.IsReceiver;
            }
            set
            {
                _lineItem.IsReceiver = value;
                OnPropertyChanged("IsReceiver");
            }
        }
        public bool SkipDuplicates
        {
            get
            {
                return _lineItem.SkipDuplicates;
            }
            set
            {
                _lineItem.SkipDuplicates = value;
                OnPropertyChanged("SkipDuplicates");
            }
        }

        public LineItemViewModel(LineItem lineItem)
        {
            _lineItem = lineItem;
        }
    }
}
