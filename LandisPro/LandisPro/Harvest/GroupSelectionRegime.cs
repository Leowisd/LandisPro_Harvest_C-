using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandisPro.Harvest
{
    class GroupSelectionRegime: HarvestRegime
    {
        private enum Enum1
        {
            START,
            ENTRYPENDING,
            ENTRYREADY,
            REENTRYPENDING,
            REENTRYREADY
        }
        private Enum1 itsState;
        private int itsEntryDecade;
        private int itsReentryInterval;
        private int itsTargetCut;
        private double itsStandProportion;
        private double itsMeanGroupSize;
        private double itsStandardDeviation;
        private int itsTotalNumberOfStands;
        private List<int> itsStands = new List<int>();

        public GroupSelectionRegime()
        {
            itsState = Enum1.START;
            itsEntryDecade = 0;
            itsReentryInterval = 0;
            itsTargetCut = 0;
            itsStandProportion = 0;
            itsMeanGroupSize = 0;
            itsStandardDeviation = 0;
            itsTotalNumberOfStands = 0;
        }

        public override void Read(StreamReader inFile)
        {
            if (itsState == Enum1.START)
            {
                base.Read(inFile);
                itsState = Enum1.ENTRYPENDING;
            }
            else
            {
                throw new Exception("Illegal call to GroupSelectionRegime.read");
            }

        }

        
    }
}