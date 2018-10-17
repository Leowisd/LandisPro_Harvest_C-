using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LandisPro
{
    class Landunits
    {
        private int numLU;
        private int currentLU;
        private int maxLU;
        private int Totaliteration;
        private int timestep;
        private uint numSpecies;
        private int flagforSECFile;
        private Landunit[] landUnits;
        private int[] VectorIteration;

        public Landunits(int n)
        {
            numLU = 0;
            currentLU = 0;
            maxLU = n;

            landUnits = new Landunit[n];
            for (int i = 0; i < n; i++)
                landUnits[i] = new Landunit();
        }

        public Landunit this[int n]
        {
            get
            {
                if (n>numLU || n<0)
                    return null;
                else 
                    return landUnits[n];
            }
            set
            {
                landUnits[n] = value;
            }
        }

        //Referrence first land unit attribute.
        public Landunit first()
        {
            currentLU = 0;

            if (numLU == 0)
                return null;
            else
                return landUnits[0];
        }

        public Landunit next()
        {
            currentLU++;

            if (currentLU >= numLU)
                return null;
            else
                return landUnits[currentLU];
        }

        public void ReprodBackup()
        {
            for (int i = 0; i < numLU; i++)
            {
                Landunit local_landunit = landUnits[i];

                for (int j = 0; j < numSpecies; j++)
                    //local_landunit.set_probReproductionOriginalBackup(j, local_landunit.get_probReproduction(j));
                    landUnits[i].probReproductionOriginalBackup[j] = landUnits[i].probReproduction[j];
            }
        }

        public void ReprodUpdate(int year)
        {
            if (flagforSECFile == 3 || flagforSECFile == 0)
                return;

            float local_val = (1 + VectorIteration[year - 1]) * timestep / 10.0f;

            for (int i = 0; i < numLU; i++)
            {
                for (int j = 0; j < numSpecies; j++)
                    landUnits[i].probReproduction[j] = landUnits[i].probReproductionOriginalBackup[j] / 10 * timestep * local_val;

                //landunit local_landunit = land_Units[i];

                //for (int j = 0; j < numSpecies; j++)
                //    local_landunit.set_probReproduction(j, local_landunit.get_probReproductionOriginalBackup(j) * local_val);
            }
        }

        public void initiateVariableVector(int NumofIter, int temp, uint num, int flag)
        {
            timestep = temp;
            Totaliteration = NumofIter;
            numSpecies = num;
            flagforSECFile = flag;
            ReprodBackup();

            VectorIteration = new int[NumofIter];

            int flagForVariance = 0;

            if (system1.frand1() > 0.5)
                flagForVariance = -1;
            else
                flagForVariance = 1;

            int onceVarianceLast = 0;
            if (flag == 0)
            {
                for (int i = 0; i < Totaliteration; i++)
                    VectorIteration[i] = 0;
            }
            else if (flag == 1)
            {
                if (timestep >= 5)
                {
                    for (int i = 0; i < Totaliteration; i++)
                        VectorIteration[i] = 0;
                }
                else
                {
                    for (int i = 0; i < Totaliteration;)
                    {
                        if (onceVarianceLast == 0)
                        {
                            int randnumber = system1.frandrand() % 2 + 3;
                            onceVarianceLast = randnumber / timestep;
                            if (onceVarianceLast == 0)
                                onceVarianceLast = 1;
                            flagForVariance = flagForVariance * (-1);
                        }
                        else
                        {
                            VectorIteration[i] = flagForVariance;
                            i++;
                            onceVarianceLast--;
                        }
                    }//end for
                }
            }//end if (flag == 1)
            else if (flag == 2)
            {
                for (int i = 0; i < Totaliteration; i++)
                {
                    if (system1.frand1() > 0.5)
                        VectorIteration[i] = 1;
                    else
                        VectorIteration[i] = -1;
                }
            }
        }

        public int Number()
        {
            return numLU;
        }

        public void attach(SpeciesAttrs s)
        {
            for (int i = 0; i < maxLU; i++)
            {
                landUnits[i] = new Landunit();
                landUnits[i].attach(s);
            }

        }
        public void Read(StreamReader infile)
        {
            numLU = 0;
            while (infile.Peek() >= 0)
            {
                if (numLU < maxLU)
                {
                    landUnits[numLU++].Read(infile);
                    landUnits[numLU].ltID = numLU;
                }
                else
                    throw new Exception("LANDUNITS::read(FILE*)-> Array bounds error.");
            }
            Console.WriteLine("Number of landUnits: " + numLU);
        }
    }

    public class Land_type_Attributes
    {
        private static float[,] growingSpaceOccupied;
        private static int[] min_shade;

        //private float[] max_growingSpaceOccupied;
        private Dictionary<int, float[]> max_growingSpaceOccupied = new Dictionary<int, float[]>();

        private Dictionary<int, string> new_landtype_map = new Dictionary<int, string>();
        //public int Num_new_landtype_map { get; set; }

        public List<int> year_arr = new List<int>();

        public static void set_gso(int gsokind, int landtypekind, float value)
        {
            growingSpaceOccupied[gsokind, landtypekind] = value;
        }

        public static float get_gso(int gsokind, int landtypekind)
        {
            return growingSpaceOccupied[gsokind, landtypekind];
        }



        public void set_maxgso(int year, float[] value)
        {
            if (value.Length != Landtype_count)
                throw new Exception("max gso array is problematic\n");

            max_growingSpaceOccupied.Add(year, value);
        }

        public float get_maxgso(int year, int landtypekind)
        {
            while (!max_growingSpaceOccupied.ContainsKey(year)) year -= 1;
            return max_growingSpaceOccupied[year][landtypekind];
        }





        public static void set_min_shade(int landtypekind, int value)
        {
            min_shade[landtypekind] = value;
        }

        public static int get_min_shade(int landtypekind)
        {
            return min_shade[landtypekind];
        }



        //public Dictionary<int, string> New_landtype_map
        //{ get { return new_landtype_map; } set {new_landtype_map = value;} }
        public void Set_new_landtype_map(int key, string value)
        {
            if (new_landtype_map.ContainsKey(key))
            {
                new_landtype_map[key] = value;
            }
            else
            {
                new_landtype_map.Add(key, value);
            }
        }

        public string Get_new_landtype_map(int key)
        {
            string result = null;
            for (; key >= 0;)
            {
                if (new_landtype_map.ContainsKey(key))
                {
                    result = new_landtype_map[key];
                    break;
                }
                key -= 1;
            }
            return result;
        }

        

        public static int Landtype_count { get; set; }

        static Land_type_Attributes()
        {
            Landtype_count = Program.landUnits.Number();//PlugIn.ModelCore.Ecoregions.Count;
            growingSpaceOccupied = new float[3, Landtype_count];
            min_shade = new int[Landtype_count];
        }


        public Land_type_Attributes()
        {
            year_arr.Add(0);
        }
    }
}
