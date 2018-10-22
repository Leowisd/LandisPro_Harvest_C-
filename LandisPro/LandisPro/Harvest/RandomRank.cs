using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandisPro.Harvest
{
    class RandomRank: StandRankingAlgorithm
    {
        public RandomRank(int someManagementAreaId, int someRotationAge): base(someManagementAreaId, someRotationAge)
        {

        }

        public override void rankStands(List<int> theRankedList)
        {
            IntArray theStandArray = new IntArray(itsManagementArea.numberOfStands());
            IntArray theAgeArray = new IntArray(itsManagementArea.numberOfStands());
            int theLength = 0;
            filter(theStandArray, theAgeArray, ref theLength);
            for (int i = 1; i <= theLength; i++)
            {
                int k = (int)(theLength * frand()) + 1;
                // Harvest Bug 04/15/2015
                if (k > theLength)
                {
                    //cerr << "k = " << k << "  k should be equal to " << theLength + 1 << endl;
                    k = theLength;
                }
                if (k<1 || k>theLength)
                    throw new Exception("Invaild range of k");
                int temp = theStandArray[i];
                theStandArray[i] = theStandArray[k];
                theStandArray[k] = temp;
            }
            assign(theStandArray, theLength, theRankedList);
        }
    }
}
