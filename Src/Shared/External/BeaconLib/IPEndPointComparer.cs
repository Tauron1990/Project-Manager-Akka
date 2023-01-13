using System;
using System.Collections.Generic;
using System.Net;

namespace BeaconLib
{
    internal sealed class IpEndPointComparer : IComparer<IPEndPoint>
    {
        internal static readonly IpEndPointComparer Instance = new IpEndPointComparer();

        public int Compare(IPEndPoint? first, IPEndPoint? secund)
        {
            int RunCompare()
            {
                if(secund is null)
                    return 1;

                int compare = string.Compare(first.Address.ToString(), secund.Address.ToString(), StringComparison.Ordinal);

                if(compare != 0) return compare;

                return secund.Port - first.Port;
            }

            return first switch
            {
                null when secund is null => 0,
                null => -1,
                _ => RunCompare()
            };
        }
    }
}