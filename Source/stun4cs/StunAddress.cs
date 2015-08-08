using System;
using System.Net;

/**
 * The Address class is used to define destinations to outgoing Stun Packets.
 *
 * <p>Organisation: <p> Louis Pasteur University, Strasbourg, France</p>
 * <p>Network Research Team (http://www-r2.u-strasbg.fr)</p></p>
 * @author Emil Ivov
 * @version 0.1
 */

namespace net.voxx.stun4cs 
{
	public class StunAddress
	{
		InetSocketAddress socketAddress = null;
		InetSocketAddress localAddress = null;

		/**
		 * Creates an address instance address from an IP address and a port number.
		 * <p>
		 * A valid port value is between 0 and 65535.
		 * A port number of <code>zero</code> will let the system pick up an
		 * ephemeral port in a <code>bind</code> operation.
		 * <P>
		 * A <code>null</code> address will assign the <i>wildcard</i> address.
		 * <p>
		 * @param	hostname	The IP address
		 * @param	port	    The port number
		 * @throws IllegalArgumentException if the port parameter is outside the
		 * specified range of valid port values.
		 */
		public StunAddress(String hostname, int port)
		{
			socketAddress = new InetSocketAddress(hostname, port);
		}

		/**
		 * Creates an address instance address from a byte array containing an IP
		 * address and a port number.
		 * <p>
		 * A valid port value is between 0 and 65535.
		 * A port number of <code>zero</code> will let the system pick up an
		 * ephemeral port in a <code>bind</code> operation.
		 * <P>
		 * A <code>null</code> address will assign the <i>wildcard</i> address.
		 * <p>
		 * @param	ipAddress The IP address
		 * @param	port      The port number
		 * @throws IllegalArgumentException if the port parameter is outside the
		 * specified range of valid port values or if ipAddress is not a valid ip
		 * address.
		 */
		public StunAddress(byte[] ipAddress, int port) 
		{

				//socketAddress = new InetSocketAddress(InetAddress.getByAddress(
				//    ipAddress), port);

				//Unknown Host - Let's skip resolution
				socketAddress = new InetSocketAddress((ipAddress[0]&0xFF) + "."
					+(ipAddress[1]&0xFF) + "."
					+(ipAddress[2]&0xFF) + "."
					+(ipAddress[3]&0xFF) + ".",
					port);
		}


		/**
		 * Creates an address instance from a hostname and a port number.
		 * <p>
		 * An attempt will be made to resolve the hostname into an InetAddress.
		 * If that attempt fails, the address will be flagged as <I>unresolved</I>.
		 * <p>
		 * A valid port value is between 0 and 65535.
		 * A port number of <code>zero</code> will let the system pick up an
		 * ephemeral port in a <code>bind</code> operation.
		 * <P>
		 * @param	address the address itself
		 * @param	port	the port number
		 * @throws IllegalArgumentException if the port parameter is outside the
		 * range of valid port values, or if the hostname parmeter is <TT>null</TT>.
		 */
		public StunAddress(IPAddress address, int port)
		{
			socketAddress = new InetSocketAddress(address, port);
		}

		/**
		 * Creates an address instance from the localhost amd a port number.
		 * <p>
		 * Addresses created with this constructor should be used cautiously as
		 * they won't be trated as equal to address created from the localhost
		 * address even though the represent the same thing. In other words - use
		 * this constructor only if you're not going to compare the resulting
		 * instance with other addresses.
		 *
		 * @param	port	the port number
		 * @throws IllegalArgumentException if the port parameter is outside the
			 * range of valid port values, or if the hostname parmeter is <TT>null</TT>.
		 */
		public StunAddress(int port) 
		{
			socketAddress = new InetSocketAddress(IPAddress.Any,port);
		}


		/**
		 * Returns the raw IP address of this Address object. The result is in
		 * network byte order: the highest order byte of the address is in
		 * getAddress()[0].
		 * @return the raw IP address of this object.
		 */

		public virtual byte[] GetAddressBytes()
		{
			return (socketAddress == null
				?null
				:socketAddress.GetAddress().GetAddressBytes());
		}


		/**
		 * Returns the port number or 0 if the addres has not been initialized.
		 * @return the port number.
		 */
		public virtual int GetPort()
		{
			return (socketAddress == null
				?0
				:socketAddress.GetPort());
		}

		/**
		 * Returns the encapsulated InetSocketAddress instance.
		 * @return the encapsulated InetSocketAddress instance.
		 */
		public virtual InetSocketAddress GetSocketAddress()
		{
			return socketAddress;
		}

		/**
		 * Constructs a string representation of this InetSocketAddress. This String
		 * is constructed by calling toString() on the InetAddress and concatenating
		 * the port number (with a colon). If the address is unresolved then the
		 * part before the colon will only contain the hostname.
		 *
		 * @return a string representation of this object.
		 */
		public override string ToString()
		{
			return socketAddress.ToString();
		}

		/**
		 * Compares this object against the specified object. The result is true if
		 * and only if the argument is not null and it represents the same address
		 * as this object.
		 * <p/>
		 * Two instances of InetSocketAddress represent the same address if both the
		 * InetAddresses (or hostnames if it is unresolved) and port numbers are
		 * equal.
		 * <p/>
		 * If both addresses are unresolved, then the hostname & the port number are
		 * compared.
		 * @param obj the object to compare against.
		 * @return true if the objects are the same; false otherwise.
		 */
		public override bool Equals(Object obj)
		{
			if(!(obj is StunAddress))
				return false;

			StunAddress target = (StunAddress)obj;
			if(   target.socketAddress == null
				&& socketAddress ==null)
				return true;

			return socketAddress.Equals(target.GetSocketAddress());
		}

		/**
		 * Gets the hostname.
		 * @return the hostname part of the address
		 */
		public virtual string getHostName()
		{
			String hostName = socketAddress.GetHostName();
		
			return hostName;
		}

		public InetSocketAddress Local 
		{
			get 
			{
				return this.localAddress;
			}

			set 
			{
				this.localAddress = value;
			}
		}
	}
}