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
	 * This class represents the STUN CHANGE-REQUEST attribute. The CHANGE-REQUEST
	 * attribute is used by the client to request that the server use a different
	 * address and/or port when sending the response.  The attribute is 32 bits
	 * long, although only two bits (A and B) are used:
	 *
	 * 0                   1                   2                   3           <br/>
	 * 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1         <br/>
	 * +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+       <br/>
	 * |0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 A B 0|       <br/>
	 * +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+       <br/>
	 *
	 * The meaning of the flags is:
	 *
	 * A: This is the "change IP" flag.  If true, it requests the server
	 *    to send the Binding Response with a different IP address than the
	 *    one the Binding Request was received on.
	 *
	 * B: This is the "change port" flag.  If true, it requests the
	 *    server to send the Binding Response with a different port than the
	 *    one the Binding Request was received on.
	 *
	 * <p>Copyright: Copyright (c) 2003</p>
	 * <p>Organisation: Louis Pasteur University, Strasbourg, France</p>
	 * <p>Network Research Team (http://www-r2.u-strasbg.fr)</p></p>
	 * @author Emil Ivov
	 * @version 0.1
	 */

	public class ChangeRequestAttribute : Attribute
	{
		/**
			 * This is the "change IP" flag.  If true, it requests the server
			 * to send the Binding Response with a different IP address than the
			 * one the Binding Request was received on.
			 */
		private bool changeIpFlag   = false;

		/**
		 * This is the "change port" flag.  If true, it requests the
		 * server to send the Binding Response with a different port than the
		 * one the Binding Request was received on.
		 */
		private bool changePortFlag = false;

		/**
		 * The length of the data contained by this attribute.
		 */
		public const int DATA_LENGTH = 4;


		/**
		 * Creates an empty ChangeRequestAttribute.
		 */
		public ChangeRequestAttribute() : base(CHANGE_REQUEST)
		{
		}

		/**
		 * Returns the human readable name of this attribute. Attribute names do
		 * not really matter from the protocol point of view. They are only used
		 * for debugging and readability.
		 * @return this attribute's name.
		 */
		public override String GetName()
		{
			return NAME;
		}

		public const string NAME = "CHANGE-REQUEST";

		/**
		 * Compares two STUN Attributes. Attributeas are considered equal when their
		 * type, length, and all data are the same.
		 *
		 * @param obj the object to compare this attribute with.
		 * @return true if the attributes are equal and false otherwise.
		 */
		public override bool Equals(Object obj)
		{
			if (! (obj is ChangeRequestAttribute)
				|| obj == null)
				return false;

			if (obj == this)
				return true;

			ChangeRequestAttribute att = (ChangeRequestAttribute) obj;
			if (att.GetAttributeType()   != GetAttributeType()
				|| att.GetDataLength()   != GetDataLength()
				//compare data
				|| att.GetChangeIpFlag() != GetChangeIpFlag()
				|| att.GetChangePortFlag()       != GetChangePortFlag()
				)
				return false;

			return true;
		}

		/**
		 * Returns the length of this attribute's body.
		 * @return the length of this attribute's value (8 bytes).
		 */
		public override int GetDataLength()
		{
			return DATA_LENGTH;
		}

		/**
		 * Returns a binary representation of this attribute.
		 * @return a binary representation of this attribute.
		 */
		public override byte[] Encode()
		{
			byte[] binValue = new byte[HEADER_LENGTH + DATA_LENGTH];

			//Type
			binValue[0] = (byte)(GetAttributeType()>>8);
			binValue[1] = (byte)(GetAttributeType()&0x00FF);
			//Length
			binValue[2] = (byte)(GetDataLength()>>8);
			binValue[3] = (byte)(GetDataLength()&0x00FF);
			//Data
			binValue[4] = 0x00;
			binValue[5] = 0x00;
			binValue[6] = 0x00;
			binValue[7] = (byte)((GetChangeIpFlag()?4:0) + (GetChangePortFlag()?2:0));

			return binValue;
		}


		//========================= set/get methods
		/**
		 * Sets the value of the changeIpFlag. The "change IP" flag,  if true,
		 * requests the server to send the Binding Response with a different IP
		 * address than the one the Binding Request was received on.
		 *
		 * @param changeIP the new value of the changeIpFlag.
		 */
		public virtual void SetChangeIpFlag(bool changeIP)
		{
			this.changeIpFlag = changeIP;
		}

		/**
		 * Returns tha value of the changeIpFlag. The "change IP" flag,  if true,
		 * requests the server to send the Binding Response with a different IP
		 * address than the one the Binding Request was received on.
		 *
		 * @return the value of the changeIpFlag.
		 */
		public virtual bool GetChangeIpFlag()
		{
			return changeIpFlag;
		}

		/**
		 * Sets the value of the changePortFlag. The "change port" flag.  If true,
		 * requests the server to send the Binding Response with a different port
		 * than the one the Binding Request was received on.
		 *
		 * @param changePort the new value of the changePort flag.
		 */
		public virtual void SetChangePortFlag(bool changePort)
		{
			this.changePortFlag = changePort;
		}

		/**
		 * Returns the value of the changePortFlag. The "change port" flag.  If true,
		 * requests the server to send the Binding Response with a different port
		 * than the one the Binding Request was received on.
		 *
		 * @return the value of the changePort flag.
		 */
		public virtual bool GetChangePortFlag()
		{
			return changePortFlag;
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
			offset += 3; // first three bytes of change req att are not used
			SetChangeIpFlag((attributeValue[offset]&4)>0);
			SetChangePortFlag((attributeValue[offset] & 0x2)>0);

		}


	}
}