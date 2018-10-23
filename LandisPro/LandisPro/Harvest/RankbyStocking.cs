using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace LandisPro.Harvest
{
    class RankbyStocking: StandRankingAlgorithm
    {
        public RankbyStocking(int someManagementAreaId, int someRotationAge) : base(someManagementAreaId, someRotationAge)
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
                SortKeyArrayDouble[i] = computeStandStocking(stand);
            }
            descendingSort_doubleArray(theStandArray, SortKeyArrayDouble, theLength);
            assign(theStandArray, theLength, theRankedList);
            SortKeyArrayDouble = null;
        }

        public double computeStandStocking(Stand stand)
        {
            Ldpoint p = new Ldpoint();
            Site site;
            double count = 0;
            int m;
            int k;
            double num_trees = 0; //N
            double Diameters = 0; //D
            double Diameters_square = 0; //D^2
            double x = BoundedPocketStandHarvester.pCoresites.stocking_x_value;
            double y = BoundedPocketStandHarvester.pCoresites.stocking_y_value;
            double z = BoundedPocketStandHarvester.pCoresites.stocking_z_value;
            Landunit l;
            for (StandIterator it = new StandIterator(stand); it.moreSites(); it.gotoNextSite())
            {
                p = it.getCurrentSite();
                l = BoundedPocketStandHarvester.pCoresites.locateLanduPt(p.y, p.x);
                site = BoundedPocketStandHarvester.pCoresites[p.y, p.x];
                count += 1;
                for (k = 1; k <= BoundedPocketStandHarvester.pCoresites.specNum; k++)
                {
                    for (m = 1; m <= site.specAtt(k).longevity / BoundedPocketStandHarvester.pCoresites.TimeStep; m++)
                    {
                        num_trees += site.SpecieIndex(k).getTreeNum(m, k);
                        Diameters += BoundedPocketStandHarvester.pCoresites.GetGrowthRates(k, m, l.ltID) * site.SpecieIndex(k).getTreeNum(m, k) / 2.54;
                        Diameters_square += BoundedPocketStandHarvester.pCoresites.GetGrowthRates(k, m, l.ltID) * BoundedPocketStandHarvester.pCoresites.GetGrowthRates(k, m, l.ltID) * site.SpecieIndex(k).getTreeNum(m, k) / 2.54 / 2.54;
                    }
                }
            }
            return (x * num_trees + y * Diameters + z * Diameters_square) / (BoundedPocketStandHarvester.pCoresites.CellSize * BoundedPocketStandHarvester.pCoresites.CellSize / 4046.86) / stand.numberOfActiveSites();
        }

    }
}
