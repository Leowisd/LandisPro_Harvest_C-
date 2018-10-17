using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandisPro
{
    class reclass
    {
        private static readonly int snr = Program.sites.numRows();
        private static readonly int snc = Program.sites.numColumns();
        private static readonly int time_step = Program.sites.TimeStep;

        public static void ageReclass(map8 m)
        {
            m.dim((uint)snr, (uint)snc);
            m.rename("Age class representation");
            for (uint j = 1; j < map8.MapmaxValue; j++)
                m.assignLeg(j, "");
            string str;
            //J.Yang hard coding changing itr*sites.TimeStep to itr
            //J.Yang maxLeg is defined as 256 in map8.h, therefore, maximum age cohorts it can output is 254 
            for (uint i = 1; i < map8.MaxValueforLegend - 4; i++)
            {
                str = string.Format("{0:   } - {1:   } yr", (i - 1) * time_step + 1, i * time_step);

                m.assignLeg(i, str);
            }
            m.assignLeg(0, "NoSpecies");
            m.assignLeg(map8.MaxValueforLegend - 1, "N/A");
            m.assignLeg(map8.MaxValueforLegend - 2, "Water");
            m.assignLeg(map8.MaxValueforLegend - 3, "NonForest");
            str = string.Format("	  >{0:   } yr", (map8.MaxValueforLegend - 4 - 1) * time_step);
            m.assignLeg(map8.MaxValueforLegend - 4, str);
            for (int i = snr; i >= 1; i--)
            {
                for (int j = 1; j <= snc; j++)
                {
                    if (Program.sites.locateLanduPt(i, j).active())
                    {
                        m[(uint)i, (uint)j] = 0;
                        uint myage = 0;
                        Site local_site = Program.sites[i, j];
                        Specie s = local_site.first();
                        while (s != null)
                        {
                            uint temp = (uint)s.oldest();
                            if (temp > myage)
                                myage = temp;
                            s = local_site.next();
                        }
                        m[(uint)i, (uint)j] = (ushort)(myage / time_step);
                    }
                    else if (Program.sites.locateLanduPt(i, j).lowland())
                        m[(uint)i, (uint)j] = (ushort)(map8.MaxValueforLegend - 3);
                    else if (Program.sites.locateLanduPt(i, j).water())
                        m[(uint)i, (uint)j] = (ushort)(map8.MaxValueforLegend - 2);
                    else
                        m[(uint)i, (uint)j] = (ushort)(map8.MaxValueforLegend - 1);
                }
            }
        }

        public static void ageReclassYoungest(map8 m)
        {
            m.dim((uint)snr, (uint)snc);
            m.rename("Age class representation");

            for (uint j = 1; j < map8.MapmaxValue; j++)
                m.assignLeg(j, "");

            string str;
            //J.Yang hard coding changing itr*sites.TimeStep to itr
            //J.Yang maxLeg is defined as 256 in map8.h, therefore, maximum age cohorts it can output is 254 
            for (uint i = 1; i < map8.MaxValueforLegend - 4; i++)
            {
                str = string.Format("{0:   } - {1:   } yr", (i - 1) * time_step + 1, i * time_step);
                m.assignLeg(i, str);
            }
            m.assignLeg(0, "NoSpecies");
            m.assignLeg(map8.MaxValueforLegend - 1, "N/A");
            m.assignLeg(map8.MaxValueforLegend - 2, "Water");
            m.assignLeg(map8.MaxValueforLegend - 3, "NonForest");

            str = string.Format("	  >{0:   } yr", (map8.MaxValueforLegend - 4 - 1) * time_step);
            m.assignLeg(map8.MaxValueforLegend - 4, str);

            for (int i = snr; i >= 1; i--)
            {
                for (int j = 1; j <= snc; j++)
                {
                    if (Program.sites.locateLanduPt(i, j).active())
                    {
                        m[(uint)i, (uint)j] = 0;
                        int myage = map8.MapmaxValue;
                        Site local_site = Program.sites[i, j];
                        Specie s = local_site.first();
                        while (s != null)
                        {
                            int temp = s.youngest();
                            if (temp < myage && s.youngest() > 0)
                                myage = temp;
                            s = local_site.next();
                        }
                        if (myage == map8.MapmaxValue)
                            myage = 0;
                        else
                            myage = myage / time_step;
                        m[(uint)i, (uint)j] = (ushort)myage;
                    }
                    else if (Program.sites.locateLanduPt(i, j).lowland())
                        m[(uint)i, (uint)j] = (ushort)(map8.MaxValueforLegend - 3);
                    else if (Program.sites.locateLanduPt(i, j).water())
                        m[(uint)i, (uint)j] = (ushort)(map8.MaxValueforLegend - 2);
                    else
                        m[(uint)i, (uint)j] = (ushort)(map8.MaxValueforLegend - 1);

                }

            }
        }

        public static void speciesAgeMap(map8 m, string ageFile)
        {
            int curSp = Program.speciesAttrs.current(ageFile);
            m.dim((uint)snr, (uint)snc);
            m.rename(ageFile);

            string str;
            for (uint i = 1; i < map8.maxLeg - 4; i++)
            {
                str = string.Format("{0:   } - {1:   } yr", (i - 1) * time_step + 1, i * time_step);

                m.assignLeg(i, str);
            }

            m.assignLeg(0, "NotPresent");

            m.assignLeg(map8.MaxValueforLegend - 1, "N/A");
            m.assignLeg(map8.MaxValueforLegend - 2, "Water");
            m.assignLeg(map8.MaxValueforLegend - 3, "NonForest");

            str = string.Format("	  >{0} yr", (map8.maxLeg - 4 - 1) * time_step);
            m.assignLeg(map8.MaxValueforLegend - 4, str);



            for (int i = snr; i >= 1; i--)
            {
                for (int j = 1; j <= snc; j++)
                {
                    if (Program.sites.locateLanduPt(i, j) == null)
                        throw new Exception("Invalid landunit error\n");

                    if (Program.sites.locateLanduPt(i, j).active())
                    {
                        m[(uint)i, (uint)j] = 0;       //where species not presents
                        if (Program.sites[i, j] == null)
                            throw new Exception("No site\n");

                        Specie s = Program.sites[i, j].current(curSp);

                        if (s == null)
                        {
                            Console.WriteLine("{0}\n", curSp);

                            throw new Exception("No Species\n");
                        }

                        if (s.query())
                        {
                            m[(uint)i, (uint)j] = (ushort)(s.oldest() / time_step); //compare ageReclass which uses +3 there???

                            if (m[(uint)i, (uint)j] > map8.MaxValueforLegend - 4)   //maximum longevity is 640 years// Notice 66 means 640 years
                                m[(uint)i, (uint)j] = (ushort)(map8.MaxValueforLegend - 4);
                        }

                    }

                    else if (Program.sites.locateLanduPt(i, j).water())

                        m[(uint)i, (uint)j] = (ushort)(map8.MaxValueforLegend - 2);

                    else if (Program.sites.locateLanduPt(i, j).lowland())

                        m[(uint)i, (uint)j] = (ushort)(map8.MaxValueforLegend - 3);

                    else

                        m[(uint)i, (uint)j] = (ushort)(map8.MaxValueforLegend - 1);

                }

            }


        }
    }
}
