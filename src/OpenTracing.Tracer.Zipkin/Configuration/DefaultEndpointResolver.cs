using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace OpenTracing.Tracer.Zipkin.Configuration
{
    /// <summary>
    /// <para>This uses a completely arbitrary algorithm!
    /// Its primarily goal is to choose "the best" IP addresses on DEV machines
    /// where it's common to have virtual IP addresses etc.</para>
    /// <para>Use your own resolver if this algorithm doesn't fit your needs.</para>
    /// </summary>
    public class DefaultEndpointResolver : IEndpointResolver
    {
        public Endpoint GetEndpoint()
        {
            var ipAddresses = new List<Tuple<IPAddress, int>>();

            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface network in networkInterfaces)
            {
                IPInterfaceProperties properties = network.GetIPProperties();

                foreach (IPAddressInformation address in properties.UnicastAddresses)
                {
                    // We're only interested in IPv4 addresses for now
                    if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    // Ignore loopback addresses (e.g., 127.0.0.1)
                    if (IPAddress.IsLoopback(address.Address))
                        continue;

                    string ipAddress = address.Address.ToString();

                    int priority =
                        // The most common home network
                        ipAddress.StartsWith("192.168.0.") ? 1
                            // Any other home network
                            : ipAddress.StartsWith("192.168.") ? 2
                                // typical server network
                                : ipAddress.StartsWith("10.") ? 3
                                    // covers 172.16-172.31
                                    : ipAddress.StartsWith("172.") ? 4
                                        // any other address
                                        : 5;

                    ipAddresses.Add(Tuple.Create(address.Address, priority));
                }
            }

            var first = ipAddresses
                .OrderBy(x => x.Item2)
                .FirstOrDefault();

            if (first != null)
            {
                return new Endpoint { IPAddress = first.Item1 };
            }

            return null;
        }
    }
}