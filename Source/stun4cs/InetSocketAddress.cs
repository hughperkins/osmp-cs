using System;
using System.Net;

namespace net.voxx.stun4cs 
{
	/**
	 * Summary description for InetSocketAddress.
	 */
	public class InetSocketAddress
	{
		private IPAddress addr;
		private int port;
		public InetSocketAddress(IPAddress addr, int port)
		{
			this.addr = addr;
			this.port = port;
		}

		public InetSocketAddress(IPHostEntry ent, int port) : this(ent.AddressList[0], port)
		{
		}

		public InetSocketAddress(String addr, int port) : this(Dns.GetHostByName(addr), port)
		{
			
		}

		public virtual IPAddress GetAddress() 
		{
			return this.addr;
		}

		public virtual int GetPort() 
		{
			return this.port;
		}

		public virtual string GetHostName() 
		{
			String ret = this.addr.ToString();
		
			return ret;
		}

		public override string ToString() 
		{
			return GetHostName()+":"+GetPort();
		}

		/**
		 * Two instances of InetSocketAddress represent the same address if both the
		 * InetAddresses (or hostnames if it is unresolved) and port numbers are
		 * equal.
		 * <p/>
		 * @param obj the object to compare against.
		 * @return true if the objects are the same; false otherwise.
		 */
		public override bool Equals(Object obj)
		{
			if(!(obj is InetSocketAddress))
				return false;

			InetSocketAddress target = (InetSocketAddress)obj;

			return (this.addr.Equals(target.addr) && this.port.Equals(target.port));
		}

	}

}