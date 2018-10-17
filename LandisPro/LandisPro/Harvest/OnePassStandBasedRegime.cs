using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandisPro.Harvest
{
    class OnePassStandBasedRegime: HarvestRegime
    {
        public enum Enum
        {
            START,
            ENTRYPENDING,
            ENTRYREADY,
            DONE
        }
        protected Enum itsState;
        protected int itsEntryDecade;
        protected int itsTargetCut;

        public OnePassStandBasedRegime()
        {
            itsState = Enum.START;
            itsEntryDecade = 0;
            itsTargetCut = 0;
        }

        ~OnePassStandBasedRegime()
        {
        }

    }
}
