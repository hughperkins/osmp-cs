using System;


/**
 * The interface is used as a callback when sending a request. The response
 * collector is then used as a means of dispatching the response.
 *
 * <p>Organisation: <p> Louis Pasteur University, Strasbourg, France</p>
 * <p>Network Research Team (http://www-r2.u-strasbg.fr)</p></p>
 * @author Emil Ivov
 * @version 0.1
 */

namespace net.voxx.stun4cs 
{
	public interface ResponseCollector
	{
		/**
		 * Dispatch the specified response.
		 * @param response the response to dispatch.
		 */
		void ProcessResponse(StunMessageEvent response);

		/**
		 * Notify the collector that no response had been received
		 * after repeated retransmissions of the original request (as described
		 * by rfc3489) and that the request should be considered unanswered.
		 */
		void ProcessTimeout();
	}
}