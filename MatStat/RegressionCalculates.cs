using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using static MatStat.MainWindow;

namespace MatStat
{
    internal class RegressionCalculates
    {
        private double crit = mainWindow.crit;
        private int _rows = 0;
        private int _columns = 0;
        double aproximaticError = 0;

        public DataTable BettaTable { get; set; }
        public DataTable YTable { get; set; }
        public string RegrText { get; set; }
        public double[][] BettaMatrix { get; set; } = new double[4][];
        public double[][] YMatrix { get; set; } = new double[5][];
        public double[][] TopBotMatrix { get; set; } = new double[2][];
        public string[] Headers = new string[8];
        public string[] Models = new string[normedGrid.Count];
        public List<MyTable> table = new List<MyTable>();

        private int _rows1 = 0;
        private int _columns1 = 0;
        int observeCount = 0;
        private (double[] MinData, double[] MaxData) dataConfig;
        public DataTable MainTable { get; set; }
        public string FutureText { get; set; }
        public double[][] Matrix { get; set; }
        public string[] Headers1 { get; set; }

        public void LoadRegression(double[][] Matrix)
        {
            _rows = Matrix.Length;
            _columns = Matrix[0].Length + 1;

            for (int i = 1; i != 9; i++)
            {
                Headers[i - 1] = mainWindow.NormGrid.Columns[i].Header.ToString();
            }
            for (int i = 0; i != normedGrid.Count; i++)
            {
                Models[i] = normedGrid[i][0];
            }
            var doubleCorrelationsCoefficients = GetDoubleCorrelationCoefficients(Matrix);
            var criterionsStudient = GetCriterionStudientForCorrelation(doubleCorrelationsCoefficients);
            var significanceCoeffecients = GetSignificanceCoeffecients(criterionsStudient);
            var factorsIndexes = GetSignificanceFactorsIndexes(significanceCoeffecients);
            var significanceFactors = GetSignificanceFactors(significanceCoeffecients, Matrix);
            var coeffecientsB = GetRegresionCoeffecients(significanceFactors, Matrix);
            var criterionStudentCoeffecientsB = GetCriterionStudentCoeffecientsB(significanceFactors, factorsIndexes, Matrix);
            var significanceCoeffecientsB = GetSignificanceCoeffecientsB(coeffecientsB, criterionStudentCoeffecientsB);
            var significanceCalculatedValues = GetSignificanceCalculatedValues(coeffecientsB, significanceCoeffecientsB, significanceFactors);
            var observableValues = Matrix.Select(x => x[0]).ToArray();
            var calculatedValues = GetCalculatedValues(coeffecientsB, significanceFactors);

            RegrText = "";
            RegrText += ShowRegressionEquation(coeffecientsB, factorsIndexes) + "\n";
            RegrText += ShowSignificanceRegressionEquation(coeffecientsB, significanceCoeffecientsB, factorsIndexes) + "\n";
            RegrText += ShowFStatistic(significanceCalculatedValues, observableValues, significanceFactors.Count());

            BettaTable = GetRegressionTable(significanceCoeffecients, Matrix);
            YTable = GetValueTable(calculatedValues, significanceCoeffecients, Matrix, Headers);
            table = DataTableToMyTable(BettaTable); // мой кастомный метод перевода в табличный вид для моей программы, т.к был баг, не знаю, понадобится ли тебе эта функция. DataTable через DefaultView можно вкинуть в datagrid
            mainWindow.d1.ItemsSource = table; // вот тут в data grid конечный результат сохраняется, в table
            mainWindow.d2.DataContext = this; // тут через binding идет привязка из переменной Ytable 
            mainWindow.d3.Text = RegrText; // это тоже важная штука, как ты понял
            LoadRegressionPage(Matrix, significanceFactors, significanceCoeffecientsB, calculatedValues, coeffecientsB, factorsIndexes); // тут вызывается прогнозирование
        }

        public void LoadRegressionPage(double[][] arr, double[][] significanceFactors1, double[] significanceCoeffecientsB1,
                                                                       double[] calculatedValues1, double[] coeffecientsB1, int[] factorsIndexes1)
        {
            _rows1 = arr.Length;
            _columns1 = arr[0].Length + 1;
            dataConfig.MinData = arr.Select(c => c.Min()).ToArray();
            dataConfig.MaxData = arr.Select(c => c.Max()).ToArray();
            Matrix = arr;

            observeCount = arr.Count();


            double[][] significanceFactors = significanceFactors1; /////////// не забыть
            double[] significanceCoeffecientsB = significanceCoeffecientsB1;
            double[] calculatedValues = calculatedValues1;
            double[] _coefficientsB = coeffecientsB1;
            double[] _textBoxesPrognoz = factorsIndexes1.Select(c => 1.0).ToArray();
            int[] factorsIndexes = factorsIndexes1;
            var count = _textBoxesPrognoz.Count();

            double[][] x;

            if (significanceCoeffecientsB[0] == 1)
            {
                x = new double[significanceFactors.Length + 1][];
                x[0] = Enumerable.Repeat(1.0, observeCount).ToArray();
                for (int i = 0; i < significanceFactors.Length; i++)
                    x[i + 1] = significanceFactors[i];
            }
            else
            {
                x = new double[significanceFactors.Length][];
                for (int i = 0; i < significanceFactors.Length; i++)
                    x[i] = significanceFactors[i];
            }

            double result = _coefficientsB[0];
            double[] x0 = new double[count - 1];
            for (int i = 0; i < count - 1; i++)
            {
                var index = factorsIndexes[i];
                var value = ((double)_textBoxesPrognoz[i] - dataConfig.MinData[index]) / (dataConfig.MaxData[index] - dataConfig.MinData[index]);
                x0[i] = value;
                result += _coefficientsB[i + 1] * value;
            }
            List<double> x0list = x0.ToList();
            if (significanceCoeffecientsB[0] == 1)
                x0list.Insert(0, 1);


            var denormolizeY = (result * (dataConfig.MaxData[0] - dataConfig.MinData[0])) + dataConfig.MinData[0];

            double normedRes = coeffecientsB1[0];
            double[] lookingForValues = GetData(calculatedValues);
            List<string> s = new List<string>()
            {
                "ToPrognoseModel", "699", lookingForValues[0].ToString(),lookingForValues[1].ToString(),
                lookingForValues[2].ToString(),lookingForValues[3].ToString(),lookingForValues[4].ToString(),
                lookingForValues[5].ToString(), lookingForValues[6].ToString()
            };
            grid.Add(s);
            double[,] normedGrid1 = new double[grid.Count,grid[0].Count-1];
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
                for (int rows1 = 0; rows1 != grid.Count; rows1++)
                {
                    Xnorm = (col[rows1] - colMin) / (colMax - colMin);
                    normedGrid1[rows1,cols-1] = Math.Round(Xnorm, 4);
                }
            }
            for (int i = 1; i < _coefficientsB.Count(); i++)
            {
                normedRes += significanceCoeffecientsB[i] > 0 ? coeffecientsB1[i] * Convert.ToDouble(normedGrid1[normedGrid1.GetUpperBound(0),i]) : 0;
            }
            double[] prices = new double[grid.Count];
            for (int i = 0; i != grid.Count; i++)
            {
                prices[i] = Convert.ToDouble(grid[i][1]);
            }
            double nonNormedRes = Convert.ToDouble(grid[Array.IndexOf(prices,prices.Max())][1]) * normedRes;
            // 0,0309
            string resultStr = $"Результат прогноза:\n" +
                               $"y = {Math.Round(normedRes,6)} ± {Math.Round(normedRes*(aproximaticError/100),4)}\n" +
                               $"Без нормализцации:\n" +
                               $"y = {Math.Round(nonNormedRes,2)} ± {Math.Round(nonNormedRes*(aproximaticError/100),4)}\n ";
            FutureText = resultStr;
            _textBoxesPrognoz[count - 1] = denormolizeY;
            List<MyTable1> data = new List<MyTable1>();
            data = DataTableToMyTable1(GetData(_textBoxesPrognoz).ToDataTable());
            mainWindow.d4.ItemsSource = data; // тут все то же самое, что и в предыдущей функции
            mainWindow.d5.Text = FutureText; // и тут тоже
        }
        private double GetStandartErrorY(double[][] x, double[][] normolizedData, double[] calculatedValues, double[] x0)
        {
            var observeCount = normolizedData.Count();
            var observableY = normolizedData.Select(c => c[0]).ToArray();
            var s = Math.Sqrt(observableY.Select((c, i) => Math.Pow(calculatedValues[i] - c, 2)).Sum() / (observeCount - x0.Count() + 1));

            var M = Matrix<double>.Build;
            var X = M.DenseOfColumnArrays(x);
            var X0 = M.DenseOfColumnArrays(x0);

            Matrix<double> temp1;
            Matrix<double> temp2;
            temp1 = X.Transpose();
            temp1 = temp1.Multiply(X);
            temp1 = temp1.Inverse();

            temp2 = X0.Transpose();
            temp2 = temp2.Multiply(temp1);
            temp2 = temp2.Multiply(X0);


            return s * temp2.Column(0).ToArray()[0] * 5;
        }
        private double[] GetData(double[] input)
        {
            double[] res = new double[7];
            res[0] = 3200;
            res[1] = 16;
            res[2] = 512;
            res[3] = 2400;
            res[4] = 14;
            res[5] = 56;
            res[6] = 1.45;
            return res;
        }

    private List<MyTable> DataTableToMyTable(DataTable arr)
    {
        List<MyTable> res = new List<MyTable>();
        foreach (DataRow row in arr.Rows)
        {
            res.Add(new MyTable(row.ItemArray[0] as string, Convert.ToDouble(row.ItemArray[1]), Convert.ToDouble(row.ItemArray[2]),
                Convert.ToDouble(row.ItemArray[3]), Convert.ToDouble(row.ItemArray[4]),
                Convert.ToDouble(row.ItemArray[5]), Convert.ToDouble(row.ItemArray[6]),
                Convert.ToDouble(row.ItemArray[7]), Convert.ToDouble(row.ItemArray[8])));
        }
        return res;
    }
        private List<MyTable1> DataTableToMyTable1(DataTable arr)
        {
            List<MyTable1> res = new List<MyTable1>();
            foreach (DataRow row in arr.Rows)
            {
                res.Add(new MyTable1(Convert.ToDouble(row.ItemArray[0]), Convert.ToDouble(row.ItemArray[1]),
                    Convert.ToDouble(row.ItemArray[2]), Convert.ToDouble(row.ItemArray[3]),
                    Convert.ToDouble(row.ItemArray[4]), Convert.ToDouble(row.ItemArray[5]),
                    Convert.ToDouble(row.ItemArray[6])));
            }
            return res;
        }
        private DataTable GetRegressionTable(double[][] significanceCoeffecients, double[][] normolizedData)
    {
        var significanceFactors = GetSignificanceFactors(significanceCoeffecients, normolizedData);
        var significanceFactorsIndexes = GetSignificanceFactorsIndexes(significanceCoeffecients);
        var coeffecientsB = GetRegresionCoeffecients(significanceFactors, normolizedData);
        var criterionStudentCoeffecientsB = GetCriterionStudentCoeffecientsB(significanceFactors, significanceFactorsIndexes, normolizedData);
        var significanceCoeffecientsB = GetSignificanceCoeffecientsB(coeffecientsB, criterionStudentCoeffecientsB);
        var gradesInteravals = GetGradesEntervals(significanceFactors, significanceFactorsIndexes, normolizedData);
        double[][] arr = new double[5][];
        arr[0] = coeffecientsB;
        arr[1] = criterionStudentCoeffecientsB;
        arr[2] = significanceCoeffecientsB;
        arr[3] = gradesInteravals.topGrade;
        arr[4] = gradesInteravals.bottomGrade;
        string[] leftStr = new string[]
        {
                "Коэффициент",
                "Критерий стьюдента",
                "Значимость",
                "Верхняя оценка",
                "Нижняя оценка"
        };
        return arr.AddColumnRegression(leftStr, Headers);
    }
    private DataTable GetValueTable(double[] calculatedValues, double[][] significanceCoeffecients, double[][] normolizedData, string[] names)
    {
        var observableCount = _rows;
        var observableValues = normolizedData.Select(x => x[0]).ToArray();
        var residualDispersion = GetSquaredDeviations(calculatedValues, observableValues) / (observableCount - 2);
        var gradesValues = GetGradesValues(calculatedValues, significanceCoeffecients, normolizedData);
        double[][] arr = new double[5][];
        arr[0] = observableValues;
        arr[1] = calculatedValues;
        arr[2] = GetAbsoluteError(calculatedValues, observableValues);
        arr[3] = gradesValues.topGrade;
        arr[4] = gradesValues.bottomGrade;
        string[] leftStr = new string[]
        {
                "Y-наблюдаемое",
                "Y-вычисленное",
                "Погрешность",
                "Верхняя оценка",
                "Нижняя оценка"
        };
        return arr.AddColumnRegression(leftStr, Models);
    }
    private (double[] topGrade, double[] bottomGrade) GetGradesEntervals(double[][] significanceFactors, int[] significanceFactorsIndexes, double[][] normolizedData)
    {
        var observableCount = _rows;
        var observableValues = normolizedData.Select(x => x[0]).ToArray();
        var coeffecientsB = GetRegresionCoeffecients(significanceFactors, normolizedData);
        var coeffecientsBCount = coeffecientsB.Length;
        var criterionStudentCoeffecientsB = GetCriterionStudentCoeffecientsB(significanceFactors, significanceFactorsIndexes, normolizedData);
        var significanceCoeffecientsB = GetSignificanceCoeffecientsB(coeffecientsB, criterionStudentCoeffecientsB);
        var calculatedValues = GetCalculatedValues(coeffecientsB, significanceFactors);
        var residualDispersion = GetSquaredDeviations(calculatedValues, observableValues) / (observableCount - 2);
        var topGrade = new double[coeffecientsBCount];
        var bottomGrade = new double[coeffecientsBCount];

        for (int i = 0; i < coeffecientsBCount; i++)
        {
            topGrade[i] = coeffecientsB[i] + 2 * residualDispersion;
            bottomGrade[i] = coeffecientsB[i] - 2 * residualDispersion;
        }

        return (topGrade, bottomGrade);
    }
    private double[][] GetSignificanceFactors(double[][] significanceCoeffecients, double[][] normolizedData)
    {
        var count = significanceCoeffecients[0].Where(x => x > 0).Count() - 1;
        var size = _columns - 1;
        var significanseData = new double[size - 1][];

        int k = 0;
        for (var i = 1; i < size; i++)
            //if (significanceCoeffecients[0][i] > 0)
            significanseData[k++] = normolizedData.Select(x => x[i]).ToArray();

        return significanseData;
    }
    private double[] GetSignificanceCalculatedValues(double[] coeffecientsB, double[] significanceCoeffecientsB, double[][] significanceFactors)
    {
        var observeCount = _rows;
        var result = new double[observeCount];

        for (int i = 0; i < observeCount; i++)
        {
            double temp = significanceCoeffecientsB[0] > 0 ? coeffecientsB[0] : 0;
            for (int j = 1; j < coeffecientsB.Length; j++)
                if (significanceCoeffecientsB[j] > 0)
                    temp += coeffecientsB[j] * significanceFactors[j - 1][i];
            result[i] = Math.Round(temp, 2);
        }

        return result;
    }
    private double[][] GetSignificanceCoeffecients(double[][] criterionsStudient)
    {
        var size = _columns - 1;
        var significanceCoeffecients = new double[size][];

        for (int i = 0; i < size; i++)
        {
            var temp = new double[size];
            for (int j = 1; j < size; j++)
            {
                temp[j] = criterionsStudient[i][j] > crit ? 1 : 0;
                significanceCoeffecients[i] = temp;
            }
        }
        return significanceCoeffecients;
    }
    private double[] GetCriterionStudentCoeffecientsB(double[][] significanceFactors, int[] significanceFactorsIndexes, double[][] normolizedData)
    {
        var observableCount = _rows;
        var observableValues = normolizedData.Select(x => x[0]).ToArray();
        var coeffecientsB = GetRegresionCoeffecients(significanceFactors, normolizedData);
        var coeffecientsBCount = coeffecientsB.Length;
        var calculatedValues = GetCalculatedValues(coeffecientsB, significanceFactors);
        var residualDispersion = GetSquaredDeviations(calculatedValues, observableValues) / (observableCount - 2);
        var standartDiviation = GetStandartDeviation(normolizedData);
        var criterionStudent = new double[coeffecientsBCount];

        criterionStudent[0] = coeffecientsB[0] / (Math.Sqrt(residualDispersion) / Math.Sqrt(observableCount));

        for (int i = 1; i < coeffecientsBCount; i++)
        {
            int index = significanceFactorsIndexes[i - 1];
            criterionStudent[i] = coeffecientsB[i] / (Math.Sqrt(residualDispersion) / (standartDiviation[index] * Math.Sqrt(observableCount)));
        }

        return criterionStudent.Select(x => Math.Abs(x)).ToArray();
    }
    private double[] GetStandartDeviation(double[][] normolizeData)
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
    private double[] GetDispersion(double[][] normolizeData)
    {
        var dispersionArrayModel = new double[_columns - 1];
        var averageArray = GetAverage(normolizeData);

        for (int i = 0; i < _columns - 1; i++)
            dispersionArrayModel[i] = normolizeData.Select(x => Math.Pow(x[i] - averageArray[i], 2)).Sum() / (normolizeData.Count() - 1);

        return dispersionArrayModel;
    }
    private double[] GetAverage(double[][] normolizeData)
    {
        var averageArrayModel = new double[_columns - 1];

        for (int i = 0; i < _columns - 1; i++)
            averageArrayModel[i] = normolizeData.Select(x => x[i]).Sum() / normolizeData.Count();

        return averageArrayModel;
    }
    private double GetSquaredDeviations(double[] calculatedValues, double[] observableValues)
    {
        return calculatedValues.Select((x, i) => Math.Pow(Math.Abs(x - observableValues[i]), 2)).Sum();
    }
    private double[] GetAbsoluteError(double[] calculatedValues, double[] observableValues)
    {
        return calculatedValues.Select((x, i) => Math.Abs(x - observableValues[i])).ToArray();
    }
    private (double[] topGrade, double[] bottomGrade) GetGradesValues(double[] calculatedValues, double[][] significanceCoeffecients, double[][] normolizedData)
    {
        var significanceFactors = GetSignificanceFactors(significanceCoeffecients, normolizedData);
        var factorsIndexes = GetSignificanceFactorsIndexes(significanceCoeffecients);
        var coeffecientsB = GetRegresionCoeffecients(significanceFactors, normolizedData);
        var criterionStudentCoeffecientsB = GetCriterionStudentCoeffecientsB(significanceFactors, factorsIndexes, normolizedData);
        var significanceCoeffecientsB = GetSignificanceCoeffecientsB(coeffecientsB, criterionStudentCoeffecientsB);
        var observeCount = normolizedData.Count();
        significanceFactors = significanceFactors.Where((c, i) => significanceCoeffecientsB[i + 1] > 0).ToArray();

        double[][] x;

        if (significanceCoeffecientsB[0] == 1)
        {
            x = new double[significanceFactors.Length + 1][];
            x[0] = Enumerable.Repeat(1.0, observeCount).ToArray();
            for (int i = 0; i < significanceFactors.Length; i++)
                x[i + 1] = significanceFactors[i];
        }
        else
        {
            x = new double[significanceFactors.Length][];
            for (int i = 0; i < significanceFactors.Length; i++)
                x[i] = significanceFactors[i];
        }

        var observableCount = _rows;
        var topGrade = new double[observableCount];
        var bottomGrade = new double[observableCount];
        var critical = StudentT.InvCDF(0, 1, observableCount - 3 - 1, 1 - 0.05);

        for (int i = 0; i < observableCount; i++)
        {
            List<double> x0 = new List<double>();
            if (significanceCoeffecientsB[0] == 1)
            {
                x0 = significanceFactors.Select(c => c[i]).ToList();
                x0.Insert(0, 1);
            }
            else
            {
                x0 = significanceFactors.Select(c => c[i]).ToList();
            }
            var standartErrorY = GetStandartErrorY(x, normolizedData, calculatedValues, x0.ToArray());
            topGrade[i] = calculatedValues[i] + critical * standartErrorY;
            bottomGrade[i] = calculatedValues[i] - critical * standartErrorY;
        }

        return (topGrade, bottomGrade);
    }
    private double[] GetCalculatedValues(double[] coeffecientsB, double[][] significanceFactors)
    {
        var observeCount = _rows;
        var result = new double[observeCount];

        for (int i = 0; i < observeCount; i++)
        {
            double temp = coeffecientsB[0];
            for (int j = 1; j < coeffecientsB.Length; j++)
                temp += coeffecientsB[j] * significanceFactors[j - 1][i];
            result[i] = Math.Round(temp, 2);
        }

        return result;
    }
    private double[] GetSignificanceCoeffecientsB(double[] coeffecientsB, double[] criterionStudent)
    {
        var observableCount = _rows;
        var coeffecientsBCount = coeffecientsB.Length;
        var critical = StudentT.InvCDF(0, 1, observableCount - 3 - 1, 1 - 0.05 / 2);

        var significanceCoeffecientsB = new double[coeffecientsBCount];

        for (int i = 0; i < coeffecientsBCount; i++)
            significanceCoeffecientsB[i] = Math.Abs(criterionStudent[i]) > critical ? 1 : 0;

        return significanceCoeffecientsB;
    }
    private double[][] GetDoubleCorrelationCoefficients(double[][] normolizeData)
    {
        var size = _columns - 1;
        var correlationsCoefficients = new double[size][];

        for (int i = 0; i < size; i++)
        {
            var temp = new double[size];
            for (int j = 0; j < size; j++)
            {
                temp[j] = MathNet.Numerics.Statistics.Correlation.Pearson(normolizeData.Select(x => x[i]), normolizeData.Select(x => x[j]));
            }
            correlationsCoefficients[i] = temp;
        }
        return correlationsCoefficients;
    }
    private int[] GetSignificanceFactorsIndexes(double[][] significanceCoeffecients)
    {
        var count = significanceCoeffecients[0].Where(x => x > 0).Count() - 1;
        var size = _columns - 1;
        var significanceFactorsIndexes = new int[size];

        int k = 0;
        for (var i = 1; i < size + 1; i++)
            //if (significanceCoeffecients[0][i] > 0)
            significanceFactorsIndexes[k++] = i;

        return significanceFactorsIndexes;
    }
    private string ShowFStatistic(double[] calculatedValues, double[] observableValues, int factorsCount)
    {
        var coeffecientsDetermination = GetCoeffecientsDetermination(calculatedValues, observableValues);
        var n = observableValues.Count();
        var critical = FisherSnedecor.InvCDF(factorsCount, n - factorsCount - 1, 1 - 0.05 / 2);
        var criterionFisher = (coeffecientsDetermination * (n - factorsCount - 1)) / ((1 - coeffecientsDetermination) * factorsCount);
        var significance = criterionFisher > critical;
        aproximaticError = GetAproximaticError(calculatedValues, observableValues);

        string result = $"F критерий Фишера = {criterionFisher}\n" +
                        $"F критическое = {critical}\n";

        if (significance)
            result += $"F > Fкрит - Уравнение регрессии значимо\n";
        else
            result += $"F < Fкрит - Уравнение регрессии незначимо\n";


        result += $"Ошибка апроксимации = {Math.Round(aproximaticError, 3) / 10} < 15%";
        return result;
    }
    private double[][] GetCriterionStudientForCorrelation(double[][] coefficientsCorrelations)
    {
        var size = _columns - 1;
        var criterionStudient = new double[size][];

        for (int i = 0; i < size; i++)
        {
            var temp = new double[size];
            for (int j = 0; j < size; j++)
            {
                if (i == j)
                    temp[j] = double.PositiveInfinity;
                else
                {
                    var r = coefficientsCorrelations[i][j];
                    temp[j] = Math.Abs(r) * Math.Sqrt((size - 2) / (1 - r * r));
                    criterionStudient[i] = temp;
                }
            }
        }
        return criterionStudient;
    }
    private double[] GetRegresionCoeffecients(double[][] significanceFactors, double[][] normolizedData)
    {
        var observeCount = normolizedData.Count();
        var x = new double[significanceFactors.Length + 1][];

        x[0] = Enumerable.Repeat(1.0, observeCount).ToArray();
        for (int i = 0; i < significanceFactors.Length; i++)
            x[i + 1] = significanceFactors[i];

        var M = Matrix<double>.Build;
        var X = M.DenseOfColumnArrays(x);
        var Y = M.DenseOfColumnArrays(new double[1][] { normolizedData.Select(c => c[0]).ToArray() });

        Matrix<double> temp1;
        Matrix<double> temp2;
        temp1 = X.Transpose();
        temp1 = temp1.Multiply(X);
        temp1 = temp1.Inverse();

        temp2 = X.Transpose();
        temp2 = temp2.Multiply(Y);

        var coeffecientsB = temp1.Multiply(temp2);

        return coeffecientsB.Column(0).ToArray();
    }
    private double GetAproximaticError(double[] calculatedValues, double[] observableValues)
    {
        double result = 0;
        for (int i = 0; i < calculatedValues.Count(); i++)
            if (observableValues[i] != 0)
                result += Math.Abs((observableValues[i] - calculatedValues[i]) / observableValues[i]);

        result /= calculatedValues.Count() + calculatedValues.Count() / 2;
        result *= 100;

        return result;
    }
    private double GetCoeffecientsDetermination(double[] calculatedValues, double[] observableValues)
    {
        var observableCount = _rows;
        var residualDispersion = observableValues.Select((x, i) => Math.Pow(x - calculatedValues[i], 2)).Sum();
        var average = observableValues.Average();
        var dispersion = observableValues.Select(x => Math.Pow(average, 2)).Sum();
        var determination = 1 - (residualDispersion / dispersion);

        return determination;
    }
    private string ShowSignificanceRegressionEquation(double[] coeffecientsB, double[] significanceCoeffecientsB, int[] factorsIndexes)
    {
        string result = string.Empty;
        result += "y = ";
        result += significanceCoeffecientsB[0] > 0 ? $"{Math.Round(coeffecientsB[0], 3)} + " : string.Empty;
        for (int i = 1; i < coeffecientsB.Count(); i++)
            result += significanceCoeffecientsB[i] > 0 ? $"{Math.Round(coeffecientsB[i], 3)}*x{factorsIndexes[i - 1]} + " : string.Empty;
        return "Уравнение регрессии с учетом значимых коэффициентов:\n" + result.Remove(result.Length - 2, 2);
    }
    private string ShowRegressionEquation(double[] coeffecientsB, int[] factorsIndexes)
    {
        string result = string.Empty;
        result += $"y = {Math.Round(coeffecientsB[0], 3)}";
        for (int i = 1; i < coeffecientsB.Count(); i++)
            result += $" + {Math.Round(coeffecientsB[i], 3)}*x{factorsIndexes[i - 1]}";
        return result;
    }
}

    public static class ForDataGrid
    {
        public static DataTable AddColumnRegression<T>(this T[][] matrix, string[] headers, string[] topHeaders)
        {
            DataTable res = new DataTable();
            res.Columns.Add("Название", typeof(string));
            for (int i = 0; i < topHeaders.Length; i++)
            {
                if (headers != null)
                {
                    res.Columns.Add(topHeaders[i], typeof(T));
                }
                else res.Columns.Add("col" + i, typeof(T));
            }
            for (int i = 0; i < headers.Length; i++)
            {
                var row = res.NewRow();
                row[0] = headers[i];
                res.Rows.Add(row);
            }
            for (int i = 0; i < matrix.Length; i++)
            {
                var row = res.Rows[i];

                for (int j = 0; j < matrix[0].Length; j++)
                {
                    row[j + 1] = matrix[i][j];
                }

            }
            return res;
        }
        public static DataTable ToDataTable<T>(this T[] input)
        {
            var res = new DataTable();
            T[][] matrix = new T[1][];
            matrix[0] = input;
            res.Columns.Add("Частота процессора", typeof(T));
            res.Columns.Add("Оперативная память", typeof(T));
            res.Columns.Add("Жесткий диск/накопитель", typeof(T));
            res.Columns.Add("Частота графического процессора", typeof(T));
            res.Columns.Add("Диагональ экрана", typeof(T));
            res.Columns.Add("Аккумулятор", typeof(T));
            res.Columns.Add("Вес", typeof(T));
            for (int i = 0; i < matrix.Length; i++)
            {
                var row = res.NewRow();

                for (int j = 0; j < matrix[0].Length; j++)
                {
                    row[j] = matrix[i][j];
                }

                res.Rows.Add(row);
            }

            return res;
        }
    }
    internal class MyTable1
    {
       // public string _model { get; set; }
       // public double _price { get; set; }
        public double _CPUClock { get; set; }
        public double _RAM { get; set; }
        public double _DriveDisk { get; set; }
        public double _GPUClock { get; set; }
        public double _Diagonal { get; set; }
        public double _Battery { get; set; }
        public double _Weight { get; set; }

        public MyTable1(double CPUClock, double RABClock, double DriveDisk, double GPUClock, double Diagonal, double Battery, double Weight)
        {
            _CPUClock = CPUClock;
            _RAM = RABClock;
            _DriveDisk = DriveDisk;
            _GPUClock = GPUClock;
            _Diagonal = Diagonal;
            _Battery = Battery;
            _Weight = Weight;
        }
        public MyTable1()
        {

        }
    }
}
