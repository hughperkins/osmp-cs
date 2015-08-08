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
	 * This class  provides factory methods to allow an application to create
	 * STUN Messages from a particular implementation
	 *
	 * <p>Organisation: Louis Pasteur University, Strasbourg, France</p>
	 *                   <p>Network Research Team (http://www-r2.u-strasbg.fr)</p></p>
	 * @author Emil Ivov
	 * @version 0.1
	 */

	public class MessageFactory
	{
		/**
		 * Creates a default binding request. The request contains a ChangeReqeust
		 * attribute with zero change ip and change port flags.
		 * @return a default binding request.
		 */
		public static Request CreateBindingRequest()
		{
			Request bindingRequest = new Request();
			try
			{
				bindingRequest.SetMessageType(Message.BINDING_REQUEST);
			}
			catch (StunException ex)
			{
				//there should be no exc here since we're the creators.
				Console.WriteLine("Exception {0} {1}", ex.Message, ex.StackTrace);
			}

			//add a change request attribute
			ChangeRequestAttribute attribute
				= AttributeFactory.CreateChangeRequestAttribute();

			bindingRequest.AddAttribute(attribute);

			return bindingRequest;
		}

		/**
		 * Creates a BindingResponse assigning the specified values to mandatory
		 * headers.
		 *
		 * @param mappedAddress     the address to assign the mappedAddressAttribute
		 * @param sourceAddress     the address to assign the sourceAddressAttribute
		 * @param changedAddress    the address to assign the changedAddressAttribute
		 * @return a BindingResponse assigning the specified values to mandatory
		 *         headers.
		 * @throws StunException ILLEGAL_ARGUMENT
		 */
		public static Response CreateBindingResponse(StunAddress mappedAddress,
			StunAddress sourceAddress,
			StunAddress changedAddress)
		{
			Response bindingResponse = new Response();
			bindingResponse.SetMessageType(Message.BINDING_RESPONSE);

			//mapped address
			MappedAddressAttribute mappedAddressAttribute =
				AttributeFactory.CreateMappedAddressAttribute(mappedAddress);

			//source address
			SourceAddressAttribute sourceAddressAttribute =
				AttributeFactory.CreateSourceAddressAttribute(sourceAddress);

			//changed address
			ChangedAddressAttribute changedAddressAttribute =
				AttributeFactory.CreateChangedAddressAttribute(changedAddress);

			bindingResponse.AddAttribute(mappedAddressAttribute);
			bindingResponse.AddAttribute(sourceAddressAttribute);
			bindingResponse.AddAttribute(changedAddressAttribute);

			return bindingResponse;
		}


		/**
		 * Creates a binding error response according to the specified error code
		 * and unknown attributes.
		 *
		 * @param errorCode the error code to encapsulate in this message
		 * @param reasonPhrase a human readable description of the error
		 * @param unknownAttributes a char[] array containing the ids of one or more
		 * attributes that had not been recognized.
		 * @throws StunException INVALID_ARGUMENTS if one or more of the given
		 * parameters had an invalid value.
		 *
		 * @return a binding error response message containing an error code and a
		 * UNKNOWN-ATTRIBUTES header
		 */
		private static Response CreateBindingErrorResponse(int errorCode,
			String reasonPhrase,
			int[] unknownAttributes)
		{
			Response bindingErrorResponse = new Response();
			bindingErrorResponse.SetMessageType(Message.BINDING_ERROR_RESPONSE);

			//init attributes
			UnknownAttributesAttribute unknownAttributesAttribute = null;
			ErrorCodeAttribute errorCodeAttribute =
				AttributeFactory.CreateErrorCodeAttribute(errorCode,reasonPhrase);

			bindingErrorResponse.AddAttribute(errorCodeAttribute);

			if(unknownAttributes != null)
			{
				unknownAttributesAttribute = AttributeFactory.
					CreateUnknownAttributesAttribute();
				for (int i = 0; i < unknownAttributes.Length; i++)
				{
					unknownAttributesAttribute.AddAttributeID(unknownAttributes[i]);
				}
				bindingErrorResponse.AddAttribute(unknownAttributesAttribute);
			}

			return bindingErrorResponse;
		}

		/**
		 * Creates a binding error response with UNKNOWN_ATTRIBUTES error code
		 * and the specified unknown attributes.
		 *
		 * @param unknownAttributes a char[] array containing the ids of one or more
		 *  attributes that had not been recognized.
		 * @throws StunException INVALID_ARGUMENTS if one or more of the given
		 * parameters had an invalid value.
		 * @return a binding error response message containing an error code and a
		 * UNKNOWN-ATTRIBUTES header
		 */
		public static Response CreateBindingErrorResponseUnknownAttributes(
			int[] unknownAttributes)
		{
			return CreateBindingErrorResponse(ErrorCodeAttribute.UNKNOWN_ATTRIBUTE,
				null,
				unknownAttributes);
		}

		/**
		 * Creates a binding error response with UNKNOWN_ATTRIBUTES error code
		 * and the specified unknown attributes and reason phrase.
		 *
		 * @param reasonPhrase a short description of the error.
		 * @param unknownAttributes a char[] array containing the ids of one or more
		 *  attributes that had not been recognized.
		 * @throws StunException INVALID_ARGUMENTS if one or more of the given
		 * parameters had an invalid value.
		 * @return a binding error response message containing an error code and a
		 * UNKNOWN-ATTRIBUTES header
		 */
		public static Response CreateBindingErrorResponseUnknownAttributes(
			String reasonPhrase,
			int[] unknownAttributes)
		{
			return CreateBindingErrorResponse(ErrorCodeAttribute.UNKNOWN_ATTRIBUTE,
				reasonPhrase,
				unknownAttributes);
		}


		/**
		 * Creates a binding error response with an ERROR-CODE attribute.
		 *
		 * @param errorCode the error code to encapsulate in this message
		 * @param reasonPhrase a human readable description of the error.
		 * @throws StunException INVALID_ARGUMENTS if one or more of the given
		 * parameters had an invalid value.
		 *
		 * @return a binding error response message containing an error code and a
		 * UNKNOWN-ATTRIBUTES header
		 */
		public static Response CreateBindingErrorResponse(int errorCode,
			String reasonPhrase)
		{
			return CreateBindingErrorResponse(errorCode, reasonPhrase, null);
		}

		/**
		* Creates a binding error response according to the specified error code.
		*
		* @param errorCode the error code to encapsulate in this message
		* attributes that had not been recognized.
		* @throws StunException INVALID_ARGUMENTS if one or more of the given
		* parameters had an invalid value.
		*
		* @return a binding error response message containing an error code and a
		* UNKNOWN-ATTRIBUTES header
		*/
		public static Response CreateBindingErrorResponse(int errorCode) 
		{
			return CreateBindingErrorResponse(errorCode, null, null);
		}

		//======================== NOT CURRENTLY SUPPORTED =============================
		public static Request CreateShareSecretRequest()
		{
			throw new Exception(
				"Shared Secret Support is not currently implemented");
		}

		public static Response CreateSharedSecretResponse()
		{
			throw new Exception(
				"Shared Secret Support is not currently implemented");
		}

		public static Response CreateSharedSecretErrorResponse()
		{
			throw new Exception(
				"Shared Secret Support is not currently implemented");
		}

	}
}