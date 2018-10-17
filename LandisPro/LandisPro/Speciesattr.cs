#define OUTPUT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LandisPro
{
    class Speciesattr
    {
        public string name; //Species Name.
        public int longevity; //Maximum seeding distance.
        public int maturity;
        public int shadeTolerance;
        public int fireTolerance;
        public int effectiveD;
        public int maxD;
        public double alpha; //Probability of vegetative seeding.
        public double vegProb;
        public int maxSproutAge; //Maximum sprouting age.
        public double reclassCoef; //Reclassification coefficient.
        //<Add By Qia on June 29 2009>
        public int SpType;
        public int BioMassCoef;
        public int MaxDQ;
        public int CarbonCoef;
        public int SDImax;
        public int TotalSeed;
        public double MaxAreaOfSTDTree;
        public double CarbonCoEfficient;
        public int MinSproutAge;

        public Speciesattr()
        {
            name = null;
            longevity = 0;
            maturity = 0;
            shadeTolerance = 0;
            fireTolerance = 0;
            effectiveD = 0;
            maxD = 0;
            alpha = 0;
            vegProb = 0;
            maxSproutAge = 0;
            reclassCoef = 0;
        }

        public void Read(StreamReader inFile, int cellSize)
        {
            string instring;

            if ((instring = inFile.ReadLine()) == null)
                Console.WriteLine("Read error in spec. attr. file.");

            string[] sarray = instring.Split(' ');

            int index = 0;

            name = sarray[index];
            index++;


            longevity = int.Parse(sarray[index]);
            index++;

            maturity = int.Parse(sarray[index]);
            index++;

            shadeTolerance = int.Parse(sarray[index]);
            index++;

            fireTolerance = int.Parse(sarray[index]);
            index++;

            effectiveD = int.Parse(sarray[index]);
            index++;

            maxD = int.Parse(sarray[index]);
            index++;

            alpha = double.Parse(sarray[index]);
            index++;

            vegProb = double.Parse(sarray[index]);
            index++;

            MinSproutAge = int.Parse(sarray[index]);
            index++;


            maxSproutAge = int.Parse(sarray[index]);
            index++;

            reclassCoef = double.Parse(sarray[index]);
            index++;

            SpType = int.Parse(sarray[index]);
            index++;

            BioMassCoef = int.Parse(sarray[index]);
            index++;

            MaxDQ = int.Parse(sarray[index]);
            index++;

            SDImax = int.Parse(sarray[index]);
            index++;

            MaxAreaOfSTDTree = 10000.0 / (double)SDImax;

            TotalSeed = int.Parse(sarray[index]);
            index++;

            CarbonCoEfficient = double.Parse(sarray[index]);
            index++;
#if (OUTPUT)

            Console.WriteLine(name + ' ' + longevity + ' ' + maturity + ' ' + shadeTolerance + ' ' + fireTolerance + ' ' + effectiveD + ' ' + maxD + ' ' + alpha + ' ' + vegProb + ' ' + MinSproutAge + ' '+maxSproutAge + ' ' + reclassCoef + ' ' + SpType + ' ' + BioMassCoef + ' ' + MaxDQ + ' ' + SDImax + ' ' + TotalSeed + ' ' + CarbonCoEfficient);

#endif
        }
    }
}
