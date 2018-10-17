using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LandisPro
{
    class Species
    {
        static SpeciesAttrs speciesAttrs;
        protected static int numSpec = 0;
        protected Specie[] species;
        private byte currentSpec;

        public Species(int n)
        {
            species = null;
            if (numSpec == 0)
            {
                species = new Specie[n];
                for (int i = 0; i < n; i++)
                {
                    species[i] = new Specie();
                }
                numSpec = n;
                currentSpec = 0;
            }
            else throw new Exception("SPECIES::SPECIES(int)-> Number of species may only be set once at construction");
        }

        public Species()
        {
            species = null;
            if (numSpec == 0)
            {
                species = null;
                currentSpec = 0;
            }
            else
            {
                species = new Specie[numSpec];
                currentSpec = 0;
                for (int i = 0; i < numSpec; i++)
                {
                    species[i] = new Specie();
                    species[i].AGELISTAllocateVector(i);
                }
            }
        }

        public Specie first()
        {
            currentSpec = 0;
            return species[currentSpec];
        }

        public Specie current(int n)
        {
            if (n >= numSpec || n < 0)
                return null;
            else
            {
                currentSpec = (byte)n;
                return species[n];
            }
        }

        public Specie next()
        {
            currentSpec++;

            if (currentSpec >= numSpec)
                return null;
            else
                return species[currentSpec];

        }

        public int Number()
        {
            return numSpec;
        }

        public Specie SpecieIndex(int n)
        {
            if (n > numSpec || n <= 0)
                throw new Exception("Specie Index Error out Bound");
            else
                return species[n - 1];
        }

        public void Read(StreamReader infile)
        //Read a set of species from a file.
        {
            infile.ReadLine();
            for (int i = 0; i < numSpec; i++)
            {
                species[i].readTreeNum(infile, i);
                species[i].initilizeDisPropagules(this.specAtt(i + 1).maturity, this.specAtt(i + 1).name);
            }
            infile.ReadLine();
            infile.ReadLine();
        }

        public static void Attach(SpeciesAttrs s)
        {
            speciesAttrs = s;
        }

        public void SetNumber(int n)
        {
            if (numSpec == 0)
            {
                numSpec = n;
            }
            else
                throw new Exception("SPECIES::setNumber()-> Number of species already set.");
        }

        public Speciesattr specAtt(int i)
        {
            if (speciesAttrs == null)
            {
                throw new Exception("SPECIES::specAtt()-> Species attributes not attached to              SPECIES.");
            }
            if (i > numSpec || i == 0)
            {
                throw new Exception("specAtt out of bound");
            }
            else
            {
                return speciesAttrs.Operator(i);
            }
        }

        public void copy(Specie[] in_all_species, int in_numSpec)
        {
            if (in_all_species == null)
                return;

            numSpec = in_numSpec;

            species = new Specie[numSpec];

            for (int i = 0; i < numSpec; i++)
            {
                species[i] = new Specie(i);
                species[i].copy(in_all_species[i]);
            }

            currentSpec = 0;
        }
    }
}
