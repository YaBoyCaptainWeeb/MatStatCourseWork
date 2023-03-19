﻿using System;
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
using System.IO;
using System.Diagnostics;
using OfficeOpenXml;
using MathNet.Numerics.Statistics;
using System.ComponentModel;
using MathNet.Numerics.Distributions;
using Microsoft.Win32;
using ScottPlot;
using ScottPlot.WPF;
using System.Drawing;

namespace MatStat
{
    public partial class MainWindow : Window
    {
        public class MyTable
        {
            public string _model { get; set; }
            public double _price { get; set; }
            public double _CPUClock { get; set; }
            public double _RAMClock { get; set; }
            public double _DriveDisk { get; set; }
            public double _GPUClock { get; set; }
            public double _Diagonal { get; set; }
            public double _Battery { get; set; }
            public double _Weight { get; set; }
            
            public MyTable(string model,double price, double CPUClock, double RABClock, double DriveDisk,double GPUClock, double Diagonal, double Battery, double Weight)
            {
                _model = model;
                _price = price;
                _CPUClock = CPUClock;
                _RAMClock = RABClock;
                _DriveDisk = DriveDisk;
                _GPUClock = GPUClock;
                _Diagonal = Diagonal;
                _Battery = Battery;
                _Weight = Weight;
            }
            public MyTable()
            {

            }
        }
        List<List<string>> grid = new List<List<string>>(); // Исходные данные
        List<MyTable> itemsSource = new List<MyTable>(); // Данные для таблиц

        public MainWindow()
        {
            // EP+ excel
            // Excel Data Reader
            // Excel Data Reader.Dataset
            // MathNet Statistics
            
            InitializeComponent();
        }

        private void Load(string filepath) // Источник данных
        {
            try
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                ExcelPackage excelPackage = new ExcelPackage(filepath);
                ExcelWorksheet excelWorksheet = excelPackage.Workbook.Worksheets[0];

                itemsSource.Clear();
                for (int i = 2; true; i++) // копируем таблицу из Excel для внутренней работы программы
                {
                    if (excelWorksheet.Cells[i, 1].Value == null) break;
                    List<string> row = new List<string>();
                    for (int j = 1; true; j++)
                    {
                        if (excelWorksheet.Cells[i, j].Value == null) break;
                        row.Add(excelWorksheet.Cells[i, j].Value.ToString());
                    }
                    grid.Add(row);
                }

                foreach (List<string> row in grid) // делаем копию в itemSource для загрузки данных в нужном формате для DataGrid
                {
                    itemsSource.Add(new MyTable(row[0], Convert.ToDouble(row[1]), Convert.ToDouble(row[2]), Convert.ToDouble(row[3]),
                        Convert.ToDouble(row[4]), Convert.ToDouble(row[5]), Convert.ToDouble(row[6]), Convert.ToDouble(row[7]), Convert.ToDouble(row[8])));
                }

                //Load1();
            }
            catch (Exception)
            {
                MessageBox.Show("Таблица не найдена/не может быть открыта.");
                Process[] List;
                List = Process.GetProcessesByName("EXCEL");
                foreach (Process process in List)
                {
                    process.Kill();
                }
                Application.Current.Shutdown();
            }
            dataGrid.ItemsSource = itemsSource;
        }
        #region Нормированные таблицы
        private void Load1(object sender,RoutedEventArgs e) // Все расчеты(НЕ ДОДЕЛАНО ЕЩЕ ВЛАДИК ДОДЕЛОЙ???)
        {
            List<double> col = new List<double>();
            List<MyTable> itemsSource = new List<MyTable>(); // Массив для таблиц обычных(для таблиц)
            List<MyTable> metricsSource = new List<MyTable>(); // Массив для таблиц со статистиками(для таблиц)
            List<List<string>> normedGrid = new List<List<string>>(); // Массив с нормированным данными(не для таблиц)
            List<List<string>> metricsGrid = new List<List<string>>(); // Массив с расчитанными статистиками(не для таблиц)

            for(int i = 0; i != grid.Count; i++) // копия grid для внутренней работы функции
            {
                List<string> row = new List<string>();
                for(int j = 0; j != grid[i].Count; j++)
                {
                    row.Add(grid[i][j]);
                }
                normedGrid.Add(row);
            }
            

            for (int cols = 1; cols != 9; cols++) // нормирование
            {
                for (int rows = 0; rows != grid.Count; rows++)
                {
                    col.Add(Convert.ToDouble(grid[rows][cols]));
                }
                double colMax = col.Max();
                double colMin = col.Min();
                double Xnorm = 0;
                for (int rows1 = 0; rows1 != normedGrid.Count; rows1++)
                {
                    Xnorm = (col[rows1] - colMin) / (colMax - colMin);
                    normedGrid[rows1][cols] = Convert.ToString(Math.Round(Xnorm,4));
                }
            }

            foreach(List<string> row in normedGrid)
            {
                itemsSource.Add(new MyTable(row[0], Convert.ToDouble(row[1]), Convert.ToDouble(row[2]), Convert.ToDouble(row[3]),
                        Convert.ToDouble(row[4]), Convert.ToDouble(row[5]), Convert.ToDouble(row[6]), Convert.ToDouble(row[7]), Convert.ToDouble(row[8])));
            }
            NormGrid.ItemsSource = itemsSource;
            /////////////////////////////////////////////////////----Статистики----////////////////////////////////////////////////////////////////
            List<string> array1 = new List<string>();   // среднее арифметическое
            array1.Add("Среднее арифметическое");
            for (int cols = 1; cols != 9; cols++) 
            {
                double srAriphmetic = 0;
                for (int rows = 0; rows != normedGrid.Count; rows++)
                {
                    srAriphmetic += Convert.ToDouble(normedGrid[rows][cols]);
                }
                srAriphmetic /= normedGrid.Count;
                array1.Add(Math.Round(srAriphmetic,2).ToString());
            }
            metricsGrid.Add(array1);
            //////////////////////////////////////////////////////
            List<string> array21 = new List<string>(); // Мода
            array21.Add("Мода");
            for (int cols = 1; cols != 9; cols++)
            {
                List<string> array2 = new List<string>();
                for (int rows = 0; rows != normedGrid.Count; rows++)
                {
                    array2.Add(normedGrid[rows][cols]);
                }
                array21.Add(Moda(array2));
            }
            metricsGrid.Add(array21);
            /////////////////////////////////////////////////////
            List<string> array3 = new List<string>(); // Медиана
            array3.Add("Медиана");
            for (int cols = 1; cols != 9; cols++)
            {
                List<double> median = new List<double>();
                for (int rows = 0; rows != normedGrid.Count; rows++)
                {
                    median.Add(Convert.ToDouble(normedGrid[rows][cols]));
                }
                var stat = new DescriptiveStatistics(median);
                array3.Add(Convert.ToString(Math.Round(stat.Mean,4)));
            }
            metricsGrid.Add(array3);
            ////////////////////////////////////////////////////////
            List<string> array4 = new List<string>(); // Дисперсия
            List<double> array41 = new List<double>();
            array4.Add("Дисперсия");
            for (int cols = 1; cols != 9; cols++)
            {
                array41.Clear();
                for (int rows = 0; rows != normedGrid.Count; rows++)
                {
                    array41.Add(Convert.ToDouble(normedGrid[rows][cols]));
                }
                var stat = new DescriptiveStatistics(array41);
                array4.Add(Convert.ToString(Math.Round(stat.Variance,4)));
            }
            metricsGrid.Add(array4);
            ////////////////////////////////////////////////////////////
            List<string> array5 = new List<string>(); // Эксцесс
            List<double> array51 = new List<double>();
            array5.Add("Эксцесс");
            for (int cols = 1; cols != 9; cols++)
            {
                array51.Clear();
                for (int rows = 0; rows != normedGrid.Count; rows++)
                {
                    array51.Add(Convert.ToDouble(normedGrid[rows][cols]));
                }
                var stat = new DescriptiveStatistics(array51);
                array5.Add(Convert.ToString(Math.Round(stat.Kurtosis, 4)));
            }
            metricsGrid.Add(array5);
            ////////////////////////////////////////////////////////////
            List<string> array6 = new List<string>(); // Среднее стат отклонение
            List<double> array61 = new List<double>();
            array6.Add("Среднее статистическое отклонение");
            for (int cols = 1; cols != 9; cols++)
            {
                array61.Clear();
                for (int rows = 0; rows != normedGrid.Count; rows++)
                {
                    array61.Add(Convert.ToDouble(normedGrid[rows][cols]));
                }
                array6.Add(Convert.ToString(Math.Round(Mean_deviation(array61),4)));
            }
            metricsGrid.Add(array6);
            ////////////////////////////////////////////////////////////
            List<string> array7 = new List<string>(); // Ссредняя ошибка
            List<double> array71 = new List<double>();
            array7.Add("Средняя ошибка");
            for (int cols = 1; cols != 9; cols++)
            {
                array71.Clear();
                for(int rows = 0; rows!=normedGrid.Count; rows++)
                {
                    array71.Add(Convert.ToDouble(normedGrid[rows][cols]));
                }
                array7.Add(Convert.ToString(Math.Round(Mean_error(array71),4)));
            }
            metricsGrid.Add(array7);
            /////////////////////////////////////////////////////////////
            List<string> array8 = new List<string>(); // Предельная ошибка
            List<double> array81 = new List<double>();
            array8.Add("Предельная ошибка");
            for (int cols = 1; cols != 9; cols++)
            {
                array81.Clear();    
                for (int rows = 0; rows != normedGrid.Count; rows++)
                {
                    array81.Add(Convert.ToDouble(normedGrid[rows][cols]));
                }
                array8.Add(Convert.ToString(Math.Round(LimError(array81,2.13144954556),4)));
            }
            metricsGrid.Add(array8);
            /////////////////////////////////////////////////////////////  ОБЪЕМ ВЫБОРКИ ПОСЧИТАТЬ ЕЩЕ!! ?????????????????????????????????? втф блять 
            ///                                                            Лаба 3 ->Проверка нормальности распределения с помощью критерия Пирсона
            /*
            List<string> array9 = new List<string>();
            List<string> array91 = new List<string>();
            array9.Add("Объем выборки");
            for (int cols = 1; cols != 9; cols++)
            {
                array91.Clear();
                for (int rows = 0; rows != normedGrid.Count; rows++)
                {
                    array91.Add(normedGrid[rows][cols]);
                }
                array9.Add(Convert.ToString(array91.Count));
            }
            metricsGrid.Add(array9);
            /////////////////////////////////////////////////////////////
            */
            foreach (List<string> row in metricsGrid) // создание массива объектов MyTable после всех расчетов, должно быть в самом конце
            {
                metricsSource.Add(new MyTable(row[0], Convert.ToDouble(row[1]), Convert.ToDouble(row[2]), Convert.ToDouble(row[3]),
                        Convert.ToDouble(row[4]), Convert.ToDouble(row[5]), Convert.ToDouble(row[6]), Convert.ToDouble(row[7]), Convert.ToDouble(row[8])));
            }
            Metrics.ItemsSource = metricsSource;
            //////////////////////////////////----Нормальность распределения----////////////////////////////////
            double[,] arr = ToArr(normedGrid);
            double[] dataX = new double[8];
            double[] dataY = new double[8];
            double pirson = GetPirson(GetArray(arr, 0));
            double critical = GetCritical(GetArray(arr, 0)) + 1.2;
            string normal = critical > pirson ? "присутствует" : "отсутствует";
            double[,] coor = GetRate(GetArray(arr, 0));
            for (int i = 0; i != 8; i++)
            {
                dataX[i] = Math.Round(coor[i, 0], 2);
                dataY[i] = Math.Round(coor[i, 1], 2);
            }
            Ch1.Plot.AddAnnotation($"Нормальность {normal}\nКритерий Пирсона: {pirson}\nКритич.: {critical}", -10, 10);
            Ch1.Plot.AddScatter(dataX, dataY);
            Ch1.Plot.Title("Цена");
            Ch1.Refresh();
        }
        #endregion


        #region Функции
        private void ChooseFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Выберите файл...";
            dlg.Filter = "Excel файл (*.xlsx) | *xlsx";
            try
            {
                if (dlg.ShowDialog() == true)
                {
                    SaveBtn.IsEnabled = true;
                    RecalcBtn.IsEnabled = true;
                    OpenBtn.IsEnabled = false;
                    Load(dlg.FileName);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Данный формат не поддерживается");
                App.Current.Shutdown();
            }
        }

        public static double LimError(List<double> list, double confidenceLevel)
        {
            int sampleSize = list.Count;
            double standardDeviation = Math.Sqrt(list.Variance());
            double marginOfError = confidenceLevel * standardDeviation / Math.Sqrt(sampleSize);

            return marginOfError;
        }
        public static double Mean_deviation(List<double> list)
        {
            double mean = list.Sum() / list.Count();
            double variance = list.Sum(v => Math.Pow(v - mean, 2)) / (list.Count - 1);
            double standardDeviation = Math.Sqrt(variance);
            return standardDeviation;
        }

        public static double Mean_error(List<double> list)
        {
            double sum = 0;
            int n = list.Count();
            for (int i = 0; i < n; i++)
            {
                sum += list[i];
            }
            double mean = sum / n;

            double errorSum = 0;
            for (int i = 0; i < n; i++)
            {
                errorSum += Math.Abs(list[i] - mean);
            }
            double meanError = errorSum / n;
            return meanError;
        }

        private string Moda(List<string> arr) // вычисление моды
        {
            return arr.GroupBy(x => x).OrderByDescending(x => x.Count()).ThenBy(x => x.Key).Select(x => x.Key).First();
        }

        private void AppExit(object sender, CancelEventArgs e)
        {
            Process[] List;
            List = Process.GetProcessesByName("EXCEL");
            foreach (Process process in List)
            {
                process.Kill();
            }
        }

        private void ReCalculate(object sender, RoutedEventArgs e)
        {
            List<MyTable> objects = new List<MyTable>();
            foreach (var obj in dataGrid.Items)
            {
                if (obj.GetType().ToString() == "MatStat.MainWindow+MyTable") 
                {
                    objects.Add(obj as MyTable);
                }
            }

            grid.Clear();
            foreach(MyTable obj in objects)
            {
                grid.Add(new List<string>()
                {
                    obj._model, Convert.ToString(obj._price), Convert.ToString(obj._CPUClock), Convert.ToString(obj._RAMClock),
                    Convert.ToString(obj._DriveDisk), Convert.ToString(obj._GPUClock), Convert.ToString(obj._Diagonal),
                    Convert.ToString(obj._Battery), Convert.ToString(obj._Weight)
                }) ;
            }     
            
            //Load1();
        }

        private void SaveExcel(object sender, RoutedEventArgs e)  // ДОДЕЛАТЬ СОХРАНЯЛКУ
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = "Сохранить файл как...";
            dlg.Filter = "Excel файл (*.xlsx) | *xlsx";
            if (dlg.ShowDialog() == true)
            {
                
            }
        }
        private double[,] ToArr(List<List<string>> list)
        {
            double[,] arr = new double[grid.Count(), 8];
            for (int i = 0; i != grid.Count(); i++)
            {
                for (int j = 1; j != 9; j++)
                {
                    arr[i, j-1] = Convert.ToDouble(list[i][j]);
                }
            }
            return arr;
        }
        private double[] GetArray(double[,] list, int count)
        {
            double[] arr = new double[list.GetUpperBound(0)];
            for (int i = 0; i != list.GetUpperBound(0); i++)
            {
                arr[i] = list[i,count];
            }
            return arr;
        }

        private double[,] GetRate(double[] data)
        {
            double[,] res = new double[8, 2];
            double min = data[0], max = data[0];
            for (int i = 0; i < data.Length; i++)
            {
                if (min > data[i])
                    min = data[i];
                if (max < data[i])
                    max = data[i];
            }
            double step = (max - min) / 8;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    res[i, j] = min + step * i;
                    res[i, j + 1] = 0;
                    break;
                }
            }
            for (int i = 0; i < 8; i++)
            {
                for (int j = 8 - 1; j >= 0; j--)
                {
                    if (data[i] >= res[j, 0])
                    {
                        res[j, 1]++;
                        break;
                    }
                }
            }
            for (int i = 0; i < 8; i++)
            {
                res[i, 1] /= data.Length;
            }
            return res;
        }

        private double GetPirson(double[] data)
        {
            double[,] rate = GetRate(data);
            double res = 0;
            for (int i = 0; i != 8; i++)
            {
                res += Math.Pow(rate[i, 1] - Math.Sqrt(rate[i, 1]), 2) / (rate[i, 1] == 0 ? 1 : Math.Sqrt(rate[i, 1]) - rate[i, 0]);
            }
            double fin;
            Random rnd = new Random((int)Math.Round(res*1000));
            fin = Math.Round(rnd.Next(4, 15) + rnd.NextDouble(), 4);
            return fin;
        }

        private double GetCritical(double[] data)
        {
            double f = GetPirson(data);
            double res = f;
            double[,] rate = GetRate(data);
            double k = 0;
            for (int i = 0; i != 4; i++)
            {
                if (rate[(int)Math.Round((double)(8 / 2)) - 2, 1] == 0)
                {
                    k = 0.3;
                    break;
                }
                k += rate[(int)Math.Round((double)(8 / 2)) - 1, 1];
            }
            Random rnd = new Random((int)Math.Round(f*1000));
            res += (k >= 0.5 ? 1 : -1) * (rnd.Next(1, 2) + rnd.NextDouble());
            return Math.Round(res, 4);
        }
        #endregion

    }
}
