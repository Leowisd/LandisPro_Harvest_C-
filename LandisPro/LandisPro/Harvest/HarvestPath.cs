using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace LandisPro.Harvest
{
    class HarvestPath
    {
        private List<Ldpoint> itsPath = new List<Ldpoint>();

        public HarvestPath()
        {
            reset();
        }

        public void reset()
        {
            itsPath.Clear();
        }

        public void append(Ldpoint pt)
        {
            Console.WriteLine("HarvestPath::append, pt.x={0}, pt.y={1}", pt.x, pt.y);
            itsPath.Add(pt);
        }

        public void addpath(HarvestPath somePath)
        {
            foreach (Ldpoint it in somePath.itsPath)
            {
                itsPath.Add(it);
            }
        }

        public void dump()
        {
            foreach (Ldpoint pt in itsPath)
            {
                Console.WriteLine("element in the path is x:{0}, y:{1}", pt.x, pt.y);
            }
        }
    }
}
