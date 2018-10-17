using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandisPro
{
    class PDP
    {
        public struct bdapdp
        {
            public short[,] sTSLBDA; //tinesince last BDA
            public char[,] cBDASeverity;
            public char[] BDAType;
        }

        public int iCols;
        public int iRows;
        //BDA
        public int iBDANum;
        public bdapdp[] pBDAPDP;
        //Fire
        public short[,] sTSLFire;
        public char[,] cFireSeverity;
        //Fuel
        public char[,] cFineFuel;
        public char[,] cCoarseFuel;
        public char[,] cFireIntensityClass; // 13.    PotentialFireIntensity
        public char[,] cFireRiskClass; // 14.    Potential FireRisk
        //BGrowth
        //can changed to short???
        public int[,] iTotalLiveBiomass;
        public int[,] iLiveBiomassBySpecies;
        public int[,] iCoarseWoodyDebris;
        public int[,] iDeadFineBiomass;
        //Harvest
        public short[,] sTSLHarvest;
        public char[,] cHarvestEvent;
        //wind
        public short[,] sTSLWind;
        public char[,] cWindSeverity;
        //Succession
        public short[,] sTSLMortality;


        public void addedto_sTSLMortality(int i, int j, short added_value)
        {
            sTSLMortality[i, j] += added_value;
        }



        public PDP() { }


        public PDP(int mode, int col, int row, int BDANo)
        {
            int i;
            iCols = col;
            iRows = row;
            iBDANum = BDANo;
            sTSLFire = null;
            cFireSeverity = null;

            cFineFuel = null;
            cCoarseFuel = null;

            cFireIntensityClass = null;
            cFireRiskClass = null;

            iTotalLiveBiomass = null;
            iLiveBiomassBySpecies = null;
            iCoarseWoodyDebris = null;
            iDeadFineBiomass = null;

            sTSLHarvest = null;
            cHarvestEvent = null;

            sTSLWind = null;
            cWindSeverity = null;

            sTSLMortality = new short[iRows+1,iCols+1];

            if ((mode & Defines.G_BDA) != 0)
            {
                if ((mode & Defines.G_WIND) != 0)
                    sTSLWind = new short[iRows,iCols];
            }

            if ((mode & Defines.G_FIRE) != 0)
            {
                sTSLFire = new short[iRows,iCols];
                cFireSeverity = new char[iRows,iCols];
            }

            if ((mode & Defines.G_FUEL) != 0)
            {
                cFineFuel = new char[iRows,iCols];
                cCoarseFuel = new char[iRows,iCols];
                cFireIntensityClass = new char[iRows,iCols];
                cFireRiskClass = new char[iRows,iCols];
            }

            if ((mode & Defines.G_FUEL) != 0)
            {
                if ((mode & Defines.G_BDA) != 0)
                {
                    pBDAPDP = new bdapdp[iBDANum];
                    for (i = 0; i < iBDANum; i++)
                    {
                        pBDAPDP[i].BDAType = new char[50];
                        pBDAPDP[i].cBDASeverity = new char[iRows,iCols];
                        pBDAPDP[i].sTSLBDA = new short[iRows,iCols];
                    }

                }
            }

            if ((mode & Defines.G_WIND) != 0)
            {
                if (sTSLWind == null)
                    sTSLWind = new short[iRows,iCols];
                if (cWindSeverity == null)
                    cWindSeverity = new char[iRows,iCols];
                sTSLWind[1,1] = 0;
            }

            if ((mode & Defines.G_HARVEST) != 0)
            {
                if (sTSLHarvest == null)
                    sTSLHarvest = new short[iRows,iCols];
                if (cHarvestEvent == null)
                    cHarvestEvent = new char[iRows,iCols];
            }
        }

        ~PDP()
        {
        //Fire
        sTSLFire = null;
        cFireSeverity = null;
        //Fuel
        cFineFuel = null;
        cCoarseFuel = null;
        cFireIntensityClass = null; // 13.    PotentialFireIntensity
        cFireRiskClass = null; // 14.    Potential FireRisk
        //BGrowth
        //can changed to short???
        iTotalLiveBiomass = null;
        iLiveBiomassBySpecies = null;
        iCoarseWoodyDebris = null;
        iDeadFineBiomass = null;
        //Harvest
        sTSLHarvest = null;
        cHarvestEvent = null;
        //wind
        sTSLWind = null;
        cWindSeverity = null;
        //Succession
        sTSLMortality = null;
    }
    }
}
