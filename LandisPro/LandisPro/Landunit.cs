//#define OUTPUT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace LandisPro
{

    class Landunit
    {
        public const int PASSIVE = 0;
        public const int ACTIVE = 1;
        public const int WATER = 2;
        public const int WETLAND = 3;
        public const int BOG = 4;
        public const int LOWLAND = 5;
        public const int NONFOREST = 6;
        public const int GRASSLAND = 7;

        public int minShade;
        public int ltID;
        public double[] probReproduction;
        public double[] probReproductionOriginalBackup;
        public string name;
        public double[] MaxRDArray = new double[4];
        public int initialLastWind;
        public double MaxRD;

        private SpeciesAttrs speciesAttrs;
        private int status;

        public Landunit()
        {
            name = null;
            minShade = 0;
            probReproduction = null;
            probReproductionOriginalBackup = null;
            speciesAttrs = null;
            ltID = 0;
        }

        //public void set_probReproductionOriginalBackup(int index, float value)
        //{
        //    probReproductionOriginalBackup[index] = value;
        //}

        //public float get_probReproduction(int index)
        //{
        //    return Establishment_probability_Attributes.get_probability(speciesAttrs[index + 1].name, name, PlugIn.ModelCore.TimeSinceStart);
        //    //return probReproduction[index];
        //}

        public void attach(SpeciesAttrs s)
        {
            speciesAttrs = s;
        }

        public bool active()
        {
            if (status == ACTIVE)
                return true;
            else
                return false;
        }

        public float probRepro(int index_in)
        {
            if (index_in <= speciesAttrs.Number() && index_in > 0)
                return (float)probReproduction[index_in - 1];
                //return Establishment_probability_Attributes.get_probability(speciesAttrs[index_in].name, name, PlugIn.ModelCore.TimeSinceStart);
            throw new Exception("LANDUNIT::probRepro(int)-> Array bounds error.");

            // return 0.0f;
        }

        public bool water()
        {
            return status == WATER;
        }

        public bool lowland()
        {
            return (status == WETLAND || status == BOG || status == LOWLAND || status == NONFOREST);
        }

        public float maxRDArray(int i)
        {
            return (float)MaxRDArray[i];
        }

        public void Read(StreamReader infile)
        {
            int i;
            if (speciesAttrs == null)
            {
                Console.WriteLine("LANDUNIT::read(FILE*)-> No attaced species attributes.");
            }

            string instring;

            int specAtNum = speciesAttrs.Number();


            instring = infile.ReadLine();
            string[] sarray = instring.Split(' ');
            name = sarray[0];
            sarray = null;

            instring = infile.ReadLine();
            sarray = instring.Split(' ');
            minShade = int.Parse(sarray[0]);
            initialLastWind = int.Parse(sarray[1]);
            sarray = null;

            MaxRDArray[0] = double.Parse(infile.ReadLine());
            MaxRDArray[1] = double.Parse(infile.ReadLine());
            MaxRDArray[2] = double.Parse(infile.ReadLine());
            MaxRDArray[3] = double.Parse(infile.ReadLine());
            MaxRD = MaxRDArray[3];

            if (probReproduction != null)
            {
                probReproduction = null;
                probReproduction = new double[speciesAttrs.Number()];
            }
            else
            {
                probReproduction = new double[speciesAttrs.Number()];
            }

            if (probReproductionOriginalBackup != null)
            {
                probReproductionOriginalBackup = null;
                probReproductionOriginalBackup = new double[speciesAttrs.Number()];
            }
            else
            {
                probReproductionOriginalBackup = new double[speciesAttrs.Number()];
            }

            for (i = 0; i < specAtNum; i++)
            {
                instring = infile.ReadLine();
                sarray = instring.Split(' ');
                probReproduction[i] = double.Parse(sarray[sarray.Length - 1]);
                sarray = null;
            }
#if (OUTPUT)
                Console.WriteLine(name);
                Console.WriteLine(minShade);
                Console.WriteLine(initialLastWind);
                Console.WriteLine(MaxRDArray[0]);
                Console.WriteLine(MaxRDArray[1]);
                Console.WriteLine(MaxRDArray[2]);
                Console.WriteLine(MaxRDArray[3]);
                for (int j = 0; j < specAtNum; j++)
                {
                    Console.WriteLine(probReproduction[j]);
                }
#endif



            infile.ReadLine();

            if ((name == "empty") || (name == "road"))
            {
                status = PASSIVE;
            }

            else if (name == "water")
            {
                status = WATER;
            }

            else if (name == "wetland")
            {
                status = WETLAND;
            }
            else if (name == "bog")
            {
                status = BOG;
            }
            else if (name == "lowland")
            {
                status = LOWLAND;
            }
            else if (name == "nonforest")
            {
                status = NONFOREST;
            }
            else if (name == "grassland")
            {
                status = GRASSLAND;
            }
            else
            {
                status = ACTIVE;
            }

        }
    }

    
}
