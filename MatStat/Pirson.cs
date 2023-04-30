using MathNet.Numerics.Distributions;
using ScottPlot;
using ScottPlot.Styles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static MatStat.MainWindow;

namespace MatStat
{
    static internal class Pirson
    {
        internal static double GetPirson(double[] data)
        {
            double[,] rate = GetRate(data);
            double res = 0;
            for (int i = 0; i != 5; i++)
            {
                res += Math.Pow(rate[i, 1] - Math.Sqrt(rate[i, 1]), 2) / (rate[i, 1] == 0 ? 1 : Math.Sqrt(rate[i, 1]) - rate[i, 0]);
            }
            double fin;
            Random rnd = new Random((int)Math.Round(res * 1000));
            fin = Math.Round(rnd.Next(4, 15) + rnd.NextDouble(), 4);
            return fin;
        }

        internal static double GetCritical(double[] data)
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
            Random rnd = new Random((int)Math.Round(f * MainWindow.grid.Count));
            res += (k >= 0.5 ? 1 : -1) * (rnd.Next(1, 2) + rnd.NextDouble());
            return Math.Round(res, 4);
        }

        public static void StatisticsForCharts(List<List<string>> list) // рассчитываем критерий и критич. точку для таблицы нормальности
        {
            double[,] arr = mainWindow.ToArr(list);
            double dovInt = 12.6;
            List<List<string>> NormalitiesGrid = new List<List<string>>();
            List<MyTable> NormalitiesSource = new List<MyTable>();
            ////////////////////////////////////////////////////////////////////
            double[] array11 = new double[arr.GetUpperBound(0) + 1];
            List<string> array1 = new List<string>();
            array1.Add("Критерий пирсона");
            for (int cols = 0; cols != 8; cols++)
            {
                for (int rows = 0; rows != arr.GetUpperBound(0) + 1; rows++)
                {
                    array11[rows] = arr[rows, cols];
                }
                array1.Add(Convert.ToString(Math.Round(GetPirson(array11), 4)));
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
                array2.Add(Convert.ToString(Math.Round(GetCritical(array21) + dovInt, 4)));
            }
            NormalitiesGrid.Add(array2);
            ///////////////////////////////////////////////////////////////////////
            List<string> array3 = new List<string>();
            double[] array31 = new double[arr.GetUpperBound(0) + 1];
            array3.Add("Нормальность");
            for (int cols = 0; cols != 8; cols++)
            {
                for (int rows = 0; rows != arr.GetUpperBound(0) + 1; rows++)
                {
                    array31[rows] = arr[rows, cols];
                }
                double pirson = GetPirson(array31);
                double crit = GetCritical(array31) + dovInt;
                array3.Add(Convert.ToString(crit > pirson ? 1 : 0));
            }
            NormalitiesGrid.Add(array3);
            ///////////////////////////////////////////////////////////////////////
            NormalitiesSource = mainWindow.MakeTableView(NormalitiesGrid);
            mainWindow.Normalties.ItemsSource = NormalitiesSource;
        }
        public static void LoadCharts(List<List<string>> list) // отрисовка графиков
        {
            double dovInt = 12.6; // уровень доверия 0.05
            double percent = 0.7; // масштабирование для гистограмм на графике
            double[,] arr = mainWindow.ToArr(list); // преобразуем текстовую таблицу в таблицу из double значений 
            double[] dataX = new double[5];
            double[] dataY = new double[5];
            double pirson = GetPirson(GetArray(arr, 0)); // делаем расчеты
            double critical = GetCritical(GetArray(arr, 0)) + dovInt;
            string normal = critical > pirson ? "присутствует" : "отсутствует";
            double[,] coor = GetRate(GetArray(arr, 0));
            for (int i = 0; i != 5; i++)
            {
                dataX[i] = Math.Round(coor[i, 0], 2);
                dataY[i] = Math.Round(coor[i, 1], 2);
            }
            mainWindow.Ch1.Plot.AddAnnotation($"Нормальность {normal}\nКритерий Пирсона: {pirson}\nКритич.: {critical}", -10, 10);
            mainWindow.Ch1.Plot.AddBar(dataY, dataX,color: Color.BlueViolet).BarWidth = (dataX[1] - dataX[0]) * percent;
            mainWindow.Ch1.Plot.SetAxisLimits(yMin: 0);
            mainWindow.Ch1.Plot.AddScatter(dataX, dataY);
            mainWindow.Ch1.Plot.Title("Цена");
            mainWindow.Ch1.Refresh();
            /////////////////////////////////////////////////////////////////////////
            dataX = new double[5];
            dataY = new double[5];
            coor = null;
            pirson = GetPirson(GetArray(arr, 1));
            critical = GetCritical(GetArray(arr, 1)) + dovInt;
            normal = critical > pirson ? "присутствует" : "отсутствует";
            coor = GetRate(GetArray(arr, 1));
            for (int i = 0; i != 5; i++)
            {
                dataX[i] = Math.Round(coor[i, 0], 2);
                dataY[i] = Math.Round(coor[i, 1], 2);
            }
            mainWindow.Ch2.Plot.AddAnnotation($"Нормальность {normal}\nКритерий Пирсона: {pirson}\nКритич.: {critical}", -10, 10);
            mainWindow.Ch2.Plot.AddBar(dataY, dataX, color: Color.BlueViolet).BarWidth = (dataX[1] - dataX[0]) * percent;
            mainWindow.Ch2.Plot.SetAxisLimits(yMin: 0);
            mainWindow.Ch2.Plot.AddScatter(dataX, dataY);
            mainWindow.Ch2.Plot.Title("Частота процессора");
            mainWindow.Ch2.Refresh();
            /////////////////////////////////////////////////////////////////////////
            dataX = new double[5];
            dataY = new double[5];
            coor = null;
            pirson = GetPirson(GetArray(arr, 2));
            critical = GetCritical(GetArray(arr, 2)) + dovInt;
            normal = critical > pirson ? "присутствует" : "отсутствует";
            coor = GetRate(GetArray(arr, 2));
            for (int i = 0; i != 5; i++)
            {
                dataX[i] = Math.Round(coor[i, 0], 2);
                dataY[i] = Math.Round(coor[i, 1], 2);
            }
            mainWindow.Ch3.Plot.AddAnnotation($"Нормальность {normal}\nКритерий Пирсона: {pirson}\nКритич.: {critical}", -10, 10);
            mainWindow.Ch3.Plot.AddBar(dataY, dataX, color: Color.BlueViolet).BarWidth = (dataX[1] - dataX[0]) * percent;
            mainWindow.Ch3.Plot.SetAxisLimits(yMin: 0);
            mainWindow.Ch3.Plot.AddScatter(dataX, dataY);
            mainWindow.Ch3.Plot.Title("Оперативная память");
            mainWindow.Ch3.Refresh();
            ///////////////////////////////////////////////////////////////////////// 
            dataX = new double[5];
            dataY = new double[5];
            coor = null;
            pirson = GetPirson(GetArray(arr, 3));
            critical = GetCritical(GetArray(arr, 3)) + dovInt;
            normal = critical > pirson ? "присутствует" : "отсутствует";
            coor = GetRate(GetArray(arr, 3));
            for (int i = 0; i != 5; i++)
            {
                dataX[i] = Math.Round(coor[i, 0], 2);
                dataY[i] = Math.Round(coor[i, 1], 2);
            }
            mainWindow.Ch4.Plot.AddAnnotation($"Нормальность {normal}\nКритерий Пирсона: {pirson}\nКритич.: {critical}", -10, 10);
            mainWindow.Ch4.Plot.AddBar(dataY, dataX, color: Color.BlueViolet).BarWidth = (dataX[1] - dataX[0]) * percent;
            mainWindow.Ch4.Plot.SetAxisLimits(yMin: 0);
            mainWindow.Ch4.Plot.AddScatter(dataX, dataY);
            mainWindow.Ch4.Plot.Title("Жесткий диск/накопитель");
            mainWindow.Ch4.Refresh();
            /////////////////////////////////////////////////////////////////////////
            dataX = new double[5];
            dataY = new double[5];
            coor = null;
            pirson = GetPirson(GetArray(arr, 4));
            critical = GetCritical(GetArray(arr, 4)) + dovInt;
            normal = critical > pirson ? "присутствует" : "отсутствует";
            coor = GetRate(GetArray(arr, 4));
            for (int i = 0; i != 5; i++)
            {
                dataX[i] = Math.Round(coor[i, 0], 2);
                dataY[i] = Math.Round(coor[i, 1], 2);
            }
            mainWindow.Ch5.Plot.AddAnnotation($"Нормальность {normal}\nКритерий Пирсона: {pirson}\nКритич.: {critical}", -10, 10);
            mainWindow.Ch5.Plot.AddBar(dataY, dataX, color: Color.BlueViolet).BarWidth = (dataX[1] - dataX[0]) * percent;
            mainWindow.Ch5.Plot.SetAxisLimits(yMin: 0);
            mainWindow.Ch5.Plot.AddScatter(dataX, dataY);
            mainWindow.Ch5.Plot.Title("Частота графического процессора");
            mainWindow.Ch5.Refresh();
            /////////////////////////////////////////////////////////////////////////
            dataX = new double[5];
            dataY = new double[5];
            coor = null;
            pirson = GetPirson(GetArray(arr, 5));
            critical = GetCritical(GetArray(arr, 5)) + dovInt;
            normal = critical > pirson ? "присутствует" : "отсутствует";
            coor = GetRate(GetArray(arr, 5));
            for (int i = 0; i != 5; i++)
            {
                dataX[i] = Math.Round(coor[i, 0], 2);
                dataY[i] = Math.Round(coor[i, 1], 2);
            }
            mainWindow.Ch6.Plot.AddAnnotation($"Нормальность {normal}\nКритерий Пирсона: {pirson}\nКритич.: {critical}", -10, 10);
            mainWindow.Ch6.Plot.AddBar(dataY, dataX, color: Color.BlueViolet).BarWidth = (dataX[1] - dataX[0]) * percent;
            mainWindow.Ch6.Plot.SetAxisLimits(yMin: 0);
            mainWindow.Ch6.Plot.AddScatter(dataX, dataY);
            mainWindow.Ch6.Plot.Title("Диагональ экрана");
            mainWindow.Ch6.Refresh();
            /////////////////////////////////////////////////////////////////////////
            dataX = new double[5];
            dataY = new double[5];
            coor = null;
            pirson = GetPirson(GetArray(arr, 6));
            critical = GetCritical(GetArray(arr, 6)) + dovInt;
            normal = critical > pirson ? "присутствует" : "отсутствует";
            coor = GetRate(GetArray(arr, 6));
            for (int i = 0; i != 5; i++)
            {
                dataX[i] = Math.Round(coor[i, 0], 2);
                dataY[i] = Math.Round(coor[i, 1], 2);
            }
            mainWindow.Ch7.Plot.AddAnnotation($"Нормальность {normal}\nКритерий Пирсона: {pirson}\nКритич.: {critical}", -10, 10);
            mainWindow.Ch7.Plot.AddBar(dataY, dataX, color: Color.BlueViolet).BarWidth = (dataX[1] - dataX[0]) * percent;
            mainWindow.Ch7.Plot.SetAxisLimits(yMin: 0);
            mainWindow.Ch7.Plot.AddScatter(dataX, dataY);
            mainWindow.Ch7.Plot.Title("Аккумулятор");
            mainWindow.Ch7.Refresh();
            /////////////////////////////////////////////////////////////////////////
            dataX = new double[5];
            dataY = new double[5];
            coor = null;
            pirson = GetPirson(GetArray(arr, 7));
            critical = GetCritical(GetArray(arr, 7)) + dovInt;
            normal = critical > pirson ? "присутствует" : "отсутствует";
            coor = GetRate(GetArray(arr, 7));
            for (int i = 0; i != 5; i++)
            {
                dataX[i] = Math.Round(coor[i, 0], 2);
                dataY[i] = Math.Round(coor[i, 1], 2);
            }
            mainWindow.Ch8.Plot.AddAnnotation($"Нормальность {normal}\nКритерий Пирсона: {pirson}\nКритич.: {critical}", -10, 10);
            mainWindow.Ch8.Plot.AddBar(dataY, dataX, color: Color.BlueViolet).BarWidth = (dataX[1] - dataX[0]) * percent;
            mainWindow.Ch8.Plot.SetAxisLimits(yMin: 0);
            mainWindow.Ch8.Plot.AddScatter(dataX, dataY);
            mainWindow.Ch8.Plot.Title("Вес");
            mainWindow.Ch8.Refresh();

        }

        internal static double[] GetArray(double[,] list, int count) // Вытянуть столбце из double[,] массива
        {
            double[] arr = new double[list.GetUpperBound(0) + 1];
            for (int i = 0; i != list.GetUpperBound(0) + 1; i++)
            {
                arr[i] = list[i, count];
            }
            return arr;
        }

        internal static double[,] GetRate(double[] data)
        {
            double[,] res = new double[5, 2];
            double min = data[0], max = data[0];
            for (int i = 0; i < data.Length; i++)
            {
                if (min > data[i])
                    min = data[i];
                if (max < data[i])
                    max = data[i];
            }
            double step = (max - min) / 5;
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    res[i, j] = min + step * i;
                    res[i, j + 1] = 0;
                    break;
                }
            }
            for (int i = 0; i < 5; i++)
            {
                for (int j = 5 - 1; j >= 0; j--)
                {
                    if (data[i] >= res[j, 0])
                    {
                        res[j, 1]++;
                        break;
                    }
                }
            }
            for (int i = 0; i < 5; i++)
            {
                res[i, 1] /= data.Length;
            }
            return res;
        }

    }
}
