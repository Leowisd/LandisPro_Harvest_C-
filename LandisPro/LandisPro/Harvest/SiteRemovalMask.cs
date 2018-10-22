using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading.Tasks;

namespace LandisPro.Harvest
{
    class SiteRemovalMask
    {
        private int numSpec;
        private CohortRemovalMask[] mask;
        private IntArray itsPlantingCode;

        public SiteRemovalMask()
        {
            itsPlantingCode = new IntArray(BoundedPocketStandHarvester.numberOfSpecies);
            if (BoundedPocketStandHarvester.numberOfSpecies<=0)
                throw new Exception("Error: invalid number of speices");
            mask = new CohortRemovalMask[BoundedPocketStandHarvester.numberOfSpecies];
            for (int i=0;i<BoundedPocketStandHarvester.numberOfSpecies;i++)
                mask[i] = new CohortRemovalMask();
            numSpec = BoundedPocketStandHarvester.numberOfSpecies;
        }

        ~SiteRemovalMask()
        {
            mask = null;
        }

        public void read(StreamReader infile)
        {
            for (int i = 0; i < numSpec; i++)
            {
                string instring;
                string[] sarray;

                instring = infile.ReadLine();
                sarray = instring.Split(' ');
                itsPlantingCode[i + 1] = int.Parse(sarray[0]);
                mask[i].read(infile);

            }
        }
    }
}
