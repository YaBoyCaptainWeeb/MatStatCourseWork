using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatStat
{
    internal class TriangleTable
    {
        public string _model { get; set; }
        public string _price { get; set; }
        public string _CPUClock { get; set; }
        public string _RAM { get; set; }
        public string _DriveDisk { get; set; }
        public string _GPUClock { get; set; }
        public string _Diagonal { get; set; }
        public string _Battery { get; set; }
        public string _Weight { get; set; }
        public TriangleTable(string model,string price, string CPUClock, string RAM, string DriveDisk, string GPUClock, string Diagonal,string Battery, string Weight)
        {
            _model = model;
            _price = price;
            _CPUClock = CPUClock;
            _RAM = RAM;
            _DriveDisk = DriveDisk;
            _GPUClock = GPUClock;
            _Diagonal = Diagonal;
            _Battery = Battery;
            _Weight = Weight;
        }

        public TriangleTable() { }
    }
}
