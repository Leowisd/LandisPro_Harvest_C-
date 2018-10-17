using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandisPro.Harvest
{
    class GroupSelectionRegime70: HarvestRegime
    {
        public enum Enum
        {
            START,
            TOHARVEST,
            TOREENTRY,
            DONE,
            PENDING
        }
        private Enum itsState;
        private float targetProportion;
        private int standProportionDenominator;
        private int rotationLength;
        private int itsEntryDecade;
        private int itsRepeatInterval;
        private int itsTargetCut;
        private int SitesCut;
        private double itsStandProportion;
        private double itsMeanGroupSize;
        private double itsStandardDeviation;
        private int itsTotalNumberOfStands;
        private List<int> itsStands = new List<int>();
        private double targetProportion_copy; //<Add By Qia on June 02 2012>
        private int standProportionDenominator_copy;
        private int rotationLength_copy;
        private int itsTargetCut_copy;
        private int SitesCut_copy;
        private double itsStandProportion_copy;
        private double itsMeanGroupSize_copy;
        private double itsStandardDeviation_copy;
        private int itsTotalNumberOfStands_copy; //</Add By Qia on June 02 2012>

        public GroupSelectionRegime70()
        {
            itsState = Enum.START;
            itsEntryDecade = 0;
            itsRepeatInterval = 0;
            itsTargetCut = 0;
            itsStandProportion = 0;      
            itsMeanGroupSize = 0;
            itsStandardDeviation = 0;
            itsTotalNumberOfStands = 0;
        }

        public override void Read(StreamReader infile)
        {

            if (itsState == Enum.START)
            {
                base.Read(infile);
                itsState = Enum.PENDING;
            }
            else
                throw new Exception("Illegal call to GroupSelectionRegime70::read.");
        }

    }
}
