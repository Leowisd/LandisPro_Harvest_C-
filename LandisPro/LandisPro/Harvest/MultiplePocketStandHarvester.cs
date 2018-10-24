using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandisPro.Harvest
{
    class MultiplePocketStandHarvester: StandHarvest
    {
        private double itsProportion;
        private double itsMeanGroupSize;
        private double itsStandardDeviation;
        private int itsTargetCut;

        public MultiplePocketStandHarvester(Stand stand, double proportion, double meanGroupSize, double standardDeviation, SiteHarvester siteHarvester)
        {
            itsProportion = proportion;
            itsMeanGroupSize = meanGroupSize;
            itsStandardDeviation = standardDeviation;
            itsTargetCut = 0;
            setStand(stand);
            setSiteHarvester(siteHarvester);
        }

        public bool isValidStartPoint(Ldpoint pt)
        {
            return GlobalFunctions.canBeHarvested(pt) && (BoundedPocketStandHarvester.visitationMap[(uint)pt.y, (uint)pt.x] != BoundedPocketStandHarvester.currentHarvestEventId);
        }

        public bool getRandomStartPoint(ref Ldpoint startPoint)
        {
            Ldpoint pt = new Ldpoint();
            bool found = false;
            for (int i = 0; i < 100; i++)
            {
                pt = getStand().getRandomPoint();
                found = isValidStartPoint(pt);
                if (found)
                {
                    break;
                }
            }
            if (!found)
            {
                Debug.Assert(getStand() != null);
                for (StandIterator it = new StandIterator(getStand()); it.moreSites(); it.gotoNextSite())
                {
                    pt = it.getCurrentSite();
                    found = isValidStartPoint(pt);
                    if (found)
                    {
                        break;
                    }
                }
            }
            startPoint = pt;
            return found;
        }
        public int getRandomGroupSize()
        {

            int gsize;
            while ((gsize = (int) GlobalFunctions.gasdev(itsMeanGroupSize, itsStandardDeviation)) < 1)
            {
                ;
            }
            return gsize;
        }


        public override int Harvest()
        {
            Ldpoint startPoint = new Ldpoint();
            int targetPocketCut = 0;
            BoundedPocketStandHarvester pocketHarvester;
            int pocketCut = 0;
            int sumCut = 0;
            itsTargetCut = (int)(getStand().numberOfActiveSites() * itsProportion);
            while (sumCut < itsTargetCut)
            {
                if (!getRandomStartPoint(ref startPoint))
                {
                    break;
                }
                targetPocketCut = getRandomGroupSize();
                pocketHarvester = new BoundedPocketStandHarvester(targetPocketCut, startPoint, getSiteHarvester(),null);
                pocketCut = pocketHarvester.Harvest();
                sumCut += pocketCut;
                pocketHarvester = null;
                pocketHarvester = null;
            }
            return sumCut;
        }
    }
}
