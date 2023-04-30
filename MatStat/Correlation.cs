using ScottPlot.Renderable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MatStat
{
    static internal class Correlation
    {

        public static double[,] CoupleCorrelate(double[,] arr) // вычисление парных коэффициентов корреляций
        {
            List<List<double>> RxyArr = new List<List<double>>();
            double[] buff = new double[arr.GetUpperBound(1) + 1]; // разделяю Heap и Стек с помощью буфферного массива, чтобы корректно добавлялись вычисленные коэффициенты
            List<double> arr1 = new List<double>();
            double numerator, denominator, denominator1;
            for(int cols = 0; cols != arr.GetUpperBound(1) + 1; cols++)
            {
                arr1.Clear();
                double xAvg = GetArray(arr, cols).Average();
                for (int current = 0; current != arr.GetUpperBound(1)+1; current++)
                {
                    numerator = 0; denominator = 0; denominator1 = 0;
                    double yAvg = GetArray(arr, current).Average();
                    for (int rows = 0; rows != arr.GetUpperBound(0) + 1; rows++) 
                    {
                        numerator += (arr[rows, cols] - xAvg) * (arr[rows, current] - yAvg);
                        denominator += Math.Pow(arr[rows, cols] - xAvg, 2);
                        denominator1 += Math.Pow(arr[rows, current] - yAvg, 2);
                    }
                    double Rxy = numerator / (Math.Sqrt(denominator * denominator1));
                    arr1.Add(Math.Round(Rxy,4));
                }
                buff = arr1.ToArray(); // копирую данные из кучи в стек
                RxyArr.Add(buff.ToList()); // добавляю данные из стека в List
                // Это нужно, чтобы list Не ссылался на одну область памяти, из-за чего происходила полная перезапись массива по новой каждый раз.
            }
            return ToArr(RxyArr);
        }

        public static double[,] CoupleCriteriaStudent(double[,] arr)
        {
            double[,] res = new double[arr.GetUpperBound(0) + 1, arr.GetUpperBound(1) + 1];
            int n = arr.GetUpperBound(0) + 1;
            for (int i = 0; i != arr.GetUpperBound(0)+1; i++)
            {
                for (int j = 0; j != arr.GetUpperBound (1) + 1; j++)
                {
                    double t = Math.Sqrt((arr[i, j] * arr[i, j] * (n - 2)) / (1 - arr[i, j] * arr[i, j]));
                    res[i,j] = Math.Round(t,4);
                }
            }
            return res;
        }
        public static double[,] SignificanceLevel(double[,] arr, double currentConstant)
        {
            double[,] res = new double[arr.GetUpperBound(0)+1,arr.GetUpperBound (1) + 1];
            for (int i = 0; i != arr.GetUpperBound(0) + 1; i++)
            {
                for (int j = 0; j != arr.GetUpperBound (1) +1; j++)
                {
                    res[i, j] = arr[i,j] > currentConstant ? 1 : 0;
                }
            }
            return res;  
        }
        public static void DrawDiagramm(double[,] CorrCoef, Canvas canv, int mode)
        {
            canv.Children.Clear();
            int size = 400;
            System.Drawing.Point center = new System.Drawing.Point(size / 2, size / 2);
            Ellipse ellipse = new Ellipse();
            ellipse.Width = size / 2;
            ellipse.Height = size / 2;
            ellipse.StrokeThickness = 2;
            ellipse.Stroke = System.Windows.Media.Brushes.Black;
           // canv.Children.Add(ellipse);
            double radius = ellipse.Height / 2;
            int step = 100;
          //  Canvas.SetTop(ellipse, center.Y - radius - step);
          //  Canvas.SetLeft(ellipse, center.X - radius - step);
            int n = CorrCoef.GetUpperBound(0) + 1;
            List<System.Windows.Point> points = new List<System.Windows.Point>();
            for (int i = 0; i < n; i++)
            {
                double angle = i * Math.PI * 2 / n;
                System.Windows.Point point = new System.Windows.Point(center.X - Math.Cos(angle) * radius, center.Y - Math.Sin(angle) * radius);
                Ellipse pointView = new Ellipse();
                pointView.Width = 10;
                pointView.Height = 10;
                pointView.Fill = System.Windows.Media.Brushes.Black;
                canv.Children.Add(pointView);
                double radiusPoint = pointView.Height / 2;
                Canvas.SetTop(pointView, point.Y - radiusPoint - step - 5);
                Canvas.SetLeft(pointView, point.X - radiusPoint - step - 5);
                points.Add(new System.Windows.Point(point.X - radiusPoint - step, point.Y - radiusPoint - step));

                TextBlock label = new TextBlock();
                label.Text = MainWindow.mainWindow.CoupleCorrelation.Columns[i + 1].Header.ToString();
                canv.Children.Add(label);

                System.Windows.Point labelPos = new System.Windows.Point(center.X - Math.Cos(angle) * (radius + 10), center.Y - Math.Sin(angle) * (radius + 10));
                Canvas.SetTop(label, labelPos.Y - 10 - step);
                Canvas.SetLeft(label, labelPos.X - 12 - step);
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    double coeff = 255 - Math.Abs(CorrCoef[i, j]) * 255;
                    Line line = new Line();
                    line.X1 = points[i].X;
                    line.Y1 = points[i].Y;
                    line.X2 = points[j].X;
                    line.Y2 = points[j].Y;
                    var val = CorrCoef[i, j];
                    switch (mode)
                    {
                        case 1:
                            if (val <= 1.0 && val >= 0.7)
                            {
                                line.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(50, 205, 50));
                            }
                            else if (val <= 0.6999 && val >= 0.5)
                            {
                                line.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 0));
                            }
                            else if (val <= 0.4999 && val >= 0.0001)
                            {
                                line.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 215, 0));
                            }
                            else if (val <= 0)
                            {
                                line.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 0, 0));
                            }
                            else
                            {
                                line.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                            }
                            line.StrokeThickness = 2;
                            break;
                        case 2:
                            if (val <= 1.0 && val >= 0.7)
                            {
                                line.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(50, 205, 50));
                                line.StrokeThickness = 2.7;
                            }
                            break;
                    }
                    canv.Children.Add(line);
                }
            }
        }

        public static double[][] PartialCorrelate(double[][] coefficientsCorrelations)
        {
            double[][] privateCorrelationsCoefficients = new double[8][];

            for (int i = 0; i < 8; i++)
            {
                var temp = new double[8];
                for (int j = 0; j < 8; j++)
                {
                    if (i == j)
                    {
                        temp[j] = 1;
                        continue;
                    }

                    temp[j] = Math.Round(-GetAlgebraicСomplement(coefficientsCorrelations, i, j) /
                              Math.Sqrt(GetAlgebraicСomplement(coefficientsCorrelations, i, i) *
                                        GetAlgebraicСomplement(coefficientsCorrelations, j, j)),4);
                }
                privateCorrelationsCoefficients[i] = temp;
            }
            return privateCorrelationsCoefficients;
        }
        public static double[][] PartialCriteriaStudent(double[][] arr)
        {
            int n = arr.GetUpperBound(0)+1;
            double[][] res = new double[n][];
            for (int i = 0; i != n; i++)
            {
                double[] temp = new double[n];
                for (int j = 0; j < n; j++)
                {
                    if (i == j)
                        temp[j] = double.PositiveInfinity;
                    else
                    {
                        double r = arr[i][j];
                        temp[j] = Math.Round(Math.Abs(r) * Math.Sqrt((n - 3) / (1 - r * r)),4);
                        res[i] = temp;
                    }
                }
            }
            return res;
        }
        private static double GetAlgebraicСomplement(double[][] matrix, int row, int col)
        {
            return Math.Pow(-1, row + col) * GetDeterminant(GetMinorMatrix(matrix, row, col));
        }
        private static double GetDeterminant(double[][] matrix)
        {
            double result = 1;
            double row = matrix.GetLength(0);
            double columns = matrix[0].GetLength(0);
            for (int k = 0; k < row; k++)
            {
                double a = matrix[k][k];
                result *= a;
                for (int i = 0; i < columns; i++)
                    matrix[k][i] /= a;

                for (int i = k + 1; i < columns; i++)
                {
                    double b = matrix[i][k];
                    for (int j = k; j < columns; j++)
                        matrix[i][j] -= matrix[k][j] * b;
                }
            }

            return result;
        }
        private static double[][] GetMinorMatrix(double[][] matrix, int row, int col)
        {
            var minorRows = matrix.GetLength(0) - 1;
            var minorColumns = matrix[0].GetLength(0) - 1;

            var minor = new double[minorRows][];
            int m = 0;
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                int k = 0;
                var temp = new double[minorColumns];
                if (i == row) continue;

                for (int j = 0; j < matrix[0].GetLength(0); j++)
                {
                    if (j == col) continue;
                    temp[k++] = matrix[i][j];
                }

                minor[m++] = temp;
            }
            return minor;
        }

        public static double[,] ToArr(List<List<string>> list) // Парсим двумерный список строк в двумерный массив
        {
            double[,] arr = new double[MainWindow.grid.Count(), 8];
            for (int i = 0; i != MainWindow.grid.Count(); i++)
            {
                for (int j = 1; j != 9; j++)
                {
                    arr[i, j - 1] = Convert.ToDouble(list[i][j]);
                }
            }
            return arr;
        }
        public static double[,] ToArr(List<List<double>> list) // парсим двумерный список в двумерный массив 
        {
            double[,] arr = new double[list.Count,list.Count];
            for (int i = 0; i != list.Count; i++)
            {
                for (int j = 0; j != list.Count; j++)
                {
                    arr[i, j] = list[i][j];
                }
            }
            return arr;
        }
        public static double[][] ToSteppedArr(double[,] list) // Парсим симметричный массив в ступенчатый
        {
            double[][] res = new double[list.GetUpperBound(0) + 1][];
            for(int i = 0; i != list.GetUpperBound(0) + 1; i++)
            {
                res[i] = new double[list.GetUpperBound(1) + 1];
                for(int j = 0; j != list.GetUpperBound(1)+1; j++)
                {
                    res[i][j] = list[i,j];
                }
            }
            return res;
        }
        private static double[] GetArray(double[,] list, int count) // вытягиваем нужный столбец, через указанный count
        {
            double[] arr = new double[list.GetUpperBound(0) + 1];
            for (int i = 0; i != list.GetUpperBound(0) + 1; i++)
            {
                arr[i] = list[i, count];
            }
            return arr;
        }
        
    }
}
