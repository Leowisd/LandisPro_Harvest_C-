using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandisPro.Harvest
{
    class TwoPassStandBasedRegime: HarvestRegime
    {
        public enum Enum
        {
            START,
            ENTRYPENDING,
            ENTRYREADY,
            REENTRYPENDING,
            REENTRYREADY,
            DONE
        }
        protected Enum itsState;
        private int itsEntryDecade;
        private int itsReentryDecade;
        private SiteRemovalMask itsReentryRemovalMask;
        private int itsTargetCut;
        private HarvestPath itsPath = new HarvestPath();

        public TwoPassStandBasedRegime()
        {
            itsState = Enum.START;
            itsEntryDecade = 0;
            itsReentryDecade = 0;
            itsReentryRemovalMask = new SiteRemovalMask();
            itsTargetCut = 0;
        }

        ~TwoPassStandBasedRegime()
        {
            itsReentryRemovalMask = null;
        }


    }
}
