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
	 * After the header are 0 or more attributes.  Each attribute is TLV
	 * encoded, with a 16 bit type, 16 bit length, and variable value:
	 *
	 *     0                   1                   2                   3       <br/>
	 *     0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1     <br/>
	 *    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+    <br/>
	 *    |         Type                  |            Length             |    <br/>
	 *    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+    <br/>
	 *    |                             Value                             .... <br/>
	 *    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+    <br/>
	 *<br/>
	 *    The following types are defined:<br/>
	 *<br/>
	 *    0x0001: MAPPED-ADDRESS                                               <br/>
	 *    0x0002: RESPONSE-ADDRESS                                             <br/>
	 *    0x0003: CHANGE-REQUEST                                               <br/>
	 *    0x0004: SOURCE-ADDRESS                                               <br/>
	 *    0x0005: CHANGED-ADDRESS                                              <br/>
	 *    0x0006: USERNAME                                                     <br/>
	 *    0x0007: PASSWORD                                                     <br/>
	 *    0x0008: MESSAGE-INTEGRITY                                            <br/>
	 *    0x0009: ERROR-CODE                                                   <br/>
	 *    0x000a: UNKNOWN-ATTRIBUTES                                           <br/>
	 *    0x000b: REFLECTED-FROM                                               <br/>
	 *                                                                         <br/>
	 * <p>Copyright: Copyright (c) 2003</p>
	 * <p>Organisation: <p> Organisation: Louis Pasteur University, Strasbourg, France</p>
	 * 					<p> Network Research Team (http://www-r2.u-strasbg.fr)</p></p>
	 * @author Emil Ivov
	 * @version 0.1
	 */
	public abstract class Attribute
	{
		public const int MAPPED_ADDRESS = 0x0001;
		public const int RESPONSE_ADDRESS = 0x0002;
		public const int CHANGE_REQUEST = 0x0003;
		public const int SOURCE_ADDRESS = 0x0004;
		public const int CHANGED_ADDRESS = 0x0005;
		public const int USERNAME = 0x0006;
		public const int PASSWORD = 0x0007;
		public const int MESSAGE_INTEGRITY = 0x0008;
		public const int ERROR_CODE = 0x0009;
		public const int UNKNOWN_ATTRIBUTES = 0x000a;
		public const int REFLECTED_FROM = 0x000b;

		/**
		 * The type of the attribute.
		 */
		private int attributeType = 0;

		/**
		 * The size of an atribute header in bytes = len(TYPE) + len(LENGTH) = 4
		 */
		public const int HEADER_LENGTH = 4;

		/**
		 * Creates an empty STUN message attribute.
		 *
		 * @param attributeType the type of the attribute.
		 */
		protected Attribute(int attributeType)
		{
			SetAttributeType(attributeType);
		}

		/**
		 * Returns the length of this attribute's body.
		 * @return the length of this attribute's value.
		 */
		public abstract int GetDataLength();

		/**
		 * Returns the human readable name of this attribute. Attribute names do
		 * not really matter from the protocol point of view. They are only used
		 * for debugging and readability.
		 * @return this attribute's name.
		 */
		public abstract String GetName();

		/**
		 * Returns the attribute's type.
		 * @return the type of this attribute.
		 */
		public int GetAttributeType()
		{
			return attributeType;
		}

		/**
		 * Sets the attribute's type.
		 * @param type the new type of this attribute
		 */
		public virtual void SetAttributeType(int type)
		{
			this.attributeType = type;
		}

		/**
		 * Compares two STUN Attributes. 2 Attributes are considered equal when they
		 * have the same type length and value.
		 *
		 * @param obj the object to compare this attribute with.
		 * @return true if the attributes are equal and false otherwise.
		 */

		// public abstract bool Equals(Object obj);

		/**
		 * Returns a binary representation of this attribute.
		 * @return a binary representation of this attribute.
		 */
		public abstract byte[] Encode();

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
		public abstract void DecodeAttributeBody(byte[] attributeValue, int offset, int length);

	}
}