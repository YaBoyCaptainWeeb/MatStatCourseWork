using LiveCharts.Defaults;
using LiveCharts.Wpf;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MatStat
{
    /// <summary>
    /// Логика взаимодействия для CustomPlot.xaml
    /// </summary>
    public partial class CustomPlot : UserControl
    {
        public CustomPlot(double[] frequency, double[] theoreticFrequency, double[] intervals, double isNormal, string title)
        {
            InitializeComponent();
            //ChartValues<ObservablePoint> points = new ChartValues<ObservablePoint>();
            ChartValues<ObservablePoint> theoreticPoints = new ChartValues<ObservablePoint>();
            SeriesCollection seriesCollection = new SeriesCollection { new LineSeries { Values = theoreticPoints } }; //new LineSeries { Values = theoreticPoints } };
            for (int i = 0; i < frequency.Length; i++)
            {
                //points.Add(new ObservablePoint(intervals[i], frequency[i]));
                theoreticPoints.Add(new ObservablePoint(intervals[i], theoreticFrequency[i]));
            }
            Chart.Series = seriesCollection;

            Title.Text = title;
            IsNormal.Text = isNormal > 0 ? "Нормальное распределение: присутвует" : "Нормальное распределение:\n отсутствует";
        }
    }
}
