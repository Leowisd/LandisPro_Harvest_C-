using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandisPro.Harvest
{
    class HarvestVolumeFittingRegime: HarvestRegime
    {
        public enum Enum
        {
            START,
            TOHARVEST,
            TOREENTRY,
            DONE,
            PENDING
        }
        public  Enum itsState;
        public int StandsCut;
	    public int SitesCut;
        public  int itsEntryDecade;
        public  int itsFinalDecade;
        public  int itsRepeatInterval;
        public  int itsTargetCut;
        public  double targetProportion;
        public  double TargetVolume; //Aug 03 2009
        public  double Mininum_BA; //May 26 2011
        public  int Small0_Large1; //May 26 2011
        public  int total_reentry_event_instances;
        public  int[] speciesOrder = new int[200]; //May 26 2011
        public  int[] flag_plant = new int[200];
        public  int[] flag_cut = new int[200];
        public  int[] num_TreePlant = new int[200];
        public  List<int> itsStands = new List<int>();
        //<Add By Qia on June 02 2012>
        public  int itsTargetCut_copy;    
        public  double targetProportion_copy;
        public  double TargetVolume_copy; //Aug 03 2009
        public  double Mininum_BA_copy; //May 26 2011
        public  int Small0_Large1_copy; //May 26 2011
        public  int[] speciesOrder_copy = new int[200]; //May 26 2011
        public  int[] flag_plant_copy = new int[200];
        public  int[] flag_cut_copy = new int[200];
        public  int[] num_TreePlant_copy = new int[200];
        //</Add By Qia on June 02 2012>
        public HarvestVolumeFittingRegime_reentry_event[] HarvestVolumeFittingRegime_reentry_event_instances;

        public HarvestVolumeFittingRegime()
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
            Mininum_BA = double.Parse(sarray[0]);

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
            TargetVolume = double.Parse(sarray[0]);

            itsTargetCut =
                (int) (BoundedPocketStandHarvester.managementAreas[getManagementAreaId()].numberofActiveSites() *
                       targetProportion);

            if (BoundedPocketStandHarvester.pCoresites.specNum>200)
                throw new Exception("Two many species for harvest.");


            instring = infile.ReadLine();
            instring = infile.ReadLine();
            instring = infile.ReadLine();
            for (int i = 0; i < BoundedPocketStandHarvester.pCoresites.specNum;i++)
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
                HarvestVolumeFittingRegime_reentry_event_instances = new HarvestVolumeFittingRegime_reentry_event[total_reentry_event_instances];
            }

            for (int ii = 0; ii < total_reentry_event_instances; ii++)
            {
                HarvestVolumeFittingRegime_reentry_event_instances[ii] = new HarvestVolumeFittingRegime_reentry_event();
                HarvestVolumeFittingRegime_reentry_event_instances[ii].HarvestVolumeFittingRegime_load_reentry_parameters(infile);
            }

            itsTargetCut =
                (int) (BoundedPocketStandHarvester.managementAreas[getManagementAreaId()].numberofActiveSites() *
                       targetProportion);
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
                TargetVolume_copy = TargetVolume;
                Mininum_BA_copy = Mininum_BA;
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

    public class HarvestVolumeFittingRegime_reentry_event
    {
        public int itsReentryInteval;
        public int itsEntryDecade;
        public int itsFinalDecade;
        public int itsTargetCut;
        public double targetProportion;
        public double TargetVolume; //Aug 03 2009
        public double Mininum_BA; //May 26 2011
        public int Small0_Large1; //May 26 2011
        public int[] speciesOrder = new int[200]; //May 26 2011
        public int[] flag_plant = new int[200];
        public int[] flag_cut = new int[200];
        public int[] num_TreePlant = new int[200];

        public void HarvestVolumeFittingRegime_load_reentry_parameters(StreamReader infile)
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
            Mininum_BA = double.Parse(sarray[0]);

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
            TargetVolume = double.Parse(sarray[0]);

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
