using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using JetBrains.Annotations;
using NLog;

namespace BeaconLib
{
    /// <summary>
    ///     Instances of this class can be autodiscovered on the local network through UDP broadcasts
    /// </summary>
    /// <remarks>
    ///     The advertisement consists of the beacon's application type and a short beacon-specific string.
    /// </remarks>
    [PublicAPI]
    public sealed class Beacon : IDisposable
    {
        internal const int DiscoveryPort = 35891;
        private static ILogger _log = LogManager.GetCurrentClassLogger();
        private readonly UdpClient _udp;

        public Beacon(string type, ushort advertisedPort)
        {
            Type = type;
            AdvertisedPort = advertisedPort;
            Data = "";

            #pragma warning disable MA0011
            _log.Info("Bind UDP beacon to {Port}", DiscoveryPort);
            #pragma warning restore MA0011
            _udp = new UdpClient();
            Init();
        }

        /// <summary>
        ///     Return the machine's hostname (usually nice to mention in the beacon text)
        /// </summary>
        public static string HostName => Dns.GetHostName();

        public string Type { get; private set; }
        public ushort AdvertisedPort { get; private set; }
        public bool Stopped { get; private set; }

        public string Data { get; set; }

        public void Dispose()
            => Stop();

        private void Init()
        {
            _udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, optionValue: true);
            _udp.Client.Bind(new IPEndPoint(IPAddress.Any, DiscoveryPort));

            try
            {
                _udp.AllowNatTraversal(allowed: true);
            }
            catch (Exception exception)
            {
                _log.Error(exception, "Error switching on NAT traversal");
            }
        }

        public void Start()
        {
            _log.Info("Starting Beacon");
            Stopped = false;
            // ReSharper disable once ArgumentsStyleLiteral
            _udp.BeginReceive(ProbeReceived, state: null);
        }

        public void Stop()
            => Stopped = true;

        #pragma warning disable AV1500
        private void ProbeReceived(IAsyncResult ar)
            #pragma warning restore AV1500
        {
            var remote = new IPEndPoint(IPAddress.Any, 0);
            byte[]? bytes = _udp.EndReceive(ar, ref remote);
            _log.Info("Incoming Probe {Adress}", remote);

            // Compare beacon type to probe type
            byte[] typeBytes = Encode(Type);
            if(HasPrefix(bytes, typeBytes.ToArray()))
            {
                _log.Info("Responding Probe {Adress}", remote);
                // If true, respond again with our type, port and payload
                byte[] responseData = Encode(Type)
                   .Concat(BitConverter.GetBytes((ushort)IPAddress.HostToNetworkOrder((short)AdvertisedPort)))
                   .Concat(Encode(Data)).ToArray();
                _udp.Send(responseData, responseData.Length, remote);
            }
            else
            {
                _log.Info("Incompatible Data");
            }

            if(!Stopped) _udp.BeginReceive(ProbeReceived, state: null);
        }

        internal static bool HasPrefix<T>(T[] haystack, T[] prefix)
        {
            return haystack.Length >= prefix.Length &&
                   #pragma warning disable AV1706
                   haystack.Zip(prefix, (a, b) => a != null && a.Equals(b)).Any();
            #pragma warning restore AV1706
        }

        /// <summary>
        ///     Convert a string to network bytes
        /// </summary>
        #pragma warning disable AV1130
        internal static byte[] Encode(string data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            short networkOrder = IPAddress.HostToNetworkOrder((short)bytes.Length);

            return BitConverter.GetBytes(networkOrder).Concat(bytes).ToArray();
        }

        /// <summary>
        ///     Convert network bytes to a string
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        internal static string Decode(IEnumerable<byte> data)
        {
            var listData = data as IList<byte> ?? data.ToList();

            short packetLenght = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(listData.Take(2).ToArray(), 0));

            if(listData.Count < 2 + packetLenght) throw new ArgumentException("Too few bytes in packet", nameof(data));

            return Encoding.UTF8.GetString(listData.Skip(2).Take(packetLenght).ToArray());
        }
    }
}