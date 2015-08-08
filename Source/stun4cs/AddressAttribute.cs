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
	 * This class is used to represent Stun attributes that contain an address. Such
	 * attributes are:
	 *
	 * MAPPED-ADDRESS <br/>
	 * RESPONSE-ADDRESS <br/>
	 * SOURCE-ADDRESS <br/>
	 * CHANGED-ADDRESS <br/>
	 * REFLECTED-FROM <br/>
	 *
	 * The different attributes are distinguished by the attributeType of
	 * net.java.stun4j.attribute.Attribute.
	 *
	 * Address attributes indicate the mapped IP address and
	 * port.  They consist of an eight bit address family, and a sixteen bit
	 * port, followed by a fixed length value representing the IP address.
	 *
	 *  0                   1                   2                   3          <br/>
	 *  0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1        <br/>
	 * +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+       <br/>
	 * |x x x x x x x x|    Family     |           Port                |       <br/>
	 * +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+       <br/>
	 * |                             Address                           |       <br/>
	 * +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+       <br/>
	 *                                                                         <br/>
	 * The port is a network byte ordered representation of the mapped port.
	 * The address family is always 0x01, corresponding to IPv4.  The first
	 * 8 bits of the MAPPED-ADDRESS are ignored, for the purposes of
	 * aligning parameters on natural boundaries.  The IPv4 address is 32
	 * bits.
	 *
	 *
	 * <p>Organisation: Louis Pasteur University, Strasbourg, France</p>
	 * <p>Network Research Team (http://www-r2.u-strasbg.fr)</p></p>
	 * @author Emil Ivov
	 * @version 0.1
	 */
	public abstract class AddressAttribute : Attribute
	{

		/**
		 * The family of the address contained by this attribute. The address family
		 * is always 0x01, corresponding to IPv4.
		 */
		const byte family = 0x01;

		/**
		 * The address represented by this message;
		 */
		protected StunAddress address = null;

		/**
		 * The length of the data contained by this attribute.
		 */
		public const int DATA_LENGTH = 8;

		/**
		 * Constructs an address attribute with the specified type.
		 *
		 * @param attributeType the type of the address attribute.
		 */
		public AddressAttribute(int attributeType) : base(attributeType)
		{
			// super(attributeType);
		}
		/**
		 * Verifies that type is a valid address attribute type.
		 * @param type the type to test
		 * @return true if the type is a valid address attribute type and false
		 * otherwise
		 */
		private bool IsTypeValid(int type)
		{
			return (type == MAPPED_ADDRESS || type == RESPONSE_ADDRESS
				|| type == SOURCE_ADDRESS || type == CHANGED_ADDRESS
				|| type == REFLECTED_FROM);

		}

		/**
		 * Sets it as this attribute's type.
		 *
		 * @param type the new type of the attribute.
		 */
		public override void SetAttributeType(int  type)
		{
			if (!this.IsTypeValid(type))
				throw new StunException(((int)type) + "is not a valid address attribute!");

			base.SetAttributeType(type);
		}

		/**
		 * Returns the human readable name of this attribute. Attribute names do
		 * not really matter from the protocol point of view. They are only used
		 * for debugging and readability.
		 * @return this attribute's name.
		 */
		public override string GetName()
		{
			switch(GetAttributeType())
			{
				case MAPPED_ADDRESS:   return MappedAddressAttribute.NAME;
				case RESPONSE_ADDRESS: return ResponseAddressAttribute.NAME;
				case SOURCE_ADDRESS:   return SourceAddressAttribute.NAME;
				case CHANGED_ADDRESS:  return ChangedAddressAttribute.NAME;
				case REFLECTED_FROM:   return ReflectedFromAttribute.NAME;
			}

			return "UNKNOWN MESSAGE";
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
			if (! (obj is AddressAttribute)
				|| obj == null)
				return false;

			if (obj == this)
				return true;

			AddressAttribute att = (AddressAttribute) obj;
			if (att.GetAttributeType() != GetAttributeType()
				|| att.GetDataLength() != GetDataLength()
				//compare data
				|| att.GetFamily()     != GetFamily()
				|| (att.GetAddress()   != null
				&& !address.Equals(att.GetAddress()))
				)
				return false;

			//addresses
			if( att.GetAddress() == null && GetAddress() == null)
				return true;

			return true;
		}

		/**
		 * Returns the length of this attribute's body.
		 * @return the length of this attribute's value (8 bytes).
		 */
		public override int GetDataLength()
		{
			return AddressAttribute.DATA_LENGTH;
		}

		/**
		 * Returns a binary representation of this attribute.
		 * @return a binary representation of this attribute.
		 */
		public override byte[] Encode()
		{
			int type = GetAttributeType();
			if (!IsTypeValid(type))
				throw new StunException(((int)type) + "is not a valid address attribute!");
			byte[] binValue = new byte[HEADER_LENGTH + DATA_LENGTH];

			//Type
			binValue[0] = (byte)((type>>8)&0x00FF);
			binValue[1] = (byte)(type&0x00FF);
			//Length
			binValue[2] = (byte)((GetDataLength()>>8)&0x00FF);
			binValue[3] = (byte)(GetDataLength()&0x00FF);
			//Not used
			binValue[4] = 0x00;
			//Family
			binValue[5] = GetFamily();
			//port
			binValue[6] = (byte)((GetPort()>>8)&0x00FF);
			binValue[7] = (byte)(GetPort()&0x00FF);
			//address
			binValue[8]  = GetAddressBytes()[0];
			binValue[9]  = GetAddressBytes()[1];
			binValue[10] = GetAddressBytes()[2];
			binValue[11] = GetAddressBytes()[3];

			return binValue;
		}

		/**
		 * Sets address to be the address transported by this attribute.
		 * @param address that this attribute should encapsulate.
		 */
		public virtual void SetAddress(StunAddress address)
		{
			this.address = address;
		}

		/**
		 * Returns the address encapsulated by this attribute.
		 * @return the address encapsulated by this attribute.
		 */
		public virtual StunAddress GetAddress()
		{
			return address;
		}

		public virtual byte[] GetAddressBytes()
		{
			return address.GetAddressBytes();
		}

		/**
		 * Returns the family that the this.address belongs to.
		 * @return the family that the this.address belongs to.
		 */
		public virtual byte GetFamily()
		{
			return family;
		}

		/**
		 * Returns the port associated with the address contained by the attribute.
		 * @return the port associated with the address contained by the attribute.
		 */
		public virtual int GetPort()
		{
			return address.GetPort();
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
			//skip through family and padding
			offset += 2;
			//port
			int port = ((int)(((attributeValue[offset++] << 8 ) & 0xff00) | (attributeValue[offset++]&0xFF) ));

			//address
			byte[] address = ( new byte[]
				{
					attributeValue[offset++], attributeValue[offset++],
					attributeValue[offset++], attributeValue[offset++]});

			SetAddress(new StunAddress(address, port));
		}

	}
}