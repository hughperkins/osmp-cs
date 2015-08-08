using System;
using System.Collections;
using System.Net.Sockets;
using System.Threading;

namespace net.voxx.stun4cs 
{
	/**
	 * Manages NetAccessPoints and MessageProcessor pooling. This class serves as a
	 * layer that masks network primitives and provides equivalent STUN abstractions.
	 * Instances that operate with the NetAccessManager are only supposed to
	 * understand STUN talk and shouldn't be aware of datagrams sockets, and etc.
	 *
	 * <p>Organisation: Louis Pasteur University, Strasbourg, France</p>
	 *                   <p>Network Research Team (http://www-r2.u-strasbg.fr)</p></p>
	 * @author Emil Ivov
	 * @version 0.1
	 */

	public class NetAccessManager : ErrorHandler, IIsRunning
	{

		/**
		 * All access points currently in use. The table maps NetAccessPointDescriptor-s
		 * to NetAccessPoint-s
		 */
		private Hashtable    netAccessPoints   = new Hashtable();

		/**
		 * A synchronized FIFO where incoming messages are stocked for processing.
		 */
		private MessageQueue messageQueue      = null;

		/**
		 * A thread pool of message processors.
		 */
		private ArrayList       messageProcessors = new ArrayList();

		/**
		 * The instance that should be notified whan an incoming message has been
		 * processed and ready for delivery
		 */
		private MessageEventHandler messageEventHandler = null;

		/**
		 * Indicates whether the access manager has been started.
		 */
		private bool isRunning = false;

		/**
		 * The size of the thread pool to start with.
		 */
		private int initialThreadPoolSize = StunStack.DEFAULT_THREAD_POOL_SIZE;

		/**
		 * Constructs a NetAccessManager.
		 */
		public NetAccessManager()
		{
			this.messageQueue = new MessageQueue(this);
		}



		/**
		 * Initializes the message processors pool and sets the status of the manager
		 * to running.
		 */
		public virtual void Start()
		{
			if(isRunning)
				return;

			this.isRunning = true;
			this.initThreadPool();
		}

		/**
		 * Stops and deletes all message processors and access points.
		 */
		public virtual void ShutDown()
		{
			if (!isRunning)
				return;

			//stop access points
			ArrayList toRemove = new ArrayList();
			foreach (NetAccessPointDescriptor napd in netAccessPoints.Keys) 
			{
				toRemove.Add(napd);
			}

			foreach (NetAccessPointDescriptor napd in toRemove) 
			{
				RemoveNetAccessPoint(napd);
			}

			//stop and empty thread pool
			while (messageProcessors.Count != 0)
			{
				MessageProcessor mp = (MessageProcessor) messageProcessors[0];
				messageProcessors.RemoveAt(0);
				mp.Stop();
			}

			//message processors should have been interrupted but it wouldn't hurt
			//notifying as well in case we decide to change one day.
			Monitor.Enter(messageQueue); 
			try
			{
				Monitor.PulseAll(messageQueue);
			} 
			finally 
			{
				Monitor.Exit(messageQueue);
			}

			isRunning = false;
		}

		/**
		 * Determines whether the NetAccessManager has been started.
		 * @return true if this NetAccessManager has been started, and false
		 * otherwise.
		 */
		public virtual bool IsRunning()
		{
			return isRunning;
		}


		/**
		 * Sets the instance to notify for incoming message events.
		 * @param evtHandler the entity that will handle incoming messages.
		 */
		public virtual void SetEventHandler(MessageEventHandler evtHandler)
		{
			messageEventHandler = evtHandler;
		}

		//------------------------ error handling -------------------------------------
		/**
		 * A civilized way of not caring!
		 * @param message a description of the error
		 * @param error   the error that has occurred
		 */
		public void HandleError(String message, Exception error)
		{
			/** @todo log */
			/**
			 * apart from logging, i am not sure what else we could do here.
			 * So for the time being - just drop.
			 */
		}

		/**
		 * Clears the faulty thread and tries to repair the damage and instanciate
		 * a replacement.
		 *
		 * @param callingThread the thread where the error occurred.
		 * @param message       A description of the error
		 * @param error         The error itself
		 */
		public void HandleFatalError(Runnable callingThread, String message, Exception error)
		{
			if (callingThread is NetAccessPoint)
			{
				NetAccessPoint ap = (NetAccessPoint)callingThread;
				bool running = ap.IsRunning();

				//make sure socket is closed
				RemoveNetAccessPoint(ap.GetDescriptor());

				if (running) 
				{
					try
					{
						Console.WriteLine("An access point has unexpectedly "
							+"stopped. AP: {0}" , ap.ToString());

						InstallNetAccessPoint(ap.GetDescriptor());
						/** @todo: log fixing the error*/
					}
					catch (StunException ex)
					{
						//make sure nothing's left and notify user
						RemoveNetAccessPoint(ap.GetDescriptor());
						Console.WriteLine("Failed to relaunch accesspoint: {0} {1} {2}" , ap, ex.Message, ex.StackTrace);
						/** @todo notify user and log */
					}
				}

				return;
			}
			else if( callingThread is MessageProcessor )
			{
				MessageProcessor mp = (MessageProcessor)callingThread;

				//make sure the guy's dead.
				mp.Stop();
				messageProcessors.Remove(mp);

				mp = new MessageProcessor(messageQueue, messageEventHandler, this);
				mp.Start();
				Console.WriteLine("A message processor has been relaunched because of an error.");

			}
			Console.WriteLine("Error was: {0} {1}", error.Message, error.StackTrace);
		}

		/**
		 * Creates and starts a new access point according to the given descriptor.
		 * If the specified access point has already been installed the method
		 * has no effect.
		 *
		 * @param apDescriptor   a description of the access point to create.
		 * @throws StunException if we fail to create or start the accesspoint.
		 */
		public virtual void InstallNetAccessPoint(NetAccessPointDescriptor apDescriptor)
		{
			if(netAccessPoints.ContainsKey(apDescriptor))
				return;

			NetAccessPoint ap = new NetAccessPoint(apDescriptor, messageQueue, this);
			netAccessPoints[apDescriptor] = ap;

			ap.Start();
		}

		/**
		 * Creates and starts a new access point based on the specified socket.
		 * If the specified access point has already been installed the method
		 * has no effect.
		 *
		 * @param  socket   the socket that the access point should use.
		 * @return an access point descriptor to allow further management of the
		 * newly created access point.
		 * @throws StunException if we fail to create or start the accesspoint.
		 */

		public NetAccessPointDescriptor InstallNetAccessPoint(MyUdpClient socket)
		{
			
			//no null check - let it through a null pointer exception
			StunAddress address = new StunAddress(socket.GetAddress().ToString(), socket.GetPort());
			NetAccessPointDescriptor apDescriptor = new NetAccessPointDescriptor(address);

			if(netAccessPoints.ContainsKey(apDescriptor))
				return apDescriptor;

			NetAccessPoint ap = new NetAccessPoint(apDescriptor, messageQueue, this);
			//call the useExternalSocket method to avoid closing the socket when
			//removing the accesspoint. Bug Report - Dave Stuart - SipQuest
			ap.UseExternalSocket(socket);
			netAccessPoints[apDescriptor] = (ap);

			ap.Start();

			return apDescriptor;
		}

		/**
		 * Stops and deletes the specified access point.
		 * @param apDescriptor the access  point to remove
		 */
		public virtual void RemoveNetAccessPoint(NetAccessPointDescriptor apDescriptor)
		{
			NetAccessPoint ap = (NetAccessPoint)netAccessPoints[apDescriptor];

			netAccessPoints.Remove(apDescriptor);

			if(ap != null)
				ap.Stop();
		}

		//---------------thread pool implementation --------------------------------
		/**
		 * Adjusts the number of concurrently running MessageProcessors.
		 * If the number is smaller or bigger than the number of threads
		 * currentlyrunning, then message processors are created/deleted so that their
		 * count matches the new value.
		 *
		 * @param threadPoolSize the number of MessageProcessors that should be running concurrently
		 * @throws StunException INVALID_ARGUMENT if threadPoolSize is not a valid size.
		 */
		public virtual void SetThreadPoolSize(int threadPoolSize)
		{
			if(threadPoolSize < 1)
				throw new StunException(StunException.ILLEGAL_ARGUMENT,
					threadPoolSize + " is not a legal thread pool size value.");

			//if we are not running just record the size so that we could init later.
			if(!isRunning)
			{
				initialThreadPoolSize = threadPoolSize;
				return;
			}

			if(messageProcessors.Count < threadPoolSize)
			{
				//create additional processors
				fillUpThreadPool(threadPoolSize);
			}
			else
			{
				//delete extra processors
				shrinkThreadPool(threadPoolSize);
			}
		}

		/**
		 * @throws StunException INVALID_ARGUMENT if threadPoolSize is not a valid size.
		 */
		private void initThreadPool()
		{
			//create additional processors
			fillUpThreadPool(initialThreadPoolSize);
		}


		/**
		 * Starts all message processors
		 *
		 * @param newSize the new thread poolsize
		 */
		private void fillUpThreadPool(int newSize)
		{
			//make sure we don't resize more than once
			// messageProcessors.ensureCapacity(newSize);

			for (int i = messageProcessors.Count; i < newSize; i++)
			{
				MessageProcessor mp = new MessageProcessor(messageQueue,
					messageEventHandler,
					this);
				messageProcessors.Add(mp);

				mp.Start();
			}

		}

		/**
		 * Starts all message processors
		 *
		 * @param newSize the new thread poolsize
		 */
		private void shrinkThreadPool(int newSize)
		{
			while(messageProcessors.Count > newSize)
			{
				MessageProcessor mp = (MessageProcessor)messageProcessors[(0)];
				messageProcessors.RemoveAt(0);
				mp.Stop();
			}
		}

		//--------------- SENDING MESSAGES -----------------------------------------
		/**
		 * Sends the specified stun message through the specified access point.
		 * @param stunMessage the message to send
		 * @param apDescriptor the access point to use to send the message
		 * @param address the destination of the message.
		 * @throws StunException if message encoding fails, ILLEGAL_ARGUMENT if the
		 * apDescriptor references an access point that had not been installed,
		 * NETWORK_ERROR if an error occurs while sending message bytes through the
		 * network socket.
		 */
		public virtual void SendMessage(Message                  stunMessage,
			NetAccessPointDescriptor apDescriptor,
			StunAddress                  address)
		{
			byte[] bytes = stunMessage.Encode();
			NetAccessPoint ap = (NetAccessPoint)netAccessPoints[apDescriptor];

			if(ap == null)
				throw new StunException(
					StunException.ILLEGAL_ARGUMENT,
					"The specified access point had not been installed.");

			try
			{
				ap.SendMessage(bytes, address);
			}
			catch (Exception ex)
			{
				throw new StunException(StunException.NETWORK_ERROR,
					"An Exception occurred while sending message bytes "
					+"through a network socket!",
					ex);
			}
		}

	}
}