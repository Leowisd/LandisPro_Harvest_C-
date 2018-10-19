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

        public override void readCustomization1(StreamReader infile)
        {
            double targetProportion;
            int standProportionDenominator;
            int rotationLength;

            string instring;
            string[] sarray;
            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading entry decade from harvest section.");
            sarray = instring.Split('#');
            itsEntryDecade = int.Parse(sarray[0]);
            itsEntryDecade = itsEntryDecade / BoundedPocketStandHarvester.pCoresites.TimeStep_Harvest;
            if (itsEntryDecade < BoundedPocketStandHarvester.pCoresites.TimeStep_Harvest)
                itsEntryDecade = 1;

            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading reentry interval from harvest section.");
            sarray = instring.Split('#');
            itsReentryInterval = int.Parse(sarray[0]);
            itsReentryInterval = itsReentryInterval / BoundedPocketStandHarvester.pCoresites.TimeStep_Harvest;
            if (itsReentryInterval < BoundedPocketStandHarvester.pCoresites.TimeStep_Harvest)
                itsReentryInterval = 1;

            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading management area target proportion from harvest section.");
            sarray = instring.Split('#');
            targetProportion = double.Parse(sarray[0]);

            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading stand proportion denominator from harvest section.");
            sarray = instring.Split('#');
            standProportionDenominator = int.Parse(sarray[0]);

            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading mean group size from harvest section.");
            sarray = instring.Split('#');
            itsMeanGroupSize = double.Parse(sarray[0]);

            if ((instring = infile.ReadLine())==null)
                throw new Exception("Error reading standard deviation from harvest section.");
            sarray = instring.Split('#');
            itsStandardDeviation = double.Parse(sarray[0]);

            itsStandardDeviation = 1.0 / standProportionDenominator;
            itsTargetCut = (int)(BoundedPocketStandHarvester.managementAreas[getManagementAreaId()].numberOfStands() *
                            targetProportion);
            rotationLength = (int) (itsReentryInterval * standProportionDenominator);
            setDuration(rotationLength);
        }

        public override void readCustomization2(StreamReader infile)
        {

        }


    }
}