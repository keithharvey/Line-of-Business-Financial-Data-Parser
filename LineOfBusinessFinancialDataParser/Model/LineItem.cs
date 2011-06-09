using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LineOfBusinessFinancialDataParser.Model
{
    public class LineItem
    {
        #region Fields

        string _name;
        double _value;

        #endregion Fields

        #region Properties

        public string Name
        {
            get { return _name; }
        }

        public double Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }
        public bool IsReceiver
        {
            get;
            set;
        }
        public bool SkipDuplicates { get; set; }

        #endregion Properties

        #region Constructors

        public LineItem(string content, double value)
        {
            _name = content;
            _value = value;
        }

        #endregion Constructors
    }
}
