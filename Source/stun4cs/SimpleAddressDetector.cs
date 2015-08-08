using System;
using System.Net;
using System.Net.Sockets;

/**
 * The class provides basic means of discovering a public IP address. All it does
 * is send a binding request through a specified port and return the mapped address
 * it got back or null if there was no reponse
 *
 * <p>Organisation: <p> Louis Pasteur University, Strasbourg, France</p>
 * <p>Network Research Team (http://www-r2.u-strasbg.fr)</p></p>
 * @author Emil Ivov
 * @version 0.1
 */
namespace net.voxx.stun4cs 
{
	public class SimpleAddressDetector
	{
		/**
		 * Indicates whether the underlying stack has been initialized and started
		 * and that the discoverer is operational.
		 */
		private bool started = false;

		/**
		 * The stack to use for STUN communication.
		 */
		private StunStack stunStack = null;

		/**
		 * The provider to send our messages through
		 */
		private StunProvider stunProvider = null;

		/**
		 * The address of the stun server
		 */
		private StunAddress serverAddress = null;

		/**
		 * A utility used to flatten the multithreaded architecture of the Stack
		 * and execute the discovery process in a synchronized manner
		 */
		private BlockingRequestSender requestSender = null;

		/**
		 * Creates a StunAddressDiscoverer. In order to use it one must start the
		 * discoverer.
		 * @param serverAddress the address of the server to interrogate.
		 */
		public SimpleAddressDetector(StunAddress serverAddress)
		{
			this.serverAddress = serverAddress;
		}


		/**
		 * Shuts down the underlying stack and prepares the object for garbage
		 * collection.
		 */
		public virtual void ShutDown()
		{
			StunStack.ShutDown();
			stunStack = null;
			stunProvider = null;
			requestSender = null;

			this.started = false;

		}

		/**
		 * Puts the discoverer into an operational state.
		 * @throws StunException if we fail to bind or some other error occurs.
		 */
		public virtual void Start()
		{
			stunStack = StunStack.Instance;
			stunStack.Start();

			stunProvider = stunStack.GetProvider();



			started = true;
		}


		/**
		 * Creates a listening point from the following address and attempts to
		 * discover how it is mapped so that using inside the application is possible.
		 * @param address the [address]:[port] pair where ther request should be
		 * sent from.
		 * @return a StunAddress object containing the mapped address or null if
		 * discovery failed.
		 * @throws StunException if something fails along the way.
		 */
		public virtual StunAddress GetMappingFor(StunAddress address)
		{
			NetAccessPointDescriptor apDesc = new NetAccessPointDescriptor(address);

			stunStack.InstallNetAccessPoint(apDesc);

			requestSender = new BlockingRequestSender(stunProvider, apDesc);
			StunMessageEvent evt = null;
			try
			{
				evt = requestSender.SendRequestAndWaitForResponse(
					MessageFactory.CreateBindingRequest(), serverAddress);
			}
			finally
			{
				//free the port to allow the application to use it.
				stunStack.RemoveNetAccessPoint(apDesc);
			}

			if(evt != null)
			{
				Response res = (Response)evt.GetMessage();
				MappedAddressAttribute maAtt =
					(MappedAddressAttribute)
					res.GetAttribute(Attribute.MAPPED_ADDRESS);
				if(maAtt != null) 
				{
					StunAddress sa = maAtt.GetAddress();
					return sa;
				}
			}

			return null;
		}

		/**
		* Creates a listening point for the specified socket and attempts to
		* discover how its local address is NAT mapped.
		* @param socket the socket whose address needs to be resolved.
		* @return a StunAddress object containing the mapped address or null if
		* discovery failed.
		* @throws StunException if something fails along the way.
		*/

		public virtual StunAddress GetMappingFor(MyUdpClient socket)
		{
			NetAccessPointDescriptor apDesc =  stunStack.InstallNetAccessPoint(socket);

			requestSender = new BlockingRequestSender(stunProvider, apDesc);
			StunMessageEvent evt = null;
			try
			{
				evt = requestSender.SendRequestAndWaitForResponse(
					MessageFactory.CreateBindingRequest(), serverAddress);
			}
			finally
			{
				stunStack.RemoveNetAccessPoint(apDesc);
			}

			if(evt != null)
			{
				Response res = (Response)evt.GetMessage();
				MappedAddressAttribute maAtt =
					(MappedAddressAttribute)
					res.GetAttribute(Attribute.MAPPED_ADDRESS);
				if(maAtt != null)
					return maAtt.GetAddress();
			}

			return null;
		}

		/**
		 * Creates a listening point from the specified port and attempts to
		 * discover how it is being mapped.
		 * @param port the local port where to send the request from.
		 * @return a StunAddress object containing the mapped address or null if
		 * discovery failed.
		 * @throws StunException if something fails along the way.
		 */
		public virtual StunAddress GetMappingFor(int port)

		{
			return GetMappingFor(new StunAddress(port));
		}


		/*
			public static void main(String[] args)
				throws Exception
			{
				SimpleAddressDetector detector = new SimpleAddressDetector(
										new StunAddress("stun01.sipphone.com", 3478));
				detector.start();
				StunAddress mappedAddr = detector.getMappingFor(5060);

				System.out.println("address is " + mappedAddr);

				detector.shutDown();
			}
		*/

	}

}