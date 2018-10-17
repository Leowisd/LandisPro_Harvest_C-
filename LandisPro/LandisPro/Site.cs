using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LandisPro
{
    class Site: Species
    {
        public double RD;
        public double maxAge;
        public int numofsites;
        public int HighestShadeTolerance;

        public Site(int n) : base(n)
        {
            numofsites = 0;
        }

        public Site() : base()
        {
            numofsites = 0;
        }

        public new void Read(StreamReader infile)
        {
            base.Read(infile);
        }

        public void copy(Site s)
        {
            if (s == null)
                return;

            RD = s.RD;
            maxAge = s.maxAge;
            numofsites = s.numofsites;
            HighestShadeTolerance = s.HighestShadeTolerance;

            base.copy(s.species, Site.numSpec);
        }
    }
}
