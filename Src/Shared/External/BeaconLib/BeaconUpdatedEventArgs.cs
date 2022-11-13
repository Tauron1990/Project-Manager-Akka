using System;
using System.Collections.Generic;

namespace BeaconLib
{
    public sealed class BeaconUpdatedEventArgs : EventArgs
    {
        public IEnumerable<BeaconLocation> BeaconLocations { get; }

        public BeaconUpdatedEventArgs(IEnumerable<BeaconLocation> beaconLocations)
            => BeaconLocations = beaconLocations;
    }
}