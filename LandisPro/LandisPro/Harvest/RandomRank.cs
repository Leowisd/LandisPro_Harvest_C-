using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LandisPro.Harvest
{
    class RandomRank: StandRankingAlgorithm
    {
        public RandomRank(int someManagementAreaId, int someRotationAge): base(someManagementAreaId, someRotationAge)
        {

        }

        public override void read(StreamReader infile)
        {

        }

        public override void rankStands(ref List<int> theRankedList)
        {
            IntArray theStandArray = new IntArray(itsManagementArea.numberOfStands());
            IntArray theAgeArray = new IntArray(itsManagementArea.numberOfStands());
            int theLength = 0;
            filter(ref theStandArray, ref theAgeArray, ref theLength);

            for (int i = 1; i <= theLength; i++)
            {
                int k = (int)(theLength * system1.frand()) + 1;
                if (k > theLength)
                {
                    k = theLength;
                }
                if (k<1 || k>theLength)
                    throw new Exception("Invaild range of k");
                int temp = theStandArray[i];
                theStandArray[i] = theStandArray[k];
                theStandArray[k] = temp;
            }
            assign(theStandArray, theLength, ref theRankedList);
        }
    }
}
