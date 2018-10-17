using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandisPro.Harvest
{
    class ManagementAreas
    {
        private int numManagementAreas;
        private ManagementArea[] managementAreas;

        public ManagementAreas()
        {
            numManagementAreas = 0;
            managementAreas = null;
        }

        ~ManagementAreas()
        {
            managementAreas = null;
        }

        public int number()
        {
            return numManagementAreas;
        }

        public ManagementArea this[int n]
        {
            get
            {
                if (n<1 && n>numManagementAreas)
                    throw new Exception("Error: Invlide number of managementAreas");

                return managementAreas[n - 1];
            }
        }

        public void construct()
        {
            int i;
            int r;
            int c;
            int id;
            int snr;
            int snc;

            if (BoundedPocketStandHarvester.pCoresites.number()<=0 || BoundedPocketStandHarvester.managementAreaMap.NumRows<=0)
                throw new Exception("error on numbers of sites");

            numManagementAreas = BoundedPocketStandHarvester.managementAreaMap.high();
            if (managementAreas != null)
                managementAreas = null;
            managementAreas = new ManagementArea[numManagementAreas];
            for (i=0;i<numManagementAreas;i++)
                managementAreas[i] = new ManagementArea();

            snc = BoundedPocketStandHarvester.pCoresites.numRows();
            snr = BoundedPocketStandHarvester.pCoresites.numColumns();

            for (i = 0; i < numManagementAreas; i++)
            {
                managementAreas[i].itsId = i + 1;
            }

            for (r = 1; r <= snr; r++)
                for (c = 1; c <= snc; c++)
                {
                    id = BoundedPocketStandHarvester.managementAreaMap[(uint)r, (uint)c];
                    if (id > 0 && BoundedPocketStandHarvester.standMap.getvalue32out((uint)r, (uint)c) > 0)
                    {
                        managementAreas[id - 1].itsActiveSites++;
                        if (BoundedPocketStandHarvester.pCoresites.locateLanduPt(r, c).active())
                            managementAreas[id - 1].itsActiveSites++;
                        managementAreas[id - 1].addStand(BoundedPocketStandHarvester.managementAreaMap.getvalue32out((uint)r,(uint)c));
                    }
                }
        }
    }
}
