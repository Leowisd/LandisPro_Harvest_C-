using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandisPro.Harvest
{
    class SiteHarvester
    {
        public Stand pstands;
        public int itsHarvestType;
        public SiteRemovalMask itsRemovalMask;
        public HarvestReport itsReport;
        public int itsDuration;

        public SiteHarvester(int harvestType, SiteRemovalMask someRemovalMask, HarvestReport someReport, int someDuration)
        {

            itsHarvestType = harvestType;
            itsRemovalMask = someRemovalMask;
            itsReport = someReport;
            itsDuration = someDuration;
        }

        public int getHarvestType()
        {
            return itsHarvestType;
        }

        public SiteRemovalMask getRemovalMask()
        {
            return itsRemovalMask;
        }

        public HarvestReport getReport()
        {
            return itsReport;
        }

        public int getDuration()
        {
            return itsDuration;
        }

        public int harvest(Ldpoint pt)
        {
            BoundedPocketStandHarvester.pCoresites.BefStChg(pt.y, pt.x);
            //</Add By Qia on Oct 23 2008>
            Site site = BoundedPocketStandHarvester.pCoresites[pt.y, pt.x];
            Landunit l;
            l = BoundedPocketStandHarvester.pCoresites.locateLanduPt(pt.y, pt.x);
            agelist a;
            CohortRemovalMask m;
            int cohortCut;
            int siteCut = 0;
            //by wei li
            int sitePlanted = 0;
            //by wei li
            for (int i = 1; i <= BoundedPocketStandHarvester.numberOfSpecies; i++)
            {
                a = (agelist)site[i];
                m = itsRemovalMask[i];

                cohortCut = harvestCohorts(a, m);
                //<Add By Qia on Feb 16 2010>
                double tmpBiomass;
                double tmpCarbon;

                for (int age = BoundedPocketStandHarvester.pCoresites.TimeStep_Harvest; age <= 320; age += BoundedPocketStandHarvester.pCoresites.TimeStep_Harvest)
                {
                    if (m.query(age)==1 && a.query(age))
                    {
                        tmpBiomass = Math.Exp(BoundedPocketStandHarvester.pCoresites.GetBiomassData(site.specAtt(i).BioMassCoef, 1) + BoundedPocketStandHarvester.pCoresites.GetBiomassData(site.specAtt(i).BioMassCoef, 2) * Math.Log(BoundedPocketStandHarvester.pCoresites.GetGrowthRates(i, age / BoundedPocketStandHarvester.pCoresites.TimeStep, l.ltID))) * (BoundedPocketStandHarvester.pCoresites[pt.y, pt.x].SpecieIndex(i).getTreeNum(age / BoundedPocketStandHarvester.pCoresites.TimeStep, i)) / 1000.00;
                        BoundedPocketStandHarvester.pCoresites.Harvest70outputIncreaseBiomassvalue(pt.y, pt.x, tmpBiomass);
                        tmpCarbon = Math.Exp(BoundedPocketStandHarvester.pCoresites.GetBiomassData(site.specAtt(i).BioMassCoef, 1) + BoundedPocketStandHarvester.pCoresites.GetBiomassData(site.specAtt(i).BioMassCoef, 2) * Math.Log(BoundedPocketStandHarvester.pCoresites.GetGrowthRates(i, age / BoundedPocketStandHarvester.pCoresites.TimeStep, l.ltID))) * BoundedPocketStandHarvester.pCoresites[pt.y, pt.x].SpecieIndex(i).getTreeNum(age / BoundedPocketStandHarvester.pCoresites.TimeStep, i);
                        BoundedPocketStandHarvester.pCoresites.Harvest70outputIncreaseCarbonvalue(pt.y, pt.x, tmpCarbon * site.specAtt(i).CarbonCoEfficient);
                    }

                }

                //</Add By Qia on Feb 16 2010>
                itsReport.addToSpeciesTotal(i, cohortCut);
                if (cohortCut > 0)
                {
                    siteCut = 1;
                }
                //by wei li
                if (itsRemovalMask.plantingCode(i)>0 && !a.query(BoundedPocketStandHarvester.pCoresites.TimeStep_Harvest))
                {
                    a.set(BoundedPocketStandHarvester.pCoresites.TimeStep_Harvest);
                    sitePlanted = 1;
                }
                //end by wei li
            }

            if (siteCut == 1)
            {
                itsReport.incrementSiteCount();
                BoundedPocketStandHarvester.pHarvestsites.BefStChg(pt.y, pt.x); //Add By Qia on Nov 07 2008
                //update PDP
                BoundedPocketStandHarvester.m_pPDP.sTSLHarvest[pt.y,pt.x] = 0;
                BoundedPocketStandHarvester.m_pPDP.cHarvestEvent[pt.y,pt.x] = (char)itsHarvestType;

                BoundedPocketStandHarvester.pHarvestsites[pt.y, pt.x].harvestExpirationDecade = (short)(BoundedPocketStandHarvester.currentDecade + itsDuration);
                BoundedPocketStandHarvester.pHarvestsites.AftStChg(pt.y, pt.x);
                GlobalFunctions.setUpdateFlags(pt.y, pt.x);
            }
            //by wei li
            if (siteCut != 0 || sitePlanted != 0)
            {
                GlobalFunctions.setUpdateFlags(pt.y, pt.x);
            }
            //by wei li
            //<Add By Qia on Oct 23 2008>
            BoundedPocketStandHarvester.pCoresites.AftStChg(pt.y, pt.x);
            //</Add By Qia on Oct 23 2008>
            return siteCut;
        }


        public int harvestCohorts(agelist cohorts, CohortRemovalMask mask)
        {
            int sumCut = 0;
            for (int age = BoundedPocketStandHarvester.pCoresites.TimeStep_Harvest; age <= 320; age += BoundedPocketStandHarvester.pCoresites.TimeStep_Harvest)
            {
                if (mask.query(age)==1 && cohorts.query(age))
                {
                    cohorts.reset(age);
                    sumCut += age;
                }
            }
            return sumCut;
        }

    }
}
