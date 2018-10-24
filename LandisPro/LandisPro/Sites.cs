using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;

namespace LandisPro
{
    class Sites
    {
        public const int NumTypes70Output = 8;
        public const int TPA = 0;
        public const int BA = 1;
        public const int Bio = 2;
        public const int Car = 3;
        public const int IV = 4;
        public const int Seeds = 5;
        public const int RDensity = 6;
        public const int DBH = 7;

        private static uint UINT_MAX = uint.MaxValue;

        public int CellSize;
        public int TimeStep;
        public int TimeStep_Harvest;
        public int MaxDistofAllSpec;
        public int MaxShadeTolerance;
        public int Pro0or401;
        public int specNum;
        public int[] OutputGeneralFlagArray;
        public int[] OutputAgerangeFlagArray;
        public int[] SpeciesAgerangeArray;
        public int[] AgeDistStat_Year;
        public int[] AgeDistStat_AgeRange;
        public int countWenjuanDebug;
        public int flagAgeRangeOutput;
        public int Flag_AgeDistStat;

        public List<Site> SortedIndex;
        private List<double[]> GrowthRates_file = new List<double[]>();
        private List<float[]> MortalityRates_file = new List<float[]>();
        private List<float[]> Volume_file = new List<float[]>();

        private List<float> AreaList = new List<float>();

        private List<int> SpecIndexArray = new List<int>();
        private List<int> AgeIndexArray = new List<int>();

        private int biomassNum;
        private double[] biomassData;
        public double BiomassThreshold;
        private Site[] map;
        private Site[] map70;
        private Landunit[] map_landtype;
        private Site[] sitetouse;
        private float[] SeedRain;
        private double[] GrowthRates;
        private double[] MortalityRates;
        private double[] Volume;
        private int rows, columns;
        private int SeedRainFlag;
        private int GrowthFlag;
        private int MortalityFlag;
        private int VolumeFlag;
        private uint[] header = new uint[32];

        public int[] flag_cut_GROUP_CUT = new int[200]; //add by Qia on Nov 10 2011
        public int[] flag_plant_GROUP_CUT = new int[200];
        public int[] num_TreePlant_GROUP_CUT = new int[200];
        public int[] flag_cut_GROUP_CUT_copy = new int[200]; //add by Qia on June 02 2012
        public int[] flag_plant_GROUP_CUT_copy = new int[200];
        public int[] num_TreePlant_GROUP_CUT_copy = new int[200];

        public double[] BiomassHarvestCost;
        public double[] CarbonHarvestCost;
        public int Harvestflag;

        public double stocking_x_value;
        public double stocking_y_value;
        public double stocking_z_value;

        public Site this[int i, int j]
        {
            get
            {
                Debug.Assert(i > 0 && i <= rows);
                Debug.Assert(j > 0 && j <= columns);

                return map70[(i - 1) * columns + j - 1];
            }
            set
            {
                Debug.Assert(i > 0 && i <= rows);
                Debug.Assert(j > 0 && j <= columns);

                map70[(i - 1) * columns + j - 1] = value;
            }
        }

        public uint[] Header
        {
            get { return header; }
        }

        public void fillinLanduPt(int i, int j, Landunit landUnitPt)
        {
            int x;
            x = (i - 1) * columns;
            x = x + j - 1;
            map_landtype[x] = landUnitPt;
        }

        public void SITE_sort()
        {
            for (int i = SortedIndex.Count - 1; i > 0; --i)
            {
                for (int j = 0; j <= i - 1; ++j)
                {
                    Site site1 = SortedIndex[j];
                    Site site2 = SortedIndex[j + 1];

                    if (SITE_compare(site1, site2) == 1)
                    {
                        Site temp = SortedIndex[j];
                        SortedIndex[j] = SortedIndex[j + 1];
                        SortedIndex[j + 1] = temp;
                    }
                }
            }
        }

        public int numRows()
        {
            return rows;
        }

        public int numColumns()
        {
            return columns;
        }

        public int number()
        {
            return numRows() * numColumns();
        }

        public int numActive()
        {
            int count = 0;
            for (int i = 0; i <= numRows() * numColumns() - 1; i++)
                if (map_landtype[i].active())
                    count++;
            return count;
        }

        public float GetSeedRain(int spec, int Distance)
        {
            int local_int = MaxDistofAllSpec / CellSize + 2;

            int id = (spec - 1) * local_int + Distance;

            if (id >= specNum * local_int)
                return 0;
            else
                return SeedRain[id];
        }

        public void GetSeedNumberOnSite(uint Row, uint Col)
        {
            Site siteptr = this[(int)Row, (int)Col];
            Landunit l = locateLanduPt((int)Row, (int)Col);
            uint const_cellsqr = (uint)(CellSize * CellSize * 1000);
            uint local_site_num = (uint)siteptr.Number();
            for (int k = 1; k <= local_site_num; k++)
            {
                Speciesattr local_speciesattr = siteptr.specAtt(k);
                Specie local_specie = siteptr.SpecieIndex(k);
                local_specie.AvailableSeed = 0;
                int totalseed_m_timestep = local_speciesattr.TotalSeed * TimeStep;
                if (local_speciesattr.SpType < 0)
                {
                    local_specie.AvailableSeed = (uint)totalseed_m_timestep;
                }
                else
                {
                    double double_val;
                    if (locateLanduPt((int)Row, (int)Col).probRepro(k) > 0 && local_speciesattr.maxD < 0)
                    {
                        int i_age_begin = local_speciesattr.maturity / TimeStep;
                        int i_age_end = local_speciesattr.longevity / TimeStep;
                        for (int i_age = i_age_begin; i_age <= i_age_end; i_age++)
                        {
                            uint local_treenum = (uint)local_specie.getTreeNum(i_age, k);
                            if (local_treenum > 0)
                            {
                                double loc_term = Math.Pow(GetGrowthRates(k, i_age, l.ltID) / 25.4, 1.605);
                                //wenjuan changed on mar 30 2011
                                double_val = loc_term * local_treenum * totalseed_m_timestep;
                                Debug.Assert(double_val <= UINT_MAX && double_val > 0);
                                local_specie.AvailableSeed += (uint)double_val;
                                if (Row == 20 && Col == 2) Console.WriteLine("{0}:+{1}={2}", i_age, double_val, local_specie.AvailableSeed);
                            }

                        }

                    }
                    if (locateLanduPt((int)Row, (int)Col).probRepro(k) > 0 && local_speciesattr.maxD > 0)
                    {
                        int maxD_d_cellsize = local_speciesattr.maxD / CellSize;

                        for (int i = (int)Row - maxD_d_cellsize; i <= Row + maxD_d_cellsize; i++)
                        {
                            for (int j = (int)Col - maxD_d_cellsize; j <= Col + maxD_d_cellsize; j++)
                            {
                                if (i >= 1 && i <= rows && j >= 1 && j <= columns && locateLanduPt(i, j) != null && locateLanduPt(i, j).active())
                                {
                                    int TempDist = (int)Math.Max(Math.Abs(i - Row), Math.Abs(j - Col));

                                    Site local_site = this[i, j];

                                    //double_val = GetSeedRain(k, TempDist) * local_site.SpecieIndex(k).MatureTree * local_site.specAtt(k).TotalSeed * timeStep;
                                    float seed_rain = GetSeedRain(k, TempDist);
                                    uint mature_tree = (uint)local_site.SpecieIndex(k).MatureTree;
                                    int local_tseed = local_site.specAtt(k).TotalSeed;

                                    double_val = seed_rain * mature_tree * local_tseed * TimeStep;

                                    //Debug.Assert(double_val <= UINT_MAX && double_val >= 0);

                                    local_specie.AvailableSeed += (uint)double_val;
                                    if (Row == 20 && Col == 2) Console.WriteLine("{0} {1}:+{2}={3}", i, j, double_val, local_specie.AvailableSeed);
                                }

                            }

                            if (local_specie.AvailableSeed > const_cellsqr)
                            {
                                local_specie.AvailableSeed = const_cellsqr;

                                break;
                            }

                        }

                        float float_rand = system1.frand1();

                        double_val = local_specie.AvailableSeed * (0.95 + float_rand * 0.1);

                        //Console.Write("{0:N6} ", float_rand);

                        Debug.Assert(double_val <= UINT_MAX && double_val >= 0);

                        local_specie.AvailableSeed = (uint)double_val;
                        if (Row == 20 && Col == 2) Console.WriteLine("frand: {0}", double_val);
                    }

                }//end else

            }// end for

        }

        public void SeedGermination(Site siteptr, Landunit l, int RDFlag)
        {
            double RDTotal;
            double seedlingTemp;

            long temp;
            int cellsize_square = CellSize * CellSize;

            uint site_num = (uint)siteptr.Number();

            double[] IndexRD = new double[site_num];
            int[] SppPresence = new int[site_num];
            long[] Seedling = new long[site_num];
            float[] SppRD = new float[6];
            int[] SppShade = new int[6];

            if (RDFlag == 0 || RDFlag == 1)
            {
                RDTotal = 0.0;
                //Console.WriteLine("site_num = {0}", site_num);
                for (int i = 1; i <= site_num; i++)
                {
                    double local_term1 = Math.Pow((GetGrowthRates(i, 1, l.ltID) / 25.4), 1.605);
                    float local_term2 = (float)siteptr.specAtt(i).MaxAreaOfSTDTree;
                    uint local_term3 = (uint)siteptr.SpecieIndex(i).AvailableSeed;
                    IndexRD[i - 1] = local_term1 * local_term2 * local_term3 / cellsize_square;
                    //Console.WriteLine("{0:N6}, {1:N6}, {2}", local_term1, local_term2, local_term3);
                    //if (local_term3 != 0)
                    //Console.WriteLine("{0}, {1}", i, local_term3);
                    RDTotal += IndexRD[i - 1];
                }

                if (RDTotal >= 0.0)
                {
                    //Console.WriteLine("RDFlag == 1");
                    if (RDTotal <= l.MaxRD - siteptr.RD)
                    {
                        for (int i = 1; i <= site_num; i++)
                        {
                            Speciesattr local_speciesattr = siteptr.specAtt(i);
                            Specie local_specie = siteptr.SpecieIndex(i);

                            if (local_speciesattr.shadeTolerance <= 4 || (local_speciesattr.shadeTolerance == 5 && siteptr.maxAge >= l.minShade))
                            {
                                temp = local_specie.TreesFromVeg * local_specie.vegPropagules;
                                local_specie.TreesFromVeg = 0;
                                float float_val = local_specie.AvailableSeed * l.probRepro(i);
                                Debug.Assert(float_val <= int.MaxValue && float_val >= int.MinValue);
                                Seedling[i - 1] = (long)float_val + temp;
                                //if (Seedling[i - 1] != 0)
                                //Console.WriteLine("first {0}, {1:N6}, {2}", local_specie.AvailableSeed, l.probRepro(i), temp);
                            }
                        }
                    }

                    else
                    {

                        for (int i = 1; i <= site_num; i++)
                        {
                            Speciesattr local_speciesattr = siteptr.specAtt(i);
                            Specie local_specie = siteptr.SpecieIndex(i);

                            if (local_speciesattr.shadeTolerance <= 4 || (local_speciesattr.shadeTolerance == 5 && siteptr.maxAge >= l.minShade))
                            {
                                temp = local_specie.TreesFromVeg * local_specie.vegPropagules;
                                local_specie.TreesFromVeg = 0;
                                seedlingTemp = IndexRD[i - 1] * (l.MaxRD - siteptr.RD) / RDTotal * cellsize_square / (Math.Pow((GetGrowthRates(i, 1, l.ltID) / 25.4), 1.605) * local_speciesattr.MaxAreaOfSTDTree);
                                double double_val = seedlingTemp * l.probRepro(i);
                                Debug.Assert(double_val <= int.MaxValue && double_val >= int.MinValue);
                                Seedling[i - 1] = (long)double_val + temp;
                            }
                        }
                    }
                }         
                else
                {
                    IndexRD = null;
                    Seedling = null;
                    SppRD = null;
                    SppShade = null;
                    SppPresence = null;
                    return;
                }
            }
            else if (RDFlag == 2)
            {
                RDTotal = 0.0;
                for (int i = 1; i <= site_num; i++)
                {
                    //changed by wenjuan//if(siteptr.specAtt(i).shadeTolerance==siteptr.HighestShadeTolerance&&siteptr.MaxAge>=l.minShade)
                    if (siteptr.specAtt(i).shadeTolerance >= siteptr.HighestShadeTolerance)
                    {
                        double local_term1 = Math.Pow((GetGrowthRates(i, 1, l.ltID) / 25.4), 1.605);
                        float local_term2 = (float)siteptr.specAtt(i).MaxAreaOfSTDTree;
                        uint local_term3 = (uint)siteptr.SpecieIndex(i).AvailableSeed;
                        IndexRD[i - 1] = local_term1 * local_term2 * local_term3 / cellsize_square;
                        RDTotal += IndexRD[i - 1];
                    }
                    else
                    {
                        Seedling[i - 1] = 0;
                    }

                }
                if (RDTotal >= 0.0)
                {
                    if (RDTotal <= l.MaxRD - siteptr.RD)
                    {
                        for (int i = 1; i <= site_num; i++)
                        {
                            Speciesattr local_speciesattr = siteptr.specAtt(i);
                            Specie local_specie = siteptr.SpecieIndex(i);

                            if (local_speciesattr.shadeTolerance >= siteptr.HighestShadeTolerance)
                            {
                                temp = local_specie.TreesFromVeg * local_specie.vegPropagules;
                                local_specie.TreesFromVeg = 0;
                                float float_val = local_specie.AvailableSeed * l.probRepro(i);
                                Debug.Assert(float_val <= int.MaxValue && float_val >= int.MinValue);
                                Seedling[i - 1] = (long)float_val + temp;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 1; i <= site_num; i++)
                        {
                            Speciesattr local_speciesattr = siteptr.specAtt(i);
                            Specie local_specie = siteptr.SpecieIndex(i);
                            if (local_speciesattr.shadeTolerance >= siteptr.HighestShadeTolerance)
                            {
                                temp = local_specie.TreesFromVeg * local_specie.vegPropagules;
                                local_specie.TreesFromVeg = 0;
                                seedlingTemp = IndexRD[i - 1] * (l.MaxRD - siteptr.RD) / RDTotal * cellsize_square / (Math.Pow((GetGrowthRates(i, 1, l.ltID) / 25.4), 1.605) * local_speciesattr.MaxAreaOfSTDTree);
                                double double_val = seedlingTemp * l.probRepro(i);
                                if (!(double_val <= int.MaxValue && double_val >= int.MinValue))
                                    Console.WriteLine("{0} {1} {2} {3}", seedlingTemp, l.probRepro(i), RDTotal == 0.0, RDTotal > 0.0);
                                Debug.Assert(double_val <= int.MaxValue && double_val >= int.MinValue);
                                Seedling[i - 1] = (long)double_val + temp;
                            }
                        }
                    }
                }
                else
                {
                    IndexRD = null;
                    Seedling = null;
                    SppRD = null;
                    SppShade = null;
                    SppPresence = null;
                    return;
                }
            }
            else if (RDFlag == 3)
            {
                RDTotal = 0.0;

                for (int i = 1; i <= site_num; i++)
                {
                    //changed by wenjuan//if(siteptr.specAtt(i).shadeTolerance==siteptr.HighestShadeTolerance&&siteptr.MaxAge>=l.minShade)
                    if (siteptr.specAtt(i).shadeTolerance == MaxShadeTolerance && siteptr.maxAge >= l.minShade)
                    {
                        double local_term1 = Math.Pow((GetGrowthRates(i, 1, l.ltID) / 25.4), 1.605);
                        float local_term2 = (float)siteptr.specAtt(i).MaxAreaOfSTDTree;
                        uint local_term3 = (uint)siteptr.SpecieIndex(i).AvailableSeed;

                        IndexRD[i - 1] = local_term1 * local_term2 * local_term3 / cellsize_square;

                        RDTotal += IndexRD[i - 1];
                    }
                    else
                    {
                        Seedling[i - 1] = 0;
                    }

                }

                if (RDTotal >= 0.0)
                {
                    if (RDTotal <= l.MaxRD - siteptr.RD)
                    {
                        for (int i = 1; i <= site_num; i++)
                        {
                            Speciesattr local_speciesattr = siteptr.specAtt(i);
                            Specie local_specie = siteptr.SpecieIndex(i);

                            if (local_speciesattr.shadeTolerance == MaxShadeTolerance && siteptr.maxAge >= l.minShade)
                            {
                                temp = local_specie.TreesFromVeg * local_specie.vegPropagules;
                                local_specie.TreesFromVeg = 0;
                                float float_val = local_specie.AvailableSeed * l.probRepro(i);
                                Debug.Assert(float_val <= int.MaxValue && float_val >= int.MinValue);
                                Seedling[i - 1] = (long)float_val + temp;
                            }

                        }

                    }
                    else
                    {
                        for (int i = 1; i <= site_num; i++)
                        {
                            Speciesattr local_speciesattr = siteptr.specAtt(i);
                            Specie local_specie = siteptr.SpecieIndex(i);

                            if (local_speciesattr.shadeTolerance == MaxShadeTolerance && siteptr.maxAge >= l.minShade)
                            {
                                temp = local_specie.TreesFromVeg * local_specie.vegPropagules;
                                local_specie.TreesFromVeg = 0;
                                seedlingTemp = IndexRD[i - 1] * (l.MaxRD - siteptr.RD) / RDTotal * cellsize_square / (Math.Pow((GetGrowthRates(i, 1, l.ltID) / 25.4), 1.605) * local_speciesattr.MaxAreaOfSTDTree);
                                double double_val = seedlingTemp * l.probRepro(i);
                                Debug.Assert(double_val <= int.MaxValue && double_val >= int.MinValue);
                                Seedling[i - 1] = (long)double_val + temp;
                            }
                        }
                    }
                }
                else
                {
                    IndexRD = null;
                    Seedling = null;
                    SppRD = null;
                    SppShade = null;
                    SppPresence = null;
                    return;
                }
            }
            else
            {
                for (int i = 1; i <= site_num; i++)
                {
                    Seedling[i - 1] = 0;
                }
            }
            //Console.WriteLine("c# seedling");
            for (int i = 1; i <= site_num; i++)
            {
                if (Seedling[i - 1] > 0)
                {
                    siteptr.SpecieIndex(i).setTreeNum(1, i, (int)Seedling[i - 1]);
                    //Console.Write("{0} ", (int)Seedling[i - 1]);
                }
                else
                    siteptr.SpecieIndex(i).setTreeNum(1, i, 0);
            }
            //Console.WriteLine("RDFlag = {0}", RDFlag);
            IndexRD = null;
            Seedling = null;
            SppRD = null;
            SppShade = null;
            SppPresence = null;

        }

        public float GetMortalityRates(int spec, int year, int landtype_index)
        {
            int local_const = 320 / TimeStep + 1;
            int index = (spec - 1) * local_const + year - 1;

            if (spec < 1 && spec > specNum && spec * year >= specNum * local_const)
                throw new Exception("Out bound in GetMortalityRates\n");

            if (MortalityRates_file.Count == 0)
                return (float)MortalityRates[index];
            else
            {
                if (landtype_index < MortalityRates_file.Count)
                    return MortalityRates_file[landtype_index][index];
                else
                    throw new Exception("Out bound in MortalityRates\n");
            }
        }

        public void NaturalMortality(Site siteptr, uint Row, uint Col, int StartAge)
        {
            Landunit l = locateLanduPt((int)Row, (int)Col);
            int cellsize_square = CellSize * CellSize;
            double DQ_const = 3.1415926 / (4 * 0.0002471 * cellsize_square * 30.48 * 30.48);
            uint site_species_num = (uint)siteptr.Number(); //number of species

            //kill all tree, else kill youngest tree
            if (StartAge == 0)
            {
                //Console.WriteLine("MortalityFlag = {0}", MortalityFlag);
                if (MortalityFlag == 0)
                {
                    double tmpDQ = 0;
                    for (int i = 1; i <= site_species_num; i++)
                    {
                        int site_spec_i_type = siteptr.specAtt(i).SpType;
                        int max_count = siteptr.specAtt(i).longevity / TimeStep;
                        if (site_spec_i_type >= 0)
                        {
                            Specie local_specie = siteptr.SpecieIndex(i);
                            for (int j = 1; j <= max_count; j++)
                            {
                                float growthrate = GetGrowthRates(i, j, l.ltID);
                                uint spec_ij_treenum = (uint)local_specie.getTreeNum(j, i);
                                //Wenjuan Suggested on Nov 16 2010
                                tmpDQ += growthrate * growthrate * DQ_const * spec_ij_treenum;
                                //if (spec_ij_treenum != 0)
                                //    Console.Write("{0}, {1:N2} ", spec_ij_treenum, growthrate);
                            }
                        }
                    }

                    //Console.Write("{0:N2} ", d_tmpDQ);
                    //Console.ReadLine();                    


                    for (int i = 1; i <= site_species_num; i++)
                    {
                        int site_spec_i_type = siteptr.specAtt(i).SpType;
                        int max_count = siteptr.specAtt(i).longevity / TimeStep;
                        Specie local_specie = siteptr.SpecieIndex(i);

                        if (site_spec_i_type >= 0)
                        {
                            for (int j = 1; j <= max_count; j++)
                            {
                                float growthrate = GetGrowthRates(i, j, l.ltID);
                                uint spec_ij_treenum = (uint)local_specie.getTreeNum(j, i);
                                double TmpMortality = TimeStep / 10 / (1.0 + Math.Exp(3.25309 - 0.00072647 * tmpDQ + 0.01668809 * growthrate / 2.54));
                                TmpMortality = (1.0f < TmpMortality ? 1.0f : TmpMortality);
                                double DeadTree = spec_ij_treenum * TmpMortality;
                                Debug.Assert(DeadTree <= UINT_MAX && DeadTree >= 0);
                                uint DeadTreeInt = (uint)DeadTree;
                                //if (DeadTree - DeadTreeInt >= 0.0001)
                                if (DeadTree > DeadTreeInt)
                                {
                                    float rand = system1.frand1();

                                    if (rand < 0.1)
                                        DeadTreeInt++;
                                }
                                local_specie.setTreeNum(j, i, (int)Math.Max(0, spec_ij_treenum - DeadTreeInt));
                                tmpDQ -= (float)(growthrate * growthrate * DQ_const * DeadTree);
                            }

                        }
                        else
                        {
                            for (int j = 1; j <= max_count; j++)
                                local_specie.setTreeNum(j, i, cellsize_square);
                        }

                    }

                }
                else
                {

                    for (int i = 1; i <= site_species_num; i++)
                    {
                        int site_spec_i_type = siteptr.specAtt(i).SpType;
                        int max_count = siteptr.specAtt(i).longevity / TimeStep;
                        Specie local_specie = siteptr.SpecieIndex(i);

                        if (site_spec_i_type >= 0)
                        {
                            for (int j = 1; j <= max_count; j++)
                            {
                                uint spec_ij_treenum = (uint)local_specie.getTreeNum(j, i);
                                double DeadTree = spec_ij_treenum * GetMortalityRates(i, j, l.ltID);

                                Debug.Assert(DeadTree <= UINT_MAX && DeadTree >= 0);

                                uint DeadTreeInt = (uint)DeadTree;

                                //if (DeadTree - DeadTreeInt >= 0.0001)
                                if (DeadTree > DeadTreeInt)
                                {
                                    float rand = system1.frand1();

                                    if (rand < 0.1)
                                        DeadTreeInt++;
                                }


                                local_specie.setTreeNum(j, i, (int)Math.Max(0, spec_ij_treenum - DeadTreeInt));
                            }

                        }
                        else
                        {
                            for (int j = 1; j <= max_count; j++)
                                local_specie.setTreeNum(j, i, cellsize_square);
                        }

                    }

                }

            }
            else //kill youngest tree
            {
                if (MortalityFlag == 0)
                {
                    double tmpDQ = 0;

                    for (int i = 1; i <= site_species_num; i++)
                    {
                        int site_spec_i_type = siteptr.specAtt(i).SpType;
                        int max_count = siteptr.specAtt(i).longevity / TimeStep;

                        if (site_spec_i_type >= 0)
                        {
                            Specie local_specie = siteptr.SpecieIndex(i);

                            for (int j = 1; j <= max_count; j++)
                            {
                                float growthrate = GetGrowthRates(i, j, l.ltID);

                                uint spec_ij_treenum = (uint)local_specie.getTreeNum(j, i);

                                //Wenjuan Suggested on Nov 16 2010
                                tmpDQ += growthrate * growthrate * DQ_const * spec_ij_treenum;
                            }
                        }
                    }


                    for (int i = 1; i <= site_species_num; i++)
                    {
                        int site_spec_i_type = siteptr.specAtt(i).SpType;
                        int max_count = siteptr.specAtt(i).longevity / TimeStep;

                        Specie local_specie = siteptr.SpecieIndex(i);

                        if (site_spec_i_type >= 0)
                        {
                            for (int j = 1; j <= StartAge; j++)
                            {
                                float growthrate = GetGrowthRates(i, j, l.ltID);

                                double TmpMortality = TimeStep / 10 / (1.0 + Math.Exp(3.25309 - 0.00072647 * tmpDQ + 0.01668809 * growthrate / 2.54));
                                TmpMortality = (1.0f < TmpMortality ? 1.0f : TmpMortality);

                                uint spec_ij_treenum = (uint)local_specie.getTreeNum(j, i);

                                double DeadTree = spec_ij_treenum * TmpMortality;

                                Debug.Assert(DeadTree <= UINT_MAX && DeadTree >= 0);

                                uint DeadTreeInt = (uint)DeadTree;

                                //if (DeadTree - DeadTreeInt >= 0.0001)
                                if (DeadTree > DeadTreeInt)
                                {
                                    float rand = system1.frand1();

                                    if (rand < 0.1)
                                        DeadTreeInt++;
                                }

                                local_specie.setTreeNum(j, i, (int)Math.Max(0, spec_ij_treenum - DeadTreeInt));

                                tmpDQ -= (float)(growthrate * growthrate * DQ_const * DeadTree);
                            }

                        }
                        else
                        {
                            for (int j = 1; j <= StartAge; j++)
                                local_specie.setTreeNum(j, i, cellsize_square);
                        }

                    }


                }
                else
                {

                    for (int i = 1; i <= site_species_num; i++)
                    {
                        int site_spec_i_type = siteptr.specAtt(i).SpType;
                        int max_count = siteptr.specAtt(i).longevity / TimeStep;

                        Specie local_specie = siteptr.SpecieIndex(i);

                        if (site_spec_i_type >= 0)
                        {
                            for (int j = 1; j <= StartAge; j++)
                            {
                                uint spec_ij_treenum = (uint)local_specie.getTreeNum(j, i);

                                double DeadTree = spec_ij_treenum * GetMortalityRates(i, j, l.ltID);

                                Debug.Assert(DeadTree <= UINT_MAX && DeadTree >= 0);

                                uint DeadTreeInt = (uint)DeadTree;

                                //if (DeadTree - DeadTreeInt >= 0.0001)
                                if (DeadTree > DeadTreeInt)
                                {
                                    float rand = system1.frand1();

                                    if (rand < 0.1)
                                        DeadTreeInt++;
                                }

                                local_specie.setTreeNum(j, i, (int)Math.Max(0, spec_ij_treenum - DeadTreeInt));

                            }

                        }
                        else
                        {
                            for (int j = 1; j <= StartAge; j++)
                                local_specie.setTreeNum(j, i, cellsize_square);
                        }

                    }

                }

            }

        }

        public void NaturalMortality_killbytargetRD(Site siteptr, uint Row, uint Col, double targetRD)
        {
            for (int i = 1; i <= siteptr.Number(); i++)
            {
                if (siteptr.specAtt(i).SpType >= 0)
                {
                    Specie local_specie = siteptr.SpecieIndex(i);

                    for (int j = siteptr.specAtt(i).longevity / TimeStep; j >= 1; j--)
                    {
                        if (local_specie.getTreeNum(j, i) > 0)
                        {
                            local_specie.setTreeNum(j, i, 0);

                            GetRDofSite((int)Row, (int)Col);

                            if (siteptr.RD <= targetRD)
                                return;
                        }
                    }
                }
            }
        }

        public void SiteDynamics(int RDflag, uint Row, uint Col)
        {
            Site siteptr = this[(int)Row, (int)Col];

            Landunit l = locateLanduPt((int)Row, (int)Col);

            if (0 == RDflag || 1 == RDflag || 2 == RDflag || 3 == RDflag)
            {
                GetSeedNumberOnSite(Row, Col);

                SeedGermination(siteptr, l, RDflag);

                GetRDofSite((int)Row, (int)Col);
                if (3 == RDflag)
                    NaturalMortality(siteptr, Row, Col, 0);//kill all ages of trees
                else
                    NaturalMortality(siteptr, Row, Col, 1);//kill the youngest of trees

                GetRDofSite((int)Row, (int)Col);

            }
            else
            {
                Debug.Assert(RDflag == 4); //otherwise, Site Dynamics Parameter Error.

                double thres_RD4 = TimeStep / 100000.0;
                double tmp = system1.drand();

                if (tmp > thres_RD4)
                {
                    NaturalMortality(siteptr, Row, Col, 0);//kill all ages of trees
                    GetRDofSite((int)Row, (int)Col);
                    if (siteptr.RD > l.MaxRD)
                    {
                        Selfthinning(siteptr, l, Row, Col);
                        GetRDofSite((int)Row, (int)Col);
                    }
                }
                else
                {
                    double targetRD = l.maxRDArray(0);
                    NaturalMortality_killbytargetRD(siteptr, Row, Col, targetRD);
                    GetRDofSite((int)Row, (int)Col);
                }
            }
            if (siteptr.RD <= 0.0)
            {
                //Console.WriteLine("\nrow = {0}, col = {1}, RDflag = {2}", Row, Col, RDflag);
                SeedGermination(siteptr, l, RDflag);
                GetRDofSite((int)Row, (int)Col);
                NaturalMortality(siteptr, Row, Col, 1);//kill the youngest of trees
                GetRDofSite((int)Row, (int)Col);
            }
            //Console.ReadLine();
        }

        public void GetMatureTree()
        {
            for (uint i = 1; i <= rows; i++)
            {
                for (uint j = 1; j <= columns; j++)
                {
                    Site siteptr = this[(int)i, (int)j];

                    uint siteptr_num = (uint)siteptr.numofsites;

                    for (int k = 1; k <= siteptr_num; k++)
                    {
                        int m_begin = siteptr.specAtt(k).maturity / TimeStep;
                        int m_limit = siteptr.specAtt(k).longevity / TimeStep;

                        Specie local_specie = siteptr.SpecieIndex(k);

                        uint treenum = 0;

                        for (int m = m_begin; m <= m_limit; m++)
                        {
                            treenum += (uint)local_specie.getTreeNum(m, k);
                        }

                        local_specie.MatureTree = (int)treenum;
                    }
                }
            }

        }


        public int SITE_compare(Site site1, Site site2)
        {
            Specie specie1 = site1.first();
            Specie specie2 = site2.first();

            int num = specie1.getAgeVectorNum();

            while (specie1 != null && specie2 != null)
            {
                if (specie1.vegPropagules > specie2.vegPropagules) return 1;
                if (specie1.vegPropagules < specie2.vegPropagules) return 2;

                if (specie1.disPropagules > specie2.disPropagules) return 1;
                if (specie1.disPropagules < specie2.disPropagules) return 2;

                for (int i = 0; i < num; ++i)
                {
                    if (specie1.getAgeVector(i) > specie2.getAgeVector(i))
                        return 1;

                    if (specie1.getAgeVector(i) < specie2.getAgeVector(i))
                        return 2;
                }

                specie1 = site1.next();
                specie2 = site2.next();
            }

            return 0;
        }

        public void fillinSitePt(int i, int j, Site local_site)
        {
            map[(i - 1) * columns + j - 1] = local_site;
        }

        public void SetBiomassNum(int num)
        {
            biomassNum = num;
            biomassData = null;
            biomassData = new double[num * 2];
        }

        public void SetBiomassThreshold(double num)
        {
            BiomassThreshold = num;
        }

        public void SetBiomassData(int i, int j, double value)
        {
            int temp;
            if (i > biomassNum || j < 1 || j > 2)
            {
                throw new Exception("index error at SetBiomass");
            }
            temp = (i - 1) * 2 + j - 1;
            biomassData[temp] = value;
        }

        public float GetBiomassData(int i, int j)
        {
            if (i > biomassNum || j < 1 || j > 2)
                throw new Exception("index error at GetBiomass");

            return (float)biomassData[(i - 1) * 2 + j - 1];
        }

        public void SetSeedRain(int spec, int Distance, float value)
        {
            int local_int = MaxDistofAllSpec / CellSize + 2;

            int id = (spec - 1) * local_int + Distance;


            if (id >= specNum * local_int)
                throw new Exception("Out bound in SetSeedRain\n");

            SeedRain[id] = value;
        }

        public int GetAgerangeCount(int specindex)
        {
            return SpeciesAgerangeArray[specindex * 500 / TimeStep];
        }

        public void GetSpeciesAgerangeArray(int specindex, int count, ref int value1, ref int value2)
        {
            if (specindex > specNum || count > 40)
                throw new Exception("Out bound in species age range\n");

            int local_index = specindex * 500 / TimeStep + count * 2;

            value1 = SpeciesAgerangeArray[local_index - 1];
            value2 = SpeciesAgerangeArray[local_index];
        }

        public int GetAgeDistStat_AgeRangeCount(int specindex)
        {
            return AgeDistStat_AgeRange[specindex * 500 / TimeStep];
        }

        public int GetAgeDistStat_YearCount(int specindex)
        {
            return AgeDistStat_Year[specindex * 500 / TimeStep];
        }

        public void GetAgeDistStat_AgeRangeVal(int specindex, int count, ref int value1, ref int value2)
        {
            if (specindex > specNum || count > 40)
                throw new Exception("Out bound in species age range\n");

            int local_index = specindex * 500 / TimeStep + count * 2;

            value1 = AgeDistStat_AgeRange[local_index - 1];
            value2 = AgeDistStat_AgeRange[local_index];
        }

        public void GetAgeDistStat_YearVal(int specindex, int count, ref int value1)
        {
            if (specindex > specNum || count > 40)
                throw new Exception("Out bound in species age range\n");

            value1 = AgeDistStat_Year[specindex * 500 / TimeStep + count];
        }

        //Get seed dispersal probability on a site and save in a matrix consists of distance and probability
        public void GetSeedDispersalProbability(string fileSeedDispersal, int seedflag)
        {
            float temp;
            Site sitetmp = this[1, 1];
            SeedRainFlag = seedflag;
            if (SeedRainFlag == 0)
            {
                for (int i = 1; i <= specNum; i++)  //Wen's code
                {
                    int local_maxD_d_cellsize = sitetmp.specAtt(i).maxD / CellSize;
                    float[] Probability = new float[local_maxD_d_cellsize + 1];
                    float probSum = 0.0f;
                    double tmp_inverse = -1.0 / local_maxD_d_cellsize;
                    for (int j = 0; j <= local_maxD_d_cellsize; j++)
                    {
                        //Probability[j] = (float)Math.Exp(-1.0 * j / local_maxD_d_cellsize);
                        Probability[j] = (float)Math.Exp(tmp_inverse * j);

                        probSum += Probability[j];
                    }
                    for (int j = 0; j <= local_maxD_d_cellsize; j++)
                    {
                        if (j == 0)
                        {
                            temp = Probability[j] / probSum;
                        }
                        else
                        {
                            temp = Probability[j] / probSum / (8 * j);
                        }
                        SetSeedRain(i, j, temp);
                    }
                    Probability = null;
                }
            }
            else
            {//Read From File
                StreamReader fp = new StreamReader(new FileStream(fileSeedDispersal, FileMode.Open));
                for (int i = 1; i <= specNum; i++)
                {
                    Speciesattr local_speciesattr = sitetmp.specAtt(i);
                    int local_speciesattr_maxD = local_speciesattr.maxD;
                    if (local_speciesattr_maxD < 0)
                    {
                        temp = system1.read_float(fp);
                        temp = Math.Abs(temp);

                        SetSeedRain(i, 0, temp);
                    }
                    else
                    {
                        int local_specatt_i_totalseed = local_speciesattr.TotalSeed;
                        if (local_speciesattr_maxD < CellSize)
                        {
                            for (int j = 0; j <= 1; j++)
                            {
                                // temp = system1.read_float(fp);
                                // temp = temp * local_specatt_i_totalseed;
                                temp = system1.read_float(fp) * local_specatt_i_totalseed;
                                SetSeedRain(i, j, temp);
                            }
                        }
                        else
                        {
                            for (int j = 0; j <= local_speciesattr_maxD / CellSize; j++)
                            {
                                // temp = system1.read_float(fp);
                                // temp = temp * local_specatt_i_totalseed;
                                temp = system1.read_float(fp) * local_specatt_i_totalseed;
                                SetSeedRain(i, j, temp);
                            }
                        }
                    }
                }
                fp.Close();
            }
        }

        public void SetGrowthRates(int flag, int spec, int year, double value, int index_landtype)
        {
            if (spec < 1 && spec > specNum && spec * year >= specNum * (320 / TimeStep + 1))
            {
               throw new Exception("Out bound in SetGrowthRates\n");
            }
            else
            {
                if (flag == 0)
                {
                    GrowthRates[(spec - 1) * (320 / TimeStep + 1) + year - 1] = value;
                }
                else
                {
                    if (index_landtype == GrowthRates_file.Count)
                    {
                        double[] temp = new double[specNum * (320 / TimeStep + 1)];
                        GrowthRates_file.Add(temp);
                    }
                    GrowthRates_file[index_landtype][(spec - 1) * (320 / TimeStep + 1) + year - 1] = value;
                }
            }
        }

        public void GetSpeciesGrowthRates(string fileGrowthRates, int growthrateflag)
        //Reads or calculates species' growth rates at corresponding ages,  no return value
        //Save the DBH and Age in a two dimension matrix, GrowthRates
        {
            int i;
            int j;
            double temp;
            Site sitetmp = new Site();
            sitetmp = this[1, 1];
            GrowthFlag = growthrateflag;
            if (GrowthFlag == 0)
            {
                for (i = 1; i <= specNum; i++)
                {
                    if (sitetmp.specAtt(i).SpType == 0)
                    {
                        for (j = 1; j <= sitetmp.specAtt(i).longevity / TimeStep + 1; j++)
                        {
                            temp = Math.Exp(-11.37 * Math.Exp(-0.088 * j * TimeStep / sitetmp.specAtt(i).longevity * 100)) * sitetmp.specAtt(i).MaxDQ;
                            SetGrowthRates(GrowthFlag, i, j, temp, 0);
                        }
                    }
                    else if (sitetmp.specAtt(i).SpType == 1)
                    {
                        for (j = 1; j <= sitetmp.specAtt(i).longevity / TimeStep + 1; j++)
                        {
                            temp = Math.Exp(-11.7 * Math.Exp(-0.12 * j * TimeStep / sitetmp.specAtt(i).longevity * 100)) * sitetmp.specAtt(i).MaxDQ;
                            SetGrowthRates(GrowthFlag, i, j, temp, 0);
                        }
                    }
                    else
                    {
                        for (j = 1; j <= sitetmp.specAtt(i).longevity / TimeStep + 1; j++)
                        {
                            SetGrowthRates(GrowthFlag, i, j, sitetmp.specAtt(i).MaxDQ, 0);
                        }
                    }
                }
            }
            else
            { //Read data from file
                StreamReader fp = new StreamReader(fileGrowthRates);
                if (fp == null)
                {
                    Console.WriteLine("Specie Growth Rates file not found.");
                }

                int numLU = 0;
                while (fp.Peek() >= 0)
                {
                    fp.ReadLine();
                    string instring;
                    string[] sarray = null;
                    for (i = 1; i <= specNum; i++)
                    {
                        instring = fp.ReadLine();
                        sarray = instring.Split(' ');
                        for (j = 1; j <= sitetmp.specAtt(i).longevity / TimeStep; j++)
                        {
                            temp = double.Parse(sarray[j]);
                            SetGrowthRates(GrowthFlag, i, j, temp, numLU);
                        }
                    }
                    fp.ReadLine();
                    numLU++;
                }
                fp.Close();
            }
        }

        public void SetMortalityRates(int flag, int spec, int year, float value, int index_landtype)
        {
            int local_const = 320 / TimeStep + 1;
            int index = (spec - 1) * local_const + year - 1;


            if (spec < 1 && spec > specNum && spec * year >= specNum * local_const)
                throw new Exception("Out bound in SetMortalityRates\n");

            if (flag == 0)
            {
                MortalityRates[index] = value;
            }
            else
            {
                if (index_landtype == MortalityRates_file.Count)
                {
                    float[] temp = new float[specNum * local_const];
                    MortalityRates_file.Add(temp);
                }

                MortalityRates_file[index_landtype][index] = value;
            }
        }

        public void GetSpeciesMortalityRates(string fileMortalityRates, int mortalityrateflag)
        {
            Site sitetmp = this[1, 1];
            MortalityFlag = mortalityrateflag;
            if (MortalityFlag != 0)
            {
                //read data from file
                StreamReader fp = new StreamReader(new FileStream(fileMortalityRates, FileMode.Open));
                //specific MortalityRates for different landtypes go here
                int numLU = 0;
                while (!system1.LDeof(fp))
                {
                    for (int i = 1; i <= specNum; i++)
                    {
                        int local_time_loop = sitetmp.specAtt(i).longevity / TimeStep;
                        for (int j = 1; j <= local_time_loop; j++)
                        {
                            float temp = system1.read_float(fp);
                            SetMortalityRates(MortalityFlag, i, j, temp, numLU);
                        }
                    }
                    numLU++;
                }
                fp.Close();
            }
        }

        public float GetGrowthRates(int spec, int year, int landtype_index)
        {
            int local_const = 320 / TimeStep + 1;
            int index = (spec - 1) * local_const + year - 1;

            Debug.Assert(spec >= 1 && spec <= specNum);
            Debug.Assert(spec * year < specNum * local_const);

            if (GrowthRates_file.Count == 0)
                return (float)GrowthRates[index];
            else
            {
                Debug.Assert(landtype_index < GrowthRates_file.Count);
                return (float)GrowthRates_file[landtype_index][index];
            }
        }

        public void SetVolume(int growthrate_flag, int spec, int year, float value, int index_landtype)
        {
            int local_const = 320 / TimeStep + 1;
            int index = (spec - 1) * local_const + year - 1;
            if (spec < 1 && spec > specNum && spec * year >= specNum * local_const)
                throw new Exception("Out bound in Volume\n");

            if (growthrate_flag == 0)
            {
                Volume[index] = value;
            }
            else
            {
                if (index_landtype == Volume_file.Count)
                {
                    float[] temp = new float[specNum * local_const];
                    Volume_file.Add(temp);
                }
                Volume_file[(index_landtype)][index] = value;
            }
        }

        public void GetVolumeRead(string fileVolumeFlag, int VolumeFlag_Flag)
        {
            Site sitetmp = this[1, 1];
            float[] VolumeTemp = new float[specNum * 6];
            VolumeFlag = VolumeFlag_Flag;
            if (VolumeFlag == 0)
            {
                for (int i = 1; i <= specNum; i++)
                {
                    for (int j = 1; j <= sitetmp.specAtt(i).longevity / TimeStep; j++)
                    {
                        double local_pow = Math.Pow(0.3048, 3.0);
                        if (GrowthFlag == 0)
                        {
                            float temp = (float)((-61.9 + 6.83 * GetGrowthRates(i, j, 0) / 2.54) * local_pow);
                            SetVolume(GrowthFlag, i, j, temp, 0);
                        }
                        else
                        {
                            for (int i_growth = 0; i_growth < GrowthRates_file.Count; i_growth++)
                            {
                                float temp = (float)((-61.9 + 6.83 * GetGrowthRates(i, j, i_growth) / 2.54) * local_pow);
                                SetVolume(GrowthFlag, i, j, temp, i_growth);
                            }
                        }
                    }
                }
            }
            else
            {//read data from file

                StreamReader fp = new StreamReader(new FileStream(fileVolumeFlag, FileMode.Open));
                float temp = system1.read_int(fp);
                Debug.Assert(temp <= int.MaxValue && temp >= int.MinValue);
                VolumeFlag = (int)temp;
                string FilenameHeights = system1.read_string(fp);
                StreamReader fpTreeheight = new StreamReader(new FileStream(FilenameHeights, FileMode.Open));

                if (VolumeFlag == 1)
                {
                    for (int i = 1; i <= specNum; i++)
                    {
                        int loc_id = (i - 1) * 6;
                        for (int j = 1; j <= 6; j++)
                        {
                            VolumeTemp[loc_id + j - 1] = system1.read_float(fp);
                        }
                    }
                    for (int i = 1; i <= specNum; i++)
                    {
                        for (int j = 1; j <= sitetmp.specAtt(i).longevity / TimeStep; j++)
                        {
                            float TreeHeight = system1.read_float(fpTreeheight);
                            int loc_id = (i - 1) * 6;
                            float vol_tmp0 = VolumeTemp[loc_id];
                            float vol_tmp1 = VolumeTemp[loc_id + 1];
                            float vol_tmp2 = VolumeTemp[loc_id + 2];
                            float vol_tmp3 = VolumeTemp[loc_id + 3];
                            float vol_tmp4 = VolumeTemp[loc_id + 4];
                            float vol_tmp5 = VolumeTemp[loc_id + 5];
                            if (GrowthFlag == 0)
                            {
                                float local_growthrate = GetGrowthRates(i, j, 0);
                                //temp = vol_tmp0 + vol_tmp1 * (float)Math.Pow(local_growthrate, vol_tmp2) + vol_tmp3 * (float)Math.Pow(local_growthrate, vol_tmp4) * (float)Math.Pow(TreeHeight, vol_tmp5);
                                temp = (float)(vol_tmp0 + vol_tmp1 * Math.Pow(local_growthrate, vol_tmp2) + vol_tmp3 * Math.Pow(local_growthrate, vol_tmp4) * Math.Pow(TreeHeight, vol_tmp5));
                                SetVolume(GrowthFlag, i, j, temp, 0);
                            }
                            else
                            {
                                for (int i_growth = 0; i_growth < GrowthRates_file.Count; i_growth++)
                                {
                                    float local_growthrate = GetGrowthRates(i, j, i_growth);
                                    //temp = vol_tmp0 + vol_tmp1 * (float)Math.Pow(local_growthrate, vol_tmp2) + vol_tmp3 * (float)Math.Pow(local_growthrate, vol_tmp4) * (float)Math.Pow(TreeHeight, vol_tmp5);
                                    temp = (float)(vol_tmp0 + vol_tmp1 * Math.Pow(local_growthrate, vol_tmp2) + vol_tmp3 * Math.Pow(local_growthrate, vol_tmp4) * Math.Pow(TreeHeight, vol_tmp5));
                                    SetVolume(GrowthFlag, i, j, temp, i_growth);
                                }
                            }
                        }
                    }
                }        
                else
                {
                    throw new Exception("compute volume wrong");
                }
                fpTreeheight.Close();
                fp.Close();
            }
            VolumeTemp = null;
        }

        public Landunit locateLanduPt(int i, int j)
            //find the landtype
        {
            int x;
            x = (i - 1) * columns;
            x = x + j - 1;
            return map_landtype[x];
        }

        public void GetRDofSite(int Row, int Col)
        //calculate the RD for a given site, No Return Value
        {
            int i;
            int j;
            int numSpec;
            Site siteptr = new Site();
            siteptr = this[Row, Col];

            siteptr.HighestShadeTolerance = 0;

            numSpec = siteptr.Number();

            siteptr.RD = 0;
            Landunit l;

            l = locateLanduPt(Row, Col);
            for (i = 1; i <= numSpec; i++)
            {
                if (siteptr.specAtt(i).SpType >= 0)
                {
                    for (j = 1; j <= siteptr.specAtt(i).longevity / TimeStep; j++)
                    {
                        double temp = Math.Pow((GetGrowthRates(i, j, l.ltID) / 25.4), 1.605) * siteptr.specAtt(i).MaxAreaOfSTDTree * siteptr.SpecieIndex(i).getTreeNum(j, i) / CellSize / CellSize;
                        siteptr.RD = siteptr.RD + temp;
                        if (siteptr.SpecieIndex(i).getTreeNum(j, i) > 0)
                        {
                            if (siteptr.HighestShadeTolerance < siteptr.specAtt(i).shadeTolerance)
                            {
                                siteptr.HighestShadeTolerance = siteptr.specAtt(i).shadeTolerance;
                            }
                        }
                    }
                }
            }
        }

        public void BiomassRead(StreamReader fp)
        {
            int temp;
            int i;
            int j;
            double value;
            string instring;


            fp.ReadLine();
            instring = fp.ReadLine();
            string[] sarray = instring.Split('#');
            temp = int.Parse(sarray[0]);
            sarray = null;
            //Console.WriteLine(temp);
            SetBiomassNum(temp);

            instring = fp.ReadLine();
            sarray = instring.Split('#');
            value = double.Parse(sarray[0]);
            sarray = null;
            //Console.WriteLine(value);
            SetBiomassThreshold(value);

            fp.ReadLine();
            for (i = 1; i <= temp; i++)
            {
                if (fp.Peek() >= 0)
                {
                    instring = fp.ReadLine();
                    sarray = instring.Split(' ');
                }
                for (j = 1; j <= 2; j++)
                {
                    value = double.Parse(sarray[j - 1]);
                    SetBiomassData(i, j, value);
                }
                //Console.WriteLine(sarray[0] + ' ' + sarray[1]);
                sarray = null;
            }
        }

        public void dim(int species, int i, int j)
        {
            if (map70 != null)
            {
                map70 = null; //Nim: added []
            }
            if (map_landtype != null)
            {
                map_landtype = null;
            }
            Site temp = new Site();
            temp.SetNumber(species);
            specNum = species;

            Pro0or401 = 1;
            map70 = new Site[i * j];
            map_landtype = new Landunit[i * j]; //Add by Qia Oct 07 2008
            SeedRain = new float[specNum * (MaxDistofAllSpec / CellSize + 2)];
            //for (int k = 0; k < species; k++)
            //{
            //    SeedRain[k] = new double[MaxDistofAllSpec / CellSize * MaxDistofAllSpec / CellSize];
            //}

            GrowthRates = new double[species * (320 / TimeStep + 1)];
            MortalityRates = new double[species * (320 / TimeStep + 1)];
            Volume = new double[species * (320 / TimeStep + 1)];
            rows = i;
            columns = j;
            OutputGeneralFlagArray = new int[(species + 1) * NumTypes70Output];
            OutputAgerangeFlagArray = new int[(species + 1) * NumTypes70Output];
            SpeciesAgerangeArray = new int[species * 500 / TimeStep];
            AgeDistStat_Year = new int[species * 500 / TimeStep];
            AgeDistStat_AgeRange = new int[species * 500 / TimeStep];

            countWenjuanDebug = 0;
        }

        public void SetOutputGeneralFlagArray(int i, int j, int value)
        {
            if (i > (specNum + 1) || j > NumTypes70Output)
            {
                throw new Exception("Out bound in Flag Array\n");
            }
            else
            {
                OutputGeneralFlagArray[i * NumTypes70Output + j] = value;
            }
        }

        public int GetOutputAgerangeFlagArray(uint i, int j)
        {
            if (i > (specNum + 1) || j > NumTypes70Output)
                throw new Exception("Out bound in Flag Array\n");

            return OutputAgerangeFlagArray[i * NumTypes70Output + j];
        }

        public void SetflagAgeOutput(int value)
        {
            flagAgeRangeOutput = value;
        }

        public void SetOutputAgerangeFlagArray(int i, int j, int value)
        {
            if (i > (specNum + 1) || j > NumTypes70Output)
            {
                throw new Exception("Out bound in Flag Array\n");
            }
            else
            {
                OutputAgerangeFlagArray[i * NumTypes70Output + j] = value;
            }
        }

        public int SetAgerangeCount(int specindex, int count)
        {
            SpeciesAgerangeArray[specindex * 500 / TimeStep] = count;
            return 1;
        }

        public void SetSpeciesAgerangeArray(int specindex, int count, int value1, int value2)
        {
            if (specindex > specNum)
            {
                throw new Exception("Out bound in species age range\n");
            }
            if (count > 40)
            {
                throw new Exception("Out bound in species age range\n");
            }
            SpeciesAgerangeArray[specindex * 500 / TimeStep + count * 2 - 1] = value1;
            SpeciesAgerangeArray[specindex * 500 / TimeStep + count * 2] = value2;
        }

        public int GetOutputGeneralFlagArray(uint i, int j)
        {
            if (i > (specNum + 1) || j > NumTypes70Output)
                throw new Exception("Out bound in Flag Array\n");
            else
                return OutputGeneralFlagArray[i * NumTypes70Output + j];
        }

        public void Read70OutputOption(string FileName)
        {
            StreamReader fp = new StreamReader(FileName);
            StreamReader fpAgeDist;

            int i;
            int j = 0;
            int count = 0;
            int value1;
            int value2;

            string temp;
            char ch;
            string[] instring;
            Console.Write("reading Landis 70 output customization file...\n");

            fp.ReadLine();

            ////general every species////
            for (i = 0; i < specNum; i++)
            {
                temp = fp.ReadLine();
                instring = temp.Split(' ');
                ///////////////////
                //TPA
                if (instring[1] == "Y")
                {
                    SetOutputGeneralFlagArray(i, TPA, 1);
                }
                else
                {
                    SetOutputGeneralFlagArray(i, TPA, 0);
                }

                // BA
                if (instring[2] == "Y")
                {
                    SetOutputGeneralFlagArray(i, BA, 1);
                }
                else
                {
                    SetOutputGeneralFlagArray(i, BA, 0);
                }

                //BIO

                if (instring[3] == "Y")
                {
                    SetOutputGeneralFlagArray(i, Bio, 1);
                }
                else
                {
                    SetOutputGeneralFlagArray(i, Bio, 0);
                }

                //IV
                if (instring[4] == "Y")
                {
                    SetOutputGeneralFlagArray(i, IV, 1);
                }
                else
                {
                    SetOutputGeneralFlagArray(i, IV, 0);
                }

                //SEEDS
                if (instring[5] == "Y")
                {
                    SetOutputGeneralFlagArray(i, Seeds, 1);
                }
                else
                {
                    SetOutputGeneralFlagArray(i, Seeds, 0);
                }

                //RD
                if (instring[6] == "Y")
                {
                    SetOutputGeneralFlagArray(i, RDensity, 1);
                }
                else
                {
                    SetOutputGeneralFlagArray(i, RDensity, 0);
                }
            }
            instring = null;
            ///////below is for general total//////////


            fp.ReadLine();
            fp.ReadLine();
            temp = fp.ReadLine();
            instring = temp.Split(' ');
            if (instring[1] == "Y")
            {
                SetOutputGeneralFlagArray(specNum, TPA, 1);

            }
            else
            {
                SetOutputGeneralFlagArray(specNum, TPA, 0);

            }
            if (instring[2] == "Y")
            {
                SetOutputGeneralFlagArray(specNum, BA, 1);
            }
            else
            {
                SetOutputGeneralFlagArray(specNum, BA, 0);
            }

            if (instring[3] == "Y")
            {
                SetOutputGeneralFlagArray(specNum, Bio, 1);

            }
            else
            {
                SetOutputGeneralFlagArray(specNum, Bio, 0);

            }

            if (instring[4] == "Y")
            {
                SetOutputGeneralFlagArray(specNum, Car, 1);
            }
            else
            {
                SetOutputGeneralFlagArray(specNum, Car, 0);
            }

            if (instring[5] == "Y")
            {
                SetOutputGeneralFlagArray(specNum, RDensity, 1);
            }
            else
            {
                SetOutputGeneralFlagArray(specNum, RDensity, 0);
            }
            instring = null;
            /////////////////////

            fp.ReadLine();
            temp = fp.ReadLine();
            instring = temp.Split(' ');
            if (instring[0] == "Y")
            {
                SetflagAgeOutput(1);
            }
            else
            {
                SetflagAgeOutput(0);
            }
            instring = null;
            /////below is age range for every species///////

            fp.ReadLine();
            fp.ReadLine();
            for (i = 0; i < specNum; i++)
            {
                temp = fp.ReadLine();
                instring = temp.Split(' ');
                //if (instring.Length <= 1) break;
                //////////////////////

                if (instring[1] == "Y")
                {
                    SetOutputAgerangeFlagArray(i, TPA, 1);
                }
                else
                {
                    SetOutputAgerangeFlagArray(i, TPA, 0);
                }

                if (instring[2] == "Y")
                {
                    SetOutputAgerangeFlagArray(i, BA, 1);
                }
                else
                {
                    SetOutputAgerangeFlagArray(i, BA, 0);
                }

                if (instring[3] == "Y")
                {
                    SetOutputAgerangeFlagArray(i, Bio, 1);
                }
                else
                {
                    SetOutputAgerangeFlagArray(i, Bio, 0);
                }

                if (instring[4] == "Y")
                {
                    SetOutputAgerangeFlagArray(i, IV, 1);
                }
                else
                {
                    SetOutputAgerangeFlagArray(i, IV, 0);
                }
            }
            instring = null;
            ///////below is for age range total//////////
            fp.ReadLine();
            fp.ReadLine();
            temp = fp.ReadLine();
            instring = temp.Split(' ');
            if (instring[1] == "Y")
            {
                SetOutputAgerangeFlagArray(specNum, TPA, 1);
            }
            else
            {
                SetOutputAgerangeFlagArray(specNum, TPA, 0);
            }

            if (instring[2] == "Y")
            {
                SetOutputAgerangeFlagArray(specNum, BA, 1);
            }
            else
            {
                SetOutputAgerangeFlagArray(specNum, BA, 0);
            }

            if (instring[3] == "Y")
            {
                SetOutputAgerangeFlagArray(specNum, Bio, 1);
            }
            else
            {
                SetOutputAgerangeFlagArray(specNum, Bio, 0);
            }

            if (instring[4] == "Y")
            {
                SetOutputAgerangeFlagArray(specNum, Car, 1);
            }
            else
            {
                SetOutputAgerangeFlagArray(specNum, Car, 0);
            }

            if (instring[5] == "Y")
            {
                SetOutputAgerangeFlagArray(specNum, RDensity, 1);
            }
            else
            {
                SetOutputAgerangeFlagArray(specNum, RDensity, 0);
            }
            instring = null;
            ////////////////////////
            ///
            fp.ReadLine();
            fp.ReadLine();
            if (flagAgeRangeOutput == 1)
            {
                for (i = 0; i < specNum; i++)
                {
                    temp = fp.ReadLine();
                    instring = temp.Split(' ');
                    count = int.Parse(instring[1]);

                    SetAgerangeCount(i, count);
                    for (j = 1; j <= count; j++)
                    {
                        value1 = int.Parse(instring[2]);
                        ch = char.Parse(instring[3]);
                        value2 = int.Parse(instring[4]);
                        SetSpeciesAgerangeArray(i, j, value1, value2);
                    }
                }
            }
            instring = null;
            ///////////////////////
            fp.ReadLine();
            fp.ReadLine();
            temp = fp.ReadLine();

            if (temp != "N/A")
            {
                Flag_AgeDistStat = 1;
                if ((fpAgeDist = new StreamReader(temp)) == null)
                {
                    throw new Exception("Can not open Read70OutputOption file");
                }
                //for (i = 0; i < specNum; i++)
                //{
                //    fscanc(fpAgeDist, "%s", temp);
                //    fscanf(fpAgeDist, "%d", count);
                //    SetAgeDistStat_YearValCount(i, count);
                //    for (j = 1; j <= count; j++)
                //    {
                //        fscanf(fpAgeDist, "%d", value1);
                //        SetAgeDistStat_YearVal(i, j, value1);
                //    }
                //}                //for (i = 0; i < specNum; i++)
                //{
                //    fscanc(fpAgeDist, "%s", temp);
                //    fscanf(fpAgeDist, "%d", count);
                //    SetAgeDistStat_YearValCount(i, count);
                //    for (j = 1; j <= count; j++)
                //    {
                //        fscanf(fpAgeDist, "%d", value1);
                //        SetAgeDistStat_YearVal(i, j, value1);
                //    }
                //}
                //for (i = 0; i < specNum; i++)
                //{
                //    fscanc(fpAgeDist, "%s", temp);
                //    fscanf(fpAgeDist, "%d", count);
                //    SetAgeDistStat_AgeRangeCount(i, count);
                //    for (j = 1; j <= count; j++)
                //    {
                //        fscanf(fpAgeDist, "%d", value1);
                //        fscanf(fpAgeDist, "%c", ch);
                //        fscanf(fpAgeDist, "%d", value2);
                //        SetAgeDistStat_AgeRangeVal(i, j, value1, value2);
                //    }
                //}
                //fpAgeDist.Close();
            }
            else
            {
                Flag_AgeDistStat = 0;
            }
            ///////////////////////
            fp.Close();
        }

        public void Harvest70outputdim()
        {
            BiomassHarvestCost = new double[rows * columns];
            CarbonHarvestCost = new double[rows * columns];
            Harvestflag = 1;
            Array.Clear(BiomassHarvestCost,0, rows * columns);
            Array.Clear(CarbonHarvestCost,0, rows * columns);
        }

        //There is a return at the begining;
        public void BefStChg(int i, int j)
            //Before Site Change
            //This function back up a site and following changes are based on this seprated site
            //sort vector is not touched here
        {
            return;
            //SITE temp;
            //temp = locateSitePt(i, j);
            //*sitetouse = temp;
            //if (temp.numofsites == 1)
            //{
            //    int pos;
            //    int ifexist = 0;
            //    SITE_LocateinSortIndex(sitetouse, pos, ifexist);
            //    if (ifexist != 0)
            //    {
            //        List<SITE>.Enumerator temp_sitePtr;
            //        temp_sitePtr = SortedIndex.begin();
            //        SortedIndex.erase(temp_sitePtr + pos);
            //        temp = null;
            //    }
            //    else
            //    {
            //        Console.Write("num of vectors {0:D}\n", SortedIndex.size());
            //        Console.Write("ERROR ERROR ERROR ERROR!!~~~{0:D}\n", pos);
            //        temp.dump();
            //        SortedIndex.at(pos).dump();
            //        SortedIndex.at(pos - 1).dump();
            //        SortedIndex.at(pos - 2).dump();
            //        SortedIndex.at(0).dump();
            //        SortedIndex.at(1).dump();
            //    }
            //}
            //else if (temp.numofsites <= 0)
            //{
            //    Console.Write("NO NO NO NO NO\n");
            //}
            //else
            //{
            //    temp.numofsites--;
            //}
            ////sitetouse->numofsites=1;
            //fillinSitePt(i, j, sitetouse);
            //return;
        }

        //There is a return at the begining
        public void AftStChg(int i, int j)
            //After Site Change
            //This function does combination and delete of the seprated site made by BefStChg(int i, int j)
            //insert this site to the sorted vector
        {
            return;
            //SITE_insert(0, sitetouse, i, j);
            //return;
        }

        public void Harvest70outputIncreaseBiomassvalue(int i, int j, double value)
        {
            int x;
#if BOUNDSCHECK  
		if (i <= 0 || i> rows || j <= 0 || j> columns)  
		{  
			 string err = new string(new char[80]);   
			 err = string.Format("SITES::operator() (int,int)-> ({0:D}, {1:D}) are illegal map					  coordinates", i, j);   
			 throw new Expection(err);    
		}   
#endif

            x = (i - 1) * columns;
            x = x + j - 1;
            BiomassHarvestCost[x] += value; // Add by Qia Oct 07 2008
        }

        public void Harvest70outputIncreaseCarbonvalue(int i, int j, double value)
        {
            int x;
#if BOUNDSCHECK 
			if (i <= 0 || i> rows || j <= 0 || j> columns) 
			{    
				 string err = new string(new char[80]);   
				 err = string.Format("SITES::operator() (int,int)-> ({0:D}, {1:D}) are illegal map						  coordinates", i, j);   
				 throw new Expection(err);    
			}
#endif

            x = (i - 1) * columns;
            x = x + j - 1;
            CarbonHarvestCost[x] += value; // Add by Qia Oct 07 2008
        }


        public void ListbubbleSort()
        {
            for (int i = AreaList.Count - 1; i > 0; --i)
            {
                for (int j = 1; j <= i; ++j)
                {
                    if (AreaList[j - 1] > AreaList[j])
                    {
                        {
                            float tmp = AreaList[j - 1];
                            AreaList[j - 1] = AreaList[j];
                            AreaList[j] = tmp;
                        }
                        {
                            int tmp = SpecIndexArray[j - 1];
                            SpecIndexArray[j - 1] = SpecIndexArray[j];
                            SpecIndexArray[j] = tmp;
                        }
                        {
                            int tmp = AgeIndexArray[j - 1];
                            AgeIndexArray[j - 1] = AgeIndexArray[j];
                            AgeIndexArray[j] = tmp;
                        }
                    }
                }
            }
        }

        public void Selfthinning(Site siteptr, Landunit l, uint row, uint col)
        {
            int[] quaterPercent = new int[specNum * 5];

            double Area_tobeThin;

            float TargetRD = (float)(siteptr.RD - l.MaxRD);

            if (TargetRD <= 0)
            {
                quaterPercent = null;
                return;
            }
            else
            {
                Area_tobeThin = TargetRD * CellSize * CellSize;
            }

            float[] thinning_percentatge = new float[5];
            thinning_percentatge[0] = 0.95f;
            thinning_percentatge[1] = 0.90f;
            thinning_percentatge[2] = 0.85f;
            thinning_percentatge[3] = 0.80f;
            thinning_percentatge[4] = 0.75f;


            for (int i = 1; i <= siteptr.Number(); i++)
            {
                int tmp_index = (i - 1) * 4;
                int longevity_d_timestep = siteptr.specAtt(i).longevity / TimeStep;

                quaterPercent[tmp_index + 0] = 1;
                quaterPercent[tmp_index + 1] = longevity_d_timestep / 4;
                quaterPercent[tmp_index + 2] = longevity_d_timestep / 2;
                quaterPercent[tmp_index + 3] = longevity_d_timestep / 4 * 3;
                quaterPercent[tmp_index + 4] = longevity_d_timestep;
            }



            for (int j = 0; j < 4; j++)
            {
                AreaList.Clear();

                SpecIndexArray.Clear();

                AgeIndexArray.Clear();

                if (j == 0)
                {
                    float subArea_tobeThin = 0;

                    for (int spec_i = 1; spec_i <= siteptr.Number(); spec_i++)
                    {
                        int local_id = (spec_i - 1) * 4 + j;

                        int age_beg = quaterPercent[local_id];
                        int age_end = quaterPercent[local_id + 1];

                        for (int age_i = age_beg; age_i < age_end; age_i++)
                        {
                            double term1 = Math.Pow((GetGrowthRates(spec_i, age_i, l.ltID) / 25.4), 1.605);
                            float term2 = (float)siteptr.specAtt(spec_i).MaxAreaOfSTDTree;
                            float term3 = thinning_percentatge[siteptr.specAtt(spec_i).shadeTolerance - 1];
                            uint term4 = (uint)siteptr.SpecieIndex(spec_i).getTreeNum(age_i, spec_i);


                            double temp = term1 * term2;
                            subArea_tobeThin += (float)(temp * term3 * term4);


                            AreaList.Add((float)temp);

                            SpecIndexArray.Add(spec_i);

                            AgeIndexArray.Add(age_i);

                        }

                    }


                    ListbubbleSort();


                    if (subArea_tobeThin >= Area_tobeThin)
                    {
                        for (int i = 0; i < AreaList.Count; i++)
                        {
                            Specie local_specie = siteptr.SpecieIndex(SpecIndexArray[i]);

                            uint local_tree_num = (uint)local_specie.getTreeNum(AgeIndexArray[i], SpecIndexArray[i]);

                            float local_thin_percent = thinning_percentatge[siteptr.specAtt(SpecIndexArray[i]).shadeTolerance - 1];


                            if (local_tree_num > 0)
                            {
                                float tempAreaInvolveTreeNum = AreaList[i] * local_tree_num * local_thin_percent;

                                if (tempAreaInvolveTreeNum > Area_tobeThin)
                                {
                                    double tmp_val = Area_tobeThin / AreaList[i];

                                    Debug.Assert(tmp_val <= uint.MaxValue && tmp_val >= int.MinValue);

                                    int treesToremove = (int)tmp_val;

                                    int treesLeft = (int)local_tree_num - treesToremove;

                                    local_specie.setTreeNum(AgeIndexArray[i], SpecIndexArray[i], Math.Max(0, treesLeft));

                                    //Console.WriteLine("Selfthinning set tree num = {0}", Math.Max(0, treesLeft));

                                    Area_tobeThin -= tempAreaInvolveTreeNum;

                                    quaterPercent = null;

                                    return;

                                }
                                else
                                {
                                    float float_treesLeft = local_tree_num * (1 - local_thin_percent);

                                    Debug.Assert(float_treesLeft <= int.MaxValue && float_treesLeft >= 0);

                                    int treesLeft = (int)float_treesLeft;

                                    local_specie.setTreeNum(AgeIndexArray[i], SpecIndexArray[i], Math.Max(0, treesLeft));

                                    //Console.WriteLine("Selfthinning set tree num = {0}", Math.Max(0, treesLeft));

                                    Area_tobeThin -= tempAreaInvolveTreeNum;

                                }

                            }

                        }

                    }

                    else
                    {

                        for (int i = 0; i < AreaList.Count; i++)
                        {
                            Specie local_specie = siteptr.SpecieIndex(SpecIndexArray[i]);

                            uint local_tree_num = (uint)local_specie.getTreeNum(AgeIndexArray[i], SpecIndexArray[i]);

                            float local_thin_percent = thinning_percentatge[siteptr.specAtt(SpecIndexArray[i]).shadeTolerance - 1];


                            if (local_tree_num > 0)
                            {
                                float tempAreaInvolveTreeNum = AreaList[i] * local_tree_num * local_thin_percent;

                                float float_treesLeft = local_tree_num * (1 - local_thin_percent);

                                Debug.Assert(float_treesLeft <= int.MaxValue && float_treesLeft >= 0);

                                int treesLeft = (int)float_treesLeft;

                                local_specie.setTreeNum(AgeIndexArray[i], SpecIndexArray[i], Math.Max(0, treesLeft));

                                //Console.WriteLine("Selfthinning set tree num = {0}", Math.Max(0, treesLeft));

                                Area_tobeThin -= tempAreaInvolveTreeNum;

                            }

                        }

                    }

                }

                AreaList.Clear();

                SpecIndexArray.Clear();

                AgeIndexArray.Clear();


                if (j == 1)
                {
                    float subArea_tobeThin = 0;

                    for (int spec_i = 1; spec_i <= siteptr.Number(); spec_i++)
                    {
                        int local_id = (spec_i - 1) * 4 + j;

                        int age_beg = quaterPercent[local_id];
                        int age_end = quaterPercent[local_id + 1];

                        for (int age_i = age_beg; age_i < age_end; age_i++)
                        {
                            double term1 = Math.Pow((GetGrowthRates(spec_i, age_i, l.ltID) / 25.4), 1.605);
                            float term2 = (float)siteptr.specAtt(spec_i).MaxAreaOfSTDTree;
                            float term3 = thinning_percentatge[siteptr.specAtt(spec_i).shadeTolerance - 1];
                            uint term4 = (uint)siteptr.SpecieIndex(spec_i).getTreeNum(age_i, spec_i);


                            double temp = term1 * term2;
                            subArea_tobeThin += (float)(temp * term3 * term4);

                            AreaList.Add((float)temp);

                            SpecIndexArray.Add(spec_i);

                            AgeIndexArray.Add(age_i);

                        }

                    }


                    ListbubbleSort();


                    if (subArea_tobeThin >= Area_tobeThin)
                    {
                        for (int i = 0; i < AreaList.Count; i++)
                        {
                            Specie local_specie = siteptr.SpecieIndex(SpecIndexArray[i]);

                            uint local_tree_num = (uint)local_specie.getTreeNum(AgeIndexArray[i], SpecIndexArray[i]);

                            float local_thin_percent = thinning_percentatge[siteptr.specAtt(SpecIndexArray[i]).shadeTolerance - 1];


                            if (local_tree_num > 0)
                            {
                                float tempAreaInvolveTreeNum = AreaList[i] * local_tree_num * local_thin_percent / 2;

                                if (tempAreaInvolveTreeNum > Area_tobeThin)
                                {
                                    double tmp_val = Area_tobeThin / AreaList[i];

                                    Debug.Assert(tmp_val <= int.MaxValue && tmp_val >= int.MinValue);

                                    int treesToremove = (int)tmp_val;

                                    int treesLeft = (int)local_tree_num - treesToremove;

                                    local_specie.setTreeNum(AgeIndexArray[i], SpecIndexArray[i], Math.Max(0, treesLeft));

                                    //Console.WriteLine("Selfthinning set tree num = {0}", Math.Max(0, treesLeft));

                                    Area_tobeThin -= tempAreaInvolveTreeNum;

                                    quaterPercent = null;

                                    return;

                                }

                                else
                                {
                                    float float_treeleft = local_tree_num * (1 - local_thin_percent / 2);

                                    Debug.Assert(float_treeleft <= int.MaxValue && float_treeleft >= int.MinValue);

                                    int treesLeft = (int)float_treeleft;

                                    local_specie.setTreeNum(AgeIndexArray[i], SpecIndexArray[i], Math.Max(0, treesLeft));

                                    //Console.WriteLine("Selfthinning set tree num = {0}", Math.Max(0, treesLeft));

                                    Area_tobeThin -= tempAreaInvolveTreeNum;

                                }

                            }

                        }

                    }
                    else
                    {
                        for (int i = 0; i < AreaList.Count; i++)
                        {
                            Specie local_specie = siteptr.SpecieIndex(SpecIndexArray[i]);

                            uint local_tree_num = (uint)local_specie.getTreeNum(AgeIndexArray[i], SpecIndexArray[i]);

                            float local_thin_percent = thinning_percentatge[siteptr.specAtt(SpecIndexArray[i]).shadeTolerance - 1];


                            if (local_tree_num > 0)
                            {
                                float tempAreaInvolveTreeNum = AreaList[i] * local_tree_num * local_thin_percent / 2;
                                {
                                    float float_treeleft = local_tree_num * (1 - local_thin_percent / 2);

                                    Debug.Assert(float_treeleft <= int.MaxValue && float_treeleft >= int.MinValue);

                                    int treesLeft = (int)float_treeleft;

                                    local_specie.setTreeNum(AgeIndexArray[i], SpecIndexArray[i], Math.Max(0, treesLeft));

                                    //Console.WriteLine("Selfthinning set tree num = {0}", Math.Max(0, treesLeft));

                                    Area_tobeThin -= tempAreaInvolveTreeNum;

                                }

                            }

                        }

                    }

                }


                AreaList.Clear();
                SpecIndexArray.Clear();
                AgeIndexArray.Clear();



                if (j == 2)
                {
                    float subArea_tobeThin = 0;

                    for (int spec_i = 1; spec_i <= siteptr.Number(); spec_i++)
                    {
                        int local_id = (spec_i - 1) * 4 + j;

                        int age_beg = quaterPercent[local_id];
                        int age_end = quaterPercent[local_id + 1];

                        for (int age_i = age_beg; age_i < age_end; age_i++)
                        {
                            double term1 = Math.Pow((GetGrowthRates(spec_i, age_i, l.ltID) / 25.4), 1.605);
                            float term2 = (float)siteptr.specAtt(spec_i).MaxAreaOfSTDTree;
                            float term3 = thinning_percentatge[siteptr.specAtt(spec_i).shadeTolerance - 1];
                            uint term4 = (uint)siteptr.SpecieIndex(spec_i).getTreeNum(age_i, spec_i);


                            double temp = term1 * term2;
                            subArea_tobeThin += (float)(temp * term3 * term4);


                            AreaList.Add((float)temp);

                            SpecIndexArray.Add(spec_i);

                            AgeIndexArray.Add(age_i);

                        }

                    }


                    ListbubbleSort();


                    if (subArea_tobeThin >= Area_tobeThin)
                    {
                        for (int i = 0; i < AreaList.Count; i++)
                        {
                            Specie local_specie = siteptr.SpecieIndex(SpecIndexArray[i]);

                            uint local_tree_num = (uint)local_specie.getTreeNum(AgeIndexArray[i], SpecIndexArray[i]);

                            float local_thin_percent = thinning_percentatge[siteptr.specAtt(SpecIndexArray[i]).shadeTolerance - 1];


                            if (local_tree_num > 0)
                            {
                                float tempAreaInvolveTreeNum = AreaList[i] * local_tree_num * local_thin_percent / 4;


                                if (tempAreaInvolveTreeNum > Area_tobeThin)
                                {
                                    double tmp_val = Area_tobeThin / (AreaList[i]);

                                    Debug.Assert(tmp_val <= int.MaxValue && tmp_val >= int.MinValue);

                                    int treesToremove = (int)tmp_val;

                                    int treesLeft = (int)local_tree_num - treesToremove;

                                    local_specie.setTreeNum(AgeIndexArray[i], SpecIndexArray[i], Math.Max(0, treesLeft));

                                    //Console.WriteLine("Selfthinning set tree num = {0}", Math.Max(0, treesLeft));

                                    Area_tobeThin -= tempAreaInvolveTreeNum;

                                    quaterPercent = null;

                                    return;

                                }
                                else
                                {
                                    float float_treeleft = local_tree_num * (1 - local_thin_percent / 4);

                                    Debug.Assert(float_treeleft <= int.MaxValue || float_treeleft >= int.MinValue);

                                    int treesLeft = (int)float_treeleft;

                                    local_specie.setTreeNum(AgeIndexArray[i], SpecIndexArray[i], Math.Max(0, treesLeft));

                                    //Console.WriteLine("Selfthinning set tree num = {0}", Math.Max(0, treesLeft));

                                    Area_tobeThin -= tempAreaInvolveTreeNum;

                                }

                            }

                        }

                    }

                    else
                    {
                        for (int i = 0; i < AreaList.Count; i++)
                        {
                            Specie local_specie = siteptr.SpecieIndex(SpecIndexArray[i]);

                            uint local_tree_num = (uint)local_specie.getTreeNum(AgeIndexArray[i], SpecIndexArray[i]);

                            float local_thin_percent = thinning_percentatge[siteptr.specAtt(SpecIndexArray[i]).shadeTolerance - 1];


                            if (local_tree_num > 0)
                            {
                                float tempAreaInvolveTreeNum = AreaList[i] * local_tree_num * local_thin_percent / 4;
                                {
                                    float float_treeleft = local_tree_num * (1 - local_thin_percent / 4);

                                    Debug.Assert(float_treeleft <= int.MaxValue && float_treeleft >= int.MinValue);

                                    int treesLeft = (int)float_treeleft;

                                    local_specie.setTreeNum(AgeIndexArray[i], SpecIndexArray[i], Math.Max(0, treesLeft));

                                    //Console.WriteLine("Selfthinning set tree num = {0}", Math.Max(0, treesLeft));

                                    Area_tobeThin -= tempAreaInvolveTreeNum;

                                }

                            }

                        }

                    }

                }


                AreaList.Clear();
                SpecIndexArray.Clear();
                AgeIndexArray.Clear();



                if (j == 3)
                {

                    float subArea_tobeThin = 0;

                    for (int spec_i = 1; spec_i <= siteptr.Number(); spec_i++)
                    {
                        int local_id = (spec_i - 1) * 4 + j;

                        int age_beg = quaterPercent[local_id];
                        int age_end = quaterPercent[local_id + 1];


                        for (int age_i = age_beg; age_i <= age_end; age_i++)
                        {
                            double term1 = Math.Pow((GetGrowthRates(spec_i, age_i, l.ltID) / 25.4), 1.605);
                            float term2 = (float)siteptr.specAtt(spec_i).MaxAreaOfSTDTree;
                            float term3 = thinning_percentatge[siteptr.specAtt(spec_i).shadeTolerance - 1];
                            uint term4 = (uint)siteptr.SpecieIndex(spec_i).getTreeNum(age_i, spec_i);


                            double temp = term1 * term2;
                            subArea_tobeThin += (float)(temp * term3 * term4);


                            AreaList.Add((float)temp);

                            SpecIndexArray.Add(spec_i);

                            AgeIndexArray.Add(age_i);
                        }

                    }


                    ListbubbleSort();


                    if (subArea_tobeThin >= Area_tobeThin)
                    {
                        for (int i = 0; i < AreaList.Count; i++)
                        {
                            Specie local_specie = siteptr.SpecieIndex(SpecIndexArray[i]);

                            uint local_tree_num = (uint)local_specie.getTreeNum(AgeIndexArray[i], SpecIndexArray[i]);

                            float local_thin_percent = thinning_percentatge[siteptr.specAtt(SpecIndexArray[i]).shadeTolerance - 1];


                            if (local_tree_num > 0)
                            {
                                float tempAreaInvolveTreeNum = AreaList[i] * local_tree_num * local_thin_percent / 8;

                                if (tempAreaInvolveTreeNum > Area_tobeThin)
                                {
                                    double tmp_val = Area_tobeThin / (AreaList[i]);

                                    Debug.Assert(tmp_val <= int.MaxValue && tmp_val >= int.MinValue);

                                    int treesToremove = (int)tmp_val;

                                    int treesLeft = (int)local_tree_num - treesToremove;

                                    local_specie.setTreeNum(AgeIndexArray[i], SpecIndexArray[i], Math.Max(0, treesLeft));

                                    //Console.WriteLine("Selfthinning set tree num = {0}", Math.Max(0, treesLeft));

                                    Area_tobeThin -= tempAreaInvolveTreeNum;

                                    quaterPercent = null;

                                    return;

                                }
                                else
                                {
                                    float float_treeleft = local_tree_num * (1 - local_thin_percent / 8);

                                    Debug.Assert(float_treeleft <= int.MaxValue && float_treeleft >= int.MinValue);

                                    int treesLeft = (int)float_treeleft;

                                    local_specie.setTreeNum(AgeIndexArray[i], SpecIndexArray[i], Math.Max(0, treesLeft));

                                    //Console.WriteLine("Selfthinning set tree num = {0}", Math.Max(0, treesLeft));

                                    Area_tobeThin -= tempAreaInvolveTreeNum;

                                }

                            }

                        }

                    }

                    else
                    {

                        for (int i = 0; i < AreaList.Count; i++)
                        {
                            Specie local_specie = siteptr.SpecieIndex(SpecIndexArray[i]);

                            uint local_tree_num = (uint)local_specie.getTreeNum(AgeIndexArray[i], SpecIndexArray[i]);

                            float local_thin_percent = thinning_percentatge[siteptr.specAtt(SpecIndexArray[i]).shadeTolerance - 1];


                            if (local_tree_num > 0)
                            {
                                float tempAreaInvolveTreeNum = AreaList[i] * local_tree_num * local_thin_percent / 8;
                                {
                                    float float_treeleft = local_tree_num * (1 - local_thin_percent / 8);

                                    Debug.Assert(float_treeleft <= int.MaxValue && float_treeleft >= 0);

                                    int treesLeft = (int)float_treeleft;

                                    local_specie.setTreeNum(AgeIndexArray[i], SpecIndexArray[i], Math.Max(0, treesLeft));

                                    Area_tobeThin -= tempAreaInvolveTreeNum;

                                }

                            }

                        }

                    }

                }

            }


            quaterPercent = null;

        }
    }
}
