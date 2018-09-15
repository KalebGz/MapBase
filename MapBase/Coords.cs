using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapBase
{
    class Coords
    {
        public Double Lat
        {
            get;
            set;
        }

        public Double Lon
        {
            get;
            set;
        }


        public Coords(String returnData)
        {
            String[] sub = returnData.Split('|');
            Lat = Convert.ToDouble(sub[0]);
            Lon = Convert.ToDouble(sub[1]);
        }
    }
}
