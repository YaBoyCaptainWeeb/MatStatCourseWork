using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MatStat
{
    internal class Correlation
    {
       // public MainWindow Main { get; set; }
        public double[,] ToArr(List<List<string>> list)
        {
            double[,] arr = new double[MainWindow.grid.Count(), 8];
            for (int i = 0; i != MainWindow.grid.Count(); i++)
            {
                for (int j = 1; j != 9; j++)
                {
                    arr[i, j - 1] = Convert.ToDouble(list[i][j]);
                }
            }
            return arr;
        }
        private double[] GetArray(double[,] list, int count)
        {
            double[] arr = new double[list.GetUpperBound(0) + 1];
            for (int i = 0; i != list.GetUpperBound(0) + 1; i++)
            {
                arr[i] = list[i, count];
            }
            return arr;
        }
    }
}
