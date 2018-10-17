#define OUTPUT

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LandisPro
{
    class SpeciesAttrs
    {
        private Speciesattr[] specAttrs;          //Array holding all species attributes.

        private int numAttrs;                    //Number of species attributes.

        private int currentAttr;                 //Current species attribute being pointed to by first and next access functions.	
        private int maxAttrs;                    //Maximum number of attributes.  Defined upon class construction.

        public int MaxDistanceofAllSpecs;
        public int MaxShadeTolerance;

        public SpeciesAttrs(int n)
        {
            numAttrs = 0;
            currentAttr = 0;
            maxAttrs = n;

            specAttrs = new Speciesattr[n];
            for (int i = 0; i < n; i++)
                specAttrs[i] = new Speciesattr();
        }

        public Speciesattr this[int index]
        {
            get
            {
                if (index > numAttrs || index == 0)
                    throw new Exception("Specie Attributes out bound");

                return specAttrs[index - 1];
            }
        }

        public int Number()
        {
            return numAttrs;
        }

        public int current(string name)
        {
            for (int i = 0; i < numAttrs; i++)
            {
                if (name.Equals(specAttrs[i].name))
                    return i;
            }

            // throw new Exception("does not exist\n");
            return -1;
        }

        public Speciesattr Operator(int n)
        {
            if (n > numAttrs || n == 0)
                throw new Exception("Specie Attributes out bound");
            else
                return specAttrs[n - 1];
        }

        public void Read(StreamReader infile, int cellSize)
        //Read set of species attributes from a file.
        {
#if (OUTPUT)
            Console.WriteLine("\nRead from SpeciesAttributes.dat");
            Console.WriteLine("=======================================");
#endif

            while (infile.Peek() >= 0)
            {
                if (numAttrs < maxAttrs)
                {
                    //specAttrs[numAttrs] = new Speciesattr();
                    specAttrs[numAttrs].Read(infile, cellSize);
                    numAttrs++;
                }
                else
                {
                    throw new Exception("SPECIESATTRS::read(FILE*)-> Array bounds error.");
                }
            }
#if (OUTPUT)
            Console.WriteLine("=======================================");
            Console.WriteLine("SpeciesAttributesc.dat reading finished!\n");
#endif

            Console.WriteLine("number of species attributes: " + numAttrs);

            MaxDistanceofAllSpecs = 0;
            for (int i = 0; i < numAttrs; i++)
            {
                if (specAttrs[i].maxD >= MaxDistanceofAllSpecs)
                {
                    MaxDistanceofAllSpecs = specAttrs[i].maxD;
                }
            }
            MaxShadeTolerance = 0;
            for (int i = 0; i < numAttrs; i++)
            {
                if (specAttrs[i].shadeTolerance >= MaxShadeTolerance && specAttrs[i].SpType >= 0)
                {
                    MaxShadeTolerance = specAttrs[i].shadeTolerance;
                }
            }
        }
    }
}
