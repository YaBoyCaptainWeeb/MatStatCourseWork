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
using LiveCharts;
using LiveCharts.Wpf;
using System.Drawing;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;
using static MatStat.XiPearsonSquare;
using static MatStat.Correlation;
using static MatStat.RegressionCalculates;
using System.Data;

namespace MatStat
{
    public partial class MainWindow : Window
    {
        public static MainWindow mainWindow; // создаем точку доступа для других классов

        public static List<List<string>> grid = new List<List<string>>(); // Исходные данные
        List<MyTable> itemsSource = new List<MyTable>(); // Данные для таблиц
        public static List<List<double>> StudentDistributionList = new List<List<double>>(); // Распределение Стьюдента
        public static List<List<string>> normedGrid;
        public int cmbSelected;

        public DataTable BettaTable { get; set; }
        public DataTable YTable { get; set; }
        public string RegrText { get; set; }
        public MainWindow()
        {            
            InitializeComponent();
            mainWindow = this;
            cmbSelected = Cmbx.SelectedIndex;
        }

        private void Load(string filepath) // Источник данных
        {
            try
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                ExcelPackage excelPackage = new ExcelPackage(filepath);
                ExcelWorksheet excelWorksheet = excelPackage.Workbook.Worksheets[0]; // Исходные данные
                ExcelWorksheet excelWorksheet1 = excelPackage.Workbook.Worksheets[2]; // Распределение Стьюдента


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

            if (grid.Count > 8)
            {
                string[,] buff = new string[30,7];
                for (int i = 4; true; i++) // копируем таблицу из Excel для распределения Стьюдента В ПРОЦЕССЕ
                {
                    if (excelWorksheet1.Cells[i, 3].Value == null) break;
                    List<string> row = new List<string>();
                    for (int j = 3; true; j++)
                    {
                        if (excelWorksheet1.Cells[i, j].Value == null) break;
                        row.Add(excelWorksheet1.Cells[i, j].Value.ToString());
                    }
                    for (int k = 0; k != row.Count; k++)
                    {
                        buff[i-4,k] = row[k];
                    }
                }

                for (int i = 0; i != buff.GetUpperBound(0)+1; i++) // Копируем значения из буферного массива в List
                {
                    List<double> row = new List<double>();
                    for (int j = 0; j != buff.GetUpperBound(1)+1; j++)
                    {
                        row.Add(Convert.ToDouble(buff[i, j]));
                    }
                    StudentDistributionList.Add(row);
                }
                foreach (List<string> row in grid) // делаем копию в itemSource для загрузки данных в нужном формате для DataGrid
                {
                    itemsSource.Add(new MyTable(row[0], Convert.ToDouble(row[1]), Convert.ToDouble(row[2]), Convert.ToDouble(row[3]),
                        Convert.ToDouble(row[4]), Convert.ToDouble(row[5]), Convert.ToDouble(row[6]), Convert.ToDouble(row[7]), Convert.ToDouble(row[8])));
                }
                dataGrid.ItemsSource = itemsSource;
                Load1();
            } else
            {
                MessageBox.Show("Количество наблюдений меньше 8, корректный рассчет стастиски будет маловероятным, добавьте больше наблюдений.", "Предупреждение"
                    , MessageBoxButton.OK, MessageBoxImage.Hand);
                Application.Current.Shutdown();
            }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Возможно, вы ввели неверные данные\n" + ex.Message.ToString() + "\n" + ex.StackTrace.ToString());

            }                
        }
        #region Нормированные таблицы и корреляция
        private void Load1() // Все расчеты
        {
            List<MyTable> itemsSource = new List<MyTable>(); // Массив для таблиц обычных(для таблиц)
            List<MyTable> metricsSource = new List<MyTable>(); // Массив для таблиц со статистиками(для таблиц)
            normedGrid = new List<List<string>>(); // Массив с нормированным данными(не для таблиц)
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
            itemsSource = MakeTableView(normedGrid);
            NormGrid.ItemsSource = itemsSource;
            /////////////////////////////////////////////////////----Статистики----////////////////////////////////////////////////////////////////
            metricsGrid = DescripiveStatistics.LoadStatistics(normedGrid);
            metricsSource = MakeTableView(metricsGrid);
            Metrics.ItemsSource = metricsSource;
            //////////////////////////////////----Нормальность распределения----////////////////////////////////
            LoadCharts(normedGrid);
            StatisticsForCharts(normedGrid);
            //////////////////////////////////----Корреляции----////////////////////////////////////////////////
            LoadCorrelations(normedGrid);
            //////////////////////////////////----Регрессия(не готова до конца)----////////////////////////////////////////////////
            RegressionCalculates regr = new RegressionCalculates();
            regr.LoadRegression(ToSteppedArr(ToArr(normedGrid)));
        }

        private void LoadCorrelations(List<List<string>> normedGrid)
        {
            /* 
            List<MyTable> coupleCorrelationSource = new List<MyTable>(); // Массив для таблицы с парной корреляцией
            List<MyTable> coupleCriteriaStudentSource = new List<MyTable>(); // Массив для таблицы с критерием для парной корреляции
            List<MyTable> coupleSignificanceSource = new List<MyTable>(); // Массив для таблицы с коэфф.значимости для парной корреляции
            List<MyTable> partialCorrelationSource = new List<MyTable>(); // Массив для таблицы с частной корреляцией
            List<MyTable> partialCriteriaStudentSource = new List<MyTable>(); // Массив для таблицы с криетрием для частной корреляции
            List<MyTable> partialSignificanceSource = new List<MyTable>(); // Массив для таблицы с коэфф.значимости для частной корреляции
            */
            double[,] coupleCorrelatedArr = Correlation.CoupleCorrelate(ToArr(normedGrid)); // Парная корреляция
            double[,] coupleCriteriaStudent = Correlation.CoupleCriteriaStudent(coupleCorrelatedArr); // Критерий Стьюдента для парной корреляции
            double[,] coupleSignificanceLevel = Correlation.SignificanceLevel(coupleCriteriaStudent, GetCurrentConstant(StudentDistributionList)); // Коэффициент значимости для парной корреляции
            CurrentConstant.Text = GetCurrentConstant(StudentDistributionList).ToString();
            double[][] partialCorrelatedArr = Correlation.PartialCorrelate(Correlation.ToSteppedArr(coupleCorrelatedArr)); // Частная корреляция
            double[][] partialCriteriaStudent = Correlation.PartialCriteriaStudent(partialCorrelatedArr); // Критерий Стьюдента для частной корреляции
            double[,] partialSignificanceLevel = Correlation.SignificanceLevel(ToArr(partialCriteriaStudent), GetCurrentConstant(StudentDistributionList)); // Коэффициент значимости для частной корреляции

            List<TriangleTable> triangleCoupleView = MakeTriangleView(coupleCorrelatedArr);
            List<TriangleTable> triangleCoupleCriteriaView = MakeTriangleView(coupleCriteriaStudent);
            List<TriangleTable> triangleCoupleSignificanceView = MakeTriangleView(coupleSignificanceLevel);
            List<TriangleTable> trianglePartialView = MakeTriangleView(ToArr(partialCorrelatedArr));
            List<TriangleTable> trianglePartialCriteriaView = MakeTriangleView(ToArr(partialCriteriaStudent));
            List<TriangleTable> trianglePartialSignificanceView = MakeTriangleView(partialSignificanceLevel);

            CoupleCorrelation.ItemsSource = triangleCoupleView;
            CoupleCriteriaStudent.ItemsSource = triangleCoupleCriteriaView;
            CoupleSignificance.ItemsSource = triangleCoupleSignificanceView;
            PartialCorrelation.ItemsSource = trianglePartialView;
            PartialCriteriaStudent.ItemsSource = trianglePartialCriteriaView;
            PartialSignificance.ItemsSource = triangleCoupleSignificanceView;

            Correlation.DrawDiagramm(coupleCorrelatedArr, Canv, 1); // Диаграмма для парных корреляций
            Correlation.DrawDiagramm(ToArr(partialCorrelatedArr), Canv1, 1); // Диаграмма для частных корреляций

            Correlation.DrawDiagramm(coupleCorrelatedArr, Canv2, 2); // Только сильная связь у парной корреляции
            Correlation.DrawDiagramm(ToArr(partialCorrelatedArr), Canv3, 2); // Только сильная связь у частной корреляции
        }
        #endregion
    
        #region Функции
        private void Selected(object sender, EventArgs e)
        {
            CurrentConstant.Text = GetCurrentConstant(StudentDistributionList).ToString();
            CorrelationRecalculate(normedGrid);
        }
        private double GetCurrentConstant(List<List<double>> arr)
        {
            return arr[grid.Count-1][Cmbx.SelectedIndex];
        }
        private void OpenStudentTable(object sender, RoutedEventArgs e)
        {
            StudentDistribution win = new StudentDistribution(StudentDistributionList);
            win.Show();
        }
        internal List<MyTable> MakeTableView(List<List<string>> array) // Приведение массивов к массиву экземпляров класса для DataGrid
        {
            List<MyTable> result = new List<MyTable>();
            foreach(List<string> row in array)
            {
                result.Add(new MyTable(row[0], Convert.ToDouble(row[1]), Convert.ToDouble(row[2]), Convert.ToDouble(row[3]),
                        Convert.ToDouble(row[4]), Convert.ToDouble(row[5]), Convert.ToDouble(row[6]), Convert.ToDouble(row[7]), Convert.ToDouble(row[8])));
            }
            return result;
        }
        private List<TriangleTable> MakeTriangleView(double[,] arr) // Тот же самый MakeTableView, только в треугольном виде
        {
            List<TriangleTable> result = new List<TriangleTable>();
            for (int i = 0; i != arr.GetUpperBound(0)+1; i++)
            {
                List<string> row = new List<string>();
                for (int j = 0; j != arr.GetUpperBound(1)+1; j++)
                {
                    if (j <= i)
                    {
                        row.Add(arr[i,j].ToString());
                    } else
                    {
                        row.Add(null);
                    }
                    
                }
                result.Add(new TriangleTable(CoupleCorrelation.Columns[i+1].Header.ToString(),row[0], row[1], row[2], row[3], row[4], row[5], row[6], row[7]));
            }
            return result;
        }

        private void ChooseFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Title = "Выберите файл...",
                Filter = "Excel файл (*.xlsx) | *xlsx"
            };
            if (dlg.ShowDialog() == true)
                {
                    SaveBtn.IsEnabled = true;
                    RecalcBtn.IsEnabled = true;
                    OpenBtn.IsEnabled = false;
                    StatisticsTab.IsEnabled = true;
                    PearsonTab.IsEnabled = true;
                    CorrelationTab.IsEnabled = true;
                    RegressionTab.IsEnabled = true;
                    Load(dlg.FileName);
                }
        }    

        private void AppExit(object sender, CancelEventArgs e)
        {
            Process[] List;
            List = Process.GetProcessesByName("EXCEL");
            foreach (Process process in List)
            {
                process.Kill();
            }
            Application.Current.Shutdown();
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
                /*
                Ch1.Plot.Clear();
                Ch2.Plot.Clear();
                Ch3.Plot.Clear();
                Ch4.Plot.Clear();
                Ch5.Plot.Clear();
                Ch6.Plot.Clear();
                Ch7.Plot.Clear();
                Ch8.Plot.Clear();
                */
                PlotPanel.Children.Clear();
                Load1();
            } else
            {
                MessageBox.Show("Для корректной работы расчетов нужно не менее 8 наблюдений.\nПожалуйста, добавьте больше наблюдений, либо верните удаленные");
            }
            
        }
        private void CorrelationRecalculate(List<List<string>> normedGrid)
        {
            if (Cmbx.SelectedIndex != cmbSelected)
            {
                LoadCorrelations(normedGrid);
                cmbSelected = Cmbx.SelectedIndex;
            }
        }

        private void SaveExcel(object sender, RoutedEventArgs e)  // ДОДЕЛАТЬ СОХРАНЯЛКУ
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                Title = "Сохранить файл как...",
                Filter = "Excel файл (*.xlsx) | *xlsx"
            };
            if (dlg.ShowDialog() == true)
            {
                
            }
        }
       
        internal double[,] ToArr(List<List<string>> list) // запарсить List в массив double[,]
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
        internal double[,] ToArr(double[][] arr) // Парсим ступенчатый массив в double (передавать только симметричные массивы)
        {
            double[,] res = new double[arr.GetUpperBound(0) + 1, arr[0].GetUpperBound(0) + 1];
            for (int i = 0; i != arr.GetUpperBound(0) + 1; i++)
            {
                for (int j = 0; j != arr[0].GetUpperBound(0) + 1; j++)
                {
                    res[i, j] = arr[i][j];
                }
            }
            return res;
        }
        #endregion
    }
}
