using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LandisPro
{
    class reclass3
    {
        public static void reclassify(int timeStep, string[] ageMaps)
        {
            int specAtNum = Program.speciesAttrs.Number();
            int yDim = Program.sites.numRows();
            int xDim = Program.sites.numColumns();
            for (int i = 0; i < specAtNum; i++)
            {
                string str = ageMaps[i] + ".age";
                string speciesName;
                //read species name from ageIndex file
                using (StreamReader inAgeIndex = new StreamReader(str))
                {
                    speciesName = system1.read_string(inAgeIndex);
                }
                int curSp =Program.speciesAttrs.current(speciesName);
                //read age map file from output directory
                str = Program.parameters.outputDir + "/" + ageMaps[i] + timeStep.ToString() + ".gis";
                using (BinaryReader inAgeMap = new BinaryReader(File.Open(str, FileMode.Open)))
                {
                    byte[] dest = new byte[128];
                    inAgeMap.Read(dest, 0, 128);
                    // read inAgeMap
                    for (int k = yDim; k > 0; k--)
                    {
                        for (int j = 1; j <= xDim; j++)
                        {
                            int coverType = inAgeMap.Read();
                            if (coverType == 255)          //species absence
                            {
                                Specie s = Program.sites[k, j].current(curSp);
                                s.clear();
                            }
                            else if (coverType >= 3) //0-empty 1-water 2-nonforest
                            {
                                Specie s = Program.sites[k, j].current(curSp);
                                s.clear();
                                s.set((coverType - 2) * Program.sites.TimeStep);
                            }
                        }//end for
                    }//end for
                }//end using
            }//end for

        }
    }
}
