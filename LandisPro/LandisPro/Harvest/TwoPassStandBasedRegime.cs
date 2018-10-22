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
        protected int itsEntryDecade;
        protected int itsReentryDecade;
        protected SiteRemovalMask itsReentryRemovalMask;
        protected int itsTargetCut;
        protected HarvestPath itsPath = new HarvestPath();

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
