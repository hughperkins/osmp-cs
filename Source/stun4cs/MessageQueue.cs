using System;
using System.Collections;
using System.Threading;

namespace net.voxx.stun4cs 
{
	/**
	 * The class is used as a part of the stack's thread pooling strategy. Method
	 * access is synchronized and message delivery ( remove() ) is blocking in the
	 * case of an empty queue.
	 *
	 * <p>Organisation: Louis Pasteur University, Strasbourg, France</p>
	 * <p>Network Research Team (http://www-r2.u-strasbg.fr)</p></p>
	 *
	 * @author Emil Ivov
	 * @version 0.1
	 */
	class MessageQueue : IIsRunning
	{
		// not sure whether Vector is the best choice here since we explicitly
		//sync all methods ... review later
		private ArrayList queue = new ArrayList();
		private IIsRunning runningChecker;

		/**
		 * Create an empty MessaeFIFO
		 */
		public MessageQueue(IIsRunning isRunning)
		{
			this.runningChecker = isRunning;
		}

		/**
		 * Returns the number of messages currently in the queue.
		 * @return the number of messages in the queue.
		 */
		public int getSize()
		{
			return queue.Count;
		}

		/**
		 * Determines whether the FIFO is currently empty.
		 * @return true if the FIFO is currently empty and false otherwise.
		 */
		public virtual bool IsEmpty()
		{
			return queue.Count == 0;
		}

		/**
		 * Adds the specified message to the queue.
		 *
		 * @param rawMessage the message to add.
		 */
		public virtual void Add(RawMessage rawMessage)
		{
			Monitor.Enter(this);
			try 
			{
				queue.Add(rawMessage);

				Monitor.PulseAll(this);
			}
			finally 
			{
				Monitor.Exit(this);
			}
		}

		/**
		 * Removes and returns the oldest message from the fifo. If there are
		 * currently no messages in the queue the method block until there is at
		 * least one message.
		 *
		 *
		 * @return the oldest message in the fifo.
		 * @throws java.lang.InterruptedException if an InterruptedException is thrown
		 * wail waiting for a new message to be added.
		 */
		public RawMessage Remove()
										   
		{
			Monitor.Enter(this);
			try
			{
				waitWhileEmpty();
				if (!this.IsRunning()) 
				{
					return null;
				}
				RawMessage rawMessage = (RawMessage) queue[0];
				queue.RemoveAt(0);
   
				return rawMessage;
			} 
			finally 
			{
				Monitor.Exit(this);
			}
		}

		public bool IsRunning() 
		{
			return this.runningChecker == null || this.runningChecker.IsRunning();
		}

		/**
		 * Blocks until there is at least one message in the queue.
		 *
		 * @throws java.lang.InterruptedException if an InterruptedException is thrown
		 * wail waiting for a new message to be added.
		 */
		private void waitWhileEmpty() 
		{
			while (IsEmpty() && IsRunning())
			{
				Monitor.Wait(this, 100);
			}
		}
	}

}