using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LandisPro.Harvest
{
    class RankbyVolume: StandRankingAlgorithm
    {
        public RankbyVolume(int someManagementAreaId, int someRotationAge) : base(someManagementAreaId, someRotationAge)
        {
        }

        public override void read(StreamReader infile)
        {
        }

        public override void rankStands(List<int> theRankedList)
        {
            int i;
            int id;
            Stand stand;
            int theLength = 0;
            double[] SortKeyArrayDouble;
            IntArray theStandArray = new IntArray(itsManagementArea.numberOfStands());
            IntArray theAgeArray = new IntArray(itsManagementArea.numberOfStands());
            IntArray theSortKeyArray = new IntArray(itsManagementArea.numberOfStands());
            filter(theStandArray, theAgeArray, ref theLength);
            SortKeyArrayDouble = new double[theLength + 1];

            for (i = 1; i <= theLength; i++)
            {
                id = theStandArray[i];
                stand = BoundedPocketStandHarvester.pstands[id];
                SortKeyArrayDouble[i] = computeStandBA(stand);
            }
            descendingSort_doubleArray(theStandArray, SortKeyArrayDouble, theLength);
            assign(theStandArray, theLength, theRankedList);
            SortKeyArrayDouble = null;
        }

        public double computeStandBA(Stand stand)
        {
            Ldpoint p = new Ldpoint();
            Site site;
            double count = 0;
            int m;
            int k;
            double TmpBasalAreaS = 0;
            Landunit l;
            for (StandIterator it = new StandIterator(stand); it.moreSites(); it.gotoNextSite())
            {
                p = it.getCurrentSite();
                site = BoundedPocketStandHarvester.pCoresites[p.y, p.x];
                l = BoundedPocketStandHarvester.pCoresites.locateLanduPt(p.y, p.x);
                count += 1;
                for (k = 1; k <= BoundedPocketStandHarvester.pCoresites.specNum; k++)
                {
                    for (m = 1; m <= site.specAtt(k).longevity / BoundedPocketStandHarvester.pCoresites.TimeStep; m++)
                    {
                        TmpBasalAreaS += BoundedPocketStandHarvester.pCoresites.GetGrowthRates(k, m, l.ltID) * BoundedPocketStandHarvester.pCoresites.GetGrowthRates(k, m, l.ltID) / 4 * 3.1415926 * site.SpecieIndex(k).getTreeNum(m, k) / 10000.00;
                    }
                }
            }
            if (count > 0)
            {
                TmpBasalAreaS = TmpBasalAreaS / count;
            }
            return TmpBasalAreaS;
        }

    }
}
