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
        List<List<string>> grid = new List<List<string>>();
        List<MyTable> itemsSource = new List<MyTable>();
        Microsoft.Office.Interop.Excel.Application Excel = new Microsoft.Office.Interop.Excel.Application();

        public MainWindow()
        {
            // EP+ excel
            // Excel Data Reader
            // Excel Data Reader.Dataset
            
            InitializeComponent();
            try
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                ExcelPackage excelPackage = new ExcelPackage(@".\Матрица.xlsx");
                ExcelWorksheet excelWorksheet = excelPackage.Workbook.Worksheets[0];
               for (int i = 2; true; i++)
               {
                    if (excelWorksheet.Cells[i, 1].Value == null) break;
                    List<string> row = new List<string>();
                    for (int j = 1; true; j++)
                    {
                        if (excelWorksheet.Cells[i,j].Value == null) break;
                        row.Add(excelWorksheet.Cells[i, j].Value.ToString());
                    }
                    grid.Add(row);
               }
               foreach(List<string> row in grid) // делаем копию в itemSource для загрузки данных в нужном формате для DataGrid
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
        }

        private void Load(object sender, RoutedEventArgs e)
        {
           dataGrid.ItemsSource = itemsSource;
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
