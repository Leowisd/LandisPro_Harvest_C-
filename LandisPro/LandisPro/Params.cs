#define OUTPUT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LandisPro
{
    class Params
    {
        public const int NO_DISPERSAL = 0; //No seed dispersal.
        public const int UNIFORM = 1; //Uniform seed dispersal.
        public const int NEIGHBORS = 2; //Seed to immediate neighbors.
        public const int DISPERSAL = 3; //Seed within effective distance.
        public const int RAND_ASYM = 4; //Seed using interpolated chaotic distances.
        public const int MAX_DIST = 5; //RAND_ASYM up to maximum distances.
        public const int SIM_RAND_ASYM = 6; //RAND_ASYM up to maximum distances, Simulated


        public string specAttrFile; //Customized By user year variances
        public string landUnitFile;
        public string landImgMapFile;
        public string siteInFile;
        public string siteImgFile;
        public string reclassInFile;
        public string outputDir;
        public string disturbance;
        public string default_plt;
        public string freq_out_put;
        public string OutputOption70;
        public string varianceSECFile;
        public int flagforSECFile; // 0,1,2, 3//0 is no change, 1 is pulse, 2 is random, 3 read from file 
        public FileStream FpSECfile;
        public int SeedRainFlag;
        public int GrowthFlag;
        public int MortalityFlag;
        public int VolumeFlag;
        public string SeedRainFile;
        public string GrowthFlagFile;
        public string MortalityFile;
        public string VolumeFile;
        public string Biomassfile;
        //#ifdef _FUEL_
        public string strFuelInitName;
        public int fuel;
        public int fuelFlag;
        public int fuelManagement; //for fuel management
        public FileStream fuelManageFile;
        public FileStream fuelManOutStand;
        //#endif
        public int timestep;
        public int timestep_BDA;
        public int timestep_Harvest;
        public int timestep_Fuel;
        public int timestep_Fire;
        public int timestep_Wind;
        public double stocking_x_value;
        public double stocking_y_value;
        public double stocking_z_value;
        public int fire; //Seeding regime: NO_DISPERSAL, UNIFORM, NEIGHBORS,
        public int harvest;
        public int standAdjacencyFlag;
        public int harvestDecadeSpan;
        public int numberOfIterations;
        public int numberOfReplicates;
        public int randSeed;
        public int cellSize;
        public int dispRegime;
        //#ifdef __HARVEST__ 
        public float harvestThreshold; //If the number of "recently harvested" sites on a
        //#endif
        //Wind
        public string strWindInitName; //Turn wind disturbances on/off
        //BDA
        public string strBDAInitName; //BDAInit.dat directory and file name
        //Harvest
        public string strHarvestInitName;
        public string strFireInitName;

        public int Read(StreamReader infile)
        //Read in all parameters from a file.
        {
            string dispType = new string(new char[40]);
            specAttrFile = null;
            landUnitFile = null;
            landImgMapFile = null; //* landtype.img
            siteImgFile = null; // *speciesmap3.img
            reclassInFile = null;
            OutputOption70 = null;
            outputDir = null;
            disturbance = null;
            freq_out_put = null;
            varianceSECFile = null;
            SeedRainFile = null;
            GrowthFlagFile = null;
            MortalityFile = null;
            Biomassfile = null;
            VolumeFile = null;
            //Wind
            strWindInitName = null;
            //BDA entry point: BDA Init file
            strBDAInitName = null; //BDA entry point
            //Fuel entry point: Fuel Init file
            strFuelInitName = null;
            //Haevset entry point: Harvest init file
            strHarvestInitName = null;
            //Fire entry point: Fire init file
            strFireInitName = null;
            string str = null;
            int dllmode = 0;

            if ((specAttrFile = infile.ReadLine()) == null)
            {
                throw new Exception("Error reading in specAttrFile from parameter file.");
            }


            if ((landUnitFile = infile.ReadLine()) == null)
            {

                throw new Exception("Error reading in landUnitFile from parameter file.");
            }


            if ((landImgMapFile = infile.ReadLine()) == null) //* landtype.img
            {
                throw new Exception("Error reading in  GIS File from parameter file."); //*
            }


            if ((siteImgFile = infile.ReadLine()) == null)//* speciesmap3.img
            {
                throw new Exception("Error reading in  GIS File from parameter file.");
            }


            if ((reclassInFile = infile.ReadLine()) == null)
            {
                throw new Exception("Error reading in reclassInFile from parameter file.");
            }


            if ((OutputOption70 = infile.ReadLine()) == null)
            {
                throw new Exception("Error reading in OutputOption70 from parameter file.");
            }


            if ((outputDir = infile.ReadLine()) == null)
            {
                throw new Exception("Error reading in outputDir from parameter file.");
            }


            if ((freq_out_put = infile.ReadLine()) == null)
            {
                throw new Exception("Error reading in freq.out.put from parameter file.");
            }


            if ((Biomassfile = infile.ReadLine()) == null)
            {
                throw new Exception("Error reading in biomassfile from parameter file.");
            }


            if ((str = infile.ReadLine()) != null)
            {
                numberOfIterations = int.Parse(str);
            }
            else
            {
                throw new Exception("Error reading in numberOfIterations from parameter file.");
            }


            if ((str = infile.ReadLine()) != null)
            {
                randSeed = int.Parse(str);
            }
            else
            {

                throw new Exception("Error reading in randSeed from parameter file.");
            }

            if ((str = infile.ReadLine()) != null)
            {
                cellSize = int.Parse(str);
            }
            else
            {
                throw new Exception("Error reading in cellSize from parameter file.");
            }

            if ((dispType = infile.ReadLine()) == null)
            {
                throw new Exception("Error reading in dispRegime from parameter file.");
            }

            if ((str = infile.ReadLine()) != null)
            {
                timestep = int.Parse(str);
            }
            else
            {
                throw new Exception("Error reading in timestep from parameter file.");
            }


            if ((str = infile.ReadLine()) != null)
            {
                timestep_Wind = int.Parse(str);
            }
            else
            {
                throw new Exception("Error reading in timestep from parameter file.");
            }


            if ((str = infile.ReadLine()) != null)
            {
                timestep_Fire = int.Parse(str);
            }
            else
            {
                throw new Exception("Error reading in timestep from parameter file.");
            }


            if ((str = infile.ReadLine()) != null)
            {
                timestep_BDA = int.Parse(str);
            }
            else
            {
                throw new Exception("Error reading in timestep from parameter file.");
            }


            if ((str = infile.ReadLine()) != null)
            {
                timestep_Fuel = int.Parse(str);
            }
            else
            {
                throw new Exception("Error reading in timestep from parameter file.");
            }


            if ((str = infile.ReadLine()) != null)
            {
                timestep_Harvest = int.Parse(str);
            }
            else
            {
                throw new Exception("Error reading in timestep from parameter file.");
            }




            if ((varianceSECFile = infile.ReadLine()) == null)
            {
                throw new Exception("Error reading in SEC parameter from parameter file.");
            }

            if (varianceSECFile.Equals("N/A"))
            {
                flagforSECFile = 0;
            }
            else if (varianceSECFile.Equals("0"))
            {
                flagforSECFile = 0;
            }
            else if (varianceSECFile.Equals("1"))
            {
                flagforSECFile = 1;
            }
            else if (varianceSECFile.Equals("2"))
            {
                flagforSECFile = 2;
            }
            else
            {
                flagforSECFile = 3;
            }



            if ((GrowthFlagFile = infile.ReadLine()) == null)
            {
                throw new Exception("Error reading in growthrate parameter from parameter file.");
            }

            if (GrowthFlagFile.Equals("N/A"))
            {
                GrowthFlag = 0;
            }
            else if (GrowthFlagFile.Equals("0"))
            {
                GrowthFlag = 0;
            }
            else
            {
                GrowthFlag = 1;
            }



            if ((MortalityFile = infile.ReadLine()) == null)
            {
                throw new Exception("Error reading in mortality parameter from parameter file.");
            }

            if (MortalityFile.Equals("N/A"))
            {

                MortalityFlag = 0;

            }
            else if (MortalityFile.Equals("0"))
            {

                MortalityFlag = 0;
            }
            else
            {
                MortalityFlag = 1;
            }

            SeedRainFlag = 0;




            if ((VolumeFile = infile.ReadLine()) == null)
            {
                throw new Exception("Error reading in seedrain parameter from parameter file.");
            }

            if (VolumeFile.Equals("N/A"))
            {
                VolumeFlag = 0;
            }

            else if (VolumeFile.Equals("0"))
            {
                VolumeFlag = 0;
            }
            else
            {
                VolumeFlag = 1;
            }


            if ((str = infile.ReadLine()) != null)
            {
                stocking_x_value = double.Parse(str);
            }
            else
            {
                throw new Exception("Error reading TargetStocking from harvest section.");
            }



            if ((str = infile.ReadLine()) != null)
            {
                stocking_y_value = double.Parse(str);
            }
            else
            {
                throw new Exception("Error reading TargetStocking from harvest section.");
            }


            if ((str = infile.ReadLine()) != null)
            {
                stocking_z_value = double.Parse(str);
            }
            else
            {
                throw new Exception("Error reading TargetStocking from harvest section.");
            }




            if ((strWindInitName = infile.ReadLine()) == null)
            {

                throw new Exception("Error reading in windthrow  flag from parameter file.");
            }


            if (strWindInitName.Equals("N/A"))
            {
                dllmode = dllmode | Defines.G_WIND;
            }



            if ((strFireInitName = infile.ReadLine()) == null)
            {

                throw new Exception("Error reading in fire flag from parameter file.");
            }

            if (!strFireInitName.Equals("N/A"))
            {
                dllmode = dllmode | Defines.G_FIRE;
            }



            if ((strBDAInitName = infile.ReadLine()) == null)
            {

                throw new Exception("Error reading in BDAInit file info");
            }

            if (!strBDAInitName.Equals("N/A"))
            {

                dllmode = dllmode | Defines.G_BDA;

                //FILE tmp = LDfopen(strBDAInitName, 1);

                //if (tmp == null)
                //{

                //    errorSys("Error Open BDA parameter file error.", STOP);
                //}

                //fscanc(tmp, "%d", BDANo);

                //LDfclose(tmp);

            }



            if ((strFuelInitName = infile.ReadLine()) == null)
            {
                throw new Exception("Error reading in FuelInit file info");
            }

            if (!strFuelInitName.Equals("N/A"))
            {

                dllmode = dllmode | Defines.G_FUEL;
            }



            if ((strHarvestInitName = infile.ReadLine()) == null)
            {

                throw new Exception("Error reading in HarvestInit file info.");
            }

            if (!strHarvestInitName.Equals("N/A"))
            {

                dllmode = dllmode | Defines.G_HARVEST;
            }





            if (dispType.Equals("NO_DISPERSAL"))
            {

                dispRegime = NO_DISPERSAL;
            }

            else if (string.Compare(dispType, "UNIFORM") == 0)
            {

                dispRegime = UNIFORM;
            }

            else if (string.Compare(dispType, "NEIGHBORS") == 0)
            {

                dispRegime = NEIGHBORS;
            }

            else if (string.Compare(dispType, "DISPERSAL") == 0)
            {

                dispRegime = DISPERSAL;
            }

            else if (string.Compare(dispType, "RAND_ASYM") == 0)
            {

                dispRegime = RAND_ASYM;
            }

            else if (string.Compare(dispType, "MAX_DIST") == 0)
            {

                dispRegime = MAX_DIST;
            }

            else if (string.Compare(dispType, "SIM_RAND_ASYM") == 0)
            {

                dispRegime = SIM_RAND_ASYM;
            }

            else
            {

                throw new Exception("Input file error or illegal seed dispersal routine.");
            }


#if (OUTPUT)
            Console.WriteLine("Reading from parameter File\n");
            Console.WriteLine("=============================");
            Console.WriteLine(specAttrFile);
            Console.WriteLine(landUnitFile);
            Console.WriteLine(landImgMapFile);
            Console.WriteLine(siteImgFile);
            Console.WriteLine(reclassInFile);
            Console.WriteLine(OutputOption70);
            Console.WriteLine(outputDir);
            Console.WriteLine(freq_out_put);
            Console.WriteLine(Biomassfile);
            Console.WriteLine(numberOfIterations);
            Console.WriteLine(randSeed);
            Console.WriteLine(cellSize);
            Console.WriteLine(dispType);
            Console.WriteLine(timestep);
            Console.WriteLine(timestep_Wind);
            Console.WriteLine(timestep_Fire);
            Console.WriteLine(timestep_BDA);
            Console.WriteLine(timestep_Fuel);
            Console.WriteLine(timestep_Harvest);
            Console.WriteLine(varianceSECFile);
            Console.WriteLine(GrowthFlagFile);
            Console.WriteLine(MortalityFile);
            Console.WriteLine(VolumeFile);
            Console.WriteLine(stocking_x_value);
            Console.WriteLine(stocking_y_value);
            Console.WriteLine(stocking_z_value);
            Console.WriteLine(strWindInitName);
            Console.WriteLine(strFireInitName);
            Console.WriteLine(strBDAInitName);
            Console.WriteLine(strFuelInitName);
            Console.WriteLine(strHarvestInitName);
            Console.WriteLine("==================================");
            Console.WriteLine("parameters.dat input finished!\n");
#endif


            return dllmode;
        }
    }
}
