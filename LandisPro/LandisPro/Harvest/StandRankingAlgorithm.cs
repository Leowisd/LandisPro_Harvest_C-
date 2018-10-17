using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandisPro.Harvest
{
    class StandRankingAlgorithm
    {
        protected ManagementArea itsManagementArea;
        protected int itsRotationAge;

        public StandRankingAlgorithm(int someManagementAreaId, int someRotationAge)
        {
            itsManagementArea = BoundedPocketStandHarvester.managementAreas[someManagementAreaId];
            itsRotationAge = someRotationAge;
        }

        public virtual void read(StreamReader infile)
        {

        }

        public virtual void rankStands(List<int> theRankedList)
        {

        }
    }
}
