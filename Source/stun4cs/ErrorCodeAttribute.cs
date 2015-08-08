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
	 * The ERROR-CODE attribute is present in the Binding Error Response and
	 * Shared Secret Error Response.  It is a numeric value in the range of
	 * 100 to 699 plus a textual reason phrase encoded in UTF-8, and is
	 * consistent in its code assignments and semantics with SIP [10] and
	 * HTTP [15].  The reason phrase is meant for user consumption, and can
	 * be anything appropriate for the response code.  The lengths of the
	 * reason phrases MUST be a multiple of 4 (measured in bytes).  This can
	 * be accomplished by added spaces to the end of the text, if necessary.
	 * Recommended reason phrases for the defined response codes are
	 * presented below.
	 *
	 * To facilitate processing, the class of the error code (the hundreds
	 * digit) is encoded separately from the rest of the code.
	 *
	 *   0                   1                   2                   3
	 *   0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
	 *  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
	 *  |                   0                     |Class|     Number    |
	 *  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
	 *  |      Reason Phrase (variable)                                ..
	 *  +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
	 *
	 * The class represents the hundreds digit of the response code.  The
	 * value MUST be between 1 and 6.  The number represents the response
	 * code modulo 100, and its value MUST be between 0 and 99.
	 *
	 * The following response codes, along with their recommended reason
	 * phrases (in brackets) are defined at this time:
	 *
	 * 400 (Bad Request): The request was malformed.  The client should not
	 *      retry the request without modification from the previous
	 *      attempt.
	 *
	 * 401 (Unauthorized): The Binding Request did not contain a MESSAGE-
	 *      INTEGRITY attribute.
	 *
	 * 420 (Unknown Attribute): The server did not understand a mandatory
	 *      attribute in the request.
	 *
	 * 430 (Stale Credentials): The Binding Request did contain a MESSAGE-
	 *      INTEGRITY attribute, but it used a shared secret that has
	 *      expired.  The client should obtain a new shared secret and try
	 *      again.
	 *
	 * 431 (Integrity Check Failure): The Binding Request contained a
	 *      MESSAGE-INTEGRITY attribute, but the HMAC failed verification.
	 *      This could be a sign of a potential attack, or client
	 *      implementation error.
	 *
	 * 432 (Missing Username): The Binding Request contained a MESSAGE-
	 *      INTEGRITY attribute, but not a USERNAME attribute.  Both must be
	 *      present for integrity checks.
	 *
	 * 433 (Use TLS): The Shared Secret request has to be sent over TLS, but
	 *      was not received over TLS.
	 *
	 * 500 (Server Error): The server has suffered a temporary error. The
	 *      client should try again.
	 *
	 * 600 (Global Failure:) The server is refusing to fulfill the request.
	 *      The client should not retry.
	 *
	 * <p>Copyright: Copyright (c) 2003</p>
	 * <p>Organisation: Louis Pasteur University, Strasbourg, France</p>
	 * <p>Network Research Team (http://www-r2.u-strasbg.fr)</p></p>
	 * @author Emil Ivov
	 * @version 0.1
	 */
	public class ErrorCodeAttribute : Attribute
	{
		// Common error codes
		public const int BAD_REQUEST   = 400;
		public const int UNAUTHORIZED  = 401;
		public const int UNKNOWN_ATTRIBUTE = 420;
		public const int STALE_CREDENTIALS = 430;
		public const int INTEGRITY_CHECK_FAILURE = 431;
		public const int MISSING_USERNAME = 432;
		public const int USE_TLS = 433;
		public const int SERVER_ERROR = 500;
		public const int GLOBAL_FAILURE = 600;


		/**
		 * The class represents the hundreds digit of the response code.  The
		 * value MUST be between 1 and 6.
		 */
		private int errorClass = 0;

		/**
		 * The number represents the response
		 * code modulo 100, and its value MUST be between 0 and 99.
		 */
		private int errorNumber = 0;

		/**
		 * The reason phrase is meant for user consumption, and can
		 * be anything appropriate for the response code.  The lengths of the
		 * reason phrases MUST be a multiple of 4 (measured in bytes).
		 */
		private String reasonPhrase = null;

		/**
		 * Constructs a new ERROR-CODE attribute
		 */
		public ErrorCodeAttribute() : base(ERROR_CODE)
		{
		}

		/**
		 * A convenience method that sets error class and number according to the
		 * specified errorCode.The class represents the hundreds digit of the error code.
		 * The value MUST be between 1 and 6.  The number represents the response
		 * code modulo 100, and its value MUST be between 0 and 99.
		 *
		 * @param errorCode the errorCode that this class encapsulates.
		 * @throws StunException if errorCode is not a valid error code.
		 */
		public virtual void SetErrorCode(int errorCode)
		{
			SetErrorClass((errorCode/100));
			SetErrorNumber((errorCode % 100));
		}

		/**
		 * A convenience method that constructs an error code from this Attribute's
		 * class and number.
		 * @return the code of the error this attribute represents.
		 */
		public virtual int GetErrorCode()
		{
			return (GetErrorClass()*100 + GetErrorNumber());
		}


		/**
		 * Sets this attribute's error number.
		 * @param errorNumber the error number to assign this attribute.
		 * @throws StunException if errorNumber is not a valid error number.
		 */
		public virtual void SetErrorNumber(int errorNumber)
		{
			if(errorNumber < 0 || errorNumber > 99)
				throw new StunException(StunException.ILLEGAL_ARGUMENT,
					errorNumber + " is not a valid error number!");
			this.errorNumber = errorNumber;
		}

		/**
		 * Returns this attribute's error number.
		 * @return  this attribute's error number.
		 */
		public virtual int GetErrorNumber()
		{
			return this.errorNumber;
		}

		/**
		 * Sets this error's error class.
		 * @param errorClass this error's error class.
		 * @throws StunException if errorClass is not a valid error class.
		 */
		public virtual void SetErrorClass(int errorClass)
		{
			if(errorClass < 0 || errorClass > 99)
				throw new StunException(StunException.ILLEGAL_ARGUMENT,
					errorClass + " is not a valid error number!");
			this.errorClass = errorClass;
		}

		/**
		 * Returns this error's error class.
		 * @return this error's error class.
		 */
		public virtual int GetErrorClass()
		{
			return errorClass;
		}


		/**
		 * Returns a default reason phrase corresponding to the specified error
		 * code, as described by rfc 3489.
		 * @param errorCode the code of the error that the reason phrase must
		 *                  describe.
		 * @return a default reason phrase corresponding to the specified error
		 * code, as described by rfc 3489.
		 */
		public static string GetDefaultReasonPhrase(int errorCode)
		{
			switch(errorCode)
			{
				case 400: return  "(Bad Request): The request was malformed.  The client should not "
							  +"retry the request without modification from the previous attempt.";
				case 401: return  "(Unauthorized): The Binding Request did not contain a MESSAGE-"
							  +"INTEGRITY attribute.";
				case 420: return  "(Unknown Attribute): The server did not understand a mandatory "
							  +"attribute in the request.";
				case 430: return  "(Stale Credentials): The Binding Request did contain a MESSAGE-"
							  +"INTEGRITY attribute, but it used a shared secret that has "
							  +"expired.  The client should obtain a new shared secret and try"
							  +"again";
				case 431: return  "(Integrity Check Failure): The Binding Request contained a "
							  +"MESSAGE-INTEGRITY attribute, but the HMAC failed verification. "
							  +"This could be a sign of a potential attack, or client "
							  +"implementation error.";
				case 432: return  "(Missing Username): The Binding Request contained a MESSAGE-"
							  +"INTEGRITY attribute, but not a USERNAME attribute.  Both must be"
							  +"present for integrity checks.";
				case 433: return  "(Use TLS): The Shared Secret request has to be sent over TLS, but"
							  +"was not received over TLS.";
				case 500: return  "(Server Error): The server has suffered a temporary error. The"
							  +"client should try again.";
				case 600: return "(Global Failure:) The server is refusing to fulfill the request."
							  +"The client should not retry.";

				default:  return "Unknown Error";
			}
		}

		/**
		 * Set's a reason phrase. The reason phrase is meant for user consumption,
		 * and can be anything appropriate for the response code.  The lengths of
		 * the reason phrases MUST be a multiple of 4 (measured in bytes).
		 *
		 * @param reasonPhrase a reason phrase that describes this error.
		 */
		public virtual void SetReasonPhrase(string reasonPhrase)
		{
			this.reasonPhrase = reasonPhrase;
		}

		/**
		 * Returns the reason phrase. The reason phrase is meant for user consumption,
		 * and can be anything appropriate for the response code.  The lengths of
		 * the reason phrases MUST be a multiple of 4 (measured in bytes).
		 *
		 * @return reasonPhrase a reason phrase that describes this error.
		 */
		public virtual string GetReasonPhrase()
		{
			return this.reasonPhrase;
		}

		/**
		 * Returns the human readable name of this attribute. Attribute names do
		 * not really matter from the protocol point of view. They are only used
		 * for debugging and readability.
		 * @return this attribute's name.
		 */
		public override string GetName()
		{
			return NAME;
		}

		public const string NAME = "ERROR-CODE";

		/**
		 * Returns the length of this attribute's body.
		 * @return the length of this attribute's value.
		 */
		public override int GetDataLength()
		{
			int len = (int)( 4 //error code numbers
				+ (int)(
				reasonPhrase == null? 0
				:reasonPhrase.Length *2
				));

			/*
			 * According to rfc 3489 The length of the
			 * reason phrases MUST be a multiple of 4 (measured in bytes)
			 */
			len += 4 - (len%4);
			return len;
		}

		/**
		 * Returns a binary representation of this attribute.
		 * @return a binary representation of this attribute.
		 */
		public override byte[] Encode()
		{
			byte[] binValue =  new byte[HEADER_LENGTH + GetDataLength()];

			//Type
			binValue[0] = (byte) (GetAttributeType() >> 8);
			binValue[1] = (byte) (GetAttributeType() & 0x00FF);
			//Length
			binValue[2] = (byte) (GetDataLength() >> 8);
			binValue[3] = (byte) (GetDataLength() & 0x00FF);

			//Not used
			binValue[4] = 0x00;
			binValue[5] = 0x00;

			//Error code
			binValue[6] = (byte) (GetErrorClass() & 0xff);
			binValue[7] = (byte) (GetErrorNumber() &0xff);

			int offset = 8;
			char[] chars = reasonPhrase.ToCharArray();
			for (int i = 0; i < reasonPhrase.Length; i++, offset += 2) 
			{
				binValue[offset]   = (byte)(chars[i]>>8);
				binValue[offset+1] = (byte)(chars[i] & 0xFF);
			}

			//The lengths of the reason phrases MUST be a multiple of 4 (measured
			//in bytes)
			if( reasonPhrase.Length%4 != 0)
			{
				binValue[binValue.Length - 2] = (byte) ( ( (int) ' ') >> 8);
				binValue[binValue.Length - 1] = (byte) ( ( (int) ' ') & 0x00FF);
			}

			return binValue;
		}

		/**
		 * Compares two STUN Attributes. Attributeas are considered equal when their
		 * type, length, and all data are the same.
		 *
		 * @param obj the object to compare this attribute with.
		 * @return true if the attributes are equal and false otherwise.
		 */
		public override bool Equals(Object obj)
		{
			if (! (obj is ErrorCodeAttribute)
				|| obj == null)
				return false;

			if (obj == this)
				return true;

			ErrorCodeAttribute att = (ErrorCodeAttribute) obj;
			if (att.GetAttributeType() != GetAttributeType()
				|| att.GetDataLength() != GetDataLength()
				//compare data
				|| att.GetErrorClass() != GetErrorClass()
				|| att.GetErrorNumber()!= GetErrorNumber()
				|| ( att.GetReasonPhrase() != null
				&& !att.GetReasonPhrase().Equals(GetReasonPhrase()))
				)
				return false;

			return true;
		}

		/**
		 * Sets this attribute's fields according to attributeValue array.
		 *
		 * @param attributeValue a binary array containing this attribute's field
		 *                       values and NOT containing the attribute header.
		 * @param offset the position where attribute values begin (most often
		 * 				 offset is equal to the index of the first byte after
		 * 				 length)
		 * @param length the length of the binary array.
		 * @throws StunException if attrubteValue contains invalid data.
		 */
		public override void DecodeAttributeBody(byte[] attributeValue, int offset, int length) 
		{

			offset += 2; //skip the 0s

			//Error code
			SetErrorClass(attributeValue[offset++]);
			SetErrorNumber(attributeValue[offset++]);

			//Reason Phrase
			char[] reasonPhrase = new char[(length-4)/2];

			for (int i = 0; i < reasonPhrase.Length; i++, offset+=2) 
			{
				reasonPhrase[i] =
					(char)(attributeValue[offset] | attributeValue[offset+1]);
			}
			SetReasonPhrase(new String(reasonPhrase).Trim());

		}


	}
}