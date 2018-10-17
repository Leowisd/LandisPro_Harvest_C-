using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace LandisPro.Harvest
{
    class CohortRemovalMask
    {
        private agelist mask;

        public CohortRemovalMask()
        {

        }

        ~CohortRemovalMask()
        {

        }

        public void read(StreamReader infile)
        {
            mask.read(infile);
        }

        public void dump()
        {
            mask.dump();
        }

        public int query()
        {
            if (mask.query())
                return 1;
            else return 0;
        }

        public int query(int n)
        {
            if (mask.query(n))
                return 1;
            else return 0;
        }
    }
}
