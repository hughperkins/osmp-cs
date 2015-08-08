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
	 *
	 * This class  provides factory methods to allow an application to create
	 * STUN Attributes from a particular implementation
	 *
	 * <p>Copyright: Copyright (c) 2003</p>
	 * <p>Organisation: Louis Pasteur University, Strasbourg, France</p>
	 * <p>Network Research Team (http://www-r2.u-strasbg.fr)</p></p>
	 * @author Emil Ivov
	 * @version 0.1
	 */

	public class AttributeFactory
	{
		private AttributeFactory()
		{
		}

		//------------------------------------ CHANGE REQUEST --------------------------

		/**
		 * Creates a ChangeRequestAttribute with "false" values for the changeIP and
		 * changePort flags.
		 * @return the newly created ChangeRequestAttribute.
		 */
		public static ChangeRequestAttribute
			CreateChangeRequestAttribute()
		{
			return CreateChangeRequestAttribute(false, false);
		}

		/**
		 * Creates a ChangeRequestAttribute with the specified flag values.
		 * @param changeIP   the value of the changeIP flag.
		 * @param changePort the value of the changePort flag.
		 * @return the newly created ChangeRequestAttribute.
		 */
		public static ChangeRequestAttribute
			CreateChangeRequestAttribute(bool changeIP,
			bool changePort)
		{
			ChangeRequestAttribute attribute = new ChangeRequestAttribute();

			attribute.SetChangeIpFlag(changeIP);
			attribute.SetChangePortFlag(changePort);

			return attribute;
		}

		//------------------------------------ CHANGED ADDRESS -------------------------

		/**
		 * Creates a changedAddressAttribute of the specified type and with the
		 * specified address and port
		 * @param address the address value of the address attribute
		 * @return the newly created address attribute.
		 */
		public static ChangedAddressAttribute
			CreateChangedAddressAttribute(StunAddress address)
		{
			ChangedAddressAttribute attribute= new ChangedAddressAttribute();

			attribute.SetAddress(address);

			return attribute;

		}

		//------------------------------------ ERROR CODE ------------------------------
		/**
		 * Creates an ErrorCodeAttribute with the specified error class and number
		 * and a default reason phrase.
		 * @param errorClass a valid error class.
		 * @param errorNumber a valid error number.
		 * @return the newly created attribute.
		 * @throws StunException if the error class or number have invalid values
		 * according to rfc3489.
		 */
		public static ErrorCodeAttribute CreateErrorCodeAttribute(
			int errorClass,
			int errorNumber
			)
			
		{
			return CreateErrorCodeAttribute(errorClass, errorNumber, null);
		}

		/**
		 * Creates an ErrorCodeAttribute with the specified error class, number and
		 * reason phrase.
		 * @param errorClass a valid error class.
		 * @param errorNumber a valid error number.
		 * @param reasonPhrase a human readable reason phrase. A null reason phrase
		 *                     would be replaced (if possible) by a default one
		 *                     as defined byte the rfc3489.
		 * @return the newly created attribute.
		 * @throws StunException if the error class or number have invalid values
		 * according to rfc3489.
		 */
		public static ErrorCodeAttribute CreateErrorCodeAttribute(
			int errorClass,
			int errorNumber,
			String reasonPhrase
			)
		{
			ErrorCodeAttribute attribute = new ErrorCodeAttribute();

			attribute.SetErrorClass(errorClass);
			attribute.SetErrorNumber(errorNumber);

			attribute.SetReasonPhrase(reasonPhrase==null?
				ErrorCodeAttribute.GetDefaultReasonPhrase(attribute.GetErrorCode())
				:reasonPhrase);

			return attribute;
		}



		/**
		 * Creates an ErrorCodeAttribute with the specified error code and a default
		 * reason phrase.
		 * @param errorCode a valid error code.
		 * @return the newly created attribute.
		 * @throws StunException if errorCode is not a valid error code as defined
		 * by rfc3489
		 */
		public static ErrorCodeAttribute CreateErrorCodeAttribute(int errorCode)
		{
			return CreateErrorCodeAttribute(errorCode, null);
		}

		/**
		 * Creates an ErrorCodeAttribute with the specified error code and reason
		 * phrase.
		 * @param errorCode a valid error code.
		 * @param reasonPhrase a human readable reason phrase. A null reason phrase
		 *                     would be replaced (if possible) by a default one
		 *                     as defined byte the rfc3489.

		 * @return the newly created attribute.
		 * @throws StunException if errorCode is not a valid error code as defined
		 * by rfc3489
		 */
		public static ErrorCodeAttribute CreateErrorCodeAttribute(
			int errorCode,
			String reasonPhrase)
		{
			ErrorCodeAttribute attribute = new ErrorCodeAttribute();

			attribute.SetErrorCode(errorCode);
			attribute.SetReasonPhrase(reasonPhrase==null?
				ErrorCodeAttribute.GetDefaultReasonPhrase(attribute.GetErrorCode())
				:reasonPhrase);


			return attribute;
		}

		//------------------------------------ MAPPED ADDRESS --------------------------

		/**
		 * Creates a MappedAddressAttribute of the specified type and with the
		 * specified address and port
		 * @param address the address value of the address attribute
		 * @return the newly created address attribute.
		 */
		public static MappedAddressAttribute CreateMappedAddressAttribute(
			StunAddress address)
		{
			MappedAddressAttribute attribute = new MappedAddressAttribute();

			attribute.SetAddress(address);

			return attribute;

		}

		//------------------------------------ REFLECTED FROM --------------------------

		/**
		 * Creates a ReflectedFromAddressAttribute of the specified type and with
		 * the specified address and port
		 * @param address the address value of the address attribute
		 * @return the newly created address attribute.
		 */
		public static ReflectedFromAttribute CreateReflectedFromAttribute(
			StunAddress address)
		{
			ReflectedFromAttribute attribute = new ReflectedFromAttribute();

			attribute.SetAddress(address);

			return attribute;

		}

		//------------------------------------ RESPONSE ADRESS -------------------------
		/**
		 * Creates a ResponseFromAddressAttribute of the specified type and with
		 * the specified address and port
		 * @param address the address value of the address attribute
		 * @return the newly created address attribute.
		 */
		public static ResponseAddressAttribute CreateResponseAddressAttribute(
			StunAddress address)
		{
			ResponseAddressAttribute attribute = new ResponseAddressAttribute();

			attribute.SetAddress(address);

			return attribute;

		}

		//------------------------------------ SOURCE ADDRESS --------------------------
		/**
		 * Creates a SourceFromAddressAttribute of the specified type and with
		 * the specified address and port
		 * @param address the address value of the address attribute
		 * @return the newly created address attribute.
		 */
		public static SourceAddressAttribute CreateSourceAddressAttribute(
			StunAddress address)
		{
			SourceAddressAttribute attribute = new SourceAddressAttribute();

			attribute.SetAddress(address);

			return attribute;

		}

		//------------------------------------ UNKNOWN ATTRIBUTES ----------------------
		/**
		 * Creates an empty UnknownAttributesAttribute.
		 * @return the newly created UnknownAttributesAttribute
		 */
		public static UnknownAttributesAttribute CreateUnknownAttributesAttribute()
		{
			UnknownAttributesAttribute attribute = new UnknownAttributesAttribute();

			return attribute;
		}

	}
}