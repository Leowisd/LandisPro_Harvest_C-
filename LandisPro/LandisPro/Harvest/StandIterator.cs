using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandisPro.Harvest
{
    class StandIterator
    {
        public Ldpoint p;
        public Stand stand;
        public StandIterator(Stand s)
        {
            stand = s;
            p = s.itsMinPoint;
            if (s.itsMinPoint.x <= 0 && s.itsMinPoint.y <= 0)
                throw new Exception("Invaild stand point");
            while (moreSites() && !stand.inStand(p.y, p.x))
                advance();
        }

        public Boolean moreSites()
        {
            return (p.y <= stand.itsMaxPoint.y);
        }

        public void advance()
        {
            if (++p.x > stand.itsMaxPoint.x)
            {
                p.y++;
                p.x = stand.itsMinPoint.x;
            }
        }

        public Ldpoint getCurrentSite()
        {
            if (!moreSites())
                throw new Exception("invaild site");
            return p;
        }

        public void gotoNextSite()
        {
            advance();
            while (moreSites() && !stand.inStand(p.y,p.x))
                advance();
        }
    }
}
