using System;
using System.Net;
using System.Net.Sockets;

/* ====================================================================
 * The Apache Software License, Version 1.1
 *
 * Copyright (c) 2000 The Apache Software Foundation.  All rights
 * reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 *
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in
 *    the documentation and/or other materials provided with the
 *    distribution.
 *
 * 3. The end-user documentation included with the redistribution,
 *    if any, must include the following acknowledgment:
 *       "This product includes software developed by the
 *        Apache Software Foundation (http://www.apache.org/)."
 *    Alternately, this acknowledgment may appear in the software itself,
 *    if and wherever such third-party acknowledgments normally appear.
 *
 * 4. The names "Apache" and "Apache Software Foundation" must
 *    not be used to endorse or promote products derived from this
 *    software without prior written permission. For written
 *    permission, please contact apache@apache.org.
 *
 * 5. Products derived from this software may not be called "Apache",
 *    nor may "Apache" appear in their name, without prior written
 *    permission of the Apache Software Foundation.
 *
 * THIS SOFTWARE IS PROVIDED ``AS IS'' AND ANY EXPRESSED OR IMPLIED
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED.  IN NO EVENT SHALL THE APACHE SOFTWARE FOUNDATION OR
 * ITS CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF
 * USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
 * OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT
 * OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
 * SUCH DAMAGE.
 * ====================================================================
 *
 * This software consists of voluntary contributions made by many
 * individuals on behalf of the Apache Software Foundation.  For more
 * information on the Apache Software Foundation, please see
 * <http://www.apache.org/>.
 *
 * Portions of this software are based upon public domain software
 * originally written at the National Center for Supercomputing Applications,
 * University of Illinois, Urbana-Champaign.
 */

namespace net.voxx.stun4cs 
{
	/**
	 * The entry point to the Stun4J stack. The class is used to start, stop and
	 * configure the stack.
	 *
	 * <p>Organisation: Louis Pasteur University, Strasbourg, France</p>
	 *               <p>Network Research Team (http://www-r2.u-strasbg.fr)</p></p>
	 * @author Emil Ivov
	 * @version 0.1
	 */
	public class StunStack
	{
		/**
		 * We shouldn't need more than one stack in the same application.
		 */
		private static StunStack stackInstance = null;

		/**
		 * Our network gateway.
		 */
		private NetAccessManager netAccessManager = new NetAccessManager();

		/**
		 * The number of threads to split our flow in.
		 */
		public const int DEFAULT_THREAD_POOL_SIZE = 3;

		private StunProvider stunProvider = null;

		/**
		 * Returns a reference to the singleton StunStack insance. If the stack
		 * had not yet been initialised, a new instance will be created.
		 *
		 * @return a reference to the StunStack.
		 */
		public static StunStack Instance
		{
			get 
			{
				lock (typeof(StunStack)) 
				{
					if (stackInstance == null)
						stackInstance = new StunStack();
					return stackInstance;
				}
			}
		}

		//----------------------- PUBLIC INTERFACE ---------------------------------
		/**
		 * Puts the stack into an operational state.
		 */
		public virtual void Start()
		{
			netAccessManager.Start();
		}

		/**
		 * Stops the stack.
		 */
		public static void ShutDown()
		{
			lock (typeof(StunStack)) 
			{
				if (stackInstance == null)
					return;
				stackInstance.netAccessManager.ShutDown();
				stackInstance.stunProvider.ShutDown();

				stackInstance.netAccessManager = null;
				stackInstance.stunProvider = null;
				stackInstance = null;
			}
		}

		public virtual void SetThreadPoolSize(int threadPoolSize)
		{
			netAccessManager.SetThreadPoolSize(threadPoolSize);
		}

		/**
		 * Creates and starts the specified Network Access Point.
		 *
		 * @param apDescriptor A descriptor containing the address and port of the
		 * STUN server that the newly created access point will communicate with.
		 * @throws StunException
		 *           <p>NETWORK_ERROR if we fail to create or bind the datagram socket.</p>
		 *           <p>ILLEGAL_STATE if the stack had not been started.</p>
		 */
		public virtual void InstallNetAccessPoint(NetAccessPointDescriptor apDescriptor)
		{
			CheckStarted();

			netAccessManager.InstallNetAccessPoint(apDescriptor);
		}

		/**
		 * Creates and starts the specified Network Access Point based on the specified
		 * socket and returns a relevant descriptor.
		 *
		 * @param sock The socket that the new access point should represent.
		 * @throws StunException
		 *           <p>NETWORK_ERROR if we fail to create or bind the datagram socket.</p>
		 *           <p>ILLEGAL_STATE if the stack had not been started.</p>
		 * @return a descriptor of the newly created access point.
		 */

		public virtual NetAccessPointDescriptor InstallNetAccessPoint(MyUdpClient sock)
		{
			CheckStarted();

			return netAccessManager.InstallNetAccessPoint(sock);
		}

		/**
		 * Stops and deletes the specified access point.
		 * @param apDescriptor the access  point to remove
		 */
		public virtual void RemoveNetAccessPoint(NetAccessPointDescriptor apDescriptor)
		{
			CheckStarted();

			netAccessManager.RemoveNetAccessPoint(apDescriptor);
		}



		/**
		 * Returns a StunProvider instance to be used for sending and receiving
		 * mesages.
		 *
		 * @return an instance of StunProvider
		 */
		public virtual StunProvider GetProvider()
		{
			return stunProvider;
		}

		//-------------------- internal stuff --------------------------------------
		/**
		 * Private constructor as we want a singleton pattern.
		 */
		private StunStack()
		{
			stunProvider = new StunProvider(this);

			netAccessManager.SetEventHandler(GetProvider());
		}

		/**
		 * Throws a StunException.ILLEGAL_STATE if the stack had not been started.
		 * @throws StunException ILLEGAL_STATE if the stack had not been started.
		 */
		public void CheckStarted()
		{
			if(netAccessManager == null || !netAccessManager.IsRunning())
				throw new StunException(
					StunException.ILLEGAL_STATE,
					"The stack needs to be started, for "
					+"the requested method to work.");
		}

		/**
		 * Returns the currently active instance of NetAccessManager.
		 * @return the currently active instance of NetAccessManager.
		 */
		public virtual NetAccessManager GetNetAccessManager()
		{
			return netAccessManager;
		}
	}

}