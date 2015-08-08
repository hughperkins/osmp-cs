using System;
using System.Threading;

namespace net.voxx.stun4cs 
{
	/**
	 *
	 *
	 * <p>Organisation: <p> Louis Pasteur University, Strasbourg, France</p>
	 * <p>Network Research Team (http://www-r2.u-strasbg.fr)</p></p>
	 * @author Emil Ivov
	 * @version 0.1
	 */
	interface ErrorHandler
	{
		/**
		 * Called when an error has occurred which may have caused data loss but the
		 * calling thread is still running.
		 *
		 * @param message A message describing the error
		 * @param error   The error itself.
		 */
		void HandleError(String message, Exception error);

		/**
		 * Called when a fatal error has occurred and the calling thread will exit.
		 *
		 * @param callingThread the thread where the error has occurred
		 * @param message       a message describing the error.
		 * @param error         the error itself.
		 */
		void HandleFatalError(Runnable callingThread, string message, Exception error);
	}
}
