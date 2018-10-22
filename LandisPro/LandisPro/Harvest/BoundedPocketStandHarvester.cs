using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LandisPro.Harvest
{
    class BoundedPocketStandHarvester
    {
        public static int currentDecade;
        public static SpeciesAttrs pspeciesAttrs;
        public static Sites pCoresites;
        public static int giRow;
        public static int giCol;
        public static PDP my_pPDP;
        public static int numberOfSpecies;
        public static int iParamstandAdjacencyFlag;
        public static int iParamharvestDecadeSpan;
        public static double fParamharvestThreshold;
        public static map16 visitationMap = new map16();
        public static map16 standMap = new map16();
        public static map16 managementAreaMap = new map16();
        public static Stands pstands;
        public static ManagementAreas managementAreas = new ManagementAreas();
        public static HarvestEventQueue harvestEvents = new HarvestEventQueue();
        //public static HARVESTSites pHarvestsites;
        public static StreamWriter harvestOutputFile1;
        public static StreamWriter harvestOutputFile2;
        public static ushort currentHarvestEventId;

        public HARVESTSites pHarvestsites;
    }
}
