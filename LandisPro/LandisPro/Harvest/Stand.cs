using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandisPro.Harvest
{
    class Stand
    {
        public int itsId;
        public int itsManagementAreaId;
        public int itsTotalSites;
        public int itsActiveSites;
        private int itsHarvestableSites;
        private int itsMeanAge;
        private int itsUpdateFlag;
        private int itsRecentHarvestFlag;
        private int itsRank;
        private int itsReserveFlag;
        public Ldpoint itsMinPoint;
        public Ldpoint itsMaxPoint;
        public List<int> itsNeighborList = new List<int>();

        public Stand()
        {
            itsId = 0;
            itsManagementAreaId = 0;
            itsTotalSites = 0;
            itsActiveSites = 0;
            itsHarvestableSites = 0;
            itsMeanAge = 0;
            itsUpdateFlag = 1;
            itsRecentHarvestFlag = 0;
            itsRank = 0;
            itsReserveFlag = 0;
            itsMinPoint = new Ldpoint();
            itsMaxPoint= new Ldpoint();
        }

        public int getManagementAreaId()
        {
            return itsManagementAreaId;
        }

        public int isNeighbor(int r, int c)
        {
            int nid;
            if (GlobalFunctions.inBounds(r, c) == 1)
            {
                return 0;
            }
            if ((nid = BoundedPocketStandHarvester.standMap.getvalue32out((uint)r, (uint)c)) <= 0 || nid == itsId)
            {
                return 0;
            }
            else
            {
                return nid;
            }
        }
        public void addNeighbor(int id)
        {
            if (!itsNeighborList.Contains(id))
            {
                itsNeighborList.Add(id);
            }
        }

        public Boolean canBeHarvested()
        {
            //cerr << "int Stand::canBeHarvested() " << endl;
            update();
            return itsHarvestableSites > 0;
            // Jacob return !isReserved() && (itsHarvestableSites > 0);
        }

        public Ldpoint getMinPoint()
        {
            return itsMinPoint;
        }

        public Ldpoint getMaxPoint()
        {
            return itsMaxPoint;
        }

        public int getId()
        {
            return itsId;
        }

        public int numberOfActiveSites()
        {

            return itsActiveSites;

        }

        public bool inStand(int r, int c)
        {

            int sid = 0;
            int tempMid = 0;

            if (BoundedPocketStandHarvester.standMap.inMap((uint)r, (uint)c) == false)
            {
                return false;
            }

            sid = BoundedPocketStandHarvester.standMap.getvalue32out((uint)r, (uint)c); //change by Qia on Nov 4 2008
            if (sid > 0)
            {
                tempMid = BoundedPocketStandHarvester.pstands[sid].getManagementAreaId();
            }
            Boolean result = (BoundedPocketStandHarvester.standMap.inMap((uint)r, (uint)c) && BoundedPocketStandHarvester.standMap.getvalue32out((uint)r, (uint)c) == itsId && tempMid == itsManagementAreaId); //change by Qia on Nov 4 2008

            return result;

        }


        public void update()
        {
            //static int count_update = 0;
            //count_update++;

            Ldpoint pt = new Ldpoint();

            Site site;

            if (itsUpdateFlag == 1)
            {
                if (itsActiveSites == 0)
                {
                    itsMeanAge = 0;
                    itsHarvestableSites = 0;
                    itsRecentHarvestFlag = 0;
                }
                else
                {
                    //static int get_updatecount = 0;
                    //get_updatecount++;
                    int sum = 0;
                    int rcount = 0;
                    itsHarvestableSites = 0;
                    Ldpoint tmp_pt = this.getMinPoint();
                    Ldpoint tmp_ptmax = this.getMaxPoint();
                    int temp_id = this.getId();
                    for (StandIterator it = new StandIterator(this); it.moreSites(); it.gotoNextSite())
                    {
                        pt = it.getCurrentSite();                 
                        site = BoundedPocketStandHarvester.pCoresites[pt.y, pt.x];
                        if (BoundedPocketStandHarvester.pCoresites.locateLanduPt(pt.y, pt.x).active()) //original landis4.0: site->landUnit->active()
                        {
                            BoundedPocketStandHarvester.pHarvestsites.BefStChg(pt.y, pt.x); //Add By Qia on Nov 10 2008
                            sum += BoundedPocketStandHarvester.pHarvestsites[pt.y, pt.x].getMaxAge(pt.y, pt.x);
                            BoundedPocketStandHarvester.pHarvestsites.AftStChg(pt.y, pt.x); //Add By Qia on Nov 10 2008                     
                            if (BoundedPocketStandHarvester.standMap.getvalue32out((uint)pt.y, (uint)pt.x) > 0 && BoundedPocketStandHarvester.pHarvestsites[pt.y, pt.x].canBeHarvested(pt.y, pt.x)) //change by Qia on Nov 4 2008
                            {

                                itsHarvestableSites++;
                            }                        
                            if (BoundedPocketStandHarvester.pHarvestsites[pt.y, pt.x].wasRecentlyHarvested())
                            {
                                rcount++;
                            }
                        }
                    }
                    itsMeanAge = sum / numberOfActiveSites();                  
                    if ((float)rcount / numberOfActiveSites() < BoundedPocketStandHarvester.fParamharvestThreshold)
                    {
                        itsRecentHarvestFlag = 0;
                    }
                    else
                    {
                        itsRecentHarvestFlag = 1;
                    }                
                }
                itsUpdateFlag = 0;
            }
        }

        public int getAge()
        {
            update();
            return itsMeanAge;
        }

        public void setRank(int rank)
        {
            itsRank = rank;
        }


    }
}
