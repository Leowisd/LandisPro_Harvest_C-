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

            BoundedPocketStandHarvester.managementAreaMap.freeMAPdata();

            BoundedPocketStandHarvester.pHarvestsites = new HARVESTSites(BoundedPocketStandHarvester.giRow, BoundedPocketStandHarvester.giCol);

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


        private static int gasdev_iset = 0;
        private static float gasdev_gset;
        private float gasdev()
        {
            float fac;
            float rsq;
            float v1;
            float v2;
            if (gasdev_iset == 0)
            {
                do
                {
                    v1 = (float)2.0 * system1.frand() - (float)1.0;
                    v2 = (float)2.0 * system1.frand() - (float)1.0;
                    rsq = v1 * v1 + v2 * v2;
                } while (rsq >= 1.0 || rsq == 0.0F);
                fac = (float)Math.Sqrt((float)-2.0 * Math.Log(rsq) / rsq);
                gasdev_gset = v1 * fac;
                gasdev_iset = 1;
                return v2 * fac;
            }
            else
            {
                gasdev_iset = 0;
                return gasdev_gset;
            }
        }


        public static double gasdev(double mean, double sd)
        {
            double gset;
            gset = gasdev() * sd + mean;
            return gset;
        }

        public static void setUpdateFlags(int r, int c)
        {

            int sid;
            int mid;
            int tempMid = 0;

            BoundedPocketStandHarvester.pHarvestsites.BefStChg(r, c); //Add By Qia on Nov 07 2008
            BoundedPocketStandHarvester.pHarvestsites[r, c].setUpdateFlag();
            BoundedPocketStandHarvester.pHarvestsites.AftStChg(r, c); //Add By Qia on Nov 07 2008

            if ((sid = BoundedPocketStandHarvester.standMap.getvalue32out((uint)r, (uint)c)) > 0) //changed By Qia on Nov 4 2008
            {
                BoundedPocketStandHarvester.pstands[sid].setUpdateFlag();
                tempMid = BoundedPocketStandHarvester.pstands[sid].getManagementAreaId();
            }

            if (sid > 0 && tempMid > 0)
            {
                BoundedPocketStandHarvester.managementAreas[tempMid].setUpdateFlag();
            }
        }

        public static void HarvestprocessEvents(int itr)
        {
            BoundedPocketStandHarvester.harvestEvents.ProcessEvent(itr);
        }

        public static bool canBeHarvested(Ldpoint thePoint)
        {
            return BoundedPocketStandHarvester.pHarvestsites[thePoint.y, thePoint.x].canBeHarvested(thePoint.y, thePoint.x);
        }

    }
}
