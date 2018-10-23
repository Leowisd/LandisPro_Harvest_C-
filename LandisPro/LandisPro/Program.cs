using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LandisPro
{
    class Program
    {
        private static string fpforTimeBU_name = null;
        private static string fpLogFileSEC_name = null;
        private static List<string> SEC_landtypefiles = new List<string>();
        private static List<string> SEC_gisfiles = new List<string>();
        private static PDP pPDP = new PDP();

        public static SpeciesAttrs speciesAttrs = new SpeciesAttrs(Defines.MAX_SPECIES);
        public static Params parameters = new Params();
        public static TimeStep time_step = new TimeStep();
        public static Sites sites = new Sites();
        public static Landunits landUnits = new Landunits(Defines.MAX_LANDUNITS);

        public static string[] reMethods = new string[Defines.MAX_RECLASS];
        public static string[] ageMaps = new string[Defines.MAX_SPECIES];
        public static int gDLLMode;
        public static int numberOfSpecies;
        public static int snr;
        public static int snc;
        public static int run_extraSeed = 0;
        public static int specAtNum;
        public static int numbOfIter;
        public static int numSitesActive;
        public static StreamWriter fpforTimeBU;

        public static Land_type_Attributes gl_land_attrs = null;

        static void Main(string[] args)
        {

            /*
             * Variable declaration
             */
            StreamReader infile = new StreamReader("parameterbasic.dat");
            gDLLMode = 0;
            int BDANo = 0;
            int[] freq = new int[6];
            int reclYear = 0;
            /*
             * Get DLL mode and read from parameters.dat
             */
            DateTime now = DateTime.Now;
            gDLLMode = parameters.Read(infile);
            time_step.gettimestep(parameters.timestep);
            sites.stocking_x_value = parameters.stocking_x_value;
            sites.stocking_y_value = parameters.stocking_y_value;
            sites.stocking_z_value = parameters.stocking_z_value;
            sites.TimeStep = parameters.timestep;
            sites.TimeStep_Harvest = parameters.timestep_Harvest;
            sites.CellSize = parameters.cellSize;

            for (int x = 0; x < 5; x++)
                freq[x] = 1;
            if ((gDLLMode & Defines.G_HARVEST) != 0)
                freq[5] = 1;
            if ((gDLLMode & Defines.G_HARVEST) != 0)
            {
                Console.WriteLine("Harvest ");
            }
            Console.WriteLine("------------------------------------------------------------");


            /*
             * read in parameters and init PDP
             */
            double[] wAdfGeoTransform = new double[6];
            IO.getInput(infile, freq, reMethods, ageMaps, ref pPDP, BDANo, wAdfGeoTransform);
            /*
            * End read in parameters & init PDP
            */

            if ((gDLLMode & Defines.G_HARVEST) != 0)
            {
                Console.WriteLine("Harvest Dll loaded in...");
                //GlobalFunctions.HarvestPass(sites, speciesAttrs);
                //sites.Harvest70outputdim();
            }
            Console.WriteLine("Finish getting input");

            IO.OutputScenario();
            IO.initiateOutput_landis70Pro();

            snr = sites.numRows();
            snc = sites.numColumns();
            numSitesActive = sites.numActive();
            specAtNum = speciesAttrs.Number();
            numbOfIter = parameters.numberOfIterations;

            sites.GetSeedDispersalProbability(parameters.SeedRainFile, parameters.SeedRainFlag);
            sites.GetSpeciesGrowthRates(parameters.GrowthFlagFile, parameters.GrowthFlag);
            sites.GetSpeciesMortalityRates(parameters.MortalityFile, parameters.MortalityFlag);
            sites.GetVolumeRead(parameters.VolumeFile, parameters.VolumeFlag);

            initiateRDofSite_Landis70();

            if (reclYear != 0)
            {
                int local_num = reclYear / sites.TimeStep;
                reclass3.reclassify(reclYear, ageMaps);
                IO.putOutput(local_num, local_num, freq);
                IO.putOutput_Landis70Pro(local_num, local_num, freq);
                IO.putOutput_AgeDistStat(local_num);
                Console.Write("Ending Landispro Succession.\n");
            }
            else
            {
                IO.putOutput(0, 0, freq);
                IO.putOutput_Landis70Pro(0, 0, freq);
                IO.putOutput_AgeDistStat(0);
            }

            if (parameters.randSeed == 0)  //random
            {
                DateTime startTime = new DateTime(1970, 1, 1);
                parameters.randSeed = (int)Convert.ToUInt32(Math.Abs((DateTime.Now - startTime).TotalSeconds));
            }
            system1.fseed(parameters.randSeed);
            Console.WriteLine("parameters.randSeed = {0}", parameters.randSeed);

            fpforTimeBU_name = parameters.outputDir + "/Running_Time_Stat.txt";
            fpLogFileSEC_name = parameters.outputDir + "/SECLog.txt";

            var now2 = DateTime.Now;
            Console.WriteLine("\nFinish the initilization at {0}", now2);
            var ltimeDiff = now2 - now;
            Console.Write("it took {0} seconds\n", ltimeDiff);

            landUnits.initiateVariableVector(parameters.numberOfIterations,parameters.timestep, (uint)specAtNum, parameters.flagforSECFile);

            using (fpforTimeBU = new StreamWriter(fpforTimeBU_name))
            {
                fpforTimeBU.Write("Initilization took: {0} seconds\n", ltimeDiff);
            }


            //Simulation loops////////////////////////////////////////////////

            for (int i = 1; i <= numbOfIter * sites.TimeStep; i++)
            {
                if (i % sites.TimeStep == 0)
                {
                    if (parameters.flagforSECFile == 3)
                    {
                        int index = i/sites.TimeStep - 1;

                    if (index == 0)
                    {
                        SEC_landtypefiles.Clear();
                        SEC_gisfiles.Clear();
                        
                        if (index < gl_land_attrs.year_arr.Count)
                        {
                            Console.Write("\nEnvironment parameter Updated.\n");
                            string SECfileMapGIS = gl_land_attrs.Get_new_landtype_map(index);                        
                            parameters.landImgMapFile = SECfileMapGIS;
                            Console.WriteLine("\nEnvironment map Updated.");
                            Landunit SECLog_use = landUnits.first();
                            int ii_count = 0;
                            using (StreamWriter fpLogFileSEC = new StreamWriter(fpLogFileSEC_name))
                            {
                                fpLogFileSEC.Write("Year: {0}\n", i);

                                for (; ii_count < landUnits.Number(); ii_count++)
                                {
                                    fpLogFileSEC.Write("Landtype{0}:\n", ii_count);
                                    for (int jj_count = 1; jj_count <= specAtNum; jj_count++)
                                    {
                                        fpLogFileSEC.Write("spec{0}: {1:N6}, ", jj_count, SECLog_use.probRepro(jj_count));
                                    }
                                    SECLog_use = landUnits.next();
                                    fpLogFileSEC.Write("\n");
                                }
                            }
                        }
                    }

                    if (index > 0)
                    {
                        if (index < SEC_landtypefiles.Count)
                        {
                            parameters.landImgMapFile = gl_land_attrs.Get_new_landtype_map(index);
                            Console.WriteLine("\nEnvironment parameter Updated.");
                            Console.WriteLine("\nEnvironment map Updated.");
                            Landunit SECLog_use = landUnits.first();
                            using (StreamWriter fpLogFileSEC = new StreamWriter(fpLogFileSEC_name))
                            {
                                fpLogFileSEC.Write("Year: {0}\n", i);
                                int ii_count = 0;
                                for (; ii_count < landUnits.Number(); ii_count++)
                                {
                                    fpLogFileSEC.Write("Landtype{0}:\n", ii_count);
                                    for (int jj_count = 1; jj_count <= specAtNum; jj_count++)
                                        fpLogFileSEC.Write("spec{0}: {1:N6}, ", jj_count, SECLog_use.probRepro(jj_count));
                                    SECLog_use = landUnits.next();
                                    fpLogFileSEC.Write("\n");
                                }
                            }
                        }
                    }
                }

            }//end if


            if ((gDLLMode & Defines.G_HARVEST) != 0 && i % sites.TimeStep_Harvest == 0)
            {
                //GlobalFunctions.HarvestPassCurrentDecade(i);  //Global Function
                for (int r = 1; r <= snr; r++)
                {
                    for (int c = 1; c <= snc; c++)
                    {
                        //setUpdateFlags(r, c); //Global Function
                    }
                }
            }

            Console.WriteLine("Processing succession at Year {0}", i);
                  
            singularLandisIteration(i, pPDP);

            int i_d_timestep = i / sites.TimeStep;
            if (i % sites.TimeStep == 0 || i == numbOfIter * sites.TimeStep)
            {
                int[] frequency = new int[6] { 1, 1, 1, 1, 1, 1 };

                if (i % (sites.TimeStep * freq[0]) == 0 && i_d_timestep <= numbOfIter)
                {
                    IO.putOutput_Landis70Pro(0, i_d_timestep, freq);
                }
                if (i == sites.TimeStep * numbOfIter)
                {
                    IO.putOutput_Landis70Pro(0, numbOfIter, frequency);
                }
                if (i % (sites.TimeStep * freq[4]) == 0 && i_d_timestep <= numbOfIter)
                {
                    IO.putOutput(0, i_d_timestep, freq);
                }
                if (i == sites.TimeStep * numbOfIter)
                {
                    IO.putOutput(0, numbOfIter, frequency);
                }
                IO.putOutput_AgeDistStat(i_d_timestep);

            }


            }

            //Simulation loops end/////////////////////////////////////////////////


            Console.WriteLine("\n\nDebug finish!");
            Console.ReadLine();
        }

        public static void initiateRDofSite_Landis70()
        //initiating Landis70 RD values
        {
            int i, j;
            for (i = 1; i <= snr; i++)
            {
                for (j = 1; j <= snc; j++)
                {
                    sites.GetRDofSite(i, j);
                }
            }
        }

        public static void succession_Landis70(PDP ppdp, int itr)
        {
            sites.GetMatureTree();

            //increase ages
            for (uint i = 1; i <= snr; ++i)
            {
                for (uint j = 1; j <= snc; ++j)
                {
                    ppdp.addedto_sTSLMortality((int)i, (int)j, (short)sites.TimeStep);
                    //define land unit
                    Landunit l = sites.locateLanduPt((int)i, (int)j);

                    if (l != null && l.active())
                    {
                        Site local_site = sites[(int)i, (int)j];

                        for (int k = 1; k <= specAtNum; ++k)
                        {
                            local_site.SpecieIndex(k).GrowTree();
                        }

                    }

                }
                //Console.ReadLine();
            }

            //seed dispersal
            initiateRDofSite_Landis70();
            Console.WriteLine("Seed Dispersal:");


            for (uint i = 1; i <= snr; ++i)
            {
                //Console.WriteLine("\n{0}%\n", 100 * i / snr);

                for (uint j = 1; j <= snc; ++j)
                {
                    //Console.WriteLine("i = {0}, j = {1}", i, j);
                    Landunit l = sites.locateLanduPt((int)i, (int)j);

                    KillTrees(i, j);
                    if (itr == 90 && i == 193 && j == 156)
                        Console.WriteLine("watch: {0}:{1}", "kill trees", sites[193, 156].SpecieIndex(2).getAgeVector(1));
                    if (l != null && l.active())
                    {
                        float local_RD = (float)sites[(int)i, (int)j].RD;

                        if (local_RD < l.maxRDArray(0))

                            sites.SiteDynamics(0, i, j);

                        else if (local_RD >= l.maxRDArray(0) && local_RD < l.maxRDArray(1))

                            sites.SiteDynamics(1, i, j);

                        else if (local_RD >= l.maxRDArray(1) && local_RD <= l.maxRDArray(2))

                            sites.SiteDynamics(2, i, j);

                        else if (local_RD > l.maxRDArray(2) && local_RD <= l.maxRDArray(3))

                            sites.SiteDynamics(3, i, j);

                        else
                        {
                            //Debug.Assert(local_RD > l.MaxRDArray(3));
                            sites.SiteDynamics(4, i, j);
                        }
                    }
                }

            }
            Console.WriteLine("end succession_Landis70 once");
        }

        public static void KillTrees(uint local_r, uint local_c)
        {
            Site local_site = sites[(int)local_r, (int)local_c];
            for (int k = 1; k <= specAtNum; ++k)//sites.specNum
            {
                int longev = speciesAttrs[k].longevity;
                int numYears = longev / 5;
                float chanceMod = 0.8f / (numYears + 0.00000001f);
                float chanceDeath = 0.2f;
                int m_beg = (longev - numYears) / sites.TimeStep;
                int m_end = longev / sites.TimeStep;

                Specie local_specie = local_site.SpecieIndex(k);

                for (int m = m_beg; m <= m_end; m++)
                {
                    int tmpTreeNum = (int)local_specie.getTreeNum(m, k);

                    int tmpMortality = 0;

                    if (tmpTreeNum > 0)
                    {
                        float local_threshold = chanceDeath * sites.TimeStep / 10;

                        for (int x = 1; x <= tmpTreeNum; x++)
                        {
                            if (system1.frand() < local_threshold)
                                tmpMortality++;
                        }
                        local_specie.setTreeNum(m, k, Math.Max(0, tmpTreeNum - tmpMortality));
                    }

                    chanceDeath += chanceMod;

                }
            }
        }

        /////////////////////////////////////////////////////////////////////////////

        //                      SINGULAR LANDIS ITERATION ROUTINE                  //

        /////////////////////////////////////////////////////////////////////////////
        public static void singularLandisIteration(int itr, PDP ppdp)
        {
            DateTime ltime, ltimeTemp;
            TimeSpan ltimeDiff;


            using (StreamWriter fpforTimeBU = File.AppendText(fpforTimeBU_name))
            {
                fpforTimeBU.WriteLine("\nProcessing succession at Year: {0}:", itr);


                if ((gDLLMode & Defines.G_HARVEST) != 0 && itr % sites.TimeStep_Harvest == 0)
                {
                    Console.WriteLine("Processing harvest events.\n");
                    ltime = DateTime.Now;

                    //HarvestprocessEvents(itr / sites.TimeStep);  //Global Function

                    //putHarvestOutput(itr / sites.TimeStep_Harvest, wAdfGeoTransform); //output img files, is it necessary?

                    ltimeTemp = DateTime.Now;
                    ltimeDiff = ltimeTemp - ltime;
                    fpforTimeBU.WriteLine("Processing harvest: " + ltimeDiff +" seconds");
                }

                if (itr % sites.TimeStep == 0)
                {
                    ltime = DateTime.Now;

                    Console.WriteLine("Start succession ... at {0}", ltime);

                    system1.fseed(parameters.randSeed + itr / sites.TimeStep * 6);

                    landUnits.ReprodUpdate(itr / sites.TimeStep);
                    //Console.WriteLine("random number: {0}", system1.frand());
                    succession_Landis70(ppdp, itr);
                    //Console.WriteLine("random number: {0}", system1.frand());
                    ltimeTemp = DateTime.Now;

                    ltimeDiff = ltimeTemp - ltime;

                    Console.WriteLine("Finish succession at {0} sit took {1} seconds", DateTime.Now, ltimeDiff);

                    fpforTimeBU.WriteLine("Processing succession: {0} seconds", ltimeDiff);

                    fpforTimeBU.Flush();
                }
            }

            system1.fseed(parameters.randSeed);
        }
    }
}
