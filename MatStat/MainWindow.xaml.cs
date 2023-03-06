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
using ExcelDataReader;
using OfficeOpenXml;
using System.ComponentModel;

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
        List<MyTable> itemsSource = new List<MyTable>();
        Microsoft.Office.Interop.Excel.Application Excel = new Microsoft.Office.Interop.Excel.Application();

        public MainWindow()
        {
            // EP+ excel
            // Excel Data Reader
            // Excel Data Reader.Dataset
            
            InitializeComponent();
        }

        private void Load(object sender, RoutedEventArgs e) // Источник данных
        {
            try
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                ExcelPackage excelPackage = new ExcelPackage(@".\Матрица.xlsx");
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
        private void Load1(object sender, RoutedEventArgs e) // Выборки, нормированная таблица(НЕ ДОДЕЛАНО ЕЩЕ ВЛАДИК ДОДЕЛОЙ???)
                                                             // Нормирование есть, степень доверия осталась, не ебу как делоть
        {
            List<double> col = new List<double>();
            List<MyTable> itemsSource = new List<MyTable>();
            List<MyTable> metricsSource = new List<MyTable>();
            List<List<string>> normedGrid = new List<List<string>>();
            List<List<string>> metricsGrid = new List<List<string>>();

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
                    normedGrid[rows1][cols] = Convert.ToString(Math.Round(Xnorm,2));
                }
            }

            foreach(List<string> row in normedGrid)
            {
                itemsSource.Add(new MyTable(row[0], Convert.ToDouble(row[1]), Convert.ToDouble(row[2]), Convert.ToDouble(row[3]),
                        Convert.ToDouble(row[4]), Convert.ToDouble(row[5]), Convert.ToDouble(row[6]), Convert.ToDouble(row[7]), Convert.ToDouble(row[8])));
            }
            NormGrid.ItemsSource = itemsSource;
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
                array3.Add(Median(median));
            }
            metricsGrid.Add(array3);
            ////////////////////////////////////////////////////////
            List<string> array4 = new List<string>(); // Дисперсия
            List<double> array41 = new List<double>();
            array4.Add("Дисперсия");
            for (int cols = 1; cols != 9; cols++)
            {
                array41.Clear();
                double srAriphmetic = 0;
                for (int rows = 0; rows != normedGrid.Count; rows++)
                {
                    srAriphmetic += Convert.ToDouble(normedGrid[rows][cols]);
                }
                srAriphmetic /= normedGrid.Count;
                srAriphmetic = Math.Round(srAriphmetic, 2);
                for (int rows = 0; rows != normedGrid.Count; rows++)
                {
                    array41.Add(Math.Pow((Convert.ToDouble(normedGrid[rows][cols]) - srAriphmetic), 2));
                }
                array4.Add(Convert.ToString(Math.Round(array41.Sum() / (normedGrid.Count - 1),2)));
            }
            metricsGrid.Add(array4);
            ////////////////////////////////////////////////////////////
            foreach (List<string> row in metricsGrid) // создание массива объектов MyTable после всех расчетов, должно быть в самом конце
            {
                metricsSource.Add(new MyTable(row[0], Convert.ToDouble(row[1]), Convert.ToDouble(row[2]), Convert.ToDouble(row[3]),
                        Convert.ToDouble(row[4]), Convert.ToDouble(row[5]), Convert.ToDouble(row[6]), Convert.ToDouble(row[7]), Convert.ToDouble(row[8])));
            }
            Metrics.ItemsSource = metricsSource;
        }
        #endregion
        #region Не нормированные таблицы
        private void Load2(object sender, RoutedEventArgs e) // не нормированная таблица
        {
            List<MyTable> itemsSource = new List<MyTable>();
            List<MyTable> metricsSource = new List<MyTable>();
            List<List<string>> table = new List<List<string>>();

            for (int i = 0; i != grid.Count; i++) // копия grid для внутренней работы функции
            {
                List<string> row = new List<string>();
                for (int j = 0; j != grid[i].Count; j++)
                {
                    row.Add(grid[i][j]);
                }
                table.Add(row);
            }
            foreach (List<string> row in table)
            {
                itemsSource.Add(new MyTable(row[0], Convert.ToDouble(row[1]), Convert.ToDouble(row[2]), Convert.ToDouble(row[3]),
                        Convert.ToDouble(row[4]), Convert.ToDouble(row[5]), Convert.ToDouble(row[6]), Convert.ToDouble(row[7]), Convert.ToDouble(row[8])));
            }

            NormGrid.ItemsSource = itemsSource;

            List<string> str = new List<string>();   // среднее арифметическое еще дорабатывать
            str.Add("Среднее арифметическое");
            for (int cols = 1; cols != 9; cols++)
            {
                double srAriphmetic = 0;
                for (int rows = 0; rows != grid.Count; rows++)
                {
                    srAriphmetic += Convert.ToDouble(grid[rows][cols]);
                }
                srAriphmetic /= grid.Count;
                str.Add(Math.Round(srAriphmetic, 2).ToString());
            }

            
                metricsSource.Add(new MyTable(str[0], Convert.ToDouble(str[1]), Convert.ToDouble(str[2]), Convert.ToDouble(str[3]),
                        Convert.ToDouble(str[4]), Convert.ToDouble(str[5]), Convert.ToDouble(str[6]), Convert.ToDouble(str[7]), Convert.ToDouble(str[8])));
            Metrics.ItemsSource = metricsSource;
        }
        #endregion

        private string Median(List<double> arr)
        {
            int numCount = arr.Count;
            int halfindex = arr.Count / 2;
            var sortedItems = arr.OrderBy(x => x);
            double median;
            if ((numCount%2) == 0)
            {
                median = ((sortedItems.ElementAt(halfindex) + sortedItems.ElementAt((halfindex - 1))) / 2);
            } else
            {
                median = sortedItems.ElementAt(halfindex);
            }
            return Convert.ToString(median);
        }

        private string Moda(List<string> arr)
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
    }
}
