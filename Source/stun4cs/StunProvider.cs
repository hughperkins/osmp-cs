using System;
using System.Collections;

namespace net.voxx.stun4cs 
{
	/**
	 * The StunProvider class is an implementation of a Stun Transaction Layer. STUN
	 * transactions are extremely simple and are only used to correlate requests and
	 * responses. In the Stun4J implementation it is the transaction layer that
	 * ensures reliable delivery.
	 *
	 * <p>Organisation: <p> Louis Pasteur University, Strasbourg, France</p>
	 * <p>Network Research Team (http://www-r2.u-strasbg.fr)</p></p>
	 * @author Emil Ivov
	 * @version 0.1
	 */

	public class StunProvider : MessageEventHandler
	{

		/**
			 * Stores active client transactions mapped against TransactionID-s.
			 */
		private Hashtable clientTransactions = new Hashtable();

		/**
		 * Currently open server transactions. The vector contains transaction ids
		 * of all non-answered received requests.
		 */
		private ArrayList    serverTransactions = new ArrayList();

		/**
		 * The stack that created us.
		 */
		private StunStack stunStack                  = null;

		/**
		 * The instance to notify for incoming StunRequests;
		 */
		private RequestListener  requestListener    = null;


		//------------------ public interface
		/**
		 * Creates the provider.
		 * @param stunStack The currently active stack instance.
		 */
		public StunProvider(StunStack stunStack)
		{
			this.stunStack = stunStack;
		}

		/**
		 * Sends the specified request through the specified access point, and
		 * registers the specified ResponseCollector for later notification.
		 * @param  request     the request to send
		 * @param  sendTo      the destination address of the request.
		 * @param  sendThrough the access point to use when sending the request
		 * @param  collector   the instance to notify when a response arrives or the
		 *                     the transaction timeouts
		 * @throws StunException
		 * ILLEGAL_STATE if the stun stack is not started. <br/>
		 * ILLEGAL_ARGUMENT if the apDescriptor references an access point that had
		 * not been installed <br/>
		 * NETWORK_ERROR if an error occurs while sending message bytes through the
		 * network socket. <br/>

		 */
		public virtual void SendRequest( Request                  request,
			StunAddress              sendTo,
			NetAccessPointDescriptor sendThrough,
			ResponseCollector        collector )
		{
			stunStack.CheckStarted();

			StunClientTransaction clientTransaction =
				new StunClientTransaction(this,
				request,
				sendTo,
				sendThrough,
				collector);

			clientTransactions[clientTransaction.GetTransactionID()] =
				clientTransaction;
			clientTransaction.SendRequest();
		}

		/**
		 * Sends the specified response message through the specified access point.
		 *
		 * @param transactionID the id of the transaction to use when sending the
		 *    response. Actually we are getting kind of redundant here as we already
		 *    have the id in the response object, but I am bringing out as an extra
		 *    parameter as the user might otherwise forget to explicitly set it.
		 * @param response      the message to send.
		 * @param sendThrough   the access point to use when sending the message.
		 * @param sendTo        the destination of the message.
		 * @throws StunException TRANSACTION_DOES_NOT_EXIST if the response message
		 * has an invalid transaction id. <br/>
		 * ILLEGAL_STATE if the stun stack is not started. <br/>
		 * ILLEGAL_ARGUMENT if the apDescriptor references an access point that had
		 * not been installed <br/>
		 * NETWORK_ERROR if an error occurs while sending message bytes through the
		 * network socket. <br/>
		 */
		public virtual void SendResponse(byte[]                   transactionID,
			Response                 response,
			NetAccessPointDescriptor sendThrough,
			StunAddress                  sendTo)
		{
			stunStack.CheckStarted();

			TransactionID tid = TransactionID.CreateTransactionID(transactionID);


			serverTransactions.Remove(tid);
#if false
				throw new StunException(StunException.TRANSACTION_DOES_NOT_EXIST,
					"The trensaction specified in the response "
					+"object does not exist.");
#endif

			response.SetTransactionID(transactionID);
			GetNetAccessManager().SendMessage(response, sendThrough, sendTo);
		}

		/**
		 * Sets the listener that should be notified when a new Request is received.
		 * @param requestListener the listener interested in incoming requests.
		 */
		public  virtual void SetRequestListener(RequestListener requestListener)
		{
			this.requestListener = requestListener;
		}

		//------------- stack internals ------------------------------------------------
		/**
		 * Returns the currently active instance of NetAccessManager. Used by client
		 * transactions when sending messages.
		 * @return the currently active instance of NetAccessManager.
		 */
		public NetAccessManager GetNetAccessManager()
		{
			return stunStack.GetNetAccessManager();
		}

		/**
		 * Removes a client transaction from this providers client transactions list.
		 * Method is used by ClientStunTransaction-s themselves when a timeout occurs.
		 * @param tran the transaction to remove.
		 */
		public void RemoveClientTransaction(StunClientTransaction tran)
		{
			clientTransactions.Remove(tran.GetTransactionID());
		}

		/**
		 * Called to notify this provider for an incoming message.
		 * @param event the event object that contains the new message.
		 */
		public virtual void HandleMessageEvent(StunMessageEvent @event)
		{
			Message msg = @event.GetMessage();
			//request
			if(msg is Request)
			{
				TransactionID serverTid = TransactionID.
					CreateTransactionID(msg.GetTransactionID());

				serverTransactions.Add(serverTid);
				if(requestListener != null)
					requestListener.requestReceived(@event);
			}
				//response
			else if(msg is Response)
			{
				TransactionID tid = TransactionID.
					CreateTransactionID(msg.GetTransactionID());

				StunClientTransaction tran = (StunClientTransaction)clientTransactions[tid];
				clientTransactions.Remove(tid);

				if(tran != null)
				{
					tran.HandleResponse(@event);
				}
				else
				{
					//do nothing - just drop the phantom response.
				}

			}

		}

		/**
		 * Cancels all running transactions and prepares for garbage collection
		 */
		public virtual void ShutDown()
		{
			requestListener = null;

			ArrayList toRemove = new ArrayList();
			foreach (object o in clientTransactions.Keys) 
			{
				toRemove.Add(o);
			}

			foreach (TransactionID id in toRemove) 
			{
				StunClientTransaction sct = (StunClientTransaction) clientTransactions[id];
				clientTransactions.Remove(id);
				if (sct != null) 
				{
					sct.Cancel();
				}
			}
		}

	}
}