﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandisPro.Harvest
{
    class HarvestRegime: HarvestEvent
    {
        private int itsManagementAreaId;
        private int itsRotationAge;
        private SiteRemovalMask itsRemovalMask;
        private StandRankingAlgorithm itsRankAlgorithm;
        private int itsDuration;
        private HarvestReport itsReport;

        public HarvestRegime()
        {
            itsManagementAreaId = 0;
            itsRotationAge = 0;
            itsRemovalMask = new SiteRemovalMask();
            itsRankAlgorithm = null;
            itsDuration = 0;
            itsReport = new HarvestReport();
        }

        ~HarvestRegime()
        {
            itsRemovalMask = null;
            itsRankAlgorithm = null;
            itsReport = null;
        }

        public int getManagementAreaId()
        {

            return itsManagementAreaId;

        }
        public int getRotationAge()
        {

            return itsRotationAge;

        }
        public SiteRemovalMask getRemovalMask()
        {

            return itsRemovalMask;

        }
        public StandRankingAlgorithm getRankAlgorithm()
        {

            return itsRankAlgorithm;

        }
        public int getDuration()
        {

            return itsDuration;

        }
        public HarvestReport getReport()
        {

            return itsReport;

        }
        public void setDuration(int duration)
        {

            itsDuration = duration;

        }

        public override void Read(StreamReader inFile)
        {
            int rankAlgorithmId;
            string label = "Harvest Module\0";
            int id;
            string instring;
            string[] sarray;

            instring = inFile.ReadLine();
            sarray = instring.Split('#');
            id = int.Parse(sarray[0]);
            if (id != 1)
                throw new Exception("Error reading label from harvest section.");
            SetLabel(label);
            SetUserInputId(id);

            instring = inFile.ReadLine();
            sarray = instring.Split('#');
            itsManagementAreaId = int.Parse(sarray[0]);
            if (itsManagementAreaId != 1)
                throw new Exception("Error reading management area id from harvest section.");

            instring = inFile.ReadLine();
            sarray = instring.Split('#');
            itsRotationAge = int.Parse(sarray[0]);
            if (itsRotationAge != 1)
                throw new Exception("Error reading rotation age from harvest section.");

            instring = inFile.ReadLine();
            sarray = instring.Split('#');
            rankAlgorithmId = int.Parse(sarray[0]);
            if (rankAlgorithmId != 1)
                throw new Exception("Error reading rank algorithm from harvest section.");

            readCustomization1(inFile);

            itsRemovalMask = new SiteRemovalMask();
            if (label != "Basal_Area_Thinning")
            {
                readCustomization2(inFile);
            }


        }

        protected virtual void readCustomization1(StreamReader infile)
        {

        }

        protected virtual void readCustomization2(StreamReader infile)
        {

        }

        protected virtual int isHarvestDone()
        {
            return 0;
        }

        protected virtual int harvestStand(Stand stand)
        {
            return 0;
        }

        protected void writeReport(StreamWriter outfile)
        {

        }
    }
}