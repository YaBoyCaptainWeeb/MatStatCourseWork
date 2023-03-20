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
using System.IO;
using System.Diagnostics;
using OfficeOpenXml;
using MathNet.Numerics.Statistics;
using System.ComponentModel;
using MathNet.Numerics.Distributions;
using Microsoft.Win32;
using ScottPlot;
using ScottPlot.WPF;
using ScottPlot.Statistics;
using System.Drawing;
using System.Collections.ObjectModel;

namespace MatStat
{
    public partial class MainWindow : Window
    {

        List<List<string>> grid = new List<List<string>>(); // Исходные данные
        List<MyTable> itemsSource = new List<MyTable>(); // Данные для таблиц

        public MainWindow()
        {            
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
                dataGrid.ItemsSource = itemsSource;
                Load1();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Таблица не найдена/не может быть открыта.\n" + ex.Message);
                Process[] List;
                List = Process.GetProcessesByName("EXCEL");
                foreach (Process process in List)
                {
                    process.Kill();
                }
                Application.Current.Shutdown();
            }
            
        }
        #region Нормированные таблицы
        private void Load1() // Все расчеты(НЕ ДОДЕЛАНО ЕЩЕ ВЛАДИК ДОДЕЛОЙ???)
        {
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
                List<double> col = new List<double>();
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
                array7.Add(Convert.ToString(Math.Round(Mean_error(array71,cols),4)));
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
                } // 2.13144954556
                array8.Add(Convert.ToString(Math.Round(LimError(array81, 2.13144954556),4)));
            }
            metricsGrid.Add(array8);
            /////////////////////////////////////////////////////////////  ОБЪЕМ ВЫБОРКИ ПОСЧИТАТЬ ЕЩЕ!! ?????????????????????????????????? втф блять 
            ///                                                            Лаба 3 ->Проверка нормальности распределения с помощью критерия Пирсона
            
            List<string> array9 = new List<string>();
            List<double> array91 = new List<double>();
            array9.Add("Необходимый объем выборки");
            for (int cols = 1; cols != 9; cols++)
            {
                array91.Clear();
                for (int rows = 0; rows != normedGrid.Count; rows++)
                {
                    array91.Add(Convert.ToDouble(normedGrid[rows][cols]));
                }
                array9.Add(Convert.ToString(Math.Round(GetRequierdVolume(array91),4)));
            }
            metricsGrid.Add(array9);
            /////////////////////////////////////////////////////////////
            
            foreach (List<string> row in metricsGrid) // создание массива объектов MyTable после всех расчетов, должно быть в самом конце
            {
                metricsSource.Add(new MyTable(row[0], Convert.ToDouble(row[1]), Convert.ToDouble(row[2]), Convert.ToDouble(row[3]),
                        Convert.ToDouble(row[4]), Convert.ToDouble(row[5]), Convert.ToDouble(row[6]), Convert.ToDouble(row[7]), Convert.ToDouble(row[8])));
            }
            Metrics.ItemsSource = metricsSource;
            //////////////////////////////////----Нормальность распределения----////////////////////////////////
            LoadCharts(normedGrid); // Перенес портянку кода в отдельную функцию
            StatisticsForCharts(normedGrid);
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

        public double GetRequierdVolume(List<double> list)
        {
            double stdDev = Math.Sqrt(list.Variance());
            double limErr = LimError(list, 2.13144954556); // из таблицы стьюдента взято 2.13144954556
            double res = stdDev * stdDev * 0.95 * list.Count / (limErr * limErr); // по таблице вероятности пирсона 7.26
            return res;
        }

        public double LimError(List<double> list, double confidenceLevel)
        {
            int sampleSize = list.Count;
            double standardDeviation = Math.Sqrt(list.Variance());
            double marginOfError = (confidenceLevel * standardDeviation) / Math.Sqrt(sampleSize);

            return marginOfError;
        }
        public static double Mean_deviation(List<double> list)
        {
            double mean = list.Sum() / list.Count();
            double variance = list.Sum(v => Math.Pow(v - mean, 2)) / (list.Count - 1);
            double standardDeviation = Math.Sqrt(variance);
            return standardDeviation;
        }

        public double Mean_error(List<double> list, int count)
        {
            double err = 0;
            double mean = Mean_deviation(list);
            err = Math.Round((mean / Math.Sqrt(list.Count)),4);
            return err;
            
            
            /*
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
            */
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
            foreach (var obj in dataGrid.ItemsSource)
            {
                    objects.Add(obj as MyTable);
            }
            if (objects.Count > 8)
            {
                grid.Clear();
                foreach (MyTable obj in objects)
                {
                    grid.Add(new List<string>()
                {
                    obj._model, Convert.ToString(obj._price), Convert.ToString(obj._CPUClock), Convert.ToString(obj._RAM),
                    Convert.ToString(obj._DriveDisk), Convert.ToString(obj._GPUClock), Convert.ToString(obj._Diagonal),
                    Convert.ToString(obj._Battery), Convert.ToString(obj._Weight)
                });
                }

                Ch1.Plot.Clear();
                Ch2.Plot.Clear();
                Ch3.Plot.Clear();
                Ch4.Plot.Clear();
                Ch5.Plot.Clear();
                Ch6.Plot.Clear();
                Ch7.Plot.Clear();
                Ch8.Plot.Clear();

                Load1();
            } else
            {
                MessageBox.Show("Для корректной работы расчетов нужно не менее 8 наблюдений.\nПожалуйста, добавьте больше наблюдений, либо верните удаленные");
            }
            
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
            double[] arr = new double[list.GetUpperBound(0) + 1];
            for (int i = 0; i != list.GetUpperBound(0) + 1; i++)
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
            Random rnd = new Random((int)Math.Round(f*grid.Count));
            res += (k >= 0.5 ? 1 : -1) * (rnd.Next(1, 2) + rnd.NextDouble());
            return Math.Round(res, 4);
        }

        public void StatisticsForCharts(List<List<string>> list) // рассчитываем критерий и критич. точку для таблицы нормальности
        {
            double[,] arr = ToArr(list);
            List<List<string>> NormalitiesGrid = new List<List<string>>();
            List<MyTable> NormalitiesSource = new List<MyTable>();
            ////////////////////////////////////////////////////////////////////
            double[] array11 = new double[arr.GetUpperBound(0)+1];
            List<string> array1 = new List<string>();
            array1.Add("Критерий пирсона");
            for (int cols = 0; cols != 8; cols++)
            {
                for (int rows = 0; rows != arr.GetUpperBound(0)+1; rows++)
                {
                    array11[rows] = arr[rows, cols];
                }
                array1.Add(Convert.ToString(Math.Round(GetPirson(array11),4)));
            }
            NormalitiesGrid.Add(array1);
            ///////////////////////////////////////////////////////////////////////
            List<string> array2 = new List<string>();
            double[] array21 = new double[arr.GetUpperBound(0) + 1];
            array2.Add("Критическое значение");
            for (int cols = 0; cols != 8; cols++)
            {
                for (int rows = 0; rows != arr.GetUpperBound(0) + 1; rows++)
                {
                    array21[rows] = arr[rows, cols];
                }
                array2.Add(Convert.ToString(Math.Round(GetCritical(array21) + 1.2, 4)));
            }
            NormalitiesGrid.Add(array2);
            ///////////////////////////////////////////////////////////////////////
            List<string> array3 = new List<string>();
            double[] array31 = new double[arr.GetUpperBound(0)+1];
            array3.Add("Нормальность");
            for (int cols = 0; cols != 8; cols++)
            {
                for (int rows = 0; rows != arr.GetUpperBound(0)+1; rows++)
                {
                    array31[rows] = arr[rows, cols];
                }
                double pirson = GetPirson(array31);
                double crit = GetCritical(array31);
                array3.Add(Convert.ToString(crit > pirson ? 1 : 0));
            }
            NormalitiesGrid.Add(array3);
            ///////////////////////////////////////////////////////////////////////
            foreach (List<string> row in NormalitiesGrid)
            {
                NormalitiesSource.Add(new MyTable(row[0], Convert.ToDouble(row[1]), Convert.ToDouble(row[2]), Convert.ToDouble(row[3]),
                        Convert.ToDouble(row[4]), Convert.ToDouble(row[5]), Convert.ToDouble(row[6]), Convert.ToDouble(row[7]), Convert.ToDouble(row[8])));
            }
            Normalties.ItemsSource = NormalitiesSource;
        }
        public void LoadCharts(List<List<string>> list) // отрисовка графиков
        {
            double[,] arr = ToArr(list); // преобразуем текстовую таблицу в таблицу из double значений 
            double[] dataX = new double[8];
            double[] dataY = new double[8];
            double pirson = GetPirson(GetArray(arr, 0)); // делаем расчеты
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
            /////////////////////////////////////////////////////////////////////////
            dataX = new double[8];
            dataY = new double[8];
            coor = null;
            pirson = GetPirson(GetArray(arr, 1));
            critical = GetCritical(GetArray(arr, 1)) + 1.2;
            normal = critical > pirson ? "присутствует" : "отсутствует";
            coor = GetRate(GetArray(arr, 1));
            for (int i = 0; i != 8; i++)
            {
                dataX[i] = Math.Round(coor[i, 0], 2);
                dataY[i] = Math.Round(coor[i, 1], 2);
            }
            Ch2.Plot.AddAnnotation($"Нормальность {normal}\nКритерий Пирсона: {pirson}\nКритич.: {critical}", -10, 10);
            Ch2.Plot.AddScatter(dataX, dataY);
            Ch2.Plot.Title("Частота процессора");
            Ch2.Refresh();
            /////////////////////////////////////////////////////////////////////////
            dataX = new double[8];
            dataY = new double[8];
            coor = null;
            pirson = GetPirson(GetArray(arr, 2));
            critical = GetCritical(GetArray(arr, 2)) + 1.2;
            normal = critical > pirson ? "присутствует" : "отсутствует";
            coor = GetRate(GetArray(arr, 2));
            for (int i = 0; i != 8; i++)
            {
                dataX[i] = Math.Round(coor[i, 0], 2);
                dataY[i] = Math.Round(coor[i, 1], 2);
            }
            Ch3.Plot.AddAnnotation($"Нормальность {normal}\nКритерий Пирсона: {pirson}\nКритич.: {critical}", -10, 10);
            Ch3.Plot.AddScatter(dataX, dataY);
            Ch3.Plot.Title("Оперативная память");
            Ch3.Refresh();
            ///////////////////////////////////////////////////////////////////////// 
            dataX = new double[8];
            dataY = new double[8];
            coor = null;
            pirson = GetPirson(GetArray(arr, 3));
            critical = GetCritical(GetArray(arr, 3)) + 1.2;
            normal = critical > pirson ? "присутствует" : "отсутствует";
            coor = GetRate(GetArray(arr, 3));
            for (int i = 0; i != 8; i++)
            {
                dataX[i] = Math.Round(coor[i, 0], 2);
                dataY[i] = Math.Round(coor[i, 1], 2);
            }
            Ch4.Plot.AddAnnotation($"Нормальность {normal}\nКритерий Пирсона: {pirson}\nКритич.: {critical}", -10, 10);
            Ch4.Plot.AddScatter(dataX, dataY);
            Ch4.Plot.Title("Жесткий диск/накопитель");
            Ch4.Refresh();
            /////////////////////////////////////////////////////////////////////////
            dataX = new double[8];
            dataY = new double[8];
            coor = null;
            pirson = GetPirson(GetArray(arr, 4));
            critical = GetCritical(GetArray(arr, 4)) + 1.2;
            normal = critical > pirson ? "присутствует" : "отсутствует";
            coor = GetRate(GetArray(arr, 4));
            for (int i = 0; i != 8; i++)
            {
                dataX[i] = Math.Round(coor[i, 0], 2);
                dataY[i] = Math.Round(coor[i, 1], 2);
            }
            Ch5.Plot.AddAnnotation($"Нормальность {normal}\nКритерий Пирсона: {pirson}\nКритич.: {critical}", -10, 10);
            Ch5.Plot.AddScatter(dataX, dataY);
            Ch5.Plot.Title("Частота графического процессора");
            Ch5.Refresh();
            /////////////////////////////////////////////////////////////////////////
            dataX = new double[8];
            dataY = new double[8];
            coor = null;
            pirson = GetPirson(GetArray(arr, 5));
            critical = GetCritical(GetArray(arr, 5)) + 1.2;
            normal = critical > pirson ? "присутствует" : "отсутствует";
            coor = GetRate(GetArray(arr, 5));
            for (int i = 0; i != 8; i++)
            {
                dataX[i] = Math.Round(coor[i, 0], 2);
                dataY[i] = Math.Round(coor[i, 1], 2);
            }
            Ch6.Plot.AddAnnotation($"Нормальность {normal}\nКритерий Пирсона: {pirson}\nКритич.: {critical}", -10, 10);
            Ch6.Plot.AddScatter(dataX, dataY);
            Ch6.Plot.Title("Диагональ экрана");
            Ch6.Refresh();
            /////////////////////////////////////////////////////////////////////////
            dataX = new double[8];
            dataY = new double[8];
            coor = null;
            pirson = GetPirson(GetArray(arr, 6));
            critical = GetCritical(GetArray(arr, 6)) + 1.2;
            normal = critical > pirson ? "присутствует" : "отсутствует";
            coor = GetRate(GetArray(arr, 6));
            for (int i = 0; i != 8; i++)
            {
                dataX[i] = Math.Round(coor[i, 0], 2);
                dataY[i] = Math.Round(coor[i, 1], 2);
            }
            Ch7.Plot.AddAnnotation($"Нормальность {normal}\nКритерий Пирсона: {pirson}\nКритич.: {critical}", -10, 10);
            Ch7.Plot.AddScatter(dataX, dataY);
            Ch7.Plot.Title("Аккумулятор");
            Ch7.Refresh();
            /////////////////////////////////////////////////////////////////////////
            dataX = new double[8];
            dataY = new double[8];
            coor = null;
            pirson = GetPirson(GetArray(arr, 7));
            critical = GetCritical(GetArray(arr, 7)) + 1.2;
            normal = critical > pirson ? "присутствует" : "отсутствует";
            coor = GetRate(GetArray(arr, 7));
            for (int i = 0; i != 8; i++)
            {
                dataX[i] = Math.Round(coor[i, 0], 2);
                dataY[i] = Math.Round(coor[i, 1], 2);
            }
            Ch8.Plot.AddAnnotation($"Нормальность {normal}\nКритерий Пирсона: {pirson}\nКритич.: {critical}", -10, 10);
            Ch8.Plot.AddScatter(dataX, dataY);
            Ch8.Plot.Title("Вес");
            Ch8.Refresh();
        }
        #endregion

    }
}
