using System;

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
	 * A StunException is thrown when a general STUN exception is encountered.
	 *
	 * <p>Copyright: Copyright (c) 2003</p>
	 * <p>Organisation: Louis Pasteur University, Strasbourg, France</p>
	 * <p>Network Research Team (http://www-r2.u-strasbg.fr)</p></p>
	 * @author Emil Ivov
	 * @version 0.1
	 */

	public class StunException : Exception
							   {
								   private int errorID = 0;
		private Exception cause;

		/**
		 * Means that the the reason that caused the exception was unclear.
		 */
		public const int UNKNOWN_ERROR = 0;

		/**
		 * Indicates that the attempted operation is not possible in the current
		 * state of the object.
		 */
		public const int ILLEGAL_STATE = 1;

		/**
		 * Indicates that one or more of the passed arguments had invalid values.
		 */
		public const int ILLEGAL_ARGUMENT = 2;

		/**
		 * Indicates that an unexpected error has occurred..
		 */
		public const int INTERNAL_ERROR = 3;

		/**
		 * Thrown when trying to send responses through a non-existant transaction
		 * That may happen when a corresponding request has already been responded
		 * to or when no such request has been received.
		 */
		public const int TRANSACTION_DOES_NOT_EXIST = 3;


		/**
		 * Indicates that an unexpected error has occurred..
		 */
		public const int NETWORK_ERROR = 4;


		/**
		 * Identifies the exception.
		 */
		private int id = 0;

		/**
		 * Creates a StunException.
		 */
		public StunException()
		{

		}

		/**
		 * Creates a StunException setting id as its identifier.
		 * @param id an error ID
		 */
		public StunException(int id)
		{
			setID(id);
		}

		/**
		 * Creates a StunException, setting an error message.
		 * @param message an error message.
		 */
		public StunException(String message) : base(message)
		{
		}

		/**
		 * Creates a StunException, setting an error message and an error id.
		 * @param message an error message.
		 * @param id an error id.
		 */
		public StunException(int id, String message) : base(message)
		{
			setID(id);
		}

		/**
		 * Creates a StunException, setting an error message an error id and a cause.
		 * @param message an error message.
		 * @param id an error id.
		 * @param cause the error that caused this exception.
		 */
		public StunException(int id, String message, Exception cause) : base(message)
		{
			setID(id);
			this.cause = cause;
		}

		public virtual Exception GetCause() 
		{
			return this.cause;
		}



		/**
		 * Creates a StunException, setting an error message and a cause object.
		 * @param message an error message.
		 * @param cause the error object that caused this exception.
		 */
		//public StunException(String message, Throwable cause)
		//{
		//    super(message, cause);
		//}

		/**
		 * Creates an exception, setting the Throwable object, that caused it.
		 * @param cause the error that caused this exception.
		 */
		//public StunException(Throwable cause)
		//{
		//    super(cause);
		//}

		/**
		 * Sets the identifier of the error that caused the exception.
		 * @param id the identifier of the error that caused the exception.
		 */
		public void setID(int id)
		{
			this.id = id;
		}

		/**
		 * Returns this exception's identifier.
		 * @return this exception's identifier;
		 */
		public int getID()
		{
			return id;
		}

	}
}