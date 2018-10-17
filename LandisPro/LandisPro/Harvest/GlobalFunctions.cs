using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LandisPro.Harvest
{
    class GlobalFunctions
    {
        public static int HEventsmode = 0;

        public static void HarvestPass(Sites psi, SpeciesAttrs psa)
        {
            BoundedPocketStandHarvester.pspeciesAttrs = psa;
        }

        public static void HarvestPassCurrentDecade(int cd)
        {
            BoundedPocketStandHarvester.currentDecade = cd;
        }

        public static void HarvestPassInit(Sites psi, int isp, string stroutputdir, string strHarvestInitFile, PDP pdp)
        { 
            string harvestFile = "";
            string strstandImgMapFile = "";
            string strmgtAreaImgMapFile = "";
            string strharvestOutputFile1 = "";
            string strharvestOutputFile2 = "";
            string outputdir;

            outputdir = string.Format("{0}/{1}", stroutputdir, "Harvest");
            Directory.CreateDirectory(outputdir);
            if (!Directory.Exists(outputdir))
            {
                Console.WriteLine("Harvest: Can't create the direcory");
                Console.Read();
            }

            int timber;
            int harvest;


            BoundedPocketStandHarvester.pCoresites = psi;
            BoundedPocketStandHarvester.giRow = BoundedPocketStandHarvester.pCoresites.numRows();
            BoundedPocketStandHarvester.giCol = BoundedPocketStandHarvester.pCoresites.numColumns();

            BoundedPocketStandHarvester.my_pPDP = pdp;
            BoundedPocketStandHarvester.numberOfSpecies = isp;

            StreamReader pfHarvest = new StreamReader(strHarvestInitFile);

            string instring;
            string[] sarray;
            instring = pfHarvest.ReadLine();
            timber = int.Parse(instring);
            instring = pfHarvest.ReadLine();
            harvest = int.Parse(instring);
            if (harvest != 0)
            {
                instring = pfHarvest.ReadLine();
                sarray = instring.Split('#');
                BoundedPocketStandHarvester.iParamstandAdjacencyFlag = int.Parse(sarray[0]);
                instring = pfHarvest.ReadLine();
                sarray = instring.Split('#');
                BoundedPocketStandHarvester.iParamharvestDecadeSpan = int.Parse(sarray[0]);
                instring = pfHarvest.ReadLine();
                sarray = instring.Split('#');
                BoundedPocketStandHarvester.fParamharvestThreshold = double.Parse(sarray[0]);

                instring = pfHarvest.ReadLine();
                sarray = instring.Split('#');
                harvestFile = sarray[0].Substring(0, sarray[0].Length - 1);

                instring = pfHarvest.ReadLine();
                sarray = instring.Split('#');
                strstandImgMapFile = sarray[0].Substring(0, sarray[0].Length - 1);

                instring = pfHarvest.ReadLine();
                sarray = instring.Split('#');
                strmgtAreaImgMapFile = sarray[0].Substring(0, sarray[0].Length - 1);

                instring = pfHarvest.ReadLine();
                sarray = instring.Split('#');
                strharvestOutputFile1 = sarray[0].Substring(0, sarray[0].Length - 1);

                instring = pfHarvest.ReadLine();
                sarray = instring.Split('#');
                strharvestOutputFile2 = sarray[0].Substring(0, sarray[0].Length - 1);
            }

            pfHarvest.Close();

            StreamReader haFile = new StreamReader(harvestFile);

            Console.WriteLine("Build visitation Map");
            BoundedPocketStandHarvester.visitationMap.dim((uint)BoundedPocketStandHarvester.giRow, (uint)BoundedPocketStandHarvester.giCol);
            BoundedPocketStandHarvester.visitationMap.fill(0);

            int filenamelength;
            string fileextensive;

            BoundedPocketStandHarvester.standMap.readImg(strstandImgMapFile, BoundedPocketStandHarvester.giRow, BoundedPocketStandHarvester.giCol);
            BoundedPocketStandHarvester.managementAreaMap.readImg(strmgtAreaImgMapFile, BoundedPocketStandHarvester.giRow, BoundedPocketStandHarvester.giCol);

            BoundedPocketStandHarvester.pstands = new Stands();

            BoundedPocketStandHarvester.pstands.construct();

            BoundedPocketStandHarvester.managementAreas.construct();

            HEventsmode = BoundedPocketStandHarvester.harvestEvents.Read(haFile);

            //BoundedPocketStandHarvester.managementAreaMap.freeMAPdata();

            //BoundedPocketStandHarvester.pHarvestsites = new HARVESTSites(BoundedPocketStandHarvester.giRow, BoundedPocketStandHarvester.giCol);

            string str;
            str = string.Format("{0}/{1}", outputdir, strharvestOutputFile1);
            BoundedPocketStandHarvester.harvestOutputFile1 = new StreamWriter(str);
            str = string.Format("{0}/{1}", outputdir, strharvestOutputFile2);
            BoundedPocketStandHarvester.harvestOutputFile2 = new StreamWriter(str);

            haFile.Close();
        }

        public static int inBounds(int r, int c)
        {
            if (r >= 1 && r <= BoundedPocketStandHarvester.giRow && c >= 1 && c <= BoundedPocketStandHarvester.giCol)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}
