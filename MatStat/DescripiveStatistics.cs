using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatStat
{
    static internal class DescripiveStatistics
    {
        public static List<List<string>> LoadStatistics(List<List<string>> normedGrid)
        {
            List<List<string>> metricsGrid = new List<List<string>>();
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
                array1.Add(Math.Round(srAriphmetic, 2).ToString());
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
                array3.Add(Convert.ToString(Math.Round(stat.Mean, 4)));
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
                array4.Add(Convert.ToString(Math.Round(stat.Variance, 4)));
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
                array6.Add(Convert.ToString(Math.Round(Mean_deviation(array61), 4)));
            }
            metricsGrid.Add(array6);
            ////////////////////////////////////////////////////////////
            List<string> array7 = new List<string>(); // Ссредняя ошибка
            List<double> array71 = new List<double>();
            array7.Add("Средняя ошибка");
            for (int cols = 1; cols != 9; cols++)
            {
                array71.Clear();
                for (int rows = 0; rows != normedGrid.Count; rows++)
                {
                    array71.Add(Convert.ToDouble(normedGrid[rows][cols]));
                }
                array7.Add(Convert.ToString(Math.Round(Mean_error(array71, cols), 4)));
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
                array8.Add(Convert.ToString(Math.Round(LimError(array81, 2.13144954556), 4)));
            }
            metricsGrid.Add(array8);
            ///////////////////////////////////////////////////////////// Лаба 3 ->Проверка нормальности распределения с помощью критерия Пирсона

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
                array9.Add(Convert.ToString(Math.Round(GetRequierdVolume(array91), 4)));
            }
            metricsGrid.Add(array9);
            /////////////////////////////////////////////////////////////
            return metricsGrid;
        }
        public static double GetRequierdVolume(List<double> list)
        {
            double stdDev = Math.Sqrt(list.Variance());
            double limErr = LimError(list, 2.13144954556); // из таблицы стьюдента взято 2.13144954556
            double res = stdDev * stdDev * 0.95 * list.Count / (limErr * limErr); // по таблице вероятности пирсона 7.26
            return res;
        }

        public static double LimError(List<double> list, double confidenceLevel)
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

        public static double Mean_error(List<double> list, int count)
        {
            double err = 0;
            double mean = Mean_deviation(list);
            err = Math.Round((mean / Math.Sqrt(list.Count)), 4);
            return err;
        }

        private static string Moda(List<string> arr) // вычисление моды
        {
            return arr.GroupBy(x => x).OrderByDescending(x => x.Count()).ThenBy(x => x.Key).Select(x => x.Key).First();
        }
    }
}
