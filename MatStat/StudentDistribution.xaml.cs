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
using System.Windows.Shapes;

namespace MatStat
{
    /// <summary>
    /// Логика взаимодействия для StudentDistribution.xaml
    /// </summary>
    public class DistribTable
    {
        public int _Count { get; set; }
        public double _Num { get; set; }
        public double _Num1 { get; set; }
        public double _Num2 { get; set; }
        public double _Num3 { get; set; }
        public double _Num4 { get; set; }
        public double _Num5 { get; set; }
        public double _Num6 { get; set; }

        public DistribTable(int count, double num,double num1, double num2, double num3, double num4, double num5, double num6)
        {
            _Count = count;
            _Num = num;
            _Num1 = num1;
            _Num2 = num2;
            _Num3 = num3;
            _Num4 = num4;
            _Num5 = num5;
            _Num6 = num6;
        }
    }
    public partial class StudentDistribution : Window
    {
        public List<DistribTable> StudentDistribSource = new List<DistribTable>();
        public StudentDistribution(List<List<double>> StudentDistributionTable)
        {
            int i = 1;

            InitializeComponent();
            foreach(List<double> item in StudentDistributionTable)
            {
                StudentDistribSource.Add(new DistribTable(i, item[0], item[1], item[2], item[3], item[4], item[5], item[6]));
                i++;
            }
            StudentDistrib.ItemsSource = StudentDistribSource;
        }
    }
}
