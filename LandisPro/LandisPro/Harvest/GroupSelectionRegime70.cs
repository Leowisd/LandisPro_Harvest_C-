using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandisPro.Harvest
{
    class GroupSelectionRegime70: HarvestRegime
    {
        public enum Enum
        {
            START,
            TOHARVEST,
            TOREENTRY,
            DONE,
            PENDING
        }
        private Enum itsState;
        private double targetProportion;
        private int standProportionDenominator;
        private int rotationLength;
        private int itsEntryDecade;
        private int itsRepeatInterval;
        private int itsTargetCut;
        private int SitesCut;
        private double itsStandProportion;
        private double itsMeanGroupSize;
        private double itsStandardDeviation;
        private int itsTotalNumberOfStands;
        private List<int> itsStands = new List<int>();
        private double targetProportion_copy; //<Add By Qia on June 02 2012>
        private int standProportionDenominator_copy;
        private int rotationLength_copy;
        private int itsTargetCut_copy;
        private int SitesCut_copy;
        private double itsStandProportion_copy;
        private double itsMeanGroupSize_copy;
        private double itsStandardDeviation_copy;
        private int itsTotalNumberOfStands_copy; //</Add By Qia on June 02 2012>
        public int total_reentry_event_instances;
        public GroupSelectionRegime70_reentry_event[] GroupSelectionRegime70_reentry_event_instances;


        public GroupSelectionRegime70()
        {
            itsState = Enum.START;
            itsEntryDecade = 0;
            itsRepeatInterval = 0;
            itsTargetCut = 0;
            itsStandProportion = 0;      
            itsMeanGroupSize = 0;
            itsStandardDeviation = 0;
            itsTotalNumberOfStands = 0;
        }

        public override void Read(StreamReader infile)
        {

            if (itsState == Enum.START)
            {
                base.Read(infile);
                itsState = Enum.PENDING;
            }
            else
                throw new Exception("Illegal call to GroupSelectionRegime70::read.");
        }

        public override void readCustomization1(StreamReader infile)
        {
            string instring;
            string[] sarray;
            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading entry decade from harvest section.");
            sarray = instring.Split('#');
            itsEntryDecade = int.Parse(sarray[0]);

            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading reentry interval from harvest section.");
            sarray = instring.Split('#');
            itsRepeatInterval = int.Parse(sarray[0]);

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

            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading standard deviation from harvest section.");
            sarray = instring.Split('#');
            itsStandardDeviation = double.Parse(sarray[0]);

            itsStandProportion = 1.0 / standProportionDenominator;
            itsTargetCut = (int)(BoundedPocketStandHarvester.managementAreas[getManagementAreaId()].numberofActiveSites() * targetProportion);
            rotationLength = (int) (itsRepeatInterval * standProportionDenominator);
            SitesCut = 0;
            setDuration(rotationLength);

            if (BoundedPocketStandHarvester.pCoresites.specNum>200)
                throw new Exception("Too many species for harvest");
            instring = infile.ReadLine();
            instring = infile.ReadLine();
            instring = infile.ReadLine();
            for (int i = 0; i < BoundedPocketStandHarvester.pCoresites.specNum; i++)
            {
                instring = infile.ReadLine();
                sarray = instring.Split(' ');

                BoundedPocketStandHarvester.pCoresites.flag_cut_GROUP_CUT[i] = int.Parse(sarray[0]);
                BoundedPocketStandHarvester.pCoresites.flag_plant_GROUP_CUT[i] = int.Parse(sarray[1]);
                BoundedPocketStandHarvester.pCoresites.num_TreePlant_GROUP_CUT[i] = int.Parse(sarray[2]);
            }
            copy_initial_parameters(); //<Add By Qia on May 29 2012>

            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading standard deviation from harvest section.");
            sarray = instring.Split('#');
            total_reentry_event_instances = int.Parse(sarray[0]);

            instring = infile.ReadLine();

            if (total_reentry_event_instances > 0)
            {

                GroupSelectionRegime70_reentry_event_instances = new GroupSelectionRegime70_reentry_event[total_reentry_event_instances];
            }
            for (int ii = 0; ii < total_reentry_event_instances; ii++)
            {

                GroupSelectionRegime70_reentry_event_instances[ii].GroupSelectionRegime70_load_reentry_parameters(infile);
            }

        }

        public override void readCustomization2(StreamReader infile)
        {

        }

        public void copy_initial_parameters()
        {
            itsTargetCut_copy = itsTargetCut;
            SitesCut_copy = SitesCut;
            itsStandProportion_copy = itsStandProportion;
            itsMeanGroupSize_copy = itsMeanGroupSize;
            itsStandardDeviation_copy = itsStandardDeviation;
            itsTotalNumberOfStands_copy = itsTotalNumberOfStands;
            targetProportion_copy = targetProportion;
            standProportionDenominator_copy = standProportionDenominator;
            rotationLength_copy = standProportionDenominator;
            for (int i = 0; i < 200; i++)
            {
                BoundedPocketStandHarvester.pCoresites.flag_cut_GROUP_CUT_copy[i] = BoundedPocketStandHarvester.pCoresites.flag_cut_GROUP_CUT[i];
                BoundedPocketStandHarvester.pCoresites.flag_plant_GROUP_CUT_copy[i] = BoundedPocketStandHarvester.pCoresites.flag_plant_GROUP_CUT[i];
                BoundedPocketStandHarvester.pCoresites.num_TreePlant_GROUP_CUT_copy[i] = BoundedPocketStandHarvester.pCoresites.num_TreePlant_GROUP_CUT[i];
            }
        }



    }

    public class GroupSelectionRegime70_reentry_event
    {
        public int[] flag_cut_GROUP_CUT = new int[200];
        public int[] flag_plant_GROUP_CUT = new int[200];
        public int[] num_TreePlant_GROUP_CUT = new int[200];
        public int itsReentryInteval;
        public int itsEntryDecade;
        public int itsRepeatInterval;
        public int itsTargetCut;
        public int SitesCut;
        public double itsStandProportion;
        public double itsMeanGroupSize;
        public double itsStandardDeviation;
        public int itsTotalNumberOfStands;
        public double targetProportion;
        public int standProportionDenominator;
        public int rotationLength;

        public void GroupSelectionRegime70_load_reentry_parameters(StreamReader infile)
        {
            string insting;
            string[] sarray;

            insting = infile.ReadLine();
            insting = infile.ReadLine();
            insting = infile.ReadLine();
            insting = infile.ReadLine();
            insting = infile.ReadLine();

            insting = infile.ReadLine();
            sarray = insting.Split('#');
            itsReentryInteval = int.Parse(sarray[0]);

            insting = infile.ReadLine();
            sarray = insting.Split('#');
            itsRepeatInterval = int.Parse(sarray[0]);

            insting = infile.ReadLine();
            sarray = insting.Split('#');
            targetProportion = double.Parse(sarray[0]);

            insting = infile.ReadLine();
            sarray = insting.Split('#');
            standProportionDenominator = int.Parse(sarray[0]);

            insting = infile.ReadLine();
            sarray = insting.Split('#');
            itsMeanGroupSize = double.Parse(sarray[0]);

            insting = infile.ReadLine();
            sarray = insting.Split('#');
            itsStandardDeviation = double.Parse(sarray[0]);

            if (BoundedPocketStandHarvester.pCoresites.specNum > 200)
                throw new Exception("Two many species for harvest.");

            insting = infile.ReadLine();
            insting = infile.ReadLine();
            insting = infile.ReadLine();
            for (int i = 0; i < BoundedPocketStandHarvester.pCoresites.specNum; i++)
            {
                insting = infile.ReadLine();
                sarray = insting.Split(' ');

                flag_cut_GROUP_CUT[i] = int.Parse(sarray[0]);
                flag_plant_GROUP_CUT[i] = int.Parse(sarray[1]);
                num_TreePlant_GROUP_CUT[i] = int.Parse(sarray[2]);
            }

            insting = infile.ReadLine();
        }

    }
}
