using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OSGeo.GDAL;
using OSGeo.OSR;

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
                throw new Exception("Harvest: Can't create the direcory");
            }

            int timber;
            int harvest;


            BoundedPocketStandHarvester.pCoresites = psi;
            BoundedPocketStandHarvester.giRow = BoundedPocketStandHarvester.pCoresites.numRows();
            BoundedPocketStandHarvester.giCol = BoundedPocketStandHarvester.pCoresites.numColumns();

            BoundedPocketStandHarvester.m_pPDP = pdp;
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
            BoundedPocketStandHarvester.harvestOutputFile1_name = str;
            using (BoundedPocketStandHarvester.harvestOutputFile1 = new StreamWriter(str))
            {
            }
            
            str = string.Format("{0}/{1}", outputdir, strharvestOutputFile2);
            BoundedPocketStandHarvester.harvestOutputFile2_name = str;
            using (BoundedPocketStandHarvester.harvestOutputFile2 = new StreamWriter(str))
            {
            }

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
        private static float gasdev()
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

        private static int writeStandReport_firstTime = 1;
        public static void writeStandReport()
        {
            IntArray sumCut = new IntArray(BoundedPocketStandHarvester.pstands.number());
            int i;
            int j;
            int snr;
            int snc;
            using (StreamWriter fp = File.AppendText(BoundedPocketStandHarvester.harvestOutputFile1_name))
            {
                if (writeStandReport_firstTime == 1)
                {
                    fp.WriteLine("decade\tmanagementArea\tstand\tnumSitesHarvested");
                    writeStandReport_firstTime = 0;
                }

                snr = BoundedPocketStandHarvester.pCoresites.numRows();
                snc = BoundedPocketStandHarvester.pCoresites.numColumns();
                for (i = 1; i <= snr; i++)
                {
                    for (j = 1; j <= snc; j++)
                    {
                        //Console.WriteLine(BoundedPocketStandHarvester.pHarvestsites[i, j].getHarvestDecade()); => get wrong harvestDecade!
                        if (BoundedPocketStandHarvester.pHarvestsites[i, j].getHarvestDecade() == BoundedPocketStandHarvester.currentDecade)
                        {
                            sumCut[BoundedPocketStandHarvester.standMap.getvalue32out((uint) i, (uint) j)]++; //change by Qia on Nov 4 2008
                        }
                    }
                }

                for (i = 1; i <= BoundedPocketStandHarvester.pstands.number(); i++)
                {
                    if (sumCut[i] > 0)
                    {
                        fp.WriteLine("{0}\t{1}\t{2}\t{3}",BoundedPocketStandHarvester.currentDecade,BoundedPocketStandHarvester.pstands[i].getManagementAreaId(), i, sumCut[i]);
                    }
                }
            }
        }

        //public static void outputFileheader(StreamWriter fp)

        //{
        //    fp.WriteLine("ncols  {0}", BoundedPocketStandHarvester.pCoresites.numColumns());
        //    fp.WriteLine("nrows  {0}", BoundedPocketStandHarvester.pCoresites.numRows());
        //    fp.WriteLine("xllcorner  {0}", BoundedPocketStandHarvester.pCoresites.XLLCorner - (float)BoundedPocketStandHarvester.pCoresites.Header[30] / 2);
        //    fp.WriteLine("yllcorner  {0}", BoundedPocketStandHarvester.pCoresites.YLLCorner - (float)BoundedPocketStandHarvester.pCoresites.Header[30] * BoundedPocketStandHarvester.pCoresites.numRows() + (float)BoundedPocketStandHarvester.pCoresites.Header[30] / 2);
        //    fp.WriteLine("cellsize  {0}", (float)BoundedPocketStandHarvester.pCoresites.Header[30]);
        //    fp.WriteLine("NODATA_value  -9999");
        //}

        public static void output_harvest_Dec_Type(int itr, string str_htyp, string str_htyp1, string str_dec, string str_dec1, double[] wAdfGeoTransform)
        {
            // Harvest type map
            StreamWriter fp;
            int i;
            int j;
            int temp;
            string pszFormat = "HFA"; //*
            Driver poDriver; //*
            string[] papszMetadata; //*
            poDriver = Gdal.GetDriverByName(pszFormat); //*
            if (poDriver == null) //*
            {
                Environment.Exit(1);
            }
            papszMetadata = poDriver.GetMetadata(""); //*
            Dataset poDstDS; //*
            Band outPoBand; //*
            float cellsize = (BoundedPocketStandHarvester.pCoresites.Header[30]);
            string[] papszOptions = null; //*
            float[] pafScanline; //*
            float[] pintScanline;

            string pszSRS_WKT = null; //*

            SpatialReference oSRS = new SpatialReference(null); //*
            oSRS.SetUTM(11, 1); //*
            oSRS.SetWellKnownGeogCS("HEAD74"); //*
            oSRS.ExportToWkt(out pszSRS_WKT); //*
            pszSRS_WKT = null;

            pintScanline = new float[BoundedPocketStandHarvester.pCoresites.numRows() * BoundedPocketStandHarvester.pCoresites.numColumns()]; //*

            poDstDS = poDriver.Create(str_htyp1, BoundedPocketStandHarvester.pCoresites.numColumns(), BoundedPocketStandHarvester.pCoresites.numRows(), 1, DataType.GDT_CFloat32, papszOptions); //*
            if (poDstDS == null)
            {
                throw new Exception("Img file not be created."); //*
            }
            outPoBand = poDstDS.GetRasterBand(1); //*
            poDstDS.SetGeoTransform(wAdfGeoTransform); //*
            for (i = BoundedPocketStandHarvester.pCoresites.numRows(); i > 0; i--)
            {
                for (j = 1; j <= BoundedPocketStandHarvester.pCoresites.numColumns(); j++)
                {
                    if (BoundedPocketStandHarvester.pHarvestsites[i, j].getHarvestDecade() == BoundedPocketStandHarvester.currentDecade)
                    {
                        temp = BoundedPocketStandHarvester.pHarvestsites[i, j].getHarvestType();
                    }
                    else
                    {
                        temp = 0;
                    }
                    pintScanline[(BoundedPocketStandHarvester.pCoresites.numRows() - i) * BoundedPocketStandHarvester.pCoresites.numColumns() + j - 1] = temp; //*
                }
            }
            outPoBand.WriteRaster(0, 0, BoundedPocketStandHarvester.pCoresites.numColumns(), BoundedPocketStandHarvester.pCoresites.numRows(), pintScanline, BoundedPocketStandHarvester.pCoresites.numColumns(), BoundedPocketStandHarvester.pCoresites.numRows(), 0, 0); //*

            if (poDstDS != null)
            {
                poDstDS.Dispose();
            }

            pintScanline = null;

            pintScanline = new float[BoundedPocketStandHarvester.pCoresites.numRows() * BoundedPocketStandHarvester.pCoresites.numColumns()]; //*

            poDstDS = poDriver.Create(str_dec1, BoundedPocketStandHarvester.pCoresites.numColumns(), BoundedPocketStandHarvester.pCoresites.numRows(), 1, DataType.GDT_Float32, papszOptions); //*
            if (poDstDS == null)
            {
                throw new Exception("Img file not be created."); //*
            }
            outPoBand = poDstDS.GetRasterBand(1); //*
            poDstDS.SetGeoTransform(wAdfGeoTransform); //*
            for (i = BoundedPocketStandHarvester.pCoresites.numRows(); i > 0; i--)
            {
                for (j = 1; j <= BoundedPocketStandHarvester.pCoresites.numColumns(); j++)
                {
                    temp = BoundedPocketStandHarvester.pHarvestsites[i, j].getHarvestDecade();
                    pintScanline[(BoundedPocketStandHarvester.pCoresites.numRows() - i) * BoundedPocketStandHarvester.pCoresites.numColumns() + j - 1] = temp; //*
                }
            }    
            outPoBand.WriteRaster(0, 0, BoundedPocketStandHarvester.pCoresites.numColumns(), BoundedPocketStandHarvester.pCoresites.numRows(), pintScanline, BoundedPocketStandHarvester.pCoresites.numColumns(), BoundedPocketStandHarvester.pCoresites.numRows(), 0, 0); //*
            if (poDstDS != null)
            {
                poDstDS.Dispose(); //*
            }
            pintScanline = null;
            return;

        }

        public static void PutOutput_harvestBACut_spec(string fn, string fn1, int spec, double[] wAdfGeoTransform)
        {
            StreamWriter fpOutput;
            int i;
            int j;
            string pszFormat = "HFA"; //*
            Driver poDriver; //*
            string[] papszMetadata; //*
            poDriver = Gdal.GetDriverByName(pszFormat); //*
            if (poDriver == null) //*
            {
                Environment.Exit(1);
            }
            papszMetadata = poDriver.GetMetadata(""); //*
            Dataset poDstDS; //*
            Band outPoBand; //*
            float cellsize = BoundedPocketStandHarvester.pCoresites.Header[30];
            string[] papszOptions = null; //*
            float[] pafScanline; //*
            uint[] pintScanline;
            string pszSRS_WKT = null; //*
            SpatialReference oSRS = new SpatialReference(null);//*

            oSRS.SetUTM(11, 1); //*
            oSRS.SetWellKnownGeogCS("HEAD74"); //*
            oSRS.ExportToWkt(out pszSRS_WKT); //*
            pszSRS_WKT = null; //*
            pafScanline = new float[BoundedPocketStandHarvester.pCoresites.numRows() * BoundedPocketStandHarvester.pCoresites.numColumns()];//*
            poDstDS = poDriver.Create(fn1, BoundedPocketStandHarvester.pCoresites.numColumns(), BoundedPocketStandHarvester.pCoresites.numRows(), 1, DataType.GDT_Float32, papszOptions); //*

            if (poDstDS == null)
            {
                throw new Exception("Img file not be created."); //*
            }
            outPoBand = poDstDS.GetRasterBand(1); //*
            poDstDS.SetGeoTransform(wAdfGeoTransform); //*
            for (i = BoundedPocketStandHarvester.pCoresites.numRows(); i > 0; i--)
            {
                for (j = 1; j <= BoundedPocketStandHarvester.pCoresites.numColumns(); j++)
                {
                    pafScanline [(BoundedPocketStandHarvester.pCoresites.numRows() - i) * BoundedPocketStandHarvester.pCoresites.numColumns() + j - 1] = (float)BoundedPocketStandHarvester.pHarvestsites.GetValueHarvestBA_spec(i, j, spec); //*
                }
            }
            outPoBand.WriteRaster(0, 0, BoundedPocketStandHarvester.pCoresites.numColumns(), BoundedPocketStandHarvester.pCoresites.numRows(), pafScanline, BoundedPocketStandHarvester.pCoresites.numColumns(), BoundedPocketStandHarvester.pCoresites.numRows(), 0, 0); //*
            if (poDstDS != null)
            {
                poDstDS.Dispose(); //*
            }
            pafScanline = null;
        }

        public static void PutOutput_harvestBACut(string fn, string fn1, double[] wAdfGeoTransform)
        {
            StreamWriter fpOutput;
            int i;
            int j;
            string pszFormat = "HFA"; //*
            Driver poDriver; //*
            string[] papszMetadata; //*
            poDriver = Gdal.GetDriverByName(pszFormat); //*
            if (poDriver == null) //*
            {
                Environment.Exit(1);
            }
            papszMetadata = poDriver.GetMetadata(""); //*
            Dataset poDstDS; //*
            Band outPoBand; //*
            float cellsize = BoundedPocketStandHarvester.pCoresites.Header[30];
            string[] papszOptions = null; //*
            float[] pafScanline; //*
            uint[] pintScanline;
            string pszSRS_WKT = null; //*
            SpatialReference oSRS = new SpatialReference(null); //*
            oSRS.SetUTM(11, 1); //*
            oSRS.SetWellKnownGeogCS("HEAD74"); //*
            oSRS.ExportToWkt(out pszSRS_WKT); //*
            pafScanline = new float[BoundedPocketStandHarvester.pCoresites.numRows() * BoundedPocketStandHarvester.pCoresites.numColumns()]; //*

            poDstDS = poDriver.Create(fn1, BoundedPocketStandHarvester.pCoresites.numColumns(), BoundedPocketStandHarvester.pCoresites.numRows(), 1, DataType.GDT_Float32, papszOptions); //*
            if (poDstDS == null)
            {
                throw new Exception("Img file not be created."); //*
            }
            outPoBand = poDstDS.GetRasterBand(1); //*
            poDstDS.SetGeoTransform(wAdfGeoTransform); //*
            for (i = BoundedPocketStandHarvester.pCoresites.numRows(); i > 0; i--)
            {
                for (j = 1; j <= BoundedPocketStandHarvester.pCoresites.numColumns(); j++)
                {
                    double tmpBAout = BoundedPocketStandHarvester.pHarvestsites.GetValueHarvestBA(i, j);
                    pafScanline [(BoundedPocketStandHarvester.pCoresites.numRows() - i) * BoundedPocketStandHarvester.pCoresites.numColumns() + j - 1 ]= (float)BoundedPocketStandHarvester.pHarvestsites.GetValueHarvestBA(i, j); //*
                }
            }
            BoundedPocketStandHarvester.pHarvestsites.clearValueHarvestBA();

            outPoBand.WriteRaster(0, 0, BoundedPocketStandHarvester.pCoresites.numColumns(), BoundedPocketStandHarvester.pCoresites.numRows(), pafScanline, BoundedPocketStandHarvester.pCoresites.numColumns(), BoundedPocketStandHarvester.pCoresites.numRows(), 0, 0); //*
            if (poDstDS != null)
            {
                poDstDS.Dispose(); //*
            }
            pafScanline = null;
        }




        //public static void output_harvest_Dec_Type(int itr, string str_htyp, string str_dec)
        //{
        //    StreamWriter fp;
        //    int i;
        //    int j;
        //    int temp;

        //    if ((fp = new StreamWriter(str_htyp)) == null)
        //        throw new Exception("can not open output file 1");

        //    outputFileheader(fp);

        //    for (i = BoundedPocketStandHarvester.pCoresites.numRows(); i > 0; i--)
        //    {
        //        for (j = 1; j <= BoundedPocketStandHarvester.pCoresites.numColumns(); j++)
        //        {
        //            if (BoundedPocketStandHarvester.pHarvestsites[i, j].getHarvestDecade() == BoundedPocketStandHarvester.currentDecade)
        //            {
        //                temp = BoundedPocketStandHarvester.pHarvestsites[i, j].getHarvestType();
        //            }
        //            else
        //            {
        //                temp = 0;
        //            }
        //            fp.WriteLine(temp);
        //        }
        //    }
        //    fp.Close();

        //    if ((fp = new StreamWriter(str_dec)) == null)
        //        throw new Exception("can not open output file 1");

        //    outputFileheader(fp);

        //    for (i = BoundedPocketStandHarvester.pCoresites.numRows(); i > 0; i--)
        //    {
        //        for (j = 1; j <= BoundedPocketStandHarvester.pCoresites.numColumns(); j++)
        //        {
        //            temp = BoundedPocketStandHarvester.pHarvestsites[i, j].getHarvestDecade();
        //            fp.WriteLine();
        //        }
        //    }
        //    fp.Close();
        //    return;
        //}



    }
}
