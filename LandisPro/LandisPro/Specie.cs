using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LandisPro
{
    class Specie: agelist
    {
        public short vegPropagules; //Number of years of dispersed propagules present.
        public short disPropagules;
        public uint AvailableSeed;
        public int TreesFromVeg;
        public int MatureTree;

        private int index;

        public Specie()
        {
            vegPropagules = 0;
            disPropagules = 0;
        }

        public Specie(int index)
        {
            vegPropagules = 0;
            disPropagules = 0;
            this.index = index;
        }

        public new void clear()
        {
            vegPropagules = 0;
            disPropagules = 0;

            base.clear();
        }

        public void readTreeNum(StreamReader infile, int specIndex)

        {
            int a;
            string instring = infile.ReadLine();
            string[] sarray = instring.Split('#');
            string[] sarray2 = sarray[2].Split(' ');
            a = int.Parse(sarray2[1]);
            vegPropagules = (short)a;

            base.readTreeNum(sarray2, specIndex);

            TreesFromVeg = 0;
        }

        public void initilizeDisPropagules(int maturity, string name)
        {
            if (oldest() >= maturity)
            {
                disPropagules = 1; //can disperse, non OS
            }
        }

        public void copy(Specie in_specie)
        {
            if (in_specie == null)
                return;

            vegPropagules = in_specie.vegPropagules;

            disPropagules = in_specie.disPropagules;

            AvailableSeed = in_specie.AvailableSeed;

            TreesFromVeg = in_specie.TreesFromVeg;

            MatureTree = in_specie.MatureTree;

            index = in_specie.index;

            base.copy(in_specie.agevector);
        }
    }
}
