using System;
using System.Threading;

namespace net.voxx.stun4cs 
{
	public interface Runnable 
	{
	}

	/**
	 * The class is used to parse and dispatch incoming messages in a multithreaded
	 * manner.
	 *
	 * <p>Organisation: Louis Pasteur University, Strasbourg, France</p>
	 *                  <p>Network Research Team (http://www-r2.u-strasbg.fr)</p></p>
	 * @author Emil Ivov
	 * @version 0.1
	 */

	public interface IIsRunning 
	{
		bool IsRunning();
	}

	class MessageProcessor : Runnable, IIsRunning
	{
		private MessageQueue           messageQueue 	= null;
		private MessageEventHandler    messageHandler 	= null;
		private ErrorHandler           errorHandler		= null;

		private bool 			   isRunning	    = false;
		private Thread				   runningThread    = null;

		public MessageProcessor(MessageQueue                  queue,
			MessageEventHandler    messageHandler,
			ErrorHandler           errorHandler)
		{
			this.messageQueue    = queue;
			this.messageHandler    = messageHandler;
			this.errorHandler    = errorHandler;
		}

		/**
		 * Does the message parsing.
		 */
		public virtual void Run()
		{
			//add an extra try/catch block that handles uncatched errors and helps avoid
			//having dead threads in our pools.
			try
			{
				while (isRunning)
				{
					RawMessage rawMessage = null;
					rawMessage = messageQueue.Remove();

					// were we asked to stop?
					if (!IsRunning())
						return;

					//anything to parse?
					if (rawMessage == null)
						continue;

					Message stunMessage = null;
					try
					{
						stunMessage =
							Message.Decode(rawMessage.GetBytes(),
							0,
							rawMessage.GetMessageLength());
					}
					catch (StunException ex)
					{
						errorHandler.HandleError("Failed to decode a stun mesage!",
							ex);
						continue; //let this one go and for better luck next time.
					}

					StunAddress sa = new StunAddress(
						rawMessage.GetRemoteAddress().GetAddress(),
						rawMessage.GetRemoteAddress().GetPort() );

					sa.Local = rawMessage.GetLocalAddress();

					StunMessageEvent stunMessageEvent =
						new StunMessageEvent(rawMessage.GetNetAccessPoint(),
						stunMessage,
						sa);
					messageHandler.HandleMessageEvent(stunMessageEvent);
				}
			}
			catch(Exception err)
			{
				//notify and bail
				errorHandler.HandleFatalError(this, "Unexpected Error!", err);
			}
		}

		/**
		 * Start the message processing thread.
		 */
		public virtual void Start()
		{
			this.isRunning = true;

			runningThread = new Thread(new ThreadStart(this.Run));
			runningThread.Name = ("STUN message processor");
			runningThread.Start();
		}


		/**
		 * Shut down the message processor.
		 */
		public virtual void Stop()
		{
			this.isRunning = false;
			//        runningThread.interrupt();
		}

		/**
		 * Determines whether the processor is still running;
		 *
		 * @return true if the processor is still authorised to run, and false
		 * otherwise.
		 */
		public virtual bool IsRunning()
		{
			return isRunning;
		}
	}
}