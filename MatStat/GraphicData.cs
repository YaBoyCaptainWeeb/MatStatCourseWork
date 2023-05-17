using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatStat
{
    public class GraphicData
    {
        public double[] Frequency { get; set; }
        public double[] TheoreticFrequency { get; set; }
        public double[] Intervals { get; set; }
        public double IsNormal { get; set; }
        public string Title { get; set; }
        public GraphicData(double[] frequency, double[] theoreticFrequency, double[] intervals, double isNormal, string title)
        {
            this.Frequency = frequency;
            this.TheoreticFrequency = theoreticFrequency;
            this.Intervals = intervals;
            this.IsNormal = isNormal;
            this.Title = title;
            this.Frequency = frequency;
            this.TheoreticFrequency = theoreticFrequency;
            this.Intervals = intervals;
            this.IsNormal = isNormal;
            this.Title = title;
        }
    }
}
