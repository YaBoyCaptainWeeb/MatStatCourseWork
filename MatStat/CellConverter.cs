using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace MatStat
{
    public class CellConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double val = (double)value;
            if (val <= 1.0 && val >= 0.7)
            {
                return "LimeGreen";
            }
            else if (val <= 0.6999 && val >= 0.5)
            {
                return "Yellow";
            }
            else if (val <= 0.4999 && val >= 0.2)
            {
                return "Gold";
            }
            else if (val <= 0.1999 && val >= 0.0001)
            {
                return "Orange";
            }
            else if (val <= 0.0001 && val >= -0.4999)
            {
                return "Red";
            }
            else if (val <= -0.5 && val >= -1.0)
            {
                return "Red";
            }
            return "White";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
