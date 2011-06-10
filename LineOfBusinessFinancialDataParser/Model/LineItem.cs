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

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Creates a line item and determines what type it is.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public LineItem(string name, double value)
        {
            _name = name;
            _value = value;

            string sPattern = "IRD";
            if (System.Text.RegularExpressions.Regex.IsMatch(_name, sPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                IsReceiver = true;
            }
        }

        #endregion Constructors

    }
}
