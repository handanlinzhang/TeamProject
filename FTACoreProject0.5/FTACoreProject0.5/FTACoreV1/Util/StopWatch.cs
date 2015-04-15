using System;
using System.Linq;
using System.Collections.Generic;

namespace FTACoreSL.Util
{
    public class StopWatch
    {
        private static long starttime;
        public static void Start()
        {
            starttime = Environment.TickCount;
        }

        public static long ElapsedMilliseconds
        {
            get { return Environment.TickCount - starttime; }
        }
    }
}
