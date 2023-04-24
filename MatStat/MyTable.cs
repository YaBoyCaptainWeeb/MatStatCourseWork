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
    }
}
