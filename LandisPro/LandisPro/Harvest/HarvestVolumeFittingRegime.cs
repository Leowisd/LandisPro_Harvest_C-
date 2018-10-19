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
        public  int itsEntryDecade;
        public  int itsFinalDecade;
        public  int itsRepeatInterval;
        public  int itsTargetCut;
        public  double targetProportion;
        public  double TargetVolume; //Aug 03 2009
        public  double Mininum_BA; //May 26 2011
        public  int Small0_Large1; //May 26 2011
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

        }

        public override void readCustomization2(StreamReader infile)
        {
            setDuration(1);
        }
    }
}
