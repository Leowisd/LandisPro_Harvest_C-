using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace LandisPro.Harvest
{

    class HARVESTSite
    {
        public int itsMaxAge;
        public byte updateFlag;
        public short harvestType;
        public short lastHarvest;
        public short harvestExpirationDecade;
        public int numofsites;

        public void setUpdateFlag()
        {
            updateFlag = 1;
        }

        //same as operator=  in C++ version
        public HARVESTSite CopyFrom(HARVESTSite site)
        {
            itsMaxAge = site.itsMaxAge;
            updateFlag = site.updateFlag;
            harvestType = site.harvestType;
            lastHarvest = site.lastHarvest;
            harvestExpirationDecade = site.harvestExpirationDecade;
            numofsites = 1;
            return this;
        }

        public void update(int iRol, int iCol)
        {
            if (updateFlag == 1)
            {
                agelist a = new agelist();
                itsMaxAge = 0;
                for (int i = 1; i < BoundedPocketStandHarvester.pCoresites[iRol, iCol].Number(); i++)
                {
                    a = (agelist)BoundedPocketStandHarvester.pCoresites[iRol, iCol][i];
                    itsMaxAge = Math.Max(itsMaxAge, a.oldest());
                }
                updateFlag = 0;
            }
        }

        public int getHarvestType()
        {
            return (int)harvestType;
        }

        public int getHarvestDecade()
        {
            return (int)lastHarvest;
        }

        public int getMaxAge(int iRol, int iCol)
        {
            update(iRol, iCol);
            return itsMaxAge;
        }

        public bool canBeHarvested(int iRol, int iCol)
        {
            bool result;
            result = BoundedPocketStandHarvester.pCoresites.locateLanduPt(iRol, iCol).active();
            return result;
        }

        public bool wasRecentlyHarvested()
        {
            bool result;
            result = lastHarvest > 0 && (BoundedPocketStandHarvester.currentDecade - lastHarvest <= BoundedPocketStandHarvester.iParamharvestDecadeSpan);
            return result;
        }
    }



    class HARVESTSites
    {
        public int m_iRows;
        public int m_iCols;
        public List<HARVESTSite> SortedIndex = new List<HARVESTSite>();
        private HARVESTSite[] map;
        private HARVESTSite sitetouse;
        private double[] BA_harvest_output;
        private double[][] BA_harvest_output_spec;

        public HARVESTSites()
        {
            m_iRows = 0;
            m_iCols = 0;
        }

        public HARVESTSites(int r, int c)
        {
            int x;
            m_iRows = r;
            m_iCols = c;
            map = new HARVESTSite[r * c]; //Add by Qia Nov 07 2008
            for (int i = 0; i < r*c; i++)
            {
                map[i] = new HARVESTSite();
            }

            BA_harvest_output = new double[r * c];
            BA_harvest_output_spec = new double[BoundedPocketStandHarvester.pCoresites.specNum][];

            for (int i = 0; i < BoundedPocketStandHarvester.pCoresites.specNum; i++)
            {
                BA_harvest_output_spec[i] = new double[r * c];
            }

            if (map == null)
                throw new Exception("No Memory for harvestSite map");
            HARVESTSite site;
            sitetouse = new HARVESTSite();
            //int classsize = sizeof(HARVESTSite);

            for (int i = 1; i <= r; i++)
            {
                for (int j = 1; j <= c; j++)
                {
                    site = new HARVESTSite();
                    site.lastHarvest = 0;
                    site.harvestType = 0;
                    site.harvestExpirationDecade = 0;
                    site.itsMaxAge = 0;
                    site.updateFlag = 1;
                    site.numofsites = r * c;
                    x = (i - 1) * m_iCols;
                    x = x + j - 1;
                    map[x] = site;
                    BA_harvest_output[x] = 0.0;
                }
            }
            for (int i_spec = 0; i_spec < BoundedPocketStandHarvester.pCoresites.specNum; i_spec++)
            {
                for (int i = 1; i <= r; i++)
                {
                    for (int j = 1; j <= c; j++)
                    {
                        x = (i - 1) * m_iCols;
                        x = x + j - 1;
                        BA_harvest_output_spec[i_spec][x] = 0.0;
                    }
                }
            }

        }

        //there is a return at the begining, whihc means the function doesn't take any process.
        public void BefStChg(int i, int j)
        {
            return;
            //HARVESTSite temp;
            //temp = locateSitePt(i, j);
            //*sitetouse = temp;
            //if (temp.numofsites == 1)
            //{
            //    int pos;
            //    int ifexist = 0;
            //    SITE_LocateinSortIndex(sitetouse, pos, ifexist);
            //    if (ifexist != 0)
            //    {
            //        List<HARVESTSite>.Enumerator temp_sitePtr;
            //        temp_sitePtr = SortedIndex.begin();
            //        SortedIndex.erase(temp_sitePtr + pos);
            //        temp = null;
            //    }
            //    else
            //    {
            //        Console.Write("num of vectors {0:D}\n", SortedIndex.size());
            //        fflush(stdout);
            //        Console.Write("ERROR ERROR ERROR ERROR!!~~~{0:D}\n", pos);
            //        fflush(stdout);
            //    }
            //}
            //else if (temp.numofsites <= 0)
            //{
            //    Console.Write("NO NO NO NO NO\n");
            //    fflush(stdout);
            //}
            //else
            //{
            //    temp.numofsites--;
            //}
            ////sitetouse->numofsites=1;
            //fillinSitePt(i, j, sitetouse);
            //return;
        }

        // there is a return at the begining, whihc means the function doesn't take any process.
        public void AftStChg(int i, int j)
        {
            return;
            //SITE_insert(0, sitetouse, i, j);
            //return;
        }

        public HARVESTSite this[int i, int j]
        {
            get
            {
                int x;

                x = (i - 1) * m_iCols;
                x = x + j - 1;
                return map[x];
            }
            set
            {
                int x;

                x = (i - 1) * m_iCols;
                x = x + j - 1;
                map[x] = value;
            }
        }


    }
}
