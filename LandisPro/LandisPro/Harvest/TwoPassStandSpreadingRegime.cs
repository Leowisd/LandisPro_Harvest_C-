﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandisPro.Harvest
{
    class TwoPassStandSpreadingRegime: TwoPassStandBasedRegime
    {
        private double itsMeanStandCutSize;
        private double itsStandardDeviation;

        public TwoPassStandSpreadingRegime()
        {
            itsMeanStandCutSize = 0;
            itsStandardDeviation = 0;
        }

        public override void Read(StreamReader infile)
        {

            if (itsState == Enum.START)
            {
                base.Read(infile);
                itsState = Enum.ENTRYPENDING;
            }
            else
                throw new Exception("Illegal call to TwoPassStandSpreadingRegime::read.");
        }
    }
}