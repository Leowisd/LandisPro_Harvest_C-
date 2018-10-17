using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OSGeo.GDAL;
using OSGeo.OSR;
using LandisPro.Harvest;

namespace LandisPro
{
    class IO
    {
        public static List<string> BioMassFileNames = new List<string>();
        public static List<string> BasalFileNames = new List<string>();
        public static List<string> TreesFileNames = new List<string>();
        public static List<string> IVFileNames = new List<string>();
        public static List<string> DBHFileNames = new List<string>();
        public static List<string> RDFileNames = new List<string>();
        public static List<string> SeedsFileNames = new List<string>();

        public static int flagoutputBiomass;
        public static int flagoutputBasal;
        public static int flagoutputTrees;
        public static int flagoutputIV;
        public static int flagoutputDBH;

        public static double[] AgeDistOutputBuffer_TPA = null;
        public static double[] AgeDistOutputBuffer_BA = null;

        public static double[] AgeDistOutputBuffer_TPA_landtype = null;
        public static double[] AgeDistOutputBuffer_BA_landtype = null;

        private static int[] red2 = new int[map8.maxLeg];
        private static int[] green2 = new int[map8.maxLeg];
        private static int[] blue2 = new int[map8.maxLeg];

        public static void putOutput(int rep, int itr, int[] freq)
        {
            //string local_string = (itr * freq[4] * succession.gl_sites.TimeStep).ToString();
            string local_string = (itr * Program.sites.TimeStep).ToString();
            map8 m = new map8(Program.sites.Header);

            DateTime ltime, ltimeTemp;
            ltime = DateTime.Now;

            Console.WriteLine("Start 6.0 Style writing output at {0}", ltime);

            if (rep == 0)
            {
                for (int i = 1; i <= Program.sites.specNum; i++)
                {
                    string gl_spe_attrs_i_name = Program.speciesAttrs[i].name;
                    Console.Write("creating {0} {1} {2} {3}\n", Program.sites.specNum, rep, gl_spe_attrs_i_name, "age map");
                    reclass.speciesAgeMap(m, gl_spe_attrs_i_name);
                    m.CellSize = Program.parameters.cellSize;
                    string name = Program.parameters.outputDir + "/" + gl_spe_attrs_i_name + "_" + local_string;
                    m.write(name, red2, green2, blue2);
                }
            }

            reclass.ageReclass(m);
            m.CellSize = Program.parameters.cellSize;
            string str = Program.parameters.outputDir + "/ageOldest" + local_string;
            m.write(str, red2, green2, blue2);

            reclass.ageReclassYoungest(m);
            m.CellSize = Program.parameters.cellSize;
            str = Program.parameters.outputDir + "/ageYoungest" + local_string;
            m.write(str, red2, green2, blue2);

            ltimeTemp = DateTime.Now;
            Console.Write("\nFinish 6.0 Style writing output at {0}\n", ltimeTemp);
            Console.Write("it took {0} seconds\n", ltimeTemp - ltime);
        }

        public static void putOutput_Landis70Pro(int rep, int itr, int[] freq)
        {
            DateTime a1 = DateTime.Now;
            double TmpBiomassT = 0, TmpBiomassS = 0, TmpBasalAreaT = 0, TmpBasalAreaS = 0, TmpCarbon = 0, TmpCarbonTotal = 0, TmpRDTotal;
            int TmpTreeT = 0, TmpTreesS = 0;
            Driver poDriver = Gdal.GetDriverByName("HFA");
            if (poDriver == null)
                throw new Exception("Start GDAL driver error!");
            string[] papszMetadata = poDriver.GetMetadata("");
            string[] papszOptions = null;
            double[] wAdfGeoTransform = new double[6] { 0.00, Program.parameters.cellSize, 0.00, 600.00, 0.00, -Program.parameters.cellSize };
            float[] pafScanline = null;
            float[] pafScanline1 = null;
            float[] pafScanline2 = null;
            float[] pafScanline3 = null;
            float[] pafScanline4 = null;
            float[] pafScanline5 = null;
            int[] pintScanline = null;

            SpatialReference oSRS = new SpatialReference(null);

            oSRS.SetUTM(11, 1);

            oSRS.SetWellKnownGeogCS("HEAD74");

            string pszSRS_WKT = null;
            oSRS.ExportToWkt(out pszSRS_WKT);

            //VSIFree(pszSRS_WKT);
            pszSRS_WKT = null;
            Band outPoBand = null;
            Band outPoBand1 = null;
            Band outPoBand2 = null;
            Band outPoBand3 = null;
            Band outPoBand4 = null;
            Band outPoBand5 = null;

            Dataset poDstDS = null;
            Dataset poDstDS1 = null;
            Dataset poDstDS2 = null;
            Dataset poDstDS3 = null;
            Dataset poDstDS4 = null;
            Dataset poDstDS5 = null;

            Console.WriteLine("Start 7.0 Style writing output at {0}, itr = {1}", a1, itr);


            double local_const = 3.1415926 / (4 * 10000.00);

            string output_dir_string = Program.parameters.outputDir + "/";
            string local_string = "_" + (itr * Program.parameters.timestep).ToString() + ".img";
            int cellsize_square =Program.sites.CellSize * Program.sites.CellSize;


            int col_num = (int)Program.sites.numColumns();
            int row_num = (int)Program.sites.numRows();
            int total_size = col_num * row_num;

            pafScanline = new float[total_size];

            float Biomass_threshold = (float)Program.sites.BiomassThreshold;
            for (int k = 1; k <= Program.sites.specNum; ++k)
            {
                int m_max = Program.speciesAttrs[k].longevity / Program.parameters.timestep;
                if (Program.sites.GetOutputGeneralFlagArray((uint)k - 1, (int)Sites.Bio) != 0)
                {
                    if (BioMassFileNames[k - 1] != "N/A")
                    {
                        string fpbiomass = (output_dir_string + BioMassFileNames[k - 1] + local_string);
                        poDstDS = poDriver.Create(fpbiomass, col_num, row_num, 1, DataType.GDT_Float32, papszOptions);
                        if (poDstDS == null)
                            throw new Exception("Img file not be created.");
                        poDstDS.SetGeoTransform(wAdfGeoTransform);
                        outPoBand = poDstDS.GetRasterBand(1);
                        float biomass1 =Program.sites.GetBiomassData(Program.speciesAttrs[k].BioMassCoef, 1);
                        float biomass2 = Program.sites.GetBiomassData(Program.speciesAttrs[k].BioMassCoef, 2);
                        for (uint i = (uint)row_num; i > 0; --i)
                        {
                            for (uint j = 1; j <= col_num; ++j)
                            {
                                Landunit l = Program.sites.locateLanduPt((int)i, (int)j);
                                TmpBiomassS = 0;
                                Specie local_specis = Program.sites[(int)i, (int)j].SpecieIndex(k);
                                for (int m = 1; m <= m_max; m++)
                                {
                                    float local_grow_rate = Program.sites.GetGrowthRates(k, m, l.ltID);
                                    if (local_grow_rate >= Biomass_threshold)
                                        TmpBiomassS += Math.Exp(biomass1 + biomass2 * Math.Log(local_grow_rate)) * local_specis.getTreeNum(m, k) / 1000.00;
                                }
                                pafScanline[(row_num - i) * col_num + j - 1] = (float)TmpBiomassS;
                                //if (itr > 29 && TmpBiomassS != 0)
                                //    Console.Write("{0:F2} ", TmpBiomassS);
                            }
                        }
                        outPoBand.WriteRaster(0, 0, col_num, row_num, pafScanline, col_num, row_num, 0, 0);
                        if (poDstDS != null)
                            poDstDS.Dispose();
                    }
                }
                if (Program.sites.GetOutputGeneralFlagArray((uint)k - 1, (int)Sites.BA) != 0)
                {
                    if (BasalFileNames[k - 1] != "N/A")
                    {
                        string fpbasal = output_dir_string + BasalFileNames[k - 1] + local_string;
                        poDstDS = poDriver.Create(fpbasal, col_num, row_num, 1, DataType.GDT_Float32, papszOptions);//*
                        if (poDstDS == null)
                            throw new Exception("Img file not be created.");
                        poDstDS.SetGeoTransform(wAdfGeoTransform);
                        outPoBand = poDstDS.GetRasterBand(1);
                        for (uint i = (uint)row_num; i > 0; --i)
                        {
                            for (uint j = 1; j <= col_num; ++j)
                            {
                                Landunit l = Program.sites.locateLanduPt((int)i, (int)j);
                                TmpBasalAreaS = 0;
                                Specie local_specis = Program.sites[(int)i, (int)j].SpecieIndex(k);
                                for (int m = 1; m <= m_max; m++)
                                {
                                    float local_grow_rate = Program.sites.GetGrowthRates(k, m, l.ltID);
                                    TmpBasalAreaS += local_grow_rate * local_grow_rate * local_const * local_specis.getTreeNum(m, k);
                                }
                                pafScanline[(row_num - i) * col_num + j - 1] = (float)TmpBasalAreaS;
                                //if (itr > 29 && TmpBasalAreaS != 0)
                                //    Console.Write("{0:F1} ", TmpBasalAreaS);
                            }
                        }
                        outPoBand.WriteRaster(0, 0, col_num, row_num, pafScanline, col_num, row_num, 0, 0);//*
                        if (poDstDS != null)
                            poDstDS.Dispose();
                    }
                }
                pintScanline = new int[total_size];
                if (Program.sites.GetOutputGeneralFlagArray((uint)k - 1, (int)Sites.TPA) != 0)
                {
                    if (TreesFileNames[k - 1] != "N/A")
                    {
                        string fptree = output_dir_string + TreesFileNames[k - 1] + local_string;
                        poDstDS = poDriver.Create(fptree, col_num, row_num, 1, DataType.GDT_UInt32, papszOptions);//*
                        poDstDS.SetGeoTransform(wAdfGeoTransform);
                        outPoBand = poDstDS.GetRasterBand(1);
                        if (poDstDS == null)
                            throw new Exception("Img file not be created.");
                        for (uint i = (uint)row_num; i > 0; --i)
                        {
                            for (uint j = 1; j <= col_num; ++j)
                            {
                                TmpTreesS = 0;

                                Specie local_specis = Program.sites[(int)i, (int)j].SpecieIndex(k);
                                for (int m = 1; m <= m_max; m++)
                                {
                                    TmpTreesS += (int)local_specis.getTreeNum(m, k);
                                }
                                //if (itr > 9 && TmpTreesS != 0)
                                //    Console.Write("{0}\n", TmpTreesS);
                                pintScanline[(row_num - i) * col_num + j - 1] = TmpTreesS;
                            }
                        }
                        outPoBand.WriteRaster(0, 0, col_num, row_num, pintScanline, col_num, row_num, 0, 0);
                        if (poDstDS != null)
                            poDstDS.Dispose();
                    }
                }

                if (Program.sites.GetOutputGeneralFlagArray((uint)k - 1, (int)Sites.IV) != 0)
                {
                    if (IVFileNames[k - 1] != "N/A")
                    {
                        string fpIV = output_dir_string + IVFileNames[k - 1] + local_string;                  
                        poDstDS = poDriver.Create(fpIV, col_num, row_num, 1, DataType.GDT_Float32, papszOptions);//*
                        if (poDstDS == null)
                            throw new Exception("Img file not be created.");
                        poDstDS.SetGeoTransform(wAdfGeoTransform);
                        outPoBand = poDstDS.GetRasterBand(1);
                        for (uint i = (uint)row_num; i > 0; --i)
                        {
                            for (uint j = 1; j <= col_num; ++j)
                            {
                                Landunit l = Program.sites.locateLanduPt((int)i, (int)j);
                                TmpBasalAreaT = 0;
                                TmpTreeT = 0;
                                for (int kk = 1; kk <= Program.sites.specNum; ++kk)
                                {
                                    if (Program.speciesAttrs[kk].SpType >= 0)
                                    {
                                        Specie local_specis = Program.sites[(int)i, (int)j].SpecieIndex(kk);
                                        int local_m_max = Program.speciesAttrs[kk].longevity / Program.sites.TimeStep;
                                        for (int m = 1; m <= local_m_max; m++)
                                        {
                                            float local_grow_rate = Program.sites.GetGrowthRates(kk, m, l.ltID);
                                            uint local_tree_num = (uint)local_specis.getTreeNum(m, kk);
                                            TmpBasalAreaT += local_grow_rate * local_grow_rate * local_const * local_tree_num;
                                            TmpTreeT += (int)local_tree_num;
                                        }
                                    }
                                }
                                TmpBasalAreaS = 0;
                                Specie local_spe = Program.sites[(int)i, (int)j].SpecieIndex(k);
                                for (int m = 1; m <= m_max; ++m)
                                {
                                    float local_grow_rate = Program.sites.GetGrowthRates(k, m, l.ltID);
                                    TmpBasalAreaS += local_grow_rate * local_grow_rate * local_const * local_spe.getTreeNum(m, k);
                                }
                                if (TmpTreeT == 0 || TmpBasalAreaT < 0.0001)
                                    pafScanline[(row_num - i) * col_num + j - 1] = 0;
                                else
                                    pafScanline[(row_num - i) * col_num + j - 1] = (float)(TmpTreesS / (double)TmpTreeT + TmpBasalAreaS / TmpBasalAreaT);
                            }
                        }
                        outPoBand.WriteRaster(0, 0, col_num, row_num, pafScanline, col_num, row_num, 0, 0);
                        if (poDstDS != null)
                            poDstDS.Dispose();
                    }
                }

                if (Program.sites.GetOutputGeneralFlagArray((uint)k - 1, (int)Sites.Seeds) != 0)
                {
                    if (SeedsFileNames[k - 1] != "N/A")
                    {
                        string fpSeeds = output_dir_string + SeedsFileNames[k - 1] + local_string;//* change
                        poDstDS = poDriver.Create(fpSeeds, col_num, row_num, 1, DataType.GDT_UInt32, papszOptions);
                        if (poDstDS == null)
                            throw new Exception("Img file not be created.");
                        outPoBand = poDstDS.GetRasterBand(1);
                        poDstDS.SetGeoTransform(wAdfGeoTransform);
                        for (uint i = (uint)row_num; i > 0; --i)
                            for (uint j = 1; j <= col_num; ++j)
                            {
                                pintScanline[(row_num - i) * col_num + j - 1] = (int)Program.sites[(int)i, (int)j].SpecieIndex(k).AvailableSeed;
                                //Console.Write("{0} ", succession.gl_sites[i, j].SpecieIndex(k).AvailableSeed);
                            }
                        outPoBand.WriteRaster(0, 0, col_num, row_num, pintScanline, col_num, row_num, 0, 0);
                        if (poDstDS != null)
                            poDstDS.Dispose();

                        //Console.ReadLine();
                    }
                }



                if (Program.sites.GetOutputGeneralFlagArray((uint)k - 1, (int)Sites.RDensity) != 0)
                {
                    if (RDFileNames[k - 1] != "N/A")
                    {
                        string fpRD = output_dir_string + RDFileNames[k - 1] + local_string;
                        poDstDS = poDriver.Create(fpRD, col_num, row_num, 1, DataType.GDT_Float32, papszOptions);//*
                        if (poDstDS == null)
                            throw new Exception("Img file not be created.");
                        outPoBand = poDstDS.GetRasterBand(1);
                        poDstDS.SetGeoTransform(wAdfGeoTransform);
                        for (uint i = (uint)row_num; i > 0; --i)
                        {
                            for (uint j = 1; j <= col_num; ++j)
                            {
                                Landunit l = Program.sites.locateLanduPt((int)i, (int)j);
                                if (Program.sites[(int)i, (int)j].specAtt(k).SpType >= 0)
                                {
                                    float temp = 0.0f;
                                    Site local_site = Program.sites[(int)i, (int)j];
                                    Speciesattr local_speciesattr = local_site.specAtt(k);
                                    Specie local_species = local_site.SpecieIndex(k);
                                    int loop_num = local_speciesattr.longevity / Program.sites.TimeStep;
                                    for (int jj = 1; jj <= loop_num; jj++)
                                    {
                                        temp += (float)Math.Pow((Program.sites.GetGrowthRates(k, jj, l.ltID) / 25.4), 1.605) * local_species.getTreeNum(jj, k);
                                    }
                                    temp *= (float)local_speciesattr.MaxAreaOfSTDTree / cellsize_square;
                                    pafScanline[(row_num - i) * col_num + j - 1] = temp;
                                }
                                else
                                    pafScanline[(row_num - i) * col_num + j - 1] = 0.0f;
                            }
                        }
                        outPoBand.WriteRaster(0, 0, col_num, row_num, pafScanline, col_num, row_num, 0, 0);
                        if (poDstDS != null)
                            poDstDS.Dispose();
                    }
                }
            }//end if
            int Bio_flag = Program.sites.GetOutputGeneralFlagArray((uint)Program.sites.specNum, (int)Sites.Bio);
            int Car_flag = Program.sites.GetOutputGeneralFlagArray((uint)Program.sites.specNum, (int)Sites.Car);
            int BA_flag = Program.sites.GetOutputGeneralFlagArray((uint)Program.sites.specNum, (int)Sites.BA);
            int RDenflag = Program.sites.GetOutputGeneralFlagArray((uint)Program.sites.specNum, (int)Sites.RDensity);
            int TPA_flag = Program.sites.GetOutputGeneralFlagArray((uint)Program.sites.specNum, (int)Sites.TPA);

            if (Bio_flag != 0 || Car_flag != 0 || BA_flag != 0 || RDenflag != 0 || TPA_flag != 0)
            {
                //below is for total 

                //1.Bio
                pafScanline1 = new float[total_size];
                pafScanline2 = new float[total_size];
                pafScanline3 = new float[total_size];
                pafScanline4 = new float[total_size];
                pafScanline5 = new float[total_size];
                if (Bio_flag != 0)
                {
                    string fpTotalBiomass = output_dir_string + "TotalBio" + local_string;
                    poDstDS1 = poDriver.Create(fpTotalBiomass, col_num, row_num, 1, DataType.GDT_Float32, papszOptions);
                    if (poDstDS1 == null)
                        throw new Exception("Img file not be created.");
                    outPoBand1 = poDstDS1.GetRasterBand(1);
                    poDstDS1.SetGeoTransform(wAdfGeoTransform);
                }
                if (Car_flag != 0)
                {
                    string fpTotalcarbon = output_dir_string + "TotalCarbon" + local_string;
                    poDstDS2 = poDriver.Create(fpTotalcarbon, col_num, row_num, 1, DataType.GDT_Float32, papszOptions);
                    if (poDstDS2 == null)
                        throw new Exception("Img file not be created.");
                    outPoBand2 = poDstDS2.GetRasterBand(1);
                    poDstDS2.SetGeoTransform(wAdfGeoTransform);
                }
                if (BA_flag != 0)
                {
                    string fptotalbasl = output_dir_string + "TotalBA" + local_string;
                    poDstDS3 = poDriver.Create(fptotalbasl, col_num, row_num, 1, DataType.GDT_Float32, papszOptions);
                    if (poDstDS3 == null)
                        throw new Exception("Img file not be created.");
                    outPoBand3 = poDstDS3.GetRasterBand(1);
                    poDstDS3.SetGeoTransform(wAdfGeoTransform);
                }

                if (TPA_flag != 0)
                {
                    string fptotaltrees = output_dir_string + "TotalTrees" + local_string;
                    poDstDS4 = poDriver.Create(fptotaltrees, col_num, row_num, 1, DataType.GDT_UInt32, papszOptions);
                    if (poDstDS4 == null)
                        throw new Exception("Img file not be created.");
                    outPoBand4 = poDstDS4.GetRasterBand(1);
                    poDstDS4.SetGeoTransform(wAdfGeoTransform);
                }

                if (RDenflag != 0)
                {
                    string fpRD = output_dir_string + "RelativeDensity" + local_string;
                    poDstDS5 = poDriver.Create(fpRD, col_num, row_num, 1, DataType.GDT_Float32, papszOptions);
                    if (poDstDS5 == null)
                        throw new Exception("Img file not be created.");
                    outPoBand5 = poDstDS5.GetRasterBand(1);
                    poDstDS5.SetGeoTransform(wAdfGeoTransform);
                }

                for (uint i = (uint)row_num; i > 0; i--)
                {
                    for (uint j = 1; j <= col_num; j++)
                    {
                        Landunit l = Program.sites.locateLanduPt((int)i, (int)j);
                        TmpBiomassT = 0;
                        TmpBasalAreaT = 0;
                        TmpTreeT = 0;
                        TmpCarbon = 0;
                        TmpCarbonTotal = 0;
                        Site local_site =Program.sites[(int)i, (int)j];

                        for (int k = 1; k <= Program.sites.specNum; k++)
                        {
                            TmpCarbon = 0;
                            Speciesattr local_speciesattr = Program.speciesAttrs[k];
                            if (local_speciesattr.SpType >= 0)
                            {
                                int m_max = local_speciesattr.longevity / Program.sites.TimeStep;
                                float tmp_term1 = Program.sites.GetBiomassData(local_speciesattr.BioMassCoef, 1);
                                float tmp_term2 = Program.sites.GetBiomassData(local_speciesattr.BioMassCoef, 2);

                                Specie local_specie = local_site.SpecieIndex(k);

                                for (int m = 1; m <= m_max; m++)
                                {
                                    float local_grow_rate = Program.sites.GetGrowthRates(k, m, l.ltID);
                                    double local_value = Math.Exp(tmp_term1 + tmp_term2 * Math.Log(local_grow_rate));

                                    uint local_tree_num = (uint)local_specie.getTreeNum(m, k);

                                    double tmp_mulply = local_value * local_tree_num;

                                    if (local_grow_rate >= Biomass_threshold && Bio_flag != 0)
                                        TmpBiomassT += tmp_mulply / 1000.00;

                                    if (BA_flag != 0)
                                        TmpBasalAreaT += local_grow_rate * local_grow_rate * local_const * local_tree_num;

                                    if (TPA_flag != 0)
                                        TmpTreeT += (int)local_tree_num;

                                    if (Car_flag != 0)
                                        TmpCarbon += tmp_mulply;

                                }

                            }
                            if (Car_flag != 0)
                                TmpCarbonTotal += TmpCarbon * local_speciesattr.CarbonCoef;

                        }

                        long local_index = (row_num - i) * col_num + j - 1;

                        if (Bio_flag != 0)
                            pafScanline1[local_index] = (float)TmpBiomassT;

                        if (Car_flag != 0)
                            pafScanline2[local_index] = (float)TmpCarbonTotal / 1000.00f;

                        if (BA_flag != 0)
                            pafScanline3[local_index] = (float)TmpBasalAreaT;

                        if (RDenflag != 0)
                            pafScanline5[local_index] = (float)local_site.RD;

                        if (TPA_flag != 0)
                            pintScanline[local_index] = TmpTreeT;
                    }

                }


                if (Bio_flag != 0)
                {
                    outPoBand1.WriteRaster(0, 0, col_num, row_num, pafScanline1, col_num, row_num, 0, 0);

                    if (poDstDS1 != null)
                        poDstDS1.Dispose();
                }
                if (Car_flag != 0)
                {
                    outPoBand2.WriteRaster(0, 0, col_num, row_num, pafScanline2, col_num, row_num, 0, 0);

                    if (poDstDS2 != null)
                        poDstDS2.Dispose();
                }
                if (BA_flag != 0)
                {
                    outPoBand3.WriteRaster(0, 0, col_num, row_num, pafScanline3, col_num, row_num, 0, 0);

                    if (poDstDS3 != null)
                        poDstDS3.Dispose();
                }
                if (TPA_flag != 0)
                {
                    outPoBand4.WriteRaster(0, 0, col_num, row_num, pintScanline, col_num, row_num, 0, 0);

                    //if (itr > 29)
                    //{
                    //    for (int i = 0; i < total_size; i++)
                    //    {
                    //        if (pintScanline[i] != 0)
                    //            Console.Write("{0} ", pintScanline[i]);
                    //    }
                    //}

                    if (poDstDS4 != null)
                        poDstDS4.Dispose();
                }
                if (RDenflag != 0)
                {
                    outPoBand5.WriteRaster(0, 0, col_num, row_num, pafScanline5, col_num, row_num, 0, 0);
                    if (poDstDS5 != null)
                        poDstDS5.Dispose();
                }
            }

            //below is for species age range
            int Agerangeage1 = 0, Agerangeage2 = 0, Agerangecount = 0, Agerangeii = 0;

            if (Program.sites.flagAgeRangeOutput != 0)
            {
                for (int k = 1; k <= Program.sites.specNum; k++)
                {
                    int gl_spe_spe_attr_longevity = Program.speciesAttrs[k].longevity;

                    if (Program.sites.GetOutputAgerangeFlagArray((uint)k - 1, (int)Sites.Bio) != 0)
                    {
                        Agerangecount = Program.sites.GetAgerangeCount(k - 1);
                        float tmp_term1 = Program.sites.GetBiomassData(Program.speciesAttrs[k].BioMassCoef, 1);
                        float tmp_term2 = Program.sites.GetBiomassData(Program.speciesAttrs[k].BioMassCoef, 2);


                        for (Agerangeii = 1; Agerangeii <= Agerangecount; Agerangeii++)
                        {
                            Program.sites.GetSpeciesAgerangeArray(k - 1, Agerangeii, ref Agerangeage1, ref Agerangeage2);

                            if (BioMassFileNames[k - 1] != "N/A")
                            {
                                string fpbiomass = output_dir_string + BioMassFileNames[k - 1] + "_Age" + Agerangeage1 + "_Age" + Agerangeage2 + local_string;

                                poDstDS = poDriver.Create(fpbiomass, col_num, row_num, 1, DataType.GDT_Float32, papszOptions);//*

                                if (poDstDS == null)
                                    throw new Exception("Img file not be created.");

                                outPoBand = poDstDS.GetRasterBand(1);

                                poDstDS.SetGeoTransform(wAdfGeoTransform);

                                int beg = Math.Min(gl_spe_spe_attr_longevity, Agerangeage1) / Program.sites.TimeStep;
                                int end = Math.Min(gl_spe_spe_attr_longevity, Agerangeage2) / Program.sites.TimeStep;

                                for (uint i = (uint)row_num; i > 0; i--)
                                {
                                    for (uint j = 1; j <= col_num; j++)
                                    {
                                        Landunit l = Program.sites.locateLanduPt((int)i, (int)j);

                                        TmpBiomassS = 0;

                                        Specie local_specie = Program.sites[(int)i, (int)j].SpecieIndex(k);

                                        for (int m = beg; m <= end; m++)
                                        {
                                            float local_grow_rate = Program.sites.GetGrowthRates(k, m, l.ltID);
                                            double local_value = Math.Exp(tmp_term1 + tmp_term2 * Math.Log(local_grow_rate));

                                            if (local_grow_rate >= Biomass_threshold)
                                                TmpBiomassS += local_value * local_specie.getTreeNum(m, k);
                                        }

                                        TmpBiomassS /= 1000.00;

                                        pafScanline[(row_num - i) * col_num + j - 1] = (float)TmpBiomassS;
                                    }

                                }

                                outPoBand.WriteRaster(0, 0, col_num, row_num, pafScanline, col_num, row_num, 0, 0);

                                if (poDstDS != null)
                                    poDstDS.Dispose();

                            }

                        }

                    }



                    if (Program.sites.GetOutputAgerangeFlagArray((uint)k - 1, (int)Sites.BA) != 0)
                    {
                        Agerangecount = Program.sites.GetAgerangeCount(k - 1);

                        for (Agerangeii = 1; Agerangeii <= Agerangecount; Agerangeii++)
                        {
                            Program.sites.GetSpeciesAgerangeArray(k - 1, Agerangeii, ref Agerangeage1, ref Agerangeage2);

                            if (BasalFileNames[k - 1] != "N/A")
                            {
                                string fpbasal = output_dir_string + BasalFileNames[k - 1] + "_Age" + Agerangeage1 + "_Age" + Agerangeage2 + local_string;

                                poDstDS = poDriver.Create(fpbasal, col_num, row_num, 1, DataType.GDT_Float32, papszOptions);

                                if (poDstDS == null)
                                    throw new Exception("Img file not be created.");

                                outPoBand = poDstDS.GetRasterBand(1);

                                poDstDS.SetGeoTransform(wAdfGeoTransform);

                                int beg = Math.Min(gl_spe_spe_attr_longevity, Agerangeage1) / Program.sites.TimeStep;
                                int end = Math.Min(gl_spe_spe_attr_longevity, Agerangeage2) / Program.sites.TimeStep;

                                for (uint i = (uint)row_num; i > 0; i--)
                                {
                                    for (uint j = 1; j <= col_num; j++)
                                    {
                                        Landunit l = Program.sites.locateLanduPt((int)i, (int)j);

                                        TmpBasalAreaS = 0;

                                        Specie local_specie = Program.sites[(int)i, (int)j].SpecieIndex(k);

                                        for (int m = beg; m <= end; m++)
                                        {
                                            float local_grow_rate = Program.sites.GetGrowthRates(k, m, l.ltID);

                                            TmpBasalAreaS += local_grow_rate * local_grow_rate * local_specie.getTreeNum(m, k);
                                        }

                                        TmpBasalAreaS *= local_const;

                                        pafScanline[(row_num - i) * col_num + j - 1] = (float)TmpBasalAreaS;
                                    }
                                }

                                outPoBand.WriteRaster(0, 0, col_num, row_num, pafScanline, col_num, row_num, 0, 0);

                                if (poDstDS != null)
                                    poDstDS.Dispose();
                            }

                        }

                    }


                    if (Program.sites.GetOutputAgerangeFlagArray((uint)k - 1, (int)Sites.TPA) != 0)
                    {
                        Agerangecount = Program.sites.GetAgerangeCount(k - 1);

                        for (Agerangeii = 1; Agerangeii <= Agerangecount; Agerangeii++)
                        {
                            Program.sites.GetSpeciesAgerangeArray(k - 1, Agerangeii, ref Agerangeage1, ref Agerangeage2);

                            if (TreesFileNames[k - 1] != "N/A")
                            {
                                string fptree = output_dir_string + TreesFileNames[k - 1] + "_Age" + Agerangeage1 + "_Age" + Agerangeage2 + local_string;

                                poDstDS = poDriver.Create(fptree, col_num, row_num, 1, DataType.GDT_UInt32, papszOptions);

                                if (poDstDS == null)
                                    throw new Exception("Img file not be created.");

                                outPoBand = poDstDS.GetRasterBand(1);

                                poDstDS.SetGeoTransform(wAdfGeoTransform);

                                int beg = Math.Min(gl_spe_spe_attr_longevity, Agerangeage1) / Program.sites.TimeStep;
                                int end = Math.Min(gl_spe_spe_attr_longevity, Agerangeage2) / Program.sites.TimeStep;

                                for (uint i = (uint)row_num; i > 0; i--)
                                {
                                    for (uint j = 1; j <= col_num; j++)
                                    {
                                        TmpTreesS = 0;

                                        Specie local_specie = Program.sites[(int)i, (int)j].SpecieIndex(k);

                                        for (int m = beg; m <= end; m++)
                                            TmpTreesS += (int)local_specie.getTreeNum(m, k);

                                        pafScanline[(row_num - i) * col_num + j - 1] = TmpTreesS;

                                        //if (TmpTreesS != 0)
                                        //    Console.Write("{0} ", TmpTreesS);
                                    }

                                }


                                outPoBand.WriteRaster(0, 0, col_num, row_num, pintScanline, col_num, row_num, 0, 0);

                                if (poDstDS != null)
                                    poDstDS.Dispose();
                            }

                        }

                    }



                    if (Program.sites.GetOutputAgerangeFlagArray((uint)k - 1, (int)Sites.IV) != 0)
                    {

                        Agerangecount = Program.sites.GetAgerangeCount(k - 1);

                        for (Agerangeii = 1; Agerangeii <= Agerangecount; Agerangeii++)
                        {
                            Program.sites.GetSpeciesAgerangeArray(k - 1, Agerangeii, ref Agerangeage1, ref Agerangeage2);

                            if (IVFileNames[k - 1] != "N/A")
                            {
                                string fpIV = output_dir_string + IVFileNames[k - 1] + "_Age" + Agerangeage1 + "_Age" + Agerangeage2 + local_string;
                                poDstDS = poDriver.Create(fpIV, col_num, row_num, 1, DataType.GDT_Float32, papszOptions);

                                if (poDstDS == null)
                                    throw new Exception("Img file not be created.");

                                outPoBand = poDstDS.GetRasterBand(1);

                                poDstDS.SetGeoTransform(wAdfGeoTransform);

                                for (uint i = (uint)row_num; i > 0; i--)
                                {
                                    for (uint j = 1; j <= col_num; j++)
                                    {
                                        Landunit l = Program.sites.locateLanduPt((int)i, (int)j);
                                        TmpBasalAreaT = 0;

                                        TmpTreeT = 0;

                                        Site local_site = Program.sites[(int)i, (int)j];

                                        for (int kk = 1; kk <= Program.sites.specNum; kk++)
                                        {
                                            Specie local_specie = local_site.SpecieIndex(kk);

                                            if (Program.speciesAttrs[kk].SpType >= 0)
                                            {
                                                int loop_num = Program.speciesAttrs[kk].longevity / Program.sites.TimeStep;

                                                for (int m = 1; m <= loop_num; m++)
                                                {
                                                    float local_grow_rate = Program.sites.GetGrowthRates(kk, m, l.ltID);
                                                    uint local_tree_num = (uint)local_specie.getTreeNum(m, kk);

                                                    TmpBasalAreaT += local_grow_rate * local_grow_rate * local_const * local_tree_num;

                                                    TmpTreeT += (int)local_tree_num;
                                                }
                                            }
                                        }

                                        int local_loop_num = gl_spe_spe_attr_longevity / Program.sites.TimeStep;

                                        TmpBasalAreaS = 0;
                                        TmpTreesS = 0;

                                        Specie tmp_local_specie = Program.sites[(int)i, (int)j].SpecieIndex(k);


                                        for (int m = 1; m <= local_loop_num; m++)
                                        {
                                            float local_grow_rate = Program.sites.GetGrowthRates(k, m, l.ltID);

                                            uint local_tree_num = (uint)tmp_local_specie.getTreeNum(m, k);

                                            TmpBasalAreaS += local_grow_rate * local_grow_rate * local_const * local_tree_num;

                                            TmpTreesS += (int)local_tree_num;
                                        }


                                        if (TmpTreeT == 0 || TmpBasalAreaT < 0.0001)
                                            pafScanline[(row_num - i) * col_num + j - 1] = 0;
                                        else
                                            pafScanline[(row_num - i) * col_num + j - 1] = (float)(TmpTreesS / (double)TmpTreeT + TmpBasalAreaS / TmpBasalAreaT);
                                    }

                                }
                                //fpIV.Close();
                                outPoBand.WriteRaster(0, 0, col_num, row_num, pafScanline, col_num, row_num, 0, 0);

                                if (poDstDS != null)
                                    poDstDS.Dispose();
                            }

                        }

                    }



                }


                //below is for age range  total 
                int Bio_endagerange_flag = Program.sites.GetOutputAgerangeFlagArray((uint)Program.sites.specNum, (int)Sites.Bio);
                int Car_endagerange_flag = Program.sites.GetOutputAgerangeFlagArray((uint)Program.sites.specNum, (int)Sites.Car);
                int BA_endagerange_flag = Program.sites.GetOutputAgerangeFlagArray((uint)Program.sites.specNum, (int)Sites.BA);
                int TPA_endagerange_flag = Program.sites.GetOutputAgerangeFlagArray((uint)Program.sites.specNum, (int)Sites.TPA);
                int RDenendagerange_flag = Program.sites.GetOutputAgerangeFlagArray((uint)Program.sites.specNum, (int)Sites.RDensity);

                if (Bio_endagerange_flag != 0)
                {
                    string fpTotalBiomass = output_dir_string + "TotalBio_AgeRange" + local_string;

                    poDstDS1 = poDriver.Create(fpTotalBiomass, col_num, row_num, 1, DataType.GDT_Float32, papszOptions);

                    if (poDstDS1 == null)
                        throw new Exception("Img file not be created.");

                    outPoBand1 = poDstDS1.GetRasterBand(1);

                    poDstDS1.SetGeoTransform(wAdfGeoTransform);
                }


                if (TPA_endagerange_flag != 0)
                {
                    string fpTotaltrees = output_dir_string + "TotalTrees_AgeRange" + local_string;
                    poDstDS2 = poDriver.Create(fpTotaltrees, col_num, row_num, 1, DataType.GDT_UInt32, papszOptions);

                    if (poDstDS2 == null)
                        throw new Exception("Img file not be created.");

                    outPoBand2 = poDstDS2.GetRasterBand(1);

                    poDstDS2.SetGeoTransform(wAdfGeoTransform);
                }


                if (BA_endagerange_flag != 0)
                {
                    string fpTotalbasl = output_dir_string + "TotalBasal_AgeRange" + local_string;
                    poDstDS3 = poDriver.Create(fpTotalbasl, col_num, row_num, 1, DataType.GDT_Float32, papszOptions);

                    if (poDstDS3 == null)
                        throw new Exception("Img file not be created.");

                    outPoBand3 = poDstDS3.GetRasterBand(1);

                    poDstDS3.SetGeoTransform(wAdfGeoTransform);
                }



                if (Car_endagerange_flag != 0)
                {
                    string fpTotalcarbon = output_dir_string + "TotalCarbon_AgeRange" + local_string;

                    poDstDS4 = poDriver.Create(fpTotalcarbon, col_num, row_num, 1, DataType.GDT_Float32, papszOptions);

                    if (poDstDS4 == null)
                        throw new Exception("Img file not be created.");

                    outPoBand4 = poDstDS4.GetRasterBand(1);

                    poDstDS4.SetGeoTransform(wAdfGeoTransform);
                }



                if (RDenendagerange_flag != 0)
                {
                    string fpTotalRD = output_dir_string + "TotalRD_AgeRange" + local_string;

                    poDstDS5 = poDriver.Create(fpTotalRD, col_num, row_num, 1, DataType.GDT_Float32, papszOptions);

                    if (poDstDS5 == null)
                        throw new Exception("Img file not be created.");

                    outPoBand5 = poDstDS5.GetRasterBand(1);

                    poDstDS5.SetGeoTransform(wAdfGeoTransform);
                }



                for (uint i = (uint)row_num; i > 0; i--)
                {
                    for (uint j = 1; j <= col_num; j++)
                    {
                        Landunit l = Program.sites.locateLanduPt((int)i, (int)j);

                        TmpBiomassT = 0;

                        TmpBasalAreaT = 0;

                        TmpTreeT = 0;

                        TmpCarbon = 0;

                        TmpCarbonTotal = 0;

                        TmpRDTotal = 0;

                        Site local_site = Program.sites[(int)i, (int)j];

                        for (int k = 1; k <= Program.sites.specNum; k++)
                        {
                            TmpCarbon = 0;

                            Agerangecount = Program.sites.GetAgerangeCount(k - 1);

                            Speciesattr local_speciesattr = local_site.specAtt(k);

                            Speciesattr gl_spe_Attrs_k = Program.speciesAttrs[k];

                            float tmp_term1 = Program.sites.GetBiomassData(gl_spe_Attrs_k.BioMassCoef, 1);
                            float tmp_term2 = Program.sites.GetBiomassData(gl_spe_Attrs_k.BioMassCoef, 2);


                            for (Agerangeii = 1; Agerangeii <= Agerangecount; Agerangeii++)
                            {
                                Program.sites.GetSpeciesAgerangeArray(k - 1, Agerangeii, ref Agerangeage1, ref Agerangeage2);

                                if (gl_spe_Attrs_k.SpType >= 0)
                                {
                                    int beg = Math.Min(gl_spe_Attrs_k.longevity, Agerangeage1) / Program.sites.TimeStep;
                                    int end = Math.Min(gl_spe_Attrs_k.longevity, Agerangeage2) / Program.sites.TimeStep;

                                    for (int m = beg; m <= end; m++)
                                    {
                                        float local_term1 = Program.sites.GetGrowthRates(k, m, l.ltID);
                                        uint local_term2 = (uint)Program.sites[(int)i, (int)j].SpecieIndex(k).getTreeNum(m, k);

                                        double combination1 = Math.Exp(tmp_term1 + tmp_term2 * Math.Log(local_term1)) * local_term2;

                                        if (local_term1 >= Biomass_threshold)
                                            TmpBiomassT += combination1 / 1000.00;

                                        TmpBasalAreaT += local_term1 * local_term1 * local_const * local_term2;

                                        TmpTreeT += (int)local_term2;

                                        TmpCarbon += combination1;

                                        TmpRDTotal += Math.Pow((local_term1 / 25.4), 1.605) * local_term2;
                                    }

                                    TmpRDTotal *= local_speciesattr.MaxAreaOfSTDTree / cellsize_square;

                                }

                                TmpCarbonTotal += TmpCarbon * gl_spe_Attrs_k.CarbonCoef;

                            }


                        }

                        long local_index = (row_num - i) * col_num + j - 1;

                        if (Bio_endagerange_flag != 0)
                            pafScanline1[local_index] = (float)TmpBiomassT;

                        if (Car_endagerange_flag != 0)
                            pafScanline4[local_index] = (float)TmpCarbonTotal / 1000.00f;

                        if (BA_endagerange_flag != 0)
                            pafScanline3[local_index] = (float)TmpBasalAreaT;

                        if (RDenendagerange_flag != 0)
                            pafScanline5[local_index] = (float)TmpRDTotal;

                        if (TPA_endagerange_flag != 0)
                            pintScanline[local_index] = TmpTreeT;

                    }

                }



                if (Bio_endagerange_flag != 0)
                {
                    outPoBand1.WriteRaster(0, 0, col_num, row_num, pafScanline1, col_num, row_num, 0, 0);

                    if (poDstDS1 != null)
                        poDstDS1.Dispose();
                }

                if (Car_endagerange_flag != 0)
                {
                    outPoBand4.WriteRaster(0, 0, col_num, row_num, pafScanline4, col_num, row_num, 0, 0);

                    if (poDstDS4 != null)
                        poDstDS4.Dispose();
                }

                if (BA_endagerange_flag != 0)
                {
                    outPoBand3.WriteRaster(0, 0, col_num, row_num, pafScanline3, col_num, row_num, 0, 0);

                    if (poDstDS3 != null)
                        poDstDS3.Dispose();
                }

                if (TPA_endagerange_flag != 0)
                {
                    outPoBand2.WriteRaster(0, 0, col_num, row_num, pintScanline, col_num, row_num, 0, 0);

                    if (poDstDS2 != null)
                        poDstDS2.Dispose();
                }

                if (RDenendagerange_flag != 0)
                {
                    outPoBand5.WriteRaster(0, 0, col_num, row_num, pafScanline5, col_num, row_num, 0, 0);

                    if (poDstDS5 != null)
                        poDstDS5.Dispose();
                }

            }


            pafScanline = null;
            pafScanline1 = null;
            pafScanline2 = null;
            pafScanline3 = null;
            pafScanline4 = null;
            pafScanline5 = null;


            papszOptions = null;
            pintScanline = null;

            DateTime a2 = DateTime.Now;
            Console.WriteLine("Finish 7.0 Style writing output at {0}", a2);

            Console.WriteLine("it took {0} ", a2 - a1);

        }

        public static void SetAgeDistoutputBuffer(int type, int specIndex, int ageIndex, int yearIndex, double value)
        {
            if (Program.sites.Flag_AgeDistStat == 0)
                return;

            int dimension = 500 / Program.sites.TimeStep;

            int index = (specIndex - 1) * dimension * dimension + (ageIndex - 1) * dimension + yearIndex - 1;

            switch (type)
            {
                case (int)Sites.BA: AgeDistOutputBuffer_BA[index] = value; break;
                case (int)Sites.TPA: AgeDistOutputBuffer_TPA[index] = value; break;
            }
        }

        public static void SetAgeDistoutputBuffer_Landtype(int type, int specIndex, int ageIndex, int yearIndex, int landtypeIndex, double value)
        {
            if (Program.sites.Flag_AgeDistStat == 0)
                return;

            int dimension = 500 / Program.sites.TimeStep;

            int doubl_dim = dimension * dimension;

            long index = landtypeIndex * Program.sites.specNum * doubl_dim + (specIndex - 1) * doubl_dim + (ageIndex - 1) * dimension + yearIndex - 1;

            switch (type)
            {
                case (int)Sites.BA: AgeDistOutputBuffer_BA_landtype[index] = value; break;
                case (int)Sites.TPA: AgeDistOutputBuffer_TPA_landtype[index] = value; break;
            }
        }

        public static void putOutput_AgeDistStat(int itr)
        {
            if (Program.sites.Flag_AgeDistStat == 0)
                return;
            int age1 = 0, age2 = 0, year = 0;
            //double TmpBasalAreaS = 0;
            double local_const = 3.1415926 / (4 * 10000.00);
            int itr_m_timestep = itr * Program.sites.TimeStep;
            for (int k = 1; k <= Program.sites.specNum; ++k)
            {
                int age_range = Program.sites.GetAgeDistStat_AgeRangeCount(k - 1);

                int end_year = Program.sites.GetAgeDistStat_YearCount(k - 1);

                int local_longevity = Program.speciesAttrs[k].longevity; 

                for (int count_age = 1; count_age <= age_range; count_age++)
                {
                    Program.sites.GetAgeDistStat_AgeRangeVal(k - 1, count_age, ref age1, ref age2);
                    int beg = Math.Min(local_longevity, age1) / Program.sites.TimeStep;
                    int end = Math.Min(local_longevity, age2) / Program.sites.TimeStep;
                    for (int count_year = 1; count_year <= end_year; count_year++)
                    {
                        Program.sites.GetAgeDistStat_YearVal(k - 1, count_year, ref year);
                        if (itr_m_timestep == year)
                        {
                            double TmpBasalAreaS = 0;
                            uint TmpTreesS = 0;
                            for (uint i = (uint)Program.sites.numRows(); i > 0; --i)
                            {
                                for (uint j = 1; j <= Program.sites.numColumns(); ++j)
                                {
                                    Landunit l = Program.sites.locateLanduPt((int)i, (int)j);
                                    Specie local_specie = Program.sites[(int)i, (int)j].SpecieIndex(k);
                                    for (int m = beg; m <= end; m++)
                                    {
                                        float local_grow_rate = Program.sites.GetGrowthRates(k, m, l.ltID);
                                        TmpBasalAreaS += local_grow_rate * local_grow_rate * local_const * local_specie.getTreeNum(m, k);
                                        TmpTreesS += (uint)local_specie.getTreeNum(m, k);
                                    }
                                }
                            }
                            SetAgeDistoutputBuffer((int)Sites.BA, k, count_age, count_year, TmpBasalAreaS);
                            SetAgeDistoutputBuffer((int)Sites.TPA, k, count_age, count_year, TmpTreesS);
                        }
                    }
                }


                // for (int count_age = 1; count_age <= age_range; count_age++)
                // {
                //     succession.gl_sites.GetAgeDistStat_AgeRangeVal(k - 1, count_age, ref age1, ref age2);

                //     int beg = Math.Min(local_longevity, age1) / time_step;
                //     int end = Math.Min(local_longevity, age2) / time_step;

                //     for (int count_year = 1; count_year <= end_year; count_year++)
                //     {
                //         succession.gl_sites.GetAgeDistStat_YearVal(k - 1, count_year, ref year);

                //         if (itr_m_timestep == year)
                //         {
                //             uint TmpTreesS = 0;

                //             for (uint i = snr; i > 0; i--)
                //             {
                //                 for (uint j = 1; j <= snc; j++)
                //                 {
                //                     specie local_specie = succession.gl_sites[i, j].SpecieIndex(k);

                //                     for (int m = beg; m <= end; m++)
                //                     {
                //                         TmpTreesS += local_specie.getTreeNum(m, k);
                //                     }

                //                 }

                //             }

                //             SetAgeDistoutputBuffer((int)enum1.TPA, k, count_age, count_year, TmpTreesS);
                //         }

                //     }
                // }
            }


            for (int count_landtype = 0; count_landtype < Program.landUnits.Number(); count_landtype++)
            {
                for (int k = 1; k <= Program.sites.specNum; k++)
                {
                    int age_range = Program.sites.GetAgeDistStat_AgeRangeCount(k - 1);
                    int end_year = Program.sites.GetAgeDistStat_YearCount(k - 1);
                    int local_longevity = Program.speciesAttrs[k].longevity;
                    for (int count_age = 1; count_age <= age_range; count_age++)
                    {
                        Program.sites.GetAgeDistStat_AgeRangeVal(k - 1, count_age, ref age1, ref age2);
                        int beg = Math.Min(local_longevity, age1) / Program.sites.TimeStep;
                        int end = Math.Min(local_longevity, age2) / Program.sites.TimeStep;
                        for (int count_year = 1; count_year <= end_year; count_year++)
                        {
                            Program.sites.GetAgeDistStat_YearVal(k - 1, count_year, ref year);
                            if (itr_m_timestep == year)
                            {
                                double TmpBasalAreaS = 0;
                                uint TmpTreesS = 0;
                                for (uint i = (uint)Program.sites.numRows(); i > 0; i--)
                                {
                                    for (uint j = 1; j <= Program.sites.numColumns(); j++)
                                    {
                                        Landunit l = Program.sites.locateLanduPt((int)i, (int)j);
                                        Specie local_specie = Program.sites[(int)i, (int)j].SpecieIndex(k);
                                        if (Program.sites.locateLanduPt((int)i, (int)j) == Program.landUnits[count_landtype])
                                        {
                                            for (int m = beg; m <= end; m++)
                                            {
                                                float local_grow_rate = Program.sites.GetGrowthRates(k, m, l.ltID);
                                                TmpBasalAreaS += local_grow_rate * local_grow_rate * local_const * local_specie.getTreeNum(m, k);
                                                TmpTreesS += (uint)local_specie.getTreeNum(m, k);
                                            }
                                        }
                                    }
                                }
                                SetAgeDistoutputBuffer_Landtype((int)Sites.BA, k, count_age, count_year, count_landtype, TmpBasalAreaS);
                                SetAgeDistoutputBuffer_Landtype((int)Sites.TPA, k, count_age, count_year, count_landtype, TmpTreesS);
                            }
                        }
                    }


                    // for (int count_age = 1; count_age <= age_range; count_age++)
                    // {
                    //     succession.gl_sites.GetAgeDistStat_AgeRangeVal(k - 1, count_age, ref age1, ref age2);

                    //     int beg = Math.Min(local_longevity, age1) / time_step;
                    //     int end = Math.Min(local_longevity, age2) / time_step;

                    //     for (int count_year = 1; count_year <= end_year; count_year++)
                    //     {
                    //         succession.gl_sites.GetAgeDistStat_YearVal(k - 1, count_year, ref year);

                    //         if (itr_m_timestep == year)
                    //         {
                    //             uint TmpTreesS = 0;

                    //             for (uint i = snr; i > 0; i--)
                    //             {
                    //                 for (uint j = 1; j <= snc; j++)
                    //                 {
                    //                     specie local_specie = succession.gl_sites[i, j].SpecieIndex(k);

                    //                     if (succession.gl_sites.locateLanduPt(i, j) == succession.gl_landUnits[count_landtype])
                    //                     {
                    //                         for (int m = beg; m <= end; m++)
                    //                         {
                    //                             TmpTreesS += local_specie.getTreeNum(m, k);
                    //                         }
                    //                     }
                    //                 }
                    //             }

                    //             SetAgeDistoutputBuffer_Landtype((int)enum1.TPA, k, count_age, count_year, count_landtype, TmpTreesS);
                    //         }
                    //     }
                    // }
                }

            }
        }


        public static void deleteRedundantInitial(int[] combineMatrix, int numCovers)
        {
            for (int i = numCovers - 1; i >= 0; i--)
            {
                if (i != combineMatrix[i])
                {
                    Program.sites.SortedIndex[i] = null;
                    Program.sites.SortedIndex.RemoveAt(i);
                }
            }
        }

        public static void lookupredundant(int[] combineMatrix, int numCovers)
        {
            for (int i = 0; i < numCovers - 1; i++)
            {
                if (i == combineMatrix[i])
                {
                    Site siteI = Program.sites.SortedIndex[i];

                    for (int j = i + 1; j < numCovers; j++)
                    {
                        Site siteJ = Program.sites.SortedIndex[j];
                        if (j == combineMatrix[j])
                        {
                            int ifequal = Program.sites.SITE_compare(siteI, siteJ);
                            if (ifequal == 0)
                                combineMatrix[j] = i;
                        }

                    }

                }

            }

        }

        public static void inputImgSpec(StreamReader classFile, Dataset simgFileint, int yDim, int xDim)
        {
            int coverType;
            int numCovers = 0;
            int noDataValue;
            double[] pafScanline;
            Site[] s;
            Site[] site;
            int[] combineMatrix;
            int adMax;
            int bGotMax;
            int max = -100;

            Band poBand;
            poBand = simgFileint.GetRasterBand(1);
            double val;
            int hasval;
            poBand.GetMaximum(out val, out hasval);
            adMax = (int)val;
            //Console.WriteLine("max value is: " + adMax);

            pafScanline = new double[xDim * yDim];
            poBand.ReadRaster(0, 0, xDim, yDim, pafScanline, xDim, yDim, 0, 0);
            poBand.GetNoDataValue(out val, out hasval);
            noDataValue = (int)val;
            //Console.WriteLine("nodatavalue is: " + noDataValue);
            //Console.Read();

            Site temp = new Site();
            while (classFile.Peek() >= 0)
            {
                temp.Read(classFile);
                numCovers++;
            }

            classFile.Close();
            classFile = new StreamReader(Program.parameters.reclassInFile);

            s = new Site[numCovers];
            for (int i = 0; i < numCovers; i++)
            {
                s[i] = new Site();
                s[i].Read(classFile);
            }

            combineMatrix = null;
            //for debug?
            if (Program.sites.Pro0or401 == 0)
            {
                Program.sites.SortedIndex = new List<Site>();
                site = new Site[numCovers];
                for (int i = 0; i < numCovers; i++)
                {
                    site[i] = new Site();
                    site[i].copy(s[i]);
                    Program.sites.SortedIndex.Add(site[i]);
                }
                combineMatrix = new int[numCovers];
                for (int i = 0; i < numCovers; i++)
                {
                    combineMatrix[i] = i;
                }
            }
            if (Program.sites.Pro0or401 == 0) //for debug?
            {
                lookupredundant(combineMatrix, numCovers);
            }

            Console.WriteLine("Number of attribute classes is: " + numCovers);
            for (int i = yDim; i > 0; i--)
            {
                for (int j = 1; j <= xDim; j++)
                {
                    coverType = (int)(pafScanline[(yDim - i) * xDim + j - 1]);
                    if (coverType == noDataValue)
                    {
                        Program.sites.fillinSitePt(i, j, Program.sites.SortedIndex[combineMatrix[0]]);
                    }
                    else if (coverType < numCovers && coverType >= 0)
                    {
                        if (Program.sites.Pro0or401 == 0)
                        {
                            Site local_site = Program.sites.SortedIndex[combineMatrix[coverType]];
                            Program.sites.fillinSitePt(i,j,local_site);
                            local_site.numofsites++;
                        }
                        else
                        {
                            Program.sites[i, j] = new Site();
                            Program.sites[i, j].copy(s[coverType]);
                        }
                    }
                    else
                    {
                        s = null;
                        Console.WriteLine("CoverType: " + coverType);
                        throw new Exception("Error reading in coverType from the map file");
                    }

                }
            }

            //for debug ?
            if (Program.sites.Pro0or401 == 0)
                {
                    Console.WriteLine("releasing redundant memory");
                    deleteRedundantInitial(combineMatrix, numCovers);
                    Program.sites.SITE_sort();
                    combineMatrix = null;
                }

            s = null;
        }

        public static void inputLandtypeImg(Dataset ltimgFile, int xDim, int yDim)
        {
            int nRows;
            int nCols;
            int numRead;
            int coverType;
            int noDataValue;

            Site[] s;
            int intdata;
            double[] pafScanline;
            Band poBand;

#if __UNIX__
		ERDi4_c(dest[4], nCols);
		ERDi4_c(dest[5], nRows);
#else
            nCols = ltimgFile.RasterXSize;
            nRows = ltimgFile.RasterYSize;
#endif
            if ((nCols != xDim) && (nRows != yDim))
            {
                throw new Exception("landtype map and species map do not match.");
            }

            pafScanline = new double[xDim * yDim];
            poBand = ltimgFile.GetRasterBand(1);
            poBand.ReadRaster(0, 0, nCols, nRows, pafScanline, nCols, nRows, 0, 0);

            double val;
            int hasval;
            poBand.GetNoDataValue(out val, out hasval);
            noDataValue = (int)val;
            //Console.WriteLine("nodataVaule is: " + noDataValue);


            for (int i = yDim; i > 0; i--)
            {
                for (int j = 1; j <= xDim; j++)
                {
                    coverType = (int)pafScanline[(yDim - i) * xDim + j - 1]; //*
                    if (coverType == noDataValue)
                    {
                        Program.sites.fillinLanduPt(i, j, Program.landUnits[0]);
                    }
                    else if (coverType >= 0)
                    {
                        Program.sites.fillinLanduPt(i, j, Program.landUnits[coverType]);

                        //if ((Program.gDLLMode & Defines.G_WIND) != 0)
                        //{
                        //    (ppdp.sTSLWind)[i][j] = (short)sites.locateLanduPt(i, j).initialLastWind;
                        //}
                    }
                    else
                    {
                        Console.Write("i = ");
                        Console.Write(i);
                        Console.Write("j = ");
                        Console.Write(j);
                        Console.Write("coverType =");
                        Console.Write(coverType);
                        Console.Write("\n");
                        Console.WriteLine("illegal landtype class found3.");
                    }
                }
            }

        }

        public static void getInput(StreamReader infile, int[] freq, string[] reMethods, string[] ageMap, ref PDP ppdp, int BDANo, double[] wAdfGeoTransform)
        //This will read in all LANDIS global variables.
        {
            int colNum; //Nim: added this line
            int x;

            StreamReader saFile;
            StreamReader luFile;
            StreamReader rcFile;
            StreamReader palleteFile;
            StreamReader freqOfOutput;
            StreamReader bioMass;

            int i;
            int nRows;
            int nCols;
            int packType;
            int nRows1;
            int nCols1;

            uint[] dest = new uint[64];

            string str = new string(new char[100]);

            Dataset simgFile;
            Dataset ltimgFile;

            double[] adfGeoTransform = new double[6];

            Gdal.AllRegister();

            Console.WriteLine("Reading input.");

            if ((Directory.CreateDirectory(Program.parameters.outputDir)) == null)
                throw new Exception("BDAS: Can't create the direcory");

            if ((saFile = new StreamReader(Program.parameters.specAttrFile)) == null)
                throw new Exception("Species attribute file not found.");

            if ((luFile = new StreamReader(Program.parameters.landUnitFile)) == null)
                throw new Exception("Landtype attribute file not found.");

            if ((ltimgFile = Gdal.Open(Program.parameters.landImgMapFile, Access.GA_ReadOnly)) == null)
            {
                throw new Exception("landtype img map input file not found.");
            }
            if ((simgFile = Gdal.Open(Program.parameters.siteImgFile, Access.GA_ReadOnly)) == null)
            {
                throw new Exception("species img map input file not found.");
            }

            simgFile.GetGeoTransform(adfGeoTransform);
            for (i = 0; i < 6; i++)
            {
                wAdfGeoTransform[i] = adfGeoTransform[i];
            }


            if ((rcFile = new StreamReader(Program.parameters.reclassInFile)) == null)
                throw new Exception("Map attribute input file not found.");

            if ((bioMass = new StreamReader(Program.parameters.Biomassfile)) == null)
                throw new Exception("BioMassfile not found.");

            Program.speciesAttrs.Read(saFile, Program.parameters.cellSize);

            Program.time_step.getSpecNum(Program.speciesAttrs.Number());

            for (x = 1; x <= Program.speciesAttrs.Number(); x++)
            {
                Program.time_step.Setlongevity(x, Program.speciesAttrs.Operator(x).longevity);
            }



            if ((Program.gDLLMode & Defines.G_HARVEST) != 0)
            {
                Program.numberOfSpecies = Program.speciesAttrs.Number();
            }

            Program.landUnits.attach(Program.speciesAttrs);
            Program.landUnits.Read(luFile);
            Program.sites.BiomassRead(bioMass);
            Species.Attach(Program.speciesAttrs);
         
#if __UNIX__

ERDi4_c(dest[4], nCols);

ERDi4_c(dest[5], nRows);

#else
            nCols = simgFile.RasterXSize;
            nRows = simgFile.RasterYSize;
#endif
           
            Program.sites.MaxDistofAllSpec = Program.speciesAttrs.MaxDistanceofAllSpecs;
            Program.sites.MaxShadeTolerance = Program.speciesAttrs.MaxShadeTolerance;          
            Program.sites.dim(Program.speciesAttrs.Number(), nRows, nCols);
            Program.sites.Read70OutputOption(Program.parameters.OutputOption70);
           
            char b16or8;
            ppdp = new PDP(Program.gDLLMode, Program.sites.numColumns(), Program.sites.numRows(), BDANo);
           
#if __UNIX__
            if (packType == 512)
            {
            inputBin16(rcFile, siFile, nRows, nCols); // need to change Qia Oct 06 2008
            }
            else
            {
            inputBin8(rcFile, siFile, nRows, nCols, b16or8); // need to change Qia Oct 06 2008
            }
#else
            inputImgSpec(rcFile, simgFile, nRows, nCols);
#endif
            
           inputLandtypeImg(ltimgFile, nCols, nRows);

           if ((Program.gDLLMode & Defines.G_HARVEST) != 0)
           {
                GlobalFunctions.HarvestPassInit(Program.sites, Program.numberOfSpecies, Program.parameters.outputDir, Program.parameters.strHarvestInitName, ppdp);
           }

           saFile.Close();
           luFile.Close();
           rcFile.Close();
           bioMass.Close();

           if ((freqOfOutput = new StreamReader(Program.parameters.freq_out_put)) != null)
           {
                int c = 0;
                string instring;
                string[] sarray;
                freqOfOutput.ReadLine();
                freqOfOutput.ReadLine();
                freqOfOutput.ReadLine();
                while (freqOfOutput.Peek() >= 0)
                {
                    instring = freqOfOutput.ReadLine();
                    sarray = instring.Split('#');

                    freq[c] = int.Parse(sarray[2]);

                    if (freq[c] > Program.parameters.numberOfIterations)
                    {
                        Console.WriteLine("frequency value cannot be larger than number of iterations");
                    }
                    if (freq[c] < 0)
                    {
                        Console.WriteLine("frequency value cannot be smaller than zero");
                    }
                    c++;
                }

                Console.WriteLine();
                if (freq[0] <= 1)
                {
                    freq[0] = 1;
                    Console.WriteLine("{0}{1:D}{2}");
                    Console.WriteLine("Species maps output every------->" + freq[0] * Program.sites.TimeStep + " years.");
                    Console.WriteLine("{0}{1:D}{2}");
                    Console.WriteLine("Age maps output every----------->" + freq[0] * Program.sites.TimeStep + " years.");
                }
                else
                {
                    Console.WriteLine("{0}{1:D}{2}\n" + "Species map outputs for year---->" + freq[0] * Program.sites.TimeStep + ".");
                    Console.WriteLine("{0}{1:D}{2}\n" + "Age map outputs for year-------->" + freq[0] * Program.sites.TimeStep + ".");
                }

                if (freq[4] <= 1)
                {
                    freq[4] = 1;
                    Console.WriteLine("{0}{1:D}{2}\n" + "Age group maps output every----->" + freq[4] * Program.sites.TimeStep + " years.");
                }
                else
                {
                    Console.WriteLine("{0}{1:D}{2}\n" + "Age group map outputs for year-->" + freq[4] * Program.sites.TimeStep + ".");
                }
                Console.WriteLine();
                freqOfOutput.Close();
           }
           else
           {
                freq[0] = 1;
                freq[1] = 1;
                freq[2] = 1;
                freq[3] = 1;
                freq[4] = 1;
                if ((Program.gDLLMode & Defines.G_HARVEST) != 0)
                {
                    freq[5] = 1;
                }

                Console.WriteLine("file <FREQ_OUT.PUT> not found");
                Console.WriteLine("creating file");
                Console.WriteLine("frequency of output unknown assuming every iteration");

                StreamWriter freqOfOutputW = new StreamWriter("freq_out.put");
                if ((freqOfOutputW) != null)
                {
                    freqOfOutputW.WriteLine("#This file establishes the number of years for output for reclass methods#");
                    freqOfOutputW.WriteLine("#fire, wind, timber, and age class. A one (1) in any field will produce maps#");
                    freqOfOutputW.WriteLine("#every iteration#");
                    freqOfOutputW.WriteLine("#output maps for reclass#  2");
                    freqOfOutputW.WriteLine("#output maps for fire# 1");
                    freqOfOutputW.WriteLine("#output maps for wind# 1");
                    freqOfOutputW.WriteLine("#output maps for timber# 1");
                    freqOfOutputW.WriteLine("#output maps for age class# 2");
                    if ((Program.gDLLMode & Defines.G_HARVEST) != 0)
                    {
                        freqOfOutputW.WriteLine("#output maps for harvest# 1");
                    }
                    freqOfOutputW.Close();
                }
           }

           //if ((gDLLMode & G_WIND) != 0)
           //{

           //    for (i = 2; i < 16; i++)
           //    {
           //        HSV_to_RGB((float)(i - 2) / (float)14.0 * (float)360.0, 1.0, 1.0, red2[i], green2[i], blue2[i]);

           //    }
           //}

          
#if (TEST)
        //Write landtype map
        MAP8 m = new MAP8(sites.getHeader());
        luReclass(m);
        str = string.Format("{0}/lu", parameters.outputDir);        
        m.setCellSize(parameters.cellSize);
        m.write(str, red, green, blue, wAdfGeoTransform);      
#endif
        }

        public static void OutputScenario()
        {
            Console.Write("Output Landis Scenario.txt....\n\n");
            string strScenario = "";
            strScenario = string.Format("{0}/{1}", Program.parameters.outputDir, "Scenario.txt");
            StreamWriter pfScenario = new StreamWriter(strScenario);

            pfScenario.WriteLine("Landis Scenario.txt\n");
            pfScenario.WriteLine("Landis version:		Pro 1.0");
            pfScenario.WriteLine("Output dir:		" + Program.parameters.outputDir);
            pfScenario.WriteLine("specAttrFile:		" + Program.parameters.outputDir + "\\" + Program.parameters.specAttrFile);
            pfScenario.WriteLine("landUnitFile:		" + Program.parameters.outputDir + "\\" + Program.parameters.landUnitFile);
            pfScenario.WriteLine("siteInFile:		" + Program.parameters.outputDir + "\\" + Program.parameters.siteInFile);
            pfScenario.WriteLine("\nMAIN PARAMETERS:----------------------------------------");

            if (Program.parameters.randSeed == 1)
            {
                pfScenario.WriteLine("Random:			repeatable (1)");
            }
            else
            {
                pfScenario.WriteLine("Random:			NOT repeatable (0)");
            }

            pfScenario.WriteLine("numberOfIterations:	" + Program.parameters.numberOfIterations);
            pfScenario.WriteLine("Map size:		Row: " + Program.sites.numRows() + " x Col: " + Program.sites.numColumns());

            pfScenario.WriteLine("\n\nDLLs----------------------------------------");
            if ((Program.gDLLMode & Defines.G_BDA) != 0)
            {
                pfScenario.WriteLine("BDA ...............is turned on");
            }
            if ((Program.gDLLMode & Defines.G_WIND) != 0)
            {
                pfScenario.WriteLine("Wind ..............is turned on");
            }
            if ((Program.gDLLMode & Defines.G_HARVEST) != 0)
            {
                pfScenario.WriteLine("Harvest ...........is turned on");
            }
            if ((Program.gDLLMode & Defines.G_FUEL) != 0)
            {
                pfScenario.WriteLine("Fuel ..............is turned on");
            }
            if ((Program.gDLLMode & Defines.G_FUELMANAGEMENT) != 0)
            {
                pfScenario.WriteLine("Fuel management....is turned on");
            }
            if ((Program.gDLLMode & Defines.G_FIRE) != 0)
            {
                pfScenario.WriteLine("Fire ..............is turned on");
            }

            pfScenario.WriteLine();

            if ((Program.gDLLMode & Defines.G_BDA) == 0)
            {
                pfScenario.WriteLine("BDA ....................  off");
            }
            if ((Program.gDLLMode & Defines.G_WIND) == 0)
            {
                pfScenario.WriteLine("Wind .................... off");
            }
            if ((Program.gDLLMode & Defines.G_HARVEST) == 0)
            {
                pfScenario.WriteLine("Harvest ..................off");
            }
            if ((Program.gDLLMode & Defines.G_FUEL) == 0)
            {
                pfScenario.WriteLine("Fuel .................... off");
            }
            if ((Program.gDLLMode & Defines.G_FUELMANAGEMENT) == 0)
            {
                pfScenario.WriteLine("Fuel management ..........off");
            }
            if ((Program.gDLLMode & Defines.G_FIRE) == 0)
            {
                pfScenario.WriteLine("Fire .................... off");
            }

            pfScenario.Close();

        }

        public static void AgeDistOutputBufferInitialize(int SpecNum, int LandTypeNum)
        {
            if (Program.sites.Flag_AgeDistStat == 0)
                return;

            long leng = SpecNum * 500 / Program.sites.TimeStep * 500 / Program.sites.TimeStep;

            AgeDistOutputBuffer_TPA = new double[leng];
            AgeDistOutputBuffer_BA = new double[leng];

            AgeDistOutputBuffer_TPA_landtype = new double[leng * LandTypeNum];
            AgeDistOutputBuffer_BA_landtype = new double[leng * LandTypeNum];

        }

        public static void initiateOutput_landis70Pro()
        {
            StreamReader fpBiomass = null;
            StreamReader fpBasalArea = null;
            StreamReader fpTrees = null;
            StreamReader fpOutputSpecieNames = null;
            string filename = "";
            string str = "";

            if (Program.reMethods[0] != "N/A")
            {
                for (int i = 1; i <= Program.sites.specNum; i++)
                {
                    filename = string.Format("{0}_Bio", Program.speciesAttrs[i].name);
                    BioMassFileNames.Add(filename);
                }
                flagoutputBiomass = 1;
            }
            else
            {
                flagoutputBiomass = 0;
            }
            if (Program.reMethods[1] != "N/A")
            {
                for (int i = 1; i <= Program.sites.specNum; i++)
                {
                    filename = string.Format("{0}_BA", Program.speciesAttrs[i].name);
                    BasalFileNames.Add(filename);
                }
                flagoutputBasal = 1;
            }
            else
            {
                flagoutputBasal = 0;
            }
            if (Program.reMethods[2] != "N/A")
            {
                for (int i = 1; i <= Program.sites.specNum; i++)
                {
                    filename = string.Format("{0}_TreeNum", Program.speciesAttrs[i].name);
                    TreesFileNames.Add(filename);
                }
                flagoutputTrees = 1;
            }
            else
            {
                flagoutputTrees = 0;
            }

            for (int i = 1; i <= Program.sites.specNum; i++)
            {
                filename = string.Format("{0}_IV", Program.speciesAttrs[i].name);
                IVFileNames.Add(filename);
                filename = string.Format("{0}_AvailableSeed", Program.speciesAttrs[i].name);
                SeedsFileNames.Add(filename);
                filename = string.Format("{0}_RelativeDensity", Program.speciesAttrs[i].name);
                RDFileNames.Add(filename);
            }
            flagoutputIV = 1;
            for (int i = 1; i <= Program.sites.specNum; i++)
            {
                filename = string.Format("{0}_DBH", Program.speciesAttrs[i].name);
                DBHFileNames.Add(filename);
            }
            flagoutputDBH = 1;

            if (fpBiomass != null)
            {
                fpBiomass.Close();
            }
            if (fpBasalArea != null)
            {
                fpBasalArea.Close();
            }
            if (fpTrees != null)
            {
                fpTrees.Close();
            }
            AgeDistOutputBufferInitialize(Program.sites.specNum, Program.landUnits.Number());
        }
    }
}
