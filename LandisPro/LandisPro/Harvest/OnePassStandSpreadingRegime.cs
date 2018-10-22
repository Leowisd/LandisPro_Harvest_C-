using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandisPro.Harvest
{
    class OnePassStandSpreadingRegime: OnePassStandBasedRegime
    {
        private double itsMeanStandCutSize;
        private double itsStandardDeviation;

        public OnePassStandSpreadingRegime()
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
                throw new Exception("Illegal call to OnePassStandSpreadingRegime::read.");
        }

        public override void readCustomization1(StreamReader infile)
        {
            string instring;
            string[] sarray;

            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading entry decade from harvest section.");
            sarray = instring.Split('#');
            itsEntryDecade = int.Parse(sarray[0]);
            //<Add By Qia on April 08 2009>

            itsEntryDecade = itsEntryDecade / BoundedPocketStandHarvester.pCoresites.TimeStep_Harvest;

            if (itsEntryDecade < BoundedPocketStandHarvester.pCoresites.TimeStep_Harvest)
                itsEntryDecade = 1;

            //</Add By Qia on April 08 2009>
            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading target cut from harvest section.");
            sarray = instring.Split('#');
            itsTargetCut = int.Parse(sarray[0]);

            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading mean stand cut size from harvest section.");
            sarray = instring.Split('#');
            itsMeanStandCutSize = double.Parse(sarray[0]);

            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading standard deviation from harvest section.");
            sarray = instring.Split('#');
            itsStandardDeviation = double.Parse(sarray[0]);
        }

        public override void readCustomization2(StreamReader infile)
        {
            setDuration(1);
        }

    }
}
