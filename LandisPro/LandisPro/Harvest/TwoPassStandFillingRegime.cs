using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandisPro.Harvest
{

    //still used?
    class TwoPassStandFillingRegime: TwoPassStandBasedRegime
    {
        public override void Read(StreamReader infile)
        {
            if (itsState == Enum.START)
            {
                base.Read(infile);
                itsState = Enum.ENTRYPENDING;
            }
            else
                throw new Exception("Illegal call to TwoPassStandFillingRegime::read.");

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
                throw new Exception("Error reading reentry decade from harvest section.");
            sarray = instring.Split('#');
            itsReentryDecade = int.Parse(sarray[0]);

            itsReentryDecade = itsReentryDecade / BoundedPocketStandHarvester.pCoresites.TimeStep_Harvest;
            if (itsReentryDecade < BoundedPocketStandHarvester.pCoresites.TimeStep_Harvest)
                itsReentryDecade = 1;

            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading target cut from harvest section.");
            sarray = instring.Split('#');
            itsTargetCut = int.Parse(sarray[0]);

        }

        public override void readCustomization2(StreamReader infile)
        {
            itsReentryRemovalMask = new SiteRemovalMask();
            itsReentryRemovalMask.read(infile); //what's the correct format
            setDuration(1);
        }

    }
}
