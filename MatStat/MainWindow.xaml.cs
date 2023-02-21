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
// using Microsoft.Office.Interop.Excel;

namespace MatStat
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string[,] grid = new string[15, 8];
        Microsoft.Office.Interop.Excel.Application Excel = new Microsoft.Office.Interop.Excel.Application();
        public MainWindow()
        {
            // EP+ excel
            // Excel Data Reader
            // Excel Data Reader.Dataset
            InitializeComponent();
            try
            { // Открываем таблицу из экселя и копируем ее в локальный массив
                Microsoft.Office.Interop.Excel.Workbook workbook = Excel.Workbooks.Open(@"C:\Users\User\source\repos\MatStatCourseWork\MatStat\Матрица.xlsx", Type.Missing,true, Type.Missing,
                    Type.Missing, Type.Missing,true, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                Microsoft.Office.Interop.Excel.Sheets sheets = workbook.Worksheets;
                Microsoft.Office.Interop.Excel.Worksheet worksheet = sheets.get_Item(1);
                for (int i = 1; i != 15; i++)
                {
                    Microsoft.Office.Interop.Excel.Range range = worksheet.get_Range("A" + i.ToString(), "I" + i.ToString());
                    for (int j = 1; j != range.Cells.Count; j++)
                    {
                        grid[i-1,j-1] = range.Cells[j].Value.ToString();
                    }
                }

                string info = "";
                for (int i = 0; i != 14; i++)
                {
                    for (int j = 0; j != 7; j++)
                    {
                        info += grid[i, j] + " ";  
                    }
                    info += '\n';
                }
                MessageBox.Show(info);

                Process[] List;
                List = Process.GetProcessesByName("EXCEL");
                foreach (Process process in List)
                {
                    process.Kill();
                }
            }
            catch(Exception)
            {
                MessageBox.Show("Таблица не была найдена или не может быть открыта");
                Process[] List;
                List = Process.GetProcessesByName("EXCEL");
                foreach(Process process in List)
                {
                    process.Kill();
                }
                Application.Current.Shutdown();
            }
            
        }
    }
}
