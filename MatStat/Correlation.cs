using ScottPlot.Renderable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MatStat
{
    internal class Correlation
    {
       // public MainWindow Main { get; set; }

        public double[,] CoupleCorrelate(double[,] arr) // вычисление парных коэффициентов корреляций
        {
            List<List<double>> RxyArr = new List<List<double>>();
            double[] buff = new double[arr.GetUpperBound(1) + 1]; // разделяю Heap и Стек с помощью буфферного массива, чтобы корректно добавлялись вычисленные коэффициенты
            List<double> arr1 = new List<double>();
            double numerator, denominator, denominator1;
            for(int cols = 0; cols != arr.GetUpperBound(1) + 1; cols++)
            {
                arr1.Clear();
                double xAvg = GetArray(arr, cols).Average();
                for (int current = 0; current != arr.GetUpperBound(1)+1; current++)
                {
                    numerator = 0; denominator = 0; denominator1 = 0;
                    double yAvg = GetArray(arr, current).Average();
                    for (int rows = 0; rows != arr.GetUpperBound(0) + 1; rows++) 
                    {
                        numerator += (arr[rows, cols] - xAvg) * (arr[rows, current] - yAvg);
                        denominator += Math.Pow(arr[rows, cols] - xAvg, 2);
                        denominator1 += Math.Pow(arr[rows, current] - yAvg, 2);
                    }
                    double Rxy = numerator / (Math.Sqrt(denominator * denominator1));
                    arr1.Add(Math.Round(Rxy,4));
                }
                buff = arr1.ToArray(); // копирую данные из кучи в массив-стек
                RxyArr.Add(buff.ToList()); // добавляю данные из стека в List
            }
            return ToArr(RxyArr);
        }

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
        public double[,] ToArr(List<List<double>> list)
        {
            double[,] arr = new double[list.Count,list.Count];
            for (int i = 0; i != list.Count; i++)
            {
                for (int j = 0; j != list.Count; j++)
                {
                    arr[i, j] = list[i][j];
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
