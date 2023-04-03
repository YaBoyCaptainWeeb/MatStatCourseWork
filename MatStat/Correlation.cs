﻿using ScottPlot.Renderable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MatStat
{
    static internal class Correlation
    {
       // public MainWindow Main { get; set; }

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
