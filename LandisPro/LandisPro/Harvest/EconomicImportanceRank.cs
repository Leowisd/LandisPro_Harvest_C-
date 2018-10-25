using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandisPro.Harvest
{
    class EconomicImportanceRank: StandRankingAlgorithm
    {
        SiteRemovalMask itsRemovalMask;
        IntArray itsDollarValuePerTon;
        IntArray itsMaturity;

        public EconomicImportanceRank(int someManagementAreaId, int someRotationAge, SiteRemovalMask someSiteRemovalMask): base(someManagementAreaId, someRotationAge)
        {
            itsDollarValuePerTon = new IntArray(BoundedPocketStandHarvester.numberOfSpecies);
            itsMaturity = new IntArray(BoundedPocketStandHarvester.numberOfSpecies);

            itsRemovalMask = someSiteRemovalMask;
            for (int i = 1; i <= BoundedPocketStandHarvester.numberOfSpecies; i++)
            {
                itsDollarValuePerTon[i] = 0;
                itsMaturity[i] = 1;
            }
        }
    }
}
