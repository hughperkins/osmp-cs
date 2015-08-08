using System;
using System.Collections;

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
	 * This class represents a STUN message. STUN messages are TLV (type-length-value)
	 * encoded using big endian (network ordered) binary.  All STUN messages start
	 * with a STUN header, followed by a STUN payload.  The payload is a series of
	 * STUN attributes, the set of which depends on the message type.  The STUN
	 * header contains a STUN message type, transaction ID, and length.
	 *
	 * <p>Organisation: Louis Pasteur University, Strasbourg, France</p>
	 * 					<p>Network Research Team (http://www-r2.u-strasbg.fr)</p>
	 * @author Emil Ivov
	 * @version 0.1
	 */

	public abstract class Message
	{
		public const int BINDING_REQUEST               = 0x0001;
		public const int BINDING_RESPONSE              = 0x0101;
		public const int BINDING_ERROR_RESPONSE        = 0x0111;
		public const int SHARED_SECRET_REQUEST         = 0x0002;
		public const int SHARED_SECRET_RESPONSE        = 0x0102;
		public const int SHARED_SECRET_ERROR_RESPONSE  = 0x0112;

		//Message fields
		/**
		 * The length of Stun Message Headers in byres
		 * = len(Type) + len(DataLength) + len(Transaction ID).
		 */
		public const byte HEADER_LENGTH = 20;

		/**
		 * Indicates the type of the message. The message type can be Binding Request,
		 * Binding Response, Binding Error Response, Shared Secret Request, Shared
		 * Secret Response, or Shared Secret Error Response.
		 */
		private int messageType = 0x0000;

		/**
		 * The transaction ID is used to correlate requests and responses.
		 */
		private byte[] transactionID = null;

		/**
		 * The length of the transaction id (in bytes).
		 */
		public const byte TRANSACTION_ID_LENGTH = 16;


		/**
		 * The list of attributes contained by the message. We are using a hastable
		 * rather than a uni-dimensional list, in order to facilitate attribute
		 * search (even though it introduces some redundancies). Order is important
		 * so we'll be using a LinkedHashMap
		 */
		//not sure this is the best solution but I'm trying to keep entry order
		// protected LinkedHashMap attributes = new LinkedHashMap();
		private ArrayList attributes = new ArrayList();

		/**
		 * Desribes which attributes are present in which messages.  An
		 * M indicates that inclusion of the attribute in the message is
		 * mandatory, O means its optional, C means it's conditional based on
		 * some other aspect of the message, and N/A means that the attribute is
		 * not applicable to that message type.
		 *
		 *
		 *                                         Binding  Shared  Shared  Shared        <br/>
		 *                       Binding  Binding  Error    Secret  Secret  Secret        <br/>
		 *   Att.                Req.     Resp.    Resp.    Req.    Resp.   Error         <br/>
		 *                                                                  Resp.         <br/>
		 *   _____________________________________________________________________        <br/>
		 *   MAPPED-ADDRESS      N/A      M        N/A      N/A     N/A     N/A           <br/>
		 *   RESPONSE-ADDRESS    O        N/A      N/A      N/A     N/A     N/A           <br/>
		 *   CHANGE-REQUEST      O        N/A      N/A      N/A     N/A     N/A           <br/>
		 *   SOURCE-ADDRESS      N/A      M        N/A      N/A     N/A     N/A           <br/>
		 *   CHANGED-ADDRESS     N/A      M        N/A      N/A     N/A     N/A           <br/>
		 *   USERNAME            O        N/A      N/A      N/A     M       N/A           <br/>
		 *   PASSWORD            N/A      N/A      N/A      N/A     M       N/A           <br/>
		 *   MESSAGE-INTEGRITY   O        O        N/A      N/A     N/A     N/A           <br/>
		 *   ERROR-CODE          N/A      N/A      M        N/A     N/A     M             <br/>
		 *   UNKNOWN-ATTRIBUTES  N/A      N/A      C        N/A     N/A     C             <br/>
		 *   REFLECTED-FROM      N/A      C        N/A      N/A     N/A     N/A           <br/>
		 *
		 *
		 */
		public const byte N_A = 0;
		public const byte C   = 1;
		public const byte O   = 2;
		public const byte M   = 3;

		//Message indices
		protected const byte BINDING_REQUEST_PRESENTITY_INDEX              = 0;
		protected const byte BINDING_RESPONSE_PRESENTITY_INDEX             = 1;
		protected const byte BINDING_ERROR_RESPONSE_PRESENTITY_INDEX       = 2;
		protected const byte SHARED_SECRET_REQUEST_PRESENTITY_INDEX        = 3;
		protected const byte SHARED_SECRET_RESPONSE_PRESENTITY_INDEX       = 4;
		protected const byte SHARED_SECRET_ERROR_RESPONSE_PRESENTITY_INDEX = 5;


		private byte[][] attributePresentities = new byte[][]{
																 //                                            Binding   Shared   Shared   Shared
																 //                        Binding   Binding   Error     Secret   Secret   Secret
																 //  Att.                  Req.      Resp.     Resp.     Req.     Resp.    Error
																 //                                                                        Resp.
																 //  _______________________________________________________________________
																 /*MAPPED-ADDRESS*/    new byte[] { N_A,      M,        N_A,      N_A,     N_A,     N_A},
																 /*RESPONSE-ADDRESS*/  new byte[] { O,        N_A,      N_A,      N_A,     N_A,     N_A},
																 /*CHANGE-REQUEST*/    new byte[] { O,        N_A,      N_A,      N_A,     N_A,     N_A},
																 /*SOURCE-ADDRESS*/    new byte[] { N_A,      M,        N_A,      N_A,     N_A,     N_A},
																 /*CHANGED-ADDRESS*/   new byte[] { N_A,      M,        N_A,      N_A,     N_A,     N_A},
																 /*USERNAME*/          new byte[] { O,        N_A,      N_A,      N_A,     M,       N_A},
																 /*PASSWORD*/          new byte[] { N_A,      N_A,      N_A,      N_A,     M,       N_A},
																 /*MESSAGE-INTEGRITY*/ new byte[] { O,        O,        N_A,      N_A,     N_A,     N_A},
																 /*ERROR-CODE*/        new byte[] { N_A,      N_A,      M,        N_A,     N_A,     M},
																 /*UNKNOWN-ATTRIBUTES*/new byte[] { N_A,      N_A,      C,        N_A,     N_A,     C},
																 /*REFLECTED-FROM*/    new byte[] { N_A,      C,        N_A,      N_A,     N_A,     N_A}};




		/**
		 * Creates an empty STUN Mesage.
		 */
		protected Message()
		{
		}

		/**
		 * Returns the length of this message's body.
		 * @return the length of the data in this message.
		 */
		public virtual int GetDataLength()
		{
			int length = 0;
			Attribute att = null;

			for (int x = 0; x < attributes.Count; x++) 
			{
				att = (Attribute) attributes[x];
				length += att.GetDataLength() + Attribute.HEADER_LENGTH;
			}

			return length;
		}

		/**
		 * Adds the specified attribute to this message. If an attribute with that
		 * name was already added, it would be replaced.
		 * @param attribute the attribute to add to this message.
		 * @throws StunException if the message cannot contain
		 * such an attribute.
		 */
		public virtual void AddAttribute(Attribute attribute)
		{
			if (attribute == null || attribute is UnknownAttributesAttribute) return;

			//Character attributeType = new Character(attribute.getAttributeType());

			if (GetAttributePresentity(attribute.GetAttributeType()) == N_A)
				throw new StunException(StunException.ILLEGAL_ARGUMENT,
					"The attribute "
					+ attribute.GetName()
					+ " is not allowed in a "
					+ GetName());
			attributes.Add(attribute);

			// attributes.put(attributeType, attribute);
		}

		/**
		 * Returns the attribute with the specified type or null if no such
		 * attribute exists.
		 *
		 * @param attributeType the type of the attribute
		 * @return the attribute with the specified type or null if no such attribute
		 * exists
		 */
		public virtual Attribute GetAttribute(int attributeType)
		{
			for (int x = 0; x < this.attributes.Count; x++) 
			{
				Attribute att = (Attribute) this.attributes[x];
				if (attributeType == att.GetAttributeType()) 
				{
					return att;
				}
			}
			return null;
			//return (Attribute)attributes.get(new Character(attributeType));
		}

		/*
		 * Returns an enumeration containing all message attributes.
		 * @return an enumeration containing all message attributes..
		 */
		/*
		public Iterator getAttributes()
		{
			return attributes.entrySet().iterator();
		}
		*/

		/**
		 * Removes the specified attribute.
		 * @param attributeType the attribute to remove.
		 */
		public virtual void RemoveAttribute(int attributeType)
		{
			for (int x = 0; x < this.attributes.Count; x++) 
			{
				Attribute att = (Attribute) this.attributes[x];
				if (att.GetAttributeType() == attributeType) 
				{
					this.attributes.RemoveAt(x);
					return;
				}
			}
		}

		/**
		 * Returns the number of attributes, currently contained by the message.
		 * @return the number of attributes, currently contained by the message.
		 */
		public virtual int GetAttributeCount()
		{
			return  attributes.Count;
		}



		/**
		 * Sets this message's type to be messageType. Method is package access
		 * as it should not permit changing the type of message once it has been
		 * initialized (could provoke attribute discrepancies). Called by
		 * messageFactory.
		 * @param messageType the message type.
		 * @throws StunException ILLEGAL_ARGUMENT if message type is not valid in
		 * the current context (e.g. when trying to set a Response type to a Request
		 * and vice versa)
		 */
		public virtual void SetMessageType(int messageType)
		{
			this.messageType = messageType;
		}

		/**
		 * The message type of this message.
		 * @return the message type of the message.
		 */
		public virtual int GetMessageType()
		{
			return messageType;
		}

		/**
		 * Copies the specified tranID and sets it as this message's transactionID.
		 * @param tranID the transaction id to set in this message.
		 * @throws StunException ILLEGAL_ARGUMENT if the transaction id is not valid.
		 */
		public virtual void SetTransactionID(byte[] tranID)
		{
			if(tranID == null
				|| tranID.Length != TRANSACTION_ID_LENGTH)
				throw new StunException(StunException.ILLEGAL_ARGUMENT,
					"Invalid transaction id");

			this.transactionID = new byte[TRANSACTION_ID_LENGTH];
			for (int x = 0; x < TRANSACTION_ID_LENGTH; x++) 
			{
				this.transactionID[x] = tranID[x];
			}
		}

		/**
		 * Returns a reference to this message's transaction id.
		 * @return a reference to this message's transaction id.
		 */
		public virtual byte[] GetTransactionID()
		{
			return this.transactionID;
		}

		/**
		 * Returns whether an attribute could be present in this message.
		 * @param attributeID the id of the attribute to check .
		 * @return Message.N_A - for not applicable <br/>
		 *         Message.C   - for case depending <br/>
		 *         Message.N_A - for not applicable <br/>
		 */
		protected virtual byte GetAttributePresentity(int attributeID)
		{
			int msgIndex = -1;
			switch (messageType)
			{
				case BINDING_REQUEST:              msgIndex = BINDING_REQUEST_PRESENTITY_INDEX; break;
				case BINDING_RESPONSE:             msgIndex = BINDING_RESPONSE_PRESENTITY_INDEX; break;
				case BINDING_ERROR_RESPONSE:       msgIndex = BINDING_ERROR_RESPONSE_PRESENTITY_INDEX; break;
				case SHARED_SECRET_REQUEST:        msgIndex = SHARED_SECRET_REQUEST_PRESENTITY_INDEX; break;
				case SHARED_SECRET_RESPONSE:       msgIndex = SHARED_SECRET_RESPONSE_PRESENTITY_INDEX; break;
				case SHARED_SECRET_ERROR_RESPONSE: msgIndex = SHARED_SECRET_ERROR_RESPONSE_PRESENTITY_INDEX; break;
			}


			if (attributeID - 1 < 0 || attributeID - 1 >= attributePresentities.Length) return 0;
			if (msgIndex < 0 || msgIndex >= attributePresentities[attributeID - 1].Length) return 0;
			return attributePresentities[ attributeID - 1 ][ msgIndex ];
		}

		/**
		 * Returns the human readable name of this message. Message names do
		 * not really matter from the protocol point of view. They are only used
		 * for debugging and readability.
		 * @return this message's name.
		 */
		public virtual string GetName()
		{
			switch (messageType)
			{
				case BINDING_REQUEST:              return "BINDING REQUEST";
				case BINDING_RESPONSE:             return "BINDING RESPONSE";
				case BINDING_ERROR_RESPONSE:       return "BINDING ERROR RESPONSE";
				case SHARED_SECRET_REQUEST:        return "SHARED SECRET REQUEST";
				case SHARED_SECRET_RESPONSE:       return "SHARED SECRET RESPONSE";
				case SHARED_SECRET_ERROR_RESPONSE: return "SHARED SECRET ERROR RESPONSE";
			}

			return "UNKNOWN MESSAGE";
		}

		/**
		 * Compares two STUN Messages. Messages are considered equal when their
		 * type, length, and all their attributes are equal.
		 *
		 * @param obj the object to compare this message with.
		 * @return true if the messages are equal and false otherwise.
		 */
		public override bool Equals(Object obj)
		{
			if(!(obj is Message)
				|| obj == null)
				return false;

			if(obj == this)
				return true;

			Message msg = (Message) obj;
			if( msg.GetMessageType()   != GetMessageType())
				return false;
			if(msg.GetDataLength() != GetDataLength())
				return false;

			//compare attributes
			for (int x = 0; x < this.attributes.Count; x++) 
			{
				Attribute localAtt = (Attribute) this.attributes[x];

				if(!localAtt.Equals(msg.GetAttribute(localAtt.GetAttributeType())))
					return false;
			}

			return true;
		}


		/**
		 * Returns a binary representation of this message.
		 * @return a binary representation of this message.
		 * @throws StunException if the message does not have all required
		 * attributes.
		 */
		public virtual byte[] Encode()
		{
			//make sure we have everything necessary to encode a proper message
			this.ValidateAttributePresentity();
			int dataLength = GetDataLength();
			byte[] binMsg = new byte[HEADER_LENGTH + dataLength];
			int offset    = 0;

			binMsg[offset++] = (byte)(GetMessageType()>>8);
			binMsg[offset++] = (byte)(GetMessageType()&0xFF);

			binMsg[offset++] = (byte)(dataLength >> 8);
			binMsg[offset++] = (byte)(dataLength & 0xFF);

			byte[] tba = this.GetTransactionID();
			for (int x = 0; x < TRANSACTION_ID_LENGTH; x++) 
			{
				binMsg[offset + x] = tba[x];
			}

			offset+=TRANSACTION_ID_LENGTH;

			for (int x = 0; x < this.attributes.Count; x++) 
			{
				Attribute attribute = (Attribute) this.attributes[x];

				byte[] attBinValue = attribute.Encode();
				for (int z = 0; z < attBinValue.Length; z++) 
				{
					binMsg[z + offset] = attBinValue[z];
				}
				offset += attBinValue.Length;
			}

			return binMsg;
		}

		/**
		 * Constructs a message from its binary representation.
		 * @param binMessage the binary array that contains the encoded message
		 * @param offset the index where the message starts.
		 * @param arrayLen the length of the message
		 * @return a Message object constructed from the binMessage array
		 * @throws StunException ILLEGAL_ARGUMENT if one or more of the arguments
		 * have invalid values.
		 */
		public static Message Decode(byte[] binMessage, int offset, int arrayLen)
		{
			arrayLen = Math.Min(binMessage.Length, arrayLen);

			if(binMessage == null || arrayLen - offset < Message.HEADER_LENGTH)
				throw new StunException(StunException.ILLEGAL_ARGUMENT,
					"The given binary array is not a valid StunMessage");

			int messageType = (int)(((binMessage[offset++]<<8) & 0xff00) | (binMessage[offset++]&0xFF));
			int mti = (int) messageType;

			Message message;
			if (Message.IsResponseType(messageType))
			{
				message = new Response();
			}
			else
			{
				message = new Request();
			}
			message.SetMessageType(messageType);

			int length = (int)(((binMessage[offset++]<<8) & 0xff00) | (binMessage[offset++]&0xFF));

			if(arrayLen - offset - TRANSACTION_ID_LENGTH < length)
				throw new StunException(StunException.ILLEGAL_ARGUMENT,
					"The given binary array does not seem to "
					+"contain a whole StunMessage");

			byte[] tranID = new byte[TRANSACTION_ID_LENGTH];
			for (int x = 0; x < TRANSACTION_ID_LENGTH; x++) 
			{
				tranID[x] = binMessage[offset + x];
			}

			message.SetTransactionID(tranID);
			offset+=TRANSACTION_ID_LENGTH;

			while(offset - Message.HEADER_LENGTH< length)
			{
				Attribute att = AttributeDecoder.decode(binMessage,
					offset,
					(length - offset));
				if (att != null) 
				{
					message.AddAttribute(att);
					offset += att.GetDataLength() + Attribute.HEADER_LENGTH;
				} 
				else 
				{
					offset += Attribute.HEADER_LENGTH;
				}
			}

			return message;

		}

		/**
		 * Verify that the message has all obligatory attributes and throw an
		 * exception if this is not the case.
		 *
		 * @return true if the message has all obligatory attributes, false
		 * otherwise.
		 * @throws StunException (ILLEGAL_STATE)if the message does not have all
		 * required attributes.
		 */
		protected virtual void ValidateAttributePresentity()
		{
			for(int i = Attribute.MAPPED_ADDRESS; i < Attribute.REFLECTED_FROM; i++)
				if(GetAttributePresentity(i) == M && GetAttribute(i) == null)
					throw new StunException(StunException.ILLEGAL_STATE,
						"A mandatory attribute (type="
						+(int)i
						+ ") is missing!");

		}

		/**
		 * Determines whether type could be the type of a STUN Response (as opposed
		 * to STUN Request).
		 * @param type the type to test.
		 * @return true if type is a valid response type.
		 */
		public static bool IsResponseType(int type)
		{
			return (((type >> 8) & 1) != 0);
		}

		/**
		 * Determines whether type could be the type of a STUN Request (as opposed
		 * to STUN Response).
		 * @param type the type to test.
		 * @return true if type is a valid request type.
		 */
		public static bool IsRequestType(int type)
		{
			return !IsResponseType(type);
		}
	}
}
