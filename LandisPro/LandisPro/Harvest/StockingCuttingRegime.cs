using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandisPro.Harvest
{
    class StockingCuttingRegime: HarvestRegime
    {
        public int StandsCut;
        public int SitesCut;
        public int total_reentry_event_instances;
        public StockingCuttingRegime_reentry_event[] StockingCuttingRegime_reentry_event_instances;
        public enum Enum
        {
            START,
            TOHARVEST,
            TOREENTRY,
            DONE,
            PENDING
        }
        private Enum itsState;
        private int itsEntryDecade;
        private int itsFinalDecade;
        private int itsRepeatInterval;
        private int itsTargetCut;
        private double targetProportion;
        private double TargetStocking; //Aug 03 2009
        private double Mininum_Stocking; //May 26 2011
        private int Small0_Large1; //May 26 2011
        private int[] speciesOrder = new int[200]; //May 26 2011
        private int[] flag_plant = new int[200];
        private int[] flag_cut = new int[200];
        private int[] num_TreePlant = new int[200];
        private List<int> itsStands = new List<int>();
        //<Add By Qia on June 02 2012>
        private int itsTargetCut_copy;
        private double targetProportion_copy;
        private double TargetStocking_copy; //Aug 03 2009
        private double Mininum_Stocking_copy; //May 26 2011
        private int Small0_Large1_copy; //May 26 2011
        private int[] speciesOrder_copy = new int[200]; //May 26 2011
        private int[] flag_plant_copy = new int[200];
        private int[] flag_cut_copy = new int[200];
        private int[] num_TreePlant_copy = new int[200];

        public StockingCuttingRegime()
        {
            itsState = Enum.START;
            itsEntryDecade = 0;
            itsFinalDecade = 0;
            itsRepeatInterval = 0;
            itsTargetCut = 0;
        }

        public override void Read(StreamReader infile)
        {
            if (itsState == Enum.START)
            {
                base.Read(infile);
                itsState = Enum.PENDING;
            }
            else
                throw new Exception("Illegal call to HarvestVolumeFittingRegime::read.");
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
                throw new Exception("Error reading reentry from harvest section.");
            sarray = instring.Split('#');
            itsRepeatInterval = int.Parse(sarray[0]);

            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading TargetVolume from harvest section.");
            sarray = instring.Split('#');
            Mininum_Stocking = double.Parse(sarray[0]);

            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading TargetVolume from harvest section.");
            sarray = instring.Split('#');
            Small0_Large1 = int.Parse(sarray[0]);

            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading target proportion from harvest section.");
            sarray = instring.Split('#');
            targetProportion = double.Parse(sarray[0]);

            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading TargetVolume from harvest section.");
            sarray = instring.Split('#');
            TargetStocking = double.Parse(sarray[0]);

            itsTargetCut = (int)(BoundedPocketStandHarvester.managementAreas[getManagementAreaId()].numberofActiveSites() * targetProportion);

            if (BoundedPocketStandHarvester.pCoresites.specNum > 200)
                throw new Exception("Two many species for harvest.");

            instring = infile.ReadLine();
            instring = infile.ReadLine();
            instring = infile.ReadLine();
            for (int i = 0; i < BoundedPocketStandHarvester.pCoresites.specNum; i++)
            {
                int temp_spec_order;
                instring = infile.ReadLine();
                sarray = instring.Split(' ');

                temp_spec_order = int.Parse(sarray[0]);
                speciesOrder[temp_spec_order - 1] = i + 1;
                flag_cut[i] = int.Parse(sarray[1]);
                flag_plant[i] = int.Parse(sarray[2]);
                num_TreePlant[i] = int.Parse(sarray[3]);
            }

            copy_initial_parameters();

            total_reentry_event_instances = 0;
            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading standard deviation from harvest section.");
            sarray = instring.Split('#');
            total_reentry_event_instances = int.Parse(sarray[0]);

            instring = infile.ReadLine();

            if (total_reentry_event_instances > 0)
            {
                StockingCuttingRegime_reentry_event_instances = new StockingCuttingRegime_reentry_event[total_reentry_event_instances];
            }
            for (int ii = 0; ii < total_reentry_event_instances; ii++)
            {
                StockingCuttingRegime_reentry_event_instances[ii] = new StockingCuttingRegime_reentry_event();
                StockingCuttingRegime_reentry_event_instances[ii].StockingCuttingRegime_load_reentry_parameters(infile);
            }
            itsTargetCut = (int)(BoundedPocketStandHarvester.managementAreas[getManagementAreaId()].numberofActiveSites() * targetProportion);
            StandsCut = 0;
            SitesCut = 0;
        }

        public override void readCustomization2(StreamReader infile)
        {
            setDuration(1);
        }

        public void copy_initial_parameters()
        {
            if (1 == 1)
            { // initial or repeat
                itsTargetCut_copy = itsTargetCut;
                targetProportion_copy = targetProportion;
                TargetStocking_copy = TargetStocking;
                Mininum_Stocking_copy = Mininum_Stocking;
                Small0_Large1_copy = Small0_Large1;
                for (int i = 0; i < 200; i++)
                {
                    speciesOrder_copy[i] = speciesOrder[i];
                    flag_plant_copy[i] = flag_plant[i];
                    flag_cut_copy[i] = flag_cut[i];
                    num_TreePlant_copy[i] = num_TreePlant[i];
                }
            }
        }

    }

    public class StockingCuttingRegime_reentry_event
    {
        public int itsReentryInteval;
        public int itsEntryDecade;
        public int itsFinalDecade;
        public int itsTargetCut;
        public double targetProportion;
        public double TargetStocking; //Aug 03 2009
        public double Mininum_Stocking; //May 26 2011
        public int Small0_Large1; //May 26 2011
        public int[] speciesOrder = new int[200]; //May 26 2011
        public int[] flag_plant = new int[200];
        public int[] flag_cut = new int[200];
        public int[] num_TreePlant = new int[200];

        public void StockingCuttingRegime_load_reentry_parameters(StreamReader infile)
        {
            string instring;
            string[] sarray;

            instring = infile.ReadLine();
            instring = infile.ReadLine();
            instring = infile.ReadLine();
            instring = infile.ReadLine();
            instring = infile.ReadLine();


            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading entry decade from harvest section.");
            sarray = instring.Split('#');
            itsReentryInteval = int.Parse(sarray[0]);

            int itsRepeatInterval;
            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading reentry from harvest section.");
            sarray = instring.Split('#');
            itsRepeatInterval = int.Parse(sarray[0]);

            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading TargetVolume from harvest section.");
            sarray = instring.Split('#');
            Mininum_Stocking = double.Parse(sarray[0]);

            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading TargetVolume from harvest section.");
            sarray = instring.Split('#');
            Small0_Large1 = int.Parse(sarray[0]);

            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading target proportion from harvest section.");
            sarray = instring.Split('#');
            targetProportion = double.Parse(sarray[0]);

            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading TargetVolume from harvest section.");
            sarray = instring.Split('#');
            TargetStocking = double.Parse(sarray[0]);          

            if (BoundedPocketStandHarvester.pCoresites.specNum > 200)
                throw new Exception("Two many species for harvest.");

            instring = infile.ReadLine();
            instring = infile.ReadLine();
            instring = infile.ReadLine();
            for (int i = 0; i < BoundedPocketStandHarvester.pCoresites.specNum; i++)
            {
                int temp_spec_order;
                instring = infile.ReadLine();
                sarray = instring.Split(' ');

                temp_spec_order = int.Parse(sarray[0]);
                speciesOrder[temp_spec_order - 1] = i + 1;
                flag_cut[i] = int.Parse(sarray[1]);
                flag_plant[i] = int.Parse(sarray[2]);
                num_TreePlant[i] = int.Parse(sarray[3]);
            }
            instring = infile.ReadLine();
        }

    }
}
