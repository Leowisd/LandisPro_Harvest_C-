using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace LandisPro.Harvest
{
    class HarvestReport
    {
        private int count;
        private IntArray sumBySpecies;

        public HarvestReport()
        {
            sumBySpecies = new IntArray(BoundedPocketStandHarvester.numberOfSpecies);
            reset();
        }

        public void reset()
        {
            count = 0;
            sumBySpecies.reset();
        }

        public void incrementSiteCount()
        {

            count++;

        }
        public void addToSpeciesTotal(int speciesIndex, int value)
        {

            sumBySpecies[speciesIndex] += value;

        }
        public int numberOfSitesCut()
        {

            return count;

        }
        public int sumOfCohortsCut()
        {

            return sumBySpecies.sum();

        }
        public int sumOfCohortsCut(int speciesIndex)
        {

            return sumBySpecies[speciesIndex];

        }
        public void addReport(HarvestReport someReport)
        {

            count += someReport.count;

            for (int i = 1; i <= sumBySpecies.number(); i++)
            {

                sumBySpecies[i] += someReport.sumBySpecies[i];

            }
        }

    }
}
