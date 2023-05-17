using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
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
    static internal class XiPearsonSquare
    {
        private static readonly int _columns = 9;
        private static readonly List<string> Headers = new List<string>();
        public static void StatisticsForCharts(List<List<string>> list)
        {
            (double[] critical, double[] sum, double[] isNormal) = XiPearsonSquare.GetChiSquarePirsStat(Correlation.ToSteppedArr(mainWindow.ToArr(list)));
            List<List<string>> NormalitiesGrid = new List<List<string>>();
            List<MyTable> NormalitiesSource = new List<MyTable>
            {
                new MyTable("Критическое значение", Math.Round(critical[0], 4), Math.Round(critical[1], 4), Math.Round(critical[2], 4), Math.Round(critical[3], 4), Math.Round(critical[4], 4),
                Math.Round(critical[5], 4), Math.Round(critical[6], 4), Math.Round(critical[7], 4)),
                new MyTable("Критерий Пирсона", Math.Round(sum[0], 4), Math.Round(sum[1], 4), Math.Round(sum[2], 4), Math.Round(sum[3], 4), Math.Round(sum[4], 4),
                Math.Round(sum[5], 4), Math.Round(sum[6], 4), Math.Round(sum[7], 4)),
                new MyTable("Нормальность", Math.Round(isNormal[0], 4), Math.Round(isNormal[1], 4), Math.Round(isNormal[2], 4), Math.Round(isNormal[3], 4), Math.Round(isNormal[4], 4),
                Math.Round(isNormal[5], 4), Math.Round(isNormal[6], 4), Math.Round(isNormal[7], 4))
            };
            mainWindow.Normalties.ItemsSource = NormalitiesSource;
        }
        public static void LoadCharts(List<List<string>> list) // отрисовка графиков
        {
            mainWindow.PlotPanel.Children.Clear();
            for (int i = 1; i < mainWindow.Normalties.Columns.Count; i++)
            {
                Headers.Add(mainWindow.Normalties.Columns[i].Header.ToString());
            }
            GraphicData[] arr = GetChiSquarePirsStatGraphic(Correlation.ToSteppedArr(Correlation.ToArr(list)));
            for (int i = 0; i != arr.Length; i++)
            {
                mainWindow.PlotPanel.Children.Add(new CustomPlot(arr[i].Frequency, arr[i].TheoreticFrequency, arr[i].Intervals, arr[i].IsNormal, arr[i].Title));
            }
        }

        public static (double[] critical, double[] sum, double[] isNormal) GetChiSquarePirsStat(double[][] normolizeData)
        {
            var critical = new double[_columns - 1];
            var sum = new double[_columns - 1];
            var isNormal = new double[_columns - 1];
            var probabilityArrayModel = new double[_columns - 1];
            var standartDeviation = GetStandartDeviation(normolizeData);
            var average = GetAverage(normolizeData);

            for (int i = 0; i < _columns - 1; i++)
            {
                var column = normolizeData.Select(x => x[i]).ToList();
                var count = column.Count();
                var intervalsCount = 5;
                //var intervalsCount = (int)(1 + 3.3221 * Math.Log10(count));
                var analize = GetFrequencyAnalize(intervalsCount, normolizeData.Select(x => x[i]).ToArray());
                var frequency = analize.frequency;
                var intervals = analize.intervals;
                var probabilities = new double[intervalsCount];
                var theoreticFrequency = new double[intervalsCount];
                var criterion = new double[intervalsCount];

                for (int j = 0; j < intervalsCount; j++)
                    probabilities[j] = GetNormalDistribution(intervals[j + 1], standartDeviation[i], average[i]) - GetNormalDistribution(intervals[j], standartDeviation[i], average[i]);

                for (int j = 0; j < intervalsCount; j++)
                    theoreticFrequency[j] = count * probabilities[j];

                for (int j = 0; j < intervalsCount; j++)
                    criterion[j] = Math.Pow(theoreticFrequency[j] - frequency[j], 2) / theoreticFrequency[j];

                critical[i] = ChiSquared.InvCDF(intervalsCount - 2 - 1, 1 - 0.05);
                sum[i] = criterion.Sum();
                isNormal[i] = sum[i] < critical[i] ? 1 : 0;
            }

            return (critical, sum, isNormal);
        }

        private static GraphicData[] GetChiSquarePirsStatGraphic(double[][] normolizeData)
        {
            foreach(DataGridColumn col in mainWindow.Normalties.Columns)
            {
                Headers.Append(col.Header.ToString()); 
            }
            List<GraphicData> uIElements = new List<GraphicData>();
            var critical = new double[normolizeData.Length];
            var sum = new double[normolizeData.Length];
            var isNormal = new double[normolizeData.Length];
            var probabilityArrayModel = new double[normolizeData.Length];
            var standartDeviation = GetStandartDeviation(normolizeData);
            var average = GetAverage(normolizeData);

            for (int i = 0; i < _columns - 1; i++)
            {
                var column = normolizeData.Select(x => x[i]).ToList();
                var count = column.Count();
                var intervalsCount = 5;
                //var intervalsCount = (int)(1 + 3.3221 * Math.Log10(count));
                var analize = GetFrequencyAnalize(intervalsCount, normolizeData.Select(x => x[i]).ToArray());
                var frequency = analize.frequency;
                var intervals = analize.intervals;
                var probabilities = new double[intervalsCount];
                var theoreticFrequency = new double[intervalsCount];
                var criterion = new double[intervalsCount];

                for (int j = 0; j < intervalsCount; j++)
                    probabilities[j] = GetNormalDistribution(intervals[j + 1], standartDeviation[i], average[i]) - GetNormalDistribution(intervals[j], standartDeviation[i], average[i]);

                for (int j = 0; j < intervalsCount; j++)
                    theoreticFrequency[j] = count * probabilities[j];

                for (int j = 0; j < intervalsCount; j++)
                    criterion[j] = Math.Pow(theoreticFrequency[j] - frequency[j], 2) / theoreticFrequency[j];

                critical[i] = ChiSquared.InvCDF(intervalsCount - 2 - 1, 1 - 0.05);
                sum[i] = criterion.Sum();
                isNormal[i] = sum[i] < critical[i] ? 1 : 0;

                uIElements.Add(new GraphicData(frequency, theoreticFrequency, intervals, isNormal[i], Headers[i]));
            }

            return uIElements.ToArray();
        }
        #region Функции
        private static double GetNormalDistribution(double x, double standartDeviation, double average)
        {
            //return (1 / (standartDeviation * Math.Sqrt(2 * Math.PI))) * Math.Pow(Math.E, Math.Pow(-0.5 * (x - average) / standartDeviation, 2));
            return Normal.CDF(average, standartDeviation, x);
        }
        private static double[] GetDispersion(double[][] normolizeData)
        {
            var dispersionArrayModel = new double[_columns - 1];
            var averageArray = GetAverage(normolizeData);

            for (int i = 0; i < _columns - 1; i++)
                dispersionArrayModel[i] = normolizeData.Select(x => Math.Pow(x[i] - averageArray[i], 2)).Sum() / (normolizeData.Count() - 1);

            return dispersionArrayModel;
        }

        private static double[] GetStandartDeviation(double[][] normolizeData)
        {
            var dispersionArrayModel = new double[_columns - 1];
            var dispersionArray = GetDispersion(normolizeData);

            for (int i = 0; i < _columns - 1; i++)
            {
                //dispersionArrayModel[i] = Math.Sqrt(dispersionArray[i]);
                var statistics = new DescriptiveStatistics(normolizeData.Select(x => x[i]));
                dispersionArrayModel[i] = statistics.StandardDeviation;
            }
            return dispersionArrayModel;
        }

        private static double[] GetAverage(double[][] normolizeData)
        {
            var averageArrayModel = new double[_columns - 1];

            for (int i = 0; i < _columns - 1; i++)
                averageArrayModel[i] = normolizeData.Select(x => x[i]).Sum() / normolizeData.Count();

            return averageArrayModel;
        }

        private static (double[] intervals, double[] frequency) GetFrequencyAnalize(int intervalsCount, double[] column)
        {
            column = column.OrderBy(x => x).ToArray();
            var min = column.Min();
            var max = column.Max();
            var step = (max - min) / (intervalsCount);
            var intervals = new double[intervalsCount + 1];
            var frequency = new double[intervalsCount];

            intervals[0] = min;
            for (int j = 1; j < intervalsCount + 1; j++)
                intervals[j] = intervals[j - 1] + step;

            for (int k = 0; k < intervalsCount - 1; k++)
                frequency[k] = column.Where(x => x >= intervals[k] && x < intervals[k + 1]).Count();

            frequency[intervalsCount - 1] = column.Where(x => x >= intervals[intervalsCount - 1]).Count();

            return (intervals, frequency);
        }
        #endregion
    }
}
