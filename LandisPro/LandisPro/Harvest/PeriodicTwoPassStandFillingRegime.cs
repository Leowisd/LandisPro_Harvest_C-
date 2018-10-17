using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandisPro.Harvest
{
    class PeriodicTwoPassStandFillingRegime: HarvestRegime
    {
        public enum Enum
        {
            START,
            INITIALENTRYPENDING,
            INITIALENTRYREADY,
            ENTRYPENDING,
            ENTRYREADY,
            REENTRYPENDING,
            REENTRYREADY,
            DONE
        }
        private Enum itsState;
        private int itsEntryDecade;
        private int itsReentryDecade;
        private int itsReturnInterval;
        private SiteRemovalMask itsReentryRemovalMask;
        private int itsTargetCut;
        private HarvestPath itsPath;

        public PeriodicTwoPassStandFillingRegime()
        {
            itsState = Enum.START;
            itsEntryDecade = 0;
            itsReentryDecade = 0;
            itsReturnInterval = 0;
            itsReentryRemovalMask = new SiteRemovalMask();
            itsTargetCut = 0;
        }
        public override void Read(StreamReader infile)
        {
            if (itsState == Enum.START)
            {
                base.Read(infile);
                itsState = Enum.INITIALENTRYPENDING;
            }
            else
                throw new Exception("Illegal call to PeriodicTwoPassStandFillingRegime::read.");
        }

    }
}
