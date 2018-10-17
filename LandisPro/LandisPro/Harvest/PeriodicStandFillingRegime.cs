using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandisPro.Harvest
{
    class PeriodicStandFillingRegime: HarvestRegime
    {
        public enum Enum
        {
            START,
            ENTRYPENDING,
            ENTRYREADY,
            DONE
        }
        private Enum itsState;
        private int itsEntryDecade;
        private int itsFinalDecade;
        private int itsReentryInterval;
        private int itsTargetCut;

        public PeriodicStandFillingRegime()
        {
            itsState = Enum.START;
            itsEntryDecade = 0;
            itsFinalDecade = 0;
            itsReentryInterval = 0;
            itsTargetCut = 0;
        }

        public override void Read(StreamReader infile)
        {
            if (itsState == Enum.START)
            {
                base.Read(infile);
                itsState = Enum.ENTRYPENDING;
            }
            else
                throw new Exception("Illegal call to PeriodicStandFillingRegime::read.");
        }

    }
}
