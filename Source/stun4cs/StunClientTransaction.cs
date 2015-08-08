using System;
using System.Threading;

/**
 * The ClientTransaction class retransmits (what a surprise) requests as
 * specified by rfc 3489.
 *
 * Once formulated and sent, the client sends the Binding Request.  Reliability
 * is accomplished through request retransmissions.  The ClientTransaction
 * retransmits the request starting with an interval of 100ms, doubling
 * every retransmit until the interval reaches 1.6s.  Retransmissions
 * continue with intervals of 1.6s until a response is received, or a
 * total of 9 requests have been sent. If no response is received by 1.6
 * seconds after the last request has been sent, the client SHOULD
 * consider the transaction to have failed. In other words, requests
 * would be sent at times 0ms, 100ms, 300ms, 700ms, 1500ms, 3100ms,
 * 4700ms, 6300ms, and 7900ms. At 9500ms, the client considers the
 * transaction to have failed if no response has been received.
 *
 *
 * <p>Organisation: <p> Louis Pasteur University, Strasbourg, France</p>
 * <p>Network Research Team (http://www-r2.u-strasbg.fr)</p></p>
 * @author Emil Ivov
 * @version 0.1
 */


namespace net.voxx.stun4cs 
{
	public class StunClientTransaction
	{
		/**
		 * Maximum number of retransmissions. Once this number is reached and if no
		 * response is received after MAX_WAIT_INTERVAL miliseconds the request is
		 * considered unanswered.
		 */
		public const int MAX_RETRANSMISSIONS = 8;

		/**
		 * The number of miliseconds to wait before the first retansmission of the
		 * request.
		 */
		public const int ORIGINAL_WAIT_INTERVAL = 100;

		/**
		 * The maximum wait interval. Once this interval is reached we should stop
		 * doubling its value.
		 */
		public const int MAX_WAIT_INTERVAL = 1600;

		/**
		 * Indicates how many times we have retransmitted so fat.
		 */
		private int retransmissionsCounter = 0;

		/**
		 * How much did we wait after our last retransmission.
		 */
		private int lastWaitInterval       = ORIGINAL_WAIT_INTERVAL;

		/**
		 * The StunProvider that created us.
		 */
		private StunProvider      providerCallback  = null;

		/**
		 * The request that we are retransmitting.
		 */
		private Request           request           = null;

		/**
		 * The destination of the request.
		 */
		private StunAddress           requestDestination= null;


		/**
		 * The id of the transaction.
		 */
		private TransactionID    transactionID      = null;

		/**
		 * The NetAccessPoint through which the original request was sent an where
		 * we are supposed to be retransmitting.
		 */
		private NetAccessPointDescriptor apDescriptor = null;

		/**
		 * The instance to notify when a response has been received in the current
		 * transaction or when it has timed out.
		 */
		private ResponseCollector 	     responseCollector = null;

		/**
		 * Specifies whether the transaction is active or not.
		 */
		private bool cancelled = false;

		/**
		 * The date (in millis) when the next retransmission should follow.
		 */
		private long nextRetransmissionDate = -1;

		/**
		 * The thread that this transaction runs in.
		 */
		private Thread runningThread = null;

		/**
		 * Creates a client transaction
		 * @param providerCallback the provider that created us.
		 * @param request the request that we are living for.
		 * @param requestDestination the destination of the request.
		 * @param apDescriptor the access point through which we are supposed to
		 * @param responseCollector the instance that should receive this request's
		 * response.
		 * retransmit.
		 */
		public StunClientTransaction(StunProvider            providerCallback,
			Request                  request,
			StunAddress              requestDestination,
			NetAccessPointDescriptor apDescriptor,
			ResponseCollector        responseCollector)
		{
			this.providerCallback  = providerCallback;
			this.request           = request;
			this.apDescriptor      = apDescriptor;
			this.responseCollector = responseCollector;
			this.requestDestination = requestDestination;

			this.transactionID = TransactionID.CreateTransactionID();
			
			request.SetTransactionID(transactionID.GetTransactionID());

			runningThread = new Thread(new ThreadStart(this.Run));
		}

		/**
		 * Implements the retransmissions algorithm. Retransmits the request
		 * starting with an interval of 100ms, doubling every retransmit until the
		 * interval reaches 1.6s.  Retransmissions continue with intervals of 1.6s
		 * until a response is received, or a total of 9 requests have been sent.
		 * If no response is received by 1.6 seconds after the last request has been
		 * sent, we consider the transaction to have failed.
		 */
		public void Run()
		{
			try 
			{
				runningThread.Name = ("CliTran");
				while(retransmissionsCounter++ < MAX_RETRANSMISSIONS)
				{

					WaitUntilNextRetransmissionDate();
					//did someone tell us to get lost?

					if(cancelled)
						return;

					if(lastWaitInterval < MAX_WAIT_INTERVAL)
						lastWaitInterval *= 2;

					try
					{
						providerCallback.GetNetAccessManager().SendMessage(
							request, apDescriptor, requestDestination);
					}
					catch (StunException )
					{
						/** @todo log */
						//I wonder whether we should notify anyone that a retransmission
						//has failes
					};

					//	08/02/05 mtj - everything should be done in milliseconds
					ScheduleRetransmissionDate(lastWaitInterval);
				}

				responseCollector.ProcessTimeout();
				providerCallback.RemoveClientTransaction(this);
			} 
			catch (Exception e) 
			{
				Console.WriteLine("Exception in CLI thread {0} {1}", e.Message, e.StackTrace);
			}
		}

		/**
		 * Sends the request and schedules the first retransmission for after
		 * ORIGINAL_WAIT_INTERVAL and thus starts the retransmission algorithm.
		 * @throws StunException if message encoding fails, ILLEGAL_ARGUMENT if the
		 * apDescriptor references an access point that had not been installed,
		 * NETWORK_ERROR if an error occurs while sending message bytes through the
		 * network socket.

		 */
		public void SendRequest()
		{
			providerCallback.GetNetAccessManager().SendMessage(this.request,
				apDescriptor,
				requestDestination);

			//	08/02/05 mtj - function parameter should be in milliseconds
			//ScheduleTicks(ORIGINAL_WAIT_INTERVAL / 10);
			ScheduleRetransmissionDate(ORIGINAL_WAIT_INTERVAL);
			runningThread.Start();
		}

		/**
		 * Returns the request that was the reason for creating this transaction.
		 * @return the request that was the reason for creating this transaction.
		 */
		private Request GetRequest()
		{
			return this.request;
		}


		/**
		 * Waits until next retransmission is due or until the transaction is
		 * cancelled (whichever comes first).
		 */
		private void WaitUntilNextRetransmissionDate()
		{
			lock (this) 
			{
				//	08/02/05 mtj - nextRetransmissionDate is millisecs, not ticks
				long next = nextRetransmissionDate * TimeSpan.TicksPerMillisecond;
				long current = DateTime.Now.Ticks;

				//while(nextRetransmissionDate - current > 0)
				while((next - current) > 0)
				{
					//Thread.Sleep(new TimeSpan(nextRetransmissionDate - current));
					Thread.Sleep(new TimeSpan(next - current));

					//did someone ask us to get lost?
					if(cancelled)
						return;
					current = DateTime.Now.Ticks;
				}
			}
		}

		/**
		 * Sets the next retransmission date.
		 * @param timeout_ms the number of millis to wait before next retransmission.
		 */
		//private void ScheduleTicks(long timeout)
		private void ScheduleRetransmissionDate(long timeout_ms)
		{
			//	08/02/05 mtj - this.nextRetransmissionDate is supposed to be millisecs, not ticks
			//this.nextRetransmissionDate = DateTime.Now.Ticks + timeout;
			this.nextRetransmissionDate = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) + timeout_ms;
		}

		/**
		 * Cancels the transaction. Once this method is called the transaction is
		 * considered terminated and will stop retransmissions.
		 */
		public virtual void Cancel()
		{
			Monitor.Enter(this);
			try
			{
				this.cancelled = true;
				Monitor.PulseAll(this);
			} 
			finally 
			{
				Monitor.Exit(this);
			}
		}

		/**
		 * Dispatches the response then cancels itself and notifies the StunProvider
		 * for its termination.
		 * @param evt the event that contains the newly received message
		 */
		public virtual void HandleResponse(StunMessageEvent evt)
		{
			this.Cancel();
			this.responseCollector.ProcessResponse(evt);
		}

		/**
		 * Returns the ID of the current transaction.
		 *
		 * @return the ID of the transaction.
		 */
		public virtual TransactionID GetTransactionID()
		{
			return this.transactionID;
		}
	}
}