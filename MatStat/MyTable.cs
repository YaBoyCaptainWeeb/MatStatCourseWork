using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatStat
{
    internal class MyTable
    {
        public string _model { get; set; }
        public double _price { get; set; }
        public double _CPUClock { get; set; }
        public double _RAM { get; set; }
        public double _DriveDisk { get; set; }
        public double _GPUClock { get; set; }
        public double _Diagonal { get; set; }
        public double _Battery { get; set; }
        public double _Weight { get; set; }

        public MyTable(string model, double price, double CPUClock, double RABClock, double DriveDisk, double GPUClock, double Diagonal, double Battery, double Weight)
        {
            _model = model;
            _price = price;
            _CPUClock = CPUClock;
            _RAM = RABClock;
            _DriveDisk = DriveDisk;
            _GPUClock = GPUClock;
            _Diagonal = Diagonal;
            _Battery = Battery;
            _Weight = Weight;
        }
        public MyTable()
        {

        }
        public double GetParam(int n)
        {
            switch (n)
            {
                case 0:
                    return this._price;
                case 1:
                    return this._CPUClock;
                case 2:
                    return this._RAM;
                case 3:
                    return this._DriveDisk;
                case 4:
                    return this._GPUClock;
                case 5:
                    return this._Diagonal;
                case 6:
                    return this._Battery;
                case 7:
                    return this._Weight;
                default:
                    return 0;
            }
        }
    }
}
