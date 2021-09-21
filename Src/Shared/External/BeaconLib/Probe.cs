using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NLog;

namespace BeaconLib
{
    /// <summary>
    ///     Counterpart of the beacon, searches for beacons
    /// </summary>
    /// <remarks>
    ///     The beacon list event will not be raised on your main thread!
    /// </remarks>
    [PublicAPI]
    public sealed class Probe : IDisposable
    {
        private static ILogger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///     Remove beacons older than this
        /// </summary>
        private static readonly TimeSpan BeaconTimeout = new TimeSpan(0, 0, 0, 5); // seconds

        private readonly Task _thread;
        private readonly UdpClient _udp = new UdpClient();
        private readonly EventWaitHandle _waitHandle = new EventWaitHandle(initialState: false, EventResetMode.AutoReset);
        private IEnumerable<BeaconLocation> _currentBeacons = Enumerable.Empty<BeaconLocation>();

        private bool _running = true;

        #pragma warning disable AV1500
        public Probe(string beaconType)
            #pragma warning restore AV1500
        {
            _udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, optionValue: true);

            BeaconType = beaconType;
            _thread = new Task(BackgroundLoop, TaskCreationOptions.LongRunning);

            _log.Info("Bind Probe To Port 0");
            _udp.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
            try
            {
                _udp.AllowNatTraversal(allowed: true);
            }
            #pragma warning disable AV1706
            catch (Exception ex)
                #pragma warning restore AV1706
            {
                _log.Error(ex, "Error switching on NAT traversal");
            }

            _udp.BeginReceive(ResponseReceived, null);
        }

        public string BeaconType { get; private set; }

        public void Dispose()
        {
            try
            {
                Stop();
            }
            catch (Exception exception)
            {
                _log.Error(exception, "Error On Dispose Probe");
            }
        }

        public event Action<IEnumerable<BeaconLocation>>? BeaconsUpdated;

        public void Start()
        {
            _log.Info("Starting Probe");
            _thread.Start();
        }

        #pragma warning disable AV1500
        private void ResponseReceived(IAsyncResult ar)
            #pragma warning restore AV1500
        {
            _log.Info("Incomming Reponse");
            var remote = new IPEndPoint(IPAddress.Any, 0);
            var bytes = _udp.EndReceive(ar, ref remote);

            var typeBytes = Beacon.Encode(BeaconType);
            if (Beacon.HasPrefix(bytes, typeBytes))
                ProcessResponse(bytes, typeBytes, remote);
            else
                _log.Info("Incompatiple Data");

            _udp.BeginReceive(ResponseReceived, null);
        }

        private void ProcessResponse(byte[] bytes, byte[] typeBytes, IPEndPoint remote)
        {
            try
            {
                _log.Info("Processing Response");
                var portBytes = bytes.Skip(typeBytes.Length).Take(2).ToArray();
                var port = (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(portBytes, 0));
                var payload = Beacon.Decode(bytes.Skip(typeBytes.Length + 2));
                NewBeacon(new BeaconLocation(new IPEndPoint(remote.Address, port), payload, DateTime.Now));
            }
            catch (Exception exception)
            {
                _log.Error(exception, "Error on Decode Recived Beacon");
            }
        }

        private void BackgroundLoop()
        {
            while (_running)
            {
                try
                {
                    BroadcastProbe();
                }
                catch (Exception exception)
                {
                    _log.Error(exception, "Error on Sending");
                }

                _waitHandle.WaitOne(2000);
                PruneBeacons();
            }
        }

        #pragma warning disable AV1710
        private void BroadcastProbe()
            #pragma warning restore AV1710
        {
            _log.Info("Sending Request");
            var probe = Beacon.Encode(BeaconType).ToArray();
            _udp.Send(probe, probe.Length, new IPEndPoint(IPAddress.Broadcast, Beacon.DiscoveryPort));
        }

        private void PruneBeacons()
        {
            _log.Info("Prune Beacons");
            var cutOff = DateTime.Now - BeaconTimeout;
            var oldBeacons = _currentBeacons.ToList();
            var newBeacons = oldBeacons.Where(location => location.LastAdvertised >= cutOff).ToList();

            if (EnumsEqual(oldBeacons, newBeacons)) return;

            CallBeaconsUpdated(newBeacons);
        }

        private void CallBeaconsUpdated(List<BeaconLocation> newBeacons)
        {
            var onBeaconsUpdated = BeaconsUpdated;
            onBeaconsUpdated?.Invoke(newBeacons);
            _currentBeacons = newBeacons;
        }

        private void NewBeacon(BeaconLocation newBeacon)
        {
            _log.Info("Updating Beacons");
            var newBeacons = _currentBeacons
               .Where(location => !location.Equals(newBeacon))
               .Concat(new[] { newBeacon })
               .OrderBy(location => location.Data)
               .ThenBy(location => location.Address, IpEndPointComparer.Instance)
               .ToList();
            var onBeaconsUpdated = BeaconsUpdated;
            onBeaconsUpdated?.Invoke(newBeacons);
            _currentBeacons = newBeacons;
        }

        private static bool EnumsEqual<T>(List<T> xs, List<T> ys)
        {
            #pragma warning disable AV1706
            return xs.Zip(ys, (x, y) => x != null && x.Equals(y)).Count() == xs.Count;
            #pragma warning restore AV1706
        }

        public void Stop()
        {
            _log.Info("Stopping Probe");
            _running = false;
            _waitHandle.Set();
            _thread.Wait();
        }
    }
}