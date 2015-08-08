using System;
using System.Threading;

namespace net.voxx.stun4cs 
{
	/**
	 * A utility used to flatten the multithreaded architecture of the Stack
	 * and execute the discovery process in a synchronized manner. Roughly what
	 * happens here is:
	 *
	 * ApplicationThread:
	 *     sendMessage()
	 * 	   wait();
	 *
	 * StackThread:
	 *     processMessage/Timeout()
	 *     {
	 *          saveMessage();
	 *          notify();
	 *     }
	 *
	 *
	 * <p>Organisation: <p> Louis Pasteur University, Strasbourg, France</p>
	 * <p>Network Research Team (http://www-r2.u-strasbg.fr)</p></p>
	 * @author Emil Ivov
	 * @version 0.1
	 */
    // modified to be public by Hugh Perkins December 2006
	public class BlockingRequestSender	: ResponseCollector
	{
		private StunProvider             stunProvider  = null;
		private NetAccessPointDescriptor apDescriptor  = null;

		StunMessageEvent responseEvent = null;

		public BlockingRequestSender(StunProvider             stunProvider,
			NetAccessPointDescriptor apDescriptor)
		{
			this.stunProvider = stunProvider;
			this.apDescriptor = apDescriptor;
		}

		/**
		 * Saves the message event and notifies the discoverer thread so that
		 * it may resume.
		 * @param evt the newly arrived message event.
		 */
		public virtual void ProcessResponse(StunMessageEvent evt)
		{
			Monitor.Enter(this);
			try 
			{
				this.responseEvent = evt;
				Monitor.PulseAll(this);
			} 
			finally 
			{
				Monitor.Exit(this);
			}
		}

		/**
		 * Notifies the discoverer thread when a message has timeouted so that
		 * it may resume and consider it as unanswered.
		 */
		public virtual void ProcessTimeout()
		{
			Monitor.Enter(this);
			try 
			{
				Monitor.PulseAll(this);
			} 
			finally 
			{
				Monitor.Exit(this);
			}
		}

		/**
		 * Sends the specified request and blocks until a response has been
		 * received or the request transaction has timed out.
		 * @param request the reuqest to send
		 * @param serverAddress the request destination address
		 * @return the event encapsulating the response or null if no response
		 * has been received.
		 * @throws StunException NETWORK_ERROR or other if we fail to send
		 * the message
		 */
		public virtual StunMessageEvent SendRequestAndWaitForResponse(
			Request request,
			StunAddress serverAddress)
		{
			Monitor.Enter(this);
			try 
			
			{
				stunProvider.SendRequest(request, serverAddress, apDescriptor,
					this);

				Monitor.Wait(this);

				StunMessageEvent res = responseEvent;
				responseEvent = null; //prepare for next message

				return res;
			} 
			finally 
			{
				Monitor.Exit(this);
			}
		}
	}
}