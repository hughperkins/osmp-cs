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

namespace net.voxx.stun4cs {
/**
 * The UNKNOWN-ATTRIBUTES attribute is present only in a Binding Error
 * Response or Shared Secret Error Response when the response code in
 * the ERROR-CODE attribute is 420.
 *
 * The attribute contains a list of 16 bit values, each of which
 * represents an attribute type that was not understood by the server.
 * If the number of unknown attributes is an odd number, one of the
 * attributes MUST be repeated in the list, so that the total length of
 * the list is a multiple of 4 bytes.
 *
 * 0                   1                   2                   3
 * 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
 * +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
 * |      Attribute 1 Type           |     Attribute 2 Type        |
 * +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
 * |      Attribute 3 Type           |     Attribute 4 Type    ...
 * +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
 *
 * <p>Copyright: Copyright (c) 2003</p>
 * <p>Organisation: Louis Pasteur University, Strasbourg, France</p>
 * <p>Network Research Team (http://www-r2.u-strasbg.fr)</p></p>
 * @author Emil Ivov
 * @version 0.1
 */

public class UnknownAttributesAttribute : Attribute
{
    /**
     * A list of attribute types that were not understood by the server.
     */
    private ArrayList unknownAttributes = new ArrayList();



    public UnknownAttributesAttribute() : base(UNKNOWN_ATTRIBUTES)
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

    public const String NAME = "UNKNOWN-ATTRIBUTES";

   /**
    * Returns the length (in bytes) of this attribute's body.
    * If the number of unknown attributes is an odd number, one of the
    * attributes MUST be repeated in the list, so that the total length of
    * the list is a multiple of 4 bytes.
    * @return the length of this attribute's value (a multiple of 4).
    */
    public override int GetDataLength()
    {
        int len = unknownAttributes.Count;

        if( (len % 2 ) != 0 )
            len++;

        return (len*2);
    }

    /**
     * Adds the specified attribute type to the list of unknown attributes.
     * @param attributeID the id of an attribute to be listed as unknown in this
     * attribute
     */
    public virtual void AddAttributeID(int attributeID)
    {
        //some attributes may be repeated for padding
        //(packet length should be divisable by 4)
        if(!Contains(attributeID))
            unknownAttributes.Add(attributeID);
    }

    /**
     * Verifies whether the specified attributeID is contained by this attribute.
     * @param attributeID the attribute id to look for.
     * @return true if this attribute contains the specified attribute id.
     */
    public virtual bool Contains(int attributeID)
    {
        return unknownAttributes.Contains(attributeID);
    }

    /**
     * Returns an iterator over the list of attribute IDs contained by this
     * attribute.
     * @return an iterator over the list of attribute IDs contained by this
     * attribute.
     */
    //public Iterator getAttributes()
    //{
    //    return unknownAttributes.iterator();
	//
    //}

    /**
     * Returns the number of attribute IDs contained by this class.
     * @return the number of attribute IDs contained by this class.
     */
    public virtual int GetAttributeCount()
    {
        return unknownAttributes.Count;
    }

    /**
     * Returns the attribute id with index i.
     * @param index the index of the attribute id to return.
     * @return the attribute id with index i.
     */
    public virtual int GetAttribute(int index )
    {
        return ((int)unknownAttributes[index]);
    }

    /**
     * Returns a binary representation of this attribute.
     * @return a binary representation of this attribute.
     */
    public override byte[] Encode()
    {
        byte[] binValue = new byte[GetDataLength() + HEADER_LENGTH];
        int  offset     = 0;

        //Type
        binValue[offset++] = (byte) (GetAttributeType() >> 8);
        binValue[offset++] = (byte) (GetAttributeType() & 0x00FF);

        //Length
        binValue[offset++] = (byte) (GetDataLength() >> 8);
        binValue[offset++] = (byte) (GetDataLength() & 0x00FF);


		for (int x = 0; x < this.GetAttributeCount(); x++) {
            int att = this.GetAttribute(x);
            binValue[offset++] = (byte)((att>>8) & 0xff);
            binValue[offset++] = (byte)(att & 0x00FF);
        }

       // If the number of unknown attributes is an odd number, one of the
       // attributes MUST be repeated in the list, so that the total length of
       // the list is a multiple of 4 bytes.
       if(offset < binValue.Length)
       {
           int att = GetAttribute(0);
           binValue[offset++] = (byte) ((att >> 8) & 0xff);
           binValue[offset++] = (byte) (att & 0x00FF);
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
		if (! (obj is UnknownAttributesAttribute)
			|| obj == null)
			return false;

		if (obj == this)
			return true;

		UnknownAttributesAttribute att = (UnknownAttributesAttribute) obj;
		if (att.GetAttributeType() != GetAttributeType()
			|| att.GetDataLength() != GetDataLength()
			|| !unknownAttributes.Equals(att.unknownAttributes)
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

        if( (length % 2 ) != 0)
            throw new StunException("Attribute IDs are 2 bytes long and the "
                                    +"passed binary array has an odd length value.");
        int originalOffset = offset;
        for(int i = offset; i < originalOffset + length; i+=2)
        {
            int attributeID = ( (attributeValue[offset++]<<8) & 0xffff |
                               (attributeValue[offset++]) );
            AddAttributeID(attributeID);
        }

    }

}
}