using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

//from: http://www.pinvoke.net/
namespace Libuv
{
    internal enum AI : int
    {
        AI_NOTHING = 0x00,
        /// <summary>
        /// The socket address will be used in a call to the bind function.
        /// </summary>
        AI_PASSIVE = 0x01,
        /// <summary>
        /// The canonical name is returned in the first ai_canonname member.
        /// </summary>
        AI_CANONNAME = 0x02,
        /// <summary>
        /// The nodename parameter passed to the getaddrinfo function must be a numeric string. 
        /// </summary>
        AI_NUMERICHOST = 0x04,
        /// <summary>
        /// The getaddrinfo will resolve only if a global address is configured. The IPv6 and IPv4 loopback address is not considered a valid global address. This option is only supported on Windows Vista or later. 
        /// </summary>
        AI_ADDRCONFIG = 0x0400,
        /// <summary>
        /// The address information can be from a non-authoritative namespace provider. This option is only supported on Windows Vista or later for the NS_EMAIL namespace. 
        /// </summary>
        AI_NON_AUTHORITATIVE = 0x04000,
        /// <summary>
        /// The address information is from a secure channel. This option is only supported on Windows Vista or later for the NS_EMAIL namespace. 
        /// </summary>
        AI_SECURE = 0x08000,
        /// <summary>
        /// The address information is for a preferred name for a user. This option is only supported on Windows Vista or later for the NS_EMAIL namespace. 
        /// </summary>
        AI_RETURN_PREFERRED_NAMES = 0x010000,
        /// <summary>
        /// A hint to the namespace provider that the hostname being queried is being used in file share scenario. The namespace provider may ignore this hint.  
        /// </summary>
        AI_FILESERVER = 0x00040000
    }

    internal enum ADDRESS_FAMILIES_INT : int
    {
        /// <summary>
        /// Unspecified [value = 0].
        /// </summary>
        AF_UNSPEC = 0,
        /// <summary>
        /// Local to host (pipes, portals) [value = 1].
        /// </summary>
        AF_UNIX = 1,
        /// <summary>
        /// Internetwork: UDP, TCP, etc [value = 2].
        /// </summary>
        AF_INET = 2,
        /// <summary>
        /// Arpanet imp addresses [value = 3].
        /// </summary>
        AF_IMPLINK = 3,
        /// <summary>
        /// Pup protocols: e.g. BSP [value = 4].
        /// </summary>
        AF_PUP = 4,
        /// <summary>
        /// Mit CHAOS protocols [value = 5].
        /// </summary>
        AF_CHAOS = 5,
        /// <summary>
        /// XEROX NS protocols [value = 6].
        /// </summary>
        AF_NS = 6,
        /// <summary>
        /// IPX protocols: IPX, SPX, etc [value = 6].
        /// </summary>
        AF_IPX = 6,
        /// <summary>
        /// ISO protocols [value = 7].
        /// </summary>
        AF_ISO = 7,
        /// <summary>
        /// OSI is ISO [value = 7].
        /// </summary>
        AF_OSI = 7,
        /// <summary>
        /// european computer manufacturers [value = 8].
        /// </summary>
        AF_ECMA = 8,
        /// <summary>
        /// datakit protocols [value = 9].
        /// </summary>
        AF_DATAKIT = 9,
        /// <summary>
        /// CCITT protocols, X.25 etc [value = 10].
        /// </summary>
        AF_CCITT = 10,
        /// <summary>
        /// IBM SNA [value = 11].
        /// </summary>
        AF_SNA = 11,
        /// <summary>
        /// DECnet [value = 12].
        /// </summary>
        AF_DECnet = 12,
        /// <summary>
        /// Direct data link interface [value = 13].
        /// </summary>
        AF_DLI = 13,
        /// <summary>
        /// LAT [value = 14].
        /// </summary>
        AF_LAT = 14,
        /// <summary>
        /// NSC Hyperchannel [value = 15].
        /// </summary>
        AF_HYLINK = 15,
        /// <summary>
        /// AppleTalk [value = 16].
        /// </summary>
        AF_APPLETALK = 16,
        /// <summary>
        /// NetBios-style addresses [value = 17].
        /// </summary>
        AF_NETBIOS = 17,
        /// <summary>
        /// VoiceView [value = 18].
        /// </summary>
        AF_VOICEVIEW = 18,
        /// <summary>
        /// Protocols from Firefox [value = 19].
        /// </summary>
        AF_FIREFOX = 19,
        /// <summary>
        /// Somebody is using this! [value = 20].
        /// </summary>
        AF_UNKNOWN1 = 20,
        /// <summary>
        /// Banyan [value = 21].
        /// </summary>
        AF_BAN = 21,
        /// <summary>
        /// Native ATM Services [value = 22].
        /// </summary>
        AF_ATM = 22,
        /// <summary>
        /// Internetwork Version 6 [value = 23].
        /// </summary>
        AF_INET6 = 23,
        /// <summary>
        /// Microsoft Wolfpack [value = 24].
        /// </summary>
        AF_CLUSTER = 24,
        /// <summary>
        /// IEEE 1284.4 WG AF [value = 25].
        /// </summary>
        AF_12844 = 25,
        /// <summary>
        /// IrDA [value = 26].
        /// </summary>
        AF_IRDA = 26,
        /// <summary>
        /// Network Designers OSI &amp; gateway enabled protocols [value = 28].
        /// </summary>
        AF_NETDES = 28,
        /// <summary>
        /// [value = 29].
        /// </summary>
        AF_TCNPROCESS = 29,
        /// <summary>
        /// [value = 30].
        /// </summary>
        AF_TCNMESSAGE = 30,
        /// <summary>
        /// [value = 31].
        /// </summary>
        AF_ICLFXBM = 31
    }

    internal enum SOCKET_TYPE_INT : int
    {
        /// <summary>
        /// stream socket 
        /// </summary>
        SOCK_STREAM = 1,

        /// <summary>
        /// datagram socket 
        /// </summary>
        SOCK_DGRAM = 2,

        /// <summary>
        /// raw-protocol interface 
        /// </summary>
        SOCK_RAW = 3,

        /// <summary>
        /// reliably-delivered message 
        /// </summary>
        SOCK_RDM = 4,

        /// <summary>
        /// sequenced packet stream 
        /// </summary>
        SOCK_SEQPACKET = 5
    }

    internal enum PROTOCOL_INT : int
    {
        //dummy for IP  
        IPPROTO_IP = 0,
        //control message protocol  
        IPPROTO_ICMP = 1,
        //internet group management protocol  
        IPPROTO_IGMP = 2,
        //gateway^2 (deprecated)  
        IPPROTO_GGP = 3,
        //tcp  
        IPPROTO_TCP = 6,
        //pup  
        IPPROTO_PUP = 12,
        //user datagram protocol  
        IPPROTO_UDP = 17,
        //xns idp  
        IPPROTO_IDP = 22,
        //IPv6  
        IPPROTO_IPV6 = 41,
        //UNOFFICIAL net disk proto  
        IPPROTO_ND = 77,

        IPPROTO_ICLFXBM = 78,
        //raw IP packet  
        IPPROTO_RAW = 255,

        IPPROTO_MAX = 256
    }

    internal enum ADDRESS_FAMILIES : short
    {
        /// <summary>
        /// Unspecified [value = 0].
        /// </summary>
        AF_UNSPEC = 0,
        /// <summary>
        /// Local to host (pipes, portals) [value = 1].
        /// </summary>
        AF_UNIX = 1,
        /// <summary>
        /// Internetwork: UDP, TCP, etc [value = 2].
        /// </summary>
        AF_INET = 2,
        /// <summary>
        /// Arpanet imp addresses [value = 3].
        /// </summary>
        AF_IMPLINK = 3,
        /// <summary>
        /// Pup protocols: e.g. BSP [value = 4].
        /// </summary>
        AF_PUP = 4,
        /// <summary>
        /// Mit CHAOS protocols [value = 5].
        /// </summary>
        AF_CHAOS = 5,
        /// <summary>
        /// XEROX NS protocols [value = 6].
        /// </summary>
        AF_NS = 6,
        /// <summary>
        /// IPX protocols: IPX, SPX, etc [value = 6].
        /// </summary>
        AF_IPX = 6,
        /// <summary>
        /// ISO protocols [value = 7].
        /// </summary>
        AF_ISO = 7,
        /// <summary>
        /// OSI is ISO [value = 7].
        /// </summary>
        AF_OSI = 7,
        /// <summary>
        /// european computer manufacturers [value = 8].
        /// </summary>
        AF_ECMA = 8,
        /// <summary>
        /// datakit protocols [value = 9].
        /// </summary>
        AF_DATAKIT = 9,
        /// <summary>
        /// CCITT protocols, X.25 etc [value = 10].
        /// </summary>
        AF_CCITT = 10,
        /// <summary>
        /// IBM SNA [value = 11].
        /// </summary>
        AF_SNA = 11,
        /// <summary>
        /// DECnet [value = 12].
        /// </summary>
        AF_DECnet = 12,
        /// <summary>
        /// Direct data link interface [value = 13].
        /// </summary>
        AF_DLI = 13,
        /// <summary>
        /// LAT [value = 14].
        /// </summary>
        AF_LAT = 14,
        /// <summary>
        /// NSC Hyperchannel [value = 15].
        /// </summary>
        AF_HYLINK = 15,
        /// <summary>
        /// AppleTalk [value = 16].
        /// </summary>
        AF_APPLETALK = 16,
        /// <summary>
        /// NetBios-style addresses [value = 17].
        /// </summary>
        AF_NETBIOS = 17,
        /// <summary>
        /// VoiceView [value = 18].
        /// </summary>
        AF_VOICEVIEW = 18,
        /// <summary>
        /// Protocols from Firefox [value = 19].
        /// </summary>
        AF_FIREFOX = 19,
        /// <summary>
        /// Somebody is using this! [value = 20].
        /// </summary>
        AF_UNKNOWN1 = 20,
        /// <summary>
        /// Banyan [value = 21].
        /// </summary>
        AF_BAN = 21,
        /// <summary>
        /// Native ATM Services [value = 22].
        /// </summary>
        AF_ATM = 22,
        /// <summary>
        /// Internetwork Version 6 [value = 23].
        /// </summary>
        AF_INET6 = 23,
        /// <summary>
        /// Microsoft Wolfpack [value = 24].
        /// </summary>
        AF_CLUSTER = 24,
        /// <summary>
        /// IEEE 1284.4 WG AF [value = 25].
        /// </summary>
        AF_12844 = 25,
        /// <summary>
        /// IrDA [value = 26].
        /// </summary>
        AF_IRDA = 26,
        /// <summary>
        /// Network Designers OSI &amp; gateway enabled protocols [value = 28].
        /// </summary>
        AF_NETDES = 28,
        /// <summary>
        /// [value = 29].
        /// </summary>
        AF_TCNPROCESS = 29,
        /// <summary>
        /// [value = 30].
        /// </summary>
        AF_TCNMESSAGE = 30,
        /// <summary>
        /// [value = 31].
        /// </summary>
        AF_ICLFXBM = 31
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct addrinfo
    {
        internal AI flags;
        internal ADDRESS_FAMILIES_INT family;
        internal SOCKET_TYPE_INT socktype;
        internal PROTOCOL_INT protocol;
        internal uint addrlen;

        internal IntPtr canonname; //char*
        internal IntPtr addr; //sockaddr*
        internal IntPtr next; //addrinfo*

        internal IEnumerable<addrinfo> Children
        {
            get
            {
                var current = next;

                while (current != IntPtr.Zero)
                {
                    var info = (addrinfo)Marshal.PtrToStructure(current, typeof(addrinfo));
                    yield return info;

                    current = info.next;
                }
            }
        }

        internal IEnumerable<IPEndPoint> EndPoints
        {
            get
            {
                yield return this.ToEndPoint();
                foreach (var child in this.Children)
                    yield return child.ToEndPoint();
            }
        }

        internal string Name
        {
            get
            {
                const int size = 64;
                var ptr = Marshal.AllocHGlobal(size);

                try
                {
                    switch (family)
                    {
                        case ADDRESS_FAMILIES_INT.AF_INET:
                            Uvi.uv_ip4_name(addr, ptr, size);
                            break;

                        case ADDRESS_FAMILIES_INT.AF_INET6:
                            Uvi.uv_ip6_name(addr, ptr, size);
                            break;

                        default:
                            throw new Exception("Address family not supported");
                    }

                    return Marshal.PtrToStringAnsi(ptr);
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
        }

        internal int Port
        {
            get
            {
                if (addrlen < 4)
                    return 0;

                //this is not really elegant, but it works nicely and it's croos platform (win/linux)
                var data = new byte[4];
                Marshal.Copy(addr, data, 0, data.Length);

                switch (family)
                {
                    case ADDRESS_FAMILIES_INT.AF_INET:
                    case ADDRESS_FAMILIES_INT.AF_INET6:
                        return (data[2] << 8) + data[3];

                    default:
                        throw new Exception("Address family not supported");
                }
            }
        }

        internal sockaddr_in SockaddrIn
        {
            get
            {
                return (sockaddr_in)Marshal.PtrToStructure(addr, typeof(sockaddr_in));
            }
        }

        internal sockaddr_in6 SockaddrIn6
        {
            get
            {
                return (sockaddr_in6)Marshal.PtrToStructure(addr, typeof(sockaddr_in6));
            }
        }

        internal static addrinfo CreateHints()
        {
            return new addrinfo()
            {
                flags     = AI.AI_NOTHING,
                family    = ADDRESS_FAMILIES_INT.AF_UNSPEC,
                socktype  = SOCKET_TYPE_INT.SOCK_STREAM,
                protocol  = PROTOCOL_INT.IPPROTO_IP,
                canonname = IntPtr.Zero,
                addr      = IntPtr.Zero,
                addrlen   = 0,
                next      = IntPtr.Zero
            };
        }

        internal IPEndPoint ToEndPoint()
        {
            return new IPEndPoint(IPAddress.Parse(this.Name), this.Port);
        }
    }
}
