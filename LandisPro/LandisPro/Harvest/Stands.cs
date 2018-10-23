using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandisPro.Harvest
{
    class Stands
    {
        private int numStands;
        private Stand[] stands;
        public Stands()
        {
            numStands = 0;
            stands = null;
        }

        public Stand this[int n]
        {
            get
            {
                return stands[n - 1];
            }
        }

        public int number()
        {
            return numStands;
        }

        public void construct()
        {
            int i;
            int r;
            int c;
            int id;
            int nid;
            int snr;
            int snc;
            int mid;
            int currMid;

            Ldpoint pmin = new Ldpoint();
            Ldpoint pmax = new Ldpoint();
            string errorString = "";

            numStands = BoundedPocketStandHarvester.standMap.high();
            if (stands != null)
            {
                stands = null;
            }
            stands = new Stand[numStands];
            for (int k = 0; k < numStands; k++)
            {
                stands[k] = new Stand();
            }

            snr = BoundedPocketStandHarvester.pCoresites.numRows();
            snc = BoundedPocketStandHarvester.pCoresites.numColumns();

            for (i = 0; i < numStands; i++)
            {
                stands[i].itsId = i + 1;
                stands[i].itsMinPoint.x = snc;
                stands[i].itsMinPoint.y = snr;
                stands[i].itsMaxPoint.x = 1;
                stands[i].itsMaxPoint.y = 1;
            }

            for (r = 1; r <= snr; r++)
            {
                for (c = 1; c <= snc; c++)
                {
                    id = BoundedPocketStandHarvester.standMap.getvalue32out((uint)r, (uint)c);
                    if (id > 0)
                    {
                        pmin = stands[id - 1].itsMinPoint;
                        pmax = stands[id - 1].itsMaxPoint;
                        if (r < pmin.y)
                        {
                            pmin.y = r;
                        }
                        if (r > pmax.y)
                        {
                            pmax.y = r;
                        }
                        if (c < pmin.x)
                        {
                            pmin.x = c;
                        }
                        if (c > pmax.x)
                        {
                            pmax.x = c;
                        }
                        stands[id - 1].itsMinPoint = pmin;
                        stands[id - 1].itsMaxPoint = pmax;

                        mid = BoundedPocketStandHarvester.managementAreaMap[(uint)r, (uint)c];
                        if (mid <= 0)
                        {
                            errorString = string.Format("No management area defined at (x,y) = ({0:D},{1:D}) StandID: {2:D}", c, r, id);
                            throw new Exception(errorString);
                        }
                        if ((currMid = stands[id - 1].itsManagementAreaId) > 0 && currMid != mid)
                        {
                            errorString = string.Format("Stand {0:D} crosses management area boundary at (x,y):currMid:{1:D}, mid: {2:D} = ({3:D},{4:D})", id, c, r, currMid, mid);
                            throw new Exception(errorString);
                        }
                        stands[id - 1].itsManagementAreaId = mid;
                        if (c<snc)
                            if ((nid = stands[id - 1].isNeighbor(r, c + 1)) > 0)
                            {
                                stands[id - 1].addNeighbor(nid);
                                stands[nid - 1].addNeighbor(id);
                            }
                        if (r>1)
                            if ((nid = stands[id - 1].isNeighbor(r - 1, c)) > 0)
                            {
                                stands[id - 1].addNeighbor(nid);
                                stands[nid - 1].addNeighbor(id);
                            }
                        stands[id - 1].itsTotalSites++;
                        if (BoundedPocketStandHarvester.pCoresites.locateLanduPt(r, c).active())
                        {
                            stands[id - 1].itsActiveSites++;
                        }
                    }
                }

            }
        }
    }
}
