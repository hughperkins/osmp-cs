using System;
using System.Net;
using System.Net.Sockets;

namespace net.voxx.stun4cs
{
	/// <summary>
	/// Subclass of UdpClient that allows access to the underlying socket and bound IP address
	/// </summary>
	public class MyUdpClient : UdpClient
	{
		public MyUdpClient()
		{
			
		}

		public MyUdpClient ( System.String hostname , System.Int32 port ) : base (hostname, port) 
		{
		}

		public MyUdpClient ( System.Net.IPEndPoint localEP ) : base (localEP) 
		{
		}

		public MyUdpClient ( System.Int32 port , System.Net.Sockets.AddressFamily family ) : base (port, family) 
		{
		}

		public MyUdpClient ( System.Int32 port ) : base(port) 
		{
		}

		public  MyUdpClient ( System.Net.Sockets.AddressFamily family ) : base(family) 
		{
		}

		public Socket GetSocket() 
		{
			return this.Client;
		}

		public IPAddress GetAddress() 
		{
			Socket s = this.GetSocket();
			if (s != null) 
			{
				if (s.AddressFamily == AddressFamily.InterNetwork || s.AddressFamily == AddressFamily.InterNetworkV6) 
				{
					IPEndPoint ep = s.LocalEndPoint as IPEndPoint;
					if (ep != null) 
					{
						IPAddress ret = ep.Address as IPAddress;

						return ret;
					}
				}
			}

			return null;
		}

		public int GetPort() 
		{
			Socket s = this.GetSocket();
			if (s != null) 
			{
				if (s.AddressFamily == AddressFamily.InterNetwork || s.AddressFamily == AddressFamily.InterNetworkV6) 
				{
					IPEndPoint ep = s.LocalEndPoint as IPEndPoint;
					if (ep != null) 
					{
						return ep.Port;
					}
				}
			}

			return 0;
		}
	}
}
