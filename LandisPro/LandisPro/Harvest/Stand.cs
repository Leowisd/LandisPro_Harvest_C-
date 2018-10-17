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
    }
}
