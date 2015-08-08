using System;

namespace net.voxx.stun4cs 
{
	/**
	 * <p>
	 * This class implements the STUN Discovery Process as described by section 10.1
	 * of rfc 3489.
	 * </p><p>
	 * The flow makes use of three tests.  In test I, the client sends a
	 * STUN Binding Request to a server, without any flags set in the
	 * CHANGE-REQUEST attribute, and without the RESPONSE-ADDRESS attribute.
	 * This causes the server to send the response back to the address and
	 * port that the request came from.  In test II, the client sends a
	 * Binding Request with both the "change IP" and "change port" flags
	 * from the CHANGE-REQUEST attribute set.  In test III, the client sends
	 * a Binding Request with only the "change port" flag set.
	 * </p><p>
	 * The client begins by initiating test I.  If this test yields no
	 * response, the client knows right away that it is not capable of UDP
	 * connectivity.  If the test produces a response, the client examines
	 * the MAPPED-ADDRESS attribute.  If this address and port are the same
	 * as the local IP address and port of the socket used to send the
	 * request, the client knows that it is not natted.  It executes test
	 * II.
	 * </p><p>
	 * If a response is received, the client knows that it has open access
	 * to the Internet (or, at least, its behind a firewall that behaves
	 * like a full-cone NAT, but without the translation).  If no response
	 * is received, the client knows its behind a symmetric UDP firewall.
	 * </p><p>
	 * In the event that the IP address and port of the socket did not match
	 * the MAPPED-ADDRESS attribute in the response to test I, the client
	 * knows that it is behind a NAT.  It performs test II.  If a response
	 * is received, the client knows that it is behind a full-cone NAT.  If
	 * no response is received, it performs test I again, but this time,
	 * does so to the address and port from the CHANGED-ADDRESS attribute
	 * from the response to test I.  If the IP address and port returned in
	 * the MAPPED-ADDRESS attribute are not the same as the ones from the
	 * first test I, the client knows its behind a symmetric NAT.  If the
	 * address and port are the same, the client is either behind a
	 * restricted or port restricted NAT.  To make a determination about
	 * which one it is behind, the client initiates test III.  If a response
	 * is received, its behind a restricted NAT, and if no response is
	 * received, its behind a port restricted NAT.
	 * </p><p>
	 * This procedure yields substantial information about the operating
	 * condition of the client application.  In the event of multiple NATs
	 * between the client and the Internet, the type that is discovered will
	 * be the type of the most restrictive NAT between the client and the
	 * Internet.  The types of NAT, in order of restrictiveness, from most
	 * to least, are symmetric, port restricted cone, restricted cone, and
	 * full cone.
	 * </p><p>
	 * Typically, a client will re-do this discovery process periodically to
	 * detect changes, or look for inconsistent results.  It is important to
	 * note that when the discovery process is redone, it should not
	 * generally be done from the same local address and port used in the
	 * previous discovery process.  If the same local address and port are
	 * reused, bindings from the previous test may still be in existence,
	 * and these will invalidate the results of the test.  Using a different
	 * local address and port for subsequent tests resolves this problem.
	 * An alternative is to wait sufficiently long to be confident that the
	 * old bindings have expired (half an hour should more than suffice).
	 * </p><p>
	 * <p>Organisation: <p> Louis Pasteur University, Strasbourg, France</p>
	 * <p>Network Research Team (http://www-r2.u-strasbg.fr)</p></p>
	 * @author Emil Ivov
	 * @version 0.1
	 */

	public class NetworkConfigurationDiscoveryProcess
	{
		/**
		 * Indicates whether the underlying stack has been initialized and started
		 * and that the discoverer is operational.
		 */
		private bool started = false;

		/**
		 * The stack to use for STUN communication.
		 */
		private StunStack                stunStack     = null;

		/**
		 * The provider to send our messages through
		 */
		private StunProvider             stunProvider  = null;

		/**
		 * The point where we'll be listening.
		 */
		private NetAccessPointDescriptor apDescriptor  = null;

		/**
		 * The address of the stun server
		 */
		private StunAddress              serverAddress = null;

		/**
		 * A utility used to flatten the multithreaded architecture of the Stack
		 * and execute the discovery process in a synchronized manner
		 */
		private BlockingRequestSender    requestSender = null;

		/**
		 * Creates a StunAddressDiscoverer. In order to use it one must start the
		 * discoverer.
		 * @param localAddress the address where the stach should bind.
		 * @param serverAddress the address of the server to interrogate.
		 */
		public NetworkConfigurationDiscoveryProcess(StunAddress localAddress,
			StunAddress serverAddress)
		{
			apDescriptor       = new NetAccessPointDescriptor(localAddress);
			this.serverAddress = serverAddress;
		}

		/**
		 * Creates a StunAddressDiscoverer. In order to use it one must start the
		 * discoverer.
		 * @param apDescriptor the address where the stach should bind.
		 * @param serverAddress the address of the server to interrogate.
		 */
		public NetworkConfigurationDiscoveryProcess(NetAccessPointDescriptor apDescriptor,
			StunAddress serverAddress)
		{
			this.apDescriptor  = apDescriptor;
			this.serverAddress = serverAddress;

		}

		/**
		 * Shuts down the underlying stack and prepares the object for garbage
		 * collection.
		 */
		public virtual void ShutDown()
		{
			StunStack.ShutDown();
			stunStack     = null;
			stunProvider  = null;
			apDescriptor  = null;
			requestSender = null;

			this.started = false;

		}

		/**
		 * Puts the discoverer into an operational state.
		 * @throws StunException if we fail to bind or some other error occurs.
		 */
		virtual public void Start()
		{
			stunStack = StunStack.Instance;
			stunStack.Start();

			stunStack.InstallNetAccessPoint(apDescriptor);

			stunProvider = stunStack.GetProvider();

			requestSender = new BlockingRequestSender(stunProvider, apDescriptor);

			started = true;
		}

		/**
		 * Implements the discovery process itself (see class description).
		 * @return a StunDiscoveryReport containing details about the network
		 * configuration of the host where the class is executed.
		 * @throws StunException ILLEGAL_STATE if the discoverer has not been started
		 * NETWORK_ERROR or ILLEGAL_ARGUMENT if a failure occurs while executing
		 * the discovery algorithm
		 */
		virtual public StunDiscoveryReport determineAddress()
		{
			checkStarted();
			StunDiscoveryReport report = new StunDiscoveryReport();
			StunMessageEvent evt = doTestI(serverAddress);

			if(evt == null)
			{
				//UDP Blocked
				report.SetNatType(StunDiscoveryReport.UDP_BLOCKING_FIREWALL);
				return report;
			}
			else
			{
				StunAddress mappedAddress =((MappedAddressAttribute)evt.GetMessage().
					GetAttribute(Attribute.MAPPED_ADDRESS)).GetAddress();

				StunAddress backupServerAddress =((ChangedAddressAttribute) evt.GetMessage().
					GetAttribute(Attribute.CHANGED_ADDRESS)).GetAddress();
				report.SetPublicAddress(mappedAddress);
				if (mappedAddress.Equals(apDescriptor.GetAddress()))
				{
					evt = doTestII(serverAddress);
					if (evt == null)
					{
						//Sym UDP Firewall
						report.SetNatType(StunDiscoveryReport.SYMMETRIC_UDP_FIREWALL);
						return report;
					}
					else
					{
						//open internet
						report.SetNatType(StunDiscoveryReport.OPEN_INTERNET);
						return report;

					}
				}
				else
				{
					evt = doTestII(serverAddress);
					if (evt == null)
					{
						evt = doTestI(backupServerAddress);
						if(evt == null)
						{
							// System.oout.println("Failed to receive a response from backup stun server!");
							return report;
						}
						StunAddress mappedAddress2 =((MappedAddressAttribute)evt.GetMessage().
							GetAttribute(Attribute.MAPPED_ADDRESS)).GetAddress();
						if(mappedAddress.Equals(mappedAddress2))
						{
							evt = doTestIII(serverAddress);
							if(evt == null)
							{
								//port restricted cone
								report.SetNatType(StunDiscoveryReport.PORT_RESTRICTED_CONE_NAT);
								return report;
							}
							else
							{
								//restricted cone
								report.SetNatType(StunDiscoveryReport.RESTRICTED_CONE_NAT);
								return report;

							}
						}
						else
						{
							//Symmetric NAT
							report.SetNatType(StunDiscoveryReport.SYMMETRIC_NAT);
							return report;
						}
					}
					else
					{
						//full cone
						report.SetNatType(StunDiscoveryReport.FULL_CONE_NAT);
						return report;
					}
				}
			}

		}

		/**
		 * Sends a binding request to the specified server address. Both change IP
		 * and change port flags are set to false.
		 * @param serverAddress the address where to send the bindingRequest.
		 * @return The returned message encapsulating event or null if no message
		 * was received.
		 * @throws StunException if an exception occurs while sending the messge
		 */
		private StunMessageEvent doTestI(StunAddress serverAddress)
		{
			Request request = MessageFactory.CreateBindingRequest();

			ChangeRequestAttribute changeRequest = (ChangeRequestAttribute)request.GetAttribute(Attribute.CHANGE_REQUEST);
			changeRequest.SetChangeIpFlag(false);
			changeRequest.SetChangePortFlag(false);
			StunMessageEvent evt =
				requestSender.SendRequestAndWaitForResponse(request, serverAddress);
#if false
        if(evt != null)
            System.oout.println("TEST I res="+evt.GetRemoteAddress().ToString()
                               +" - "+ evt.GetRemoteAddress().GetHostName());
        else
            System.oout.println("NO RESPONSE received to TEST I.");
#endif
			return evt;
		}

		/**
		 * Sends a binding request to the specified server address with both change
		 * IP and change port flags are set to true.
		 * @param serverAddress the address where to send the bindingRequest.
		 * @return The returned message encapsulating event or null if no message
		 * was received.
		 * @throws StunException if an exception occurs while sending the messge
		 */
		private StunMessageEvent doTestII(StunAddress serverAddress)
		{
			Request request = MessageFactory.CreateBindingRequest();

			ChangeRequestAttribute changeRequest = (ChangeRequestAttribute)request.GetAttribute(Attribute.CHANGE_REQUEST);
			changeRequest.SetChangeIpFlag(true);
			changeRequest.SetChangePortFlag(true);

			StunMessageEvent evt =
				requestSender.SendRequestAndWaitForResponse(request, serverAddress);
#if false
        if(evt != null)
            System.oout.println("Test II res="+evt.GetRemoteAddress().toString()
                            +" - "+ evt.getRemoteAddress().getHostName());
        else
            System.oout.println("NO RESPONSE received to Test II.");
#endif

			return evt;
		}

		/**
		 * Sends a binding request to the specified server address with only change
		 * port flag set to true and change IP flag - to false.
		 * @param serverAddress the address where to send the bindingRequest.
		 * @return The returned message encapsulating event or null if no message
		 * was received.
		 * @throws StunException if an exception occurs while sending the messge
		 */
		private StunMessageEvent doTestIII(StunAddress serverAddress)
		{
			Request request = MessageFactory.CreateBindingRequest();

			ChangeRequestAttribute changeRequest = (ChangeRequestAttribute)request.GetAttribute(Attribute.CHANGE_REQUEST);
			changeRequest.SetChangeIpFlag(false);
			changeRequest.SetChangePortFlag(true);

			StunMessageEvent evt =
				requestSender.SendRequestAndWaitForResponse(request, serverAddress);

#if false
        if(evt != null)
            System.oout.println("Test III res="+evt.getRemoteAddress().toString()
                            +" - "+ evt.getRemoteAddress().getHostName());
        else
            Console.WriteLine("NO RESPONSE received to Test III.");
#endif

			return evt;
		}

		/**
		 * Makes shure the discoverer is operational and throws an
		 * StunException.ILLEGAL_STATE if that is not the case.
		 * @throws StunException ILLEGAL_STATE if the discoverer is not operational.
		 */
		private void checkStarted()
		{
			if(!started)
				throw new StunException(StunException.ILLEGAL_STATE,
					"The Discoverer must be started before "
					+"launching the discovery process!");
		}

		//---------- main
		/**
		 * Runs the discoverer and shows a message dialog with the returned report.
		 * @param args args[0] - stun server address, args[1] - port. in the case of
		 * no args - defaults are provided.
		 * @throws java.lang.Exception if an exception occurrs during the discovery
		 * process.
		 */
#if false
    public static void main(String[] args)
    {
        StunAddress localAddr = null;
        StunAddress serverAddr = null;
        if(args.Length == 4)
        {
            localAddr = new StunAddress(args[2], Integer.valueOf(args[3]).intValue());
            serverAddr = new StunAddress(args[0],
                                         Integer.valueOf(args[1]).intValue());
        }
        else
        {
            localAddr = new StunAddress(InetAddress.getLocalHost(), 5678);
            serverAddr = new StunAddress("stun01bak.sipphone.com.", 3479);
        }
        NetworkConfigurationDiscoveryProcess addressDiscovery =
                            new NetworkConfigurationDiscoveryProcess(localAddr, serverAddr);

        addressDiscovery.start();
        StunDiscoveryReport report = addressDiscovery.determineAddress();
        System.oout.println(report);
//        javax.swing.JOptionPane.showMessageDialog(
//                null,
//                report.toString(),
//                "Stun Discovery Process",
//                javax.swing.JOptionPane.INFORMATION_MESSAGE);
        System.exit(0);
    }
#endif
	}

}