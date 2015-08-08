using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace net.voxx.stun4cs 
{
	/**
	 * The Network Access Point is the most outward part of the stack. It is
	 * constructed around a datagram socket and takes care about forwarding incoming
	 * messages to the MessageProcessor as well as sending datagrams to the STUN server
	 * specified by the original NetAccessPointDescriptor.
	 *
	 * <p>Organisation: Louis Pasteur University, Strasbourg, France</p>
	 *                   <p>Network Research Team (http://www-r2.u-strasbg.fr)</p></p>
	 * @author Emil Ivov
	 * @version 0.1
	 */

	class NetAccessPoint : Runnable
	{
		NetAccessPoint()
		{
		}

		/**
		 * Max datagram size.
		 */
		private const int MAX_DATAGRAM_SIZE = 8 * 1024;


		/**
		 * The message queue is where incoming messages are added.
		 */
		private MessageQueue messageQueue = null;

		/**
		 * The socket object that used by this access point to access the network.
		 */
		private MyUdpClient sock;

		/**
		 * Indicates whether the access point is using a socket that was created
		 * by someone else. The variable is set to true when the AP's socket is
		 * set using the <code>useExternalSocket()</code> method and is consulted
		 * inside the stop method. When its value is true, the AP's socket is not
		 * closed when <code>stop()</code>ing the AP.
		 *
		 * This variable is part of bug fix reported by Dave Stuart - SipQuest
		 */
		private bool isUsingExternalSocket = false;

		/**
		 * A flag that is set to false to exit the message processor.
		 */
		private bool isRunning;

		/**
		 * The descriptor used to create this access point.
		 */
		private NetAccessPointDescriptor apDescriptor = null;

		/**
		 * The instance to be notified if errors occur in the network listening
		 * thread.
		 */
		private ErrorHandler             errorHandler = null;

		/**
		 * Creates a network access point.
		 * @param apDescriptor the address and port where to bind.
		 * @param messageQueue the FIFO list where incoming messages should be queued
		 * @param errorHandler the instance to notify when errors occur.
		 */
		public NetAccessPoint(NetAccessPointDescriptor apDescriptor,
			MessageQueue             messageQueue,
			ErrorHandler			    errorHandler)
		{
			this.apDescriptor = apDescriptor;
			this.messageQueue = messageQueue;
			this.errorHandler = errorHandler;
		}


		/**
		 * Start the network listening thread.
		 *
		 * @throws IOException if we fail to setup the socket.
		 */
		public virtual void Start()
		{
			//do not create the socket earlier as someone might want to set an existing one
			// was != null fixed (Ranga)

			if(sock == null)
			{
				try 
				{
					IPEndPoint ipe = new IPEndPoint(GetDescriptor().GetAddress().
						GetSocketAddress().GetAddress(), GetDescriptor().GetAddress().
						GetSocketAddress().GetPort());
					this.sock = new MyUdpClient(ipe);
				} 
				catch (SocketException se) 
				{
					Console.WriteLine("{0} {1}", se.Message, se.StackTrace);
					return;
				}
				this.isUsingExternalSocket = false;
			}
			// sock.setReceiveBufferSize(MAX_DATAGRAM_SIZE);
			this.isRunning = true;
			Thread thread = new Thread(new ThreadStart(this.Run));
			thread.Name = "NetAccessPoint: "+this.sock.GetAddress()+":"+this.sock.GetPort();

			thread.Start();
		}

		/**
		 * Returns the NetAccessPointDescriptor that contains the port and address
		 * associated with this accesspoint.
		 * @return the NetAccessPointDescriptor associated with this AP.
		 */
		public virtual NetAccessPointDescriptor GetDescriptor()
		{
			return apDescriptor;
		}

		/**
		 * The listening thread's run method.
		 */
		public virtual void Run()
		{
			while (this.isRunning)
			{
				try
				{
					byte[] message;
					IPEndPoint rep = null;
					message = sock.Receive(ref rep);

					RawMessage rawMessage = new RawMessage( message,
						message.Length, rep.Address, rep.Port,
						sock.GetAddress(), sock.GetPort(),
						GetDescriptor());

					messageQueue.Add(rawMessage);
				}
				
				catch (Exception ex)
				{
					if (!isRunning) return;

					/** @todo do some better loggin will ya!!! */
					// Console.WriteLine("A net access point has gone useless: {0} {1}", ex.Message, ex.StackTrace);

					Stop();

					errorHandler.HandleFatalError(
						this,
						"Unknown error occurred while listening for messages!",
						ex);
				}
			}
		}


		/**
		 * Shut down the access point. Close the socket for recieving
		 * incoming messages. The method want close the socket if it was created
		 * outside the stack. (Bug Report - Dave Stuart - SipQuest)
		 */
		public virtual void Stop()
		{
			this.isRunning = false;

			//avoid a needless null pointer exception
			if(   sock != null
				&& isUsingExternalSocket == false)
				sock.Close();

		}

		public virtual bool IsRunning() 
		{
			return isRunning;
		}

		/**
		 * Sends message through this access point's socket.
		 * @param message the bytes to send.
		 * @param address message destination.
		 * @throws IOException if an exception occurs while sending the message.
		 */
		public virtual void SendMessage(byte[] message, StunAddress address)
		{

			IPEndPoint ipe = new IPEndPoint(address.GetSocketAddress().GetAddress(),address.GetSocketAddress().GetPort());
			sock.Send(message, message.Length, ipe);
		}

		/**
		 * Returns a String representation of the object.
		 * @return a String representation of the object.
		 */
		public override string ToString()
		{
			return "net.java.stun4j.stack.AccessPoint@"
				+apDescriptor.GetAddress()
				+" status: "
				+ (isRunning? "not":"")
				+" running";
		}

		/**
		 * Sets a socket for the access point to use. This socket will not be closed
		 * when the AP is <code>stop()</code>ed (Bug Report - Dave Stuart - SipQuest).
		 * @param socket the socket that the AP should use.
		 */
		public void UseExternalSocket(MyUdpClient socket)
		{
			this.sock = socket;
			this.isUsingExternalSocket = true;
		}
	}
}