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
        protected int itsManagementId;
       

        public StandRankingAlgorithm(int someManagementAreaId, int someRotationAge)
        {
            itsManagementArea = BoundedPocketStandHarvester.managementAreas[someManagementAreaId];
            itsRotationAge = someRotationAge;
            itsManagementId = someManagementAreaId;
        }

        public virtual void read(StreamReader infile)
        {

        }

        public virtual void rankStands(List<int> theRankedList)
        {

        }

        public void filter(IntArray theStandArray, IntArray theAgeArray, ref int theLength)
        {
            Stand stand;

            int theNewLength = 0;

            List<int> it = new List<int>();
            it = itsManagementArea.itsStandList;
            for (int i = itsManagementId; i < it.Count; i++)
            {
                int id = it[i];
                stand = BoundedPocketStandHarvester.pstands[id];
                if (stand.canBeHarvested() && stand.getAge() >= itsRotationAge)
                {
                    theNewLength++;
                    theStandArray[theNewLength] = id;
                    theAgeArray[theNewLength] = stand.getAge();
                }
            }
            theLength = theNewLength;
        }

    }
}
