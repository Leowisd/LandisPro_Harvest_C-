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

    }
}
