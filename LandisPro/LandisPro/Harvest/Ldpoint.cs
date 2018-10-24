using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandisPro.Harvest
{
    class Ldpoint
    {
        public int x;
        public int y;

        public Ldpoint()
        {
            x = y = 0;
        }

        public Ldpoint(int tx, int ty)
        {
            x = tx;
            y = ty;
        }
    }
}
