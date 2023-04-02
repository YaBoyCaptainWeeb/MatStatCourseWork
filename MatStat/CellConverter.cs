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
                return new SolidColorBrush(System.Windows.Media.Color.FromArgb(0,0,176,80));
            }
            else if (val <= 0.69 && val >= 0.3)
            {
                return new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 252, 192, 0));
            }
            else if (val <= 0.29 && val >= 0.00001)
            {
                return "LightSalmon";
            }
            else if (val <= -0.00001 && val >= -0.29)
            {
                return "Orange";
            }
            else if (val <= -0.3 && val >= -0.69)
            {
                return "LightCoral";
            }
            else if (val <= -0.7 && val >= -1.0)
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
