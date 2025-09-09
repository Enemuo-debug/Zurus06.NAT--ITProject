using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.tools
{
    public class NicheFunctions
    {
        public static Niche MapNiche(int index)
        {
            Niche[] niches = [
                Niche.Network_Design,
                Niche.Network_Security,
                Niche.Network_Management_And_Monitoring,
                Niche.Network_Technology_And_Trends,
                Niche.Networking_Troubleshooting_And_Optimization
            ];
            return niches[index - 1];
        }
    }
}