using System;
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

            if ((instring = inFile.ReadLine())==null)
                throw new Exception("Error reading management area id from harvest section.");
            sarray = instring.Split('#');
            itsManagementAreaId = int.Parse(sarray[0]);

            if ((instring = inFile.ReadLine())==null)
                throw new Exception("Error reading rotation age from harvest section.");
            sarray = instring.Split('#');
            itsRotationAge = int.Parse(sarray[0]);

            if ((instring = inFile.ReadLine())==null)
                throw new Exception("Error reading rank algorithm from harvest section.");
            sarray = instring.Split('#');
            rankAlgorithmId = int.Parse(sarray[0]);

            readCustomization1(inFile);

            itsRemovalMask = new SiteRemovalMask();
            if (label != "Basal_Area_Thinning")
            {
                readCustomization2(inFile);
            }

            switch (rankAlgorithmId)
            {
                /*
                 * Accroding to the input data file HarvestparameterDebug.dat from Jacob, which used all possible functions, these
                 * methods may not be used in current version any longer.
                 */
                //case 6:  // may not used
                //    //itsRankAlgorithm = new OldestRank(itsManagementAreaId, itsRotationAge);
                //    break;
                //case 7: // may not used
                //    //itsRankAlgorithm = new EconomicImportanceRank(itsManagementAreaId, itsRotationAge, itsRemovalMask);
                //    break;
                //case 4: // may not used
                //    //itsRankAlgorithm = new RegulateDistributionRank(itsManagementAreaId, itsRotationAge);
                //    break;
                ///* 29-SEP-99 */
                //case 5: // may not used
                //    //itsRankAlgorithm = new RelativeOldestRank(itsManagementAreaId, itsRotationAge, itsRemovalMask);
                //    break;
                case 1:
                    itsRankAlgorithm = new RandomRank(itsManagementAreaId, itsRotationAge);
                    break;             
                case 2:
                    itsRankAlgorithm = new RankbyVolume(itsManagementAreaId, itsRotationAge);
                    break;
                case 3:
                    itsRankAlgorithm = new RankbyStocking(itsManagementAreaId, itsRotationAge); //Add By Qia on July 26 2012
                    break;
                /* -- END -- */
                default:
                    throw new Exception("Illegal rankAlgorithmId in HarvestRegime::read().");
            }
            itsRankAlgorithm.read(inFile);
        }

        public virtual void readCustomization1(StreamReader infile)
        {

        }

        public virtual void readCustomization2(StreamReader infile)
        {

        }

        public virtual int isHarvestDone()
        {
            return 0;
        }

        public virtual int harvestStand(Stand stand)
        {
            return 0;
        }

        protected void writeReport(StreamWriter outfile)
        {
            using (StreamWriter fp = File.AppendText(BoundedPocketStandHarvester.harvestOutputFile2_name))
            {
                //static int firstTime = 1; change to BoundedPocketStandHarvester.harvestWriteReportFirstTime
                if (BoundedPocketStandHarvester.harvestWriteReportFirstTime == 1)
                {
                    fp.Write("eventType\tdescription\tdecade\tmanagementArea\tsitesCut\tsumAgesCut");
                    for (int spp = 1; spp <= BoundedPocketStandHarvester.pspeciesAttrs.Number(); spp++)
                    {
                        fp.Write("\t{0}", BoundedPocketStandHarvester.pspeciesAttrs[spp].name);
                    }

                    fp.WriteLine();
                    BoundedPocketStandHarvester.harvestWriteReportFirstTime = 0;
                }

                fp.Write("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", GetSequentialId(), GetLabel(),
                BoundedPocketStandHarvester.currentDecade, getManagementAreaId(), itsReport.numberOfSitesCut(),
                itsReport.sumOfCohortsCut());

                for (int spp = 1; spp <= BoundedPocketStandHarvester.pspeciesAttrs.Number(); spp++)
                {
                    fp.Write("\t{0}", itsReport.sumOfCohortsCut(spp));
                }

                fp.WriteLine();
            }
        }

        public override void Harvest()
        {
            List<int> rankedList = new List<int>();
            Stand stand = new Stand();
            int standCut;
            itsRankAlgorithm.rankStands(ref rankedList);
            getReport().reset();
            // length of rankedList should be equal to length of  theLength;

            foreach (int it in rankedList)
                if (isHarvestDone() == 0)
                {
                    stand.Copy(BoundedPocketStandHarvester.pstands[it]);
                    if (stand.canBeHarvested() && (BoundedPocketStandHarvester.iParamstandAdjacencyFlag == 0 || !stand.neighborsWereRecentlyHarvested()))
                    {
                        standCut = harvestStand(BoundedPocketStandHarvester.pstands[it]);
                    }
                }
                else
                {
                    break;
                }

            writeReport(BoundedPocketStandHarvester.harvestOutputFile2);
        }
    }
}
