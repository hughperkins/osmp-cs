// Copyright Hugh Perkins 2006
// hughperkins@gmail.com http://manageddreams.com
//
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License version 2 as published by the
// Free Software Foundation;
//
// This program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
//  more details.
//
// You should have received a copy of the GNU General Public License along
// with this program in the file licence.txt; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-
// 1307 USA
// You can find the licence also on the web at:
// http://www.opensource.org/licenses/gpl-license.php
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Metaverse.Utility
{
    // binarypacker can pack simple types, rank-1 arrays of simple types, classes, and combinations of these things
    // For writing, it just needs whatever you want to write, and an appropriately-sized array to write to
    // For reading, it needs the Type of what you are trying to read
    // We dont embed any type information in the binary, for compactness, so it is important
    // that reader knows accurately what it should be trying to read

    // note that char is treated as an ASCII char, not unicode.  This is by design.
    // we do this for cases where we just want to use a single letter code for things
    // for normal text, need to use a System.string, which is packed as UTF8
    public class BinaryPacker
    {
        /// <summary>
        /// Anything that can be packed by BinaryPacker will be
        /// </summary>
        public BinaryPacker()
        {
        }

        //public BinaryPacker(Type[] allowedattributes)
        //{
            //this.allowedattributes = allowedattributes;
        //}

        //public Type[] allowedattributes;

        bool HasAllowedAttribute( Type[] allowedattributes, MemberInfo memberinfo)
        {
            object[] attributes = memberinfo.GetCustomAttributes(false);
            foreach (object attribute in attributes)
            {
                //LogFile.WriteLine( attribute.GetType().ToString());
                foreach (Type allowedattributetype in allowedattributes)
                {
                  //  LogFile.WriteLine(allowedattributetype.ToString() + " " + attribute.GetType().ToString());
                    if (attribute.GetType() == allowedattributetype )
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void PackObjectUsingSpecifiedAttributes(byte[] buffer, ref int nextposition, object value, Type[] allowedattributes)
        {
            Type type = value.GetType();
            foreach (FieldInfo fieldinfo in type.GetFields())
            {
                if ( HasAllowedAttribute(allowedattributes, fieldinfo))
                {
                    object fieldvalue = fieldinfo.GetValue(value);
                    // LogFile.WriteLine("packing " + fieldinfo.Name + " " + fieldvalue + " ...");
                    WriteValueToBuffer(buffer, ref nextposition, fieldvalue);
                }
            }
            foreach (System.Reflection.PropertyInfo propertyinfo in type.GetProperties())
            {
                if ( HasAllowedAttribute(allowedattributes, propertyinfo))
                {
                    object fieldvalue = propertyinfo.GetValue(value, null);
                    //LogFile.WriteLine("packing " + propertyinfo.Name + " " + fieldvalue + " ...");
                    WriteValueToBuffer(buffer, ref nextposition, fieldvalue);
                }
            }
        }

        public void UnpackIntoObjectUsingSpecifiedAttributes(byte[] buffer, ref int nextposition, object targetobject, Type[] allowedattributes)
        {
            Type type = targetobject.GetType();
            foreach (FieldInfo fieldinfo in type.GetFields())
            {
                if (HasAllowedAttribute(allowedattributes, fieldinfo))
                {
                        object fieldvalue = ReadValueFromBuffer(buffer, ref nextposition, fieldinfo.FieldType);
                       // LogFile.WriteLine("unpacking " + fieldinfo.Name + " value " + fieldvalue + " ...");
                        fieldinfo.SetValue(targetobject, fieldvalue);
                }
            }
            foreach (System.Reflection.PropertyInfo propertyinfo in type.GetProperties())
            {
                if (HasAllowedAttribute(allowedattributes, propertyinfo))
                {
                        object fieldvalue = ReadValueFromBuffer(buffer, ref nextposition, propertyinfo.PropertyType);
                       // LogFile.WriteLine("unpacking " + propertyinfo.Name + "  value " + fieldvalue + " ...");
                        propertyinfo.SetValue(targetobject, fieldvalue, null);
                }
            }
        }

        public void WriteValueToBuffer(byte[] buffer, ref int nextposition, object value)
        {
            //LogFile.WriteLine(value + " " + value.GetType());
            if (value == null)
            {
                throw new Exception("Attempted to pack null value.  Please make sure all values are not null and try again.");
            }
            Type type = value.GetType();
            if( type == typeof( bool ) )
            {
                if( (bool)value )
                {
                    buffer[nextposition] = (byte)'T';
                }
                else
                {
                    buffer[nextposition] = (byte)'F';
                }
                nextposition += 1;
            }
            else if( type == typeof( char ) )
            {
                buffer[nextposition] = Encoding.ASCII.GetBytes( value.ToString() )[0];
                nextposition ++;
            }
            else if( type == typeof( string ) )
            {
                byte[]payload = Encoding.UTF8.GetBytes( (string)value );

                short length = (short)payload.GetLength(0);
                WriteValueToBuffer(buffer, ref nextposition, length);
                
                Buffer.BlockCopy( payload, 0, buffer, nextposition, payload.GetLength(0) );
                nextposition += payload.GetLength(0);
            }
            else if( type == typeof( double ) )
            {
               // LogFile.WriteLine("Pack double " + value);
                byte[]result = BitConverter.GetBytes( (double)value );
                Buffer.BlockCopy( result, 0, buffer, nextposition, result.Length );
                
                nextposition += result.Length;
            }
            else if( type == typeof( int ) )
            {
                byte[]result = BitConverter.GetBytes( (int)value );
                Buffer.BlockCopy( result, 0, buffer, nextposition, result.Length );
                
                nextposition += result.Length;
            }
            else if (type == typeof(short))
            {
                byte[] result = BitConverter.GetBytes((short)value);
                Buffer.BlockCopy(result, 0, buffer, nextposition, result.Length);

                nextposition += result.Length;
            }
            else if (type.IsArray && type.GetArrayRank() == 1)
                // we handle simple one-dimensional arrays of simple types
            {
                Array valueasarray = value as Array;
                int size = valueasarray.GetLength(0);
                // first we write the size, then the data
                WriteValueToBuffer(buffer, ref nextposition, size);

                // shortcut for byte arrays:
                if (type.GetElementType() == typeof(byte))
                {
                   // LogFile.WriteLine("blockcopy " + size + " bytes");
                    Buffer.BlockCopy(valueasarray, 0, buffer, nextposition, size);
                    nextposition += size;
                }
                else
                {
                    foreach (object item in valueasarray)
                    {
                        WriteValueToBuffer(buffer, ref nextposition, item);
                    }
                }
                //byte[] result = BitConverter.GetBytes((short)value);
                //Buffer.BlockCopy(result, 0, buffer, nextposition, result.Length);

                //nextposition += result.Length;
            }
            else if (type.IsClass) // pack public fields from class
            {
                //LogFile.WriteLine("Pack class " + type.Name);
                foreach (FieldInfo fieldinfo in type.GetFields())
                {
                    //if (allowedattributes == null || HasAllowedAttribute(fieldinfo))
                    //{
                        object fieldvalue = fieldinfo.GetValue(value);
                  //      LogFile.WriteLine("packing " + fieldinfo.Name + " " + fieldvalue + " ...");
                        WriteValueToBuffer(buffer, ref nextposition, fieldvalue);
                    //}
                }
                foreach (System.Reflection.PropertyInfo propertyinfo in type.GetProperties())
                {
                    //if (allowedattributes == null || HasAllowedAttribute(propertyinfo))
                    //{
                        object fieldvalue = propertyinfo.GetValue(value, null);
                    //    LogFile.WriteLine("packing " + propertyinfo.Name + " " + fieldvalue + " ...");
                        WriteValueToBuffer(buffer, ref nextposition, fieldvalue);
                    //}
                }
            }
            else
            {
                throw new Exception("Unknown type: " + type.ToString() + " " + value.ToString());
            }
        }

        // Note to self: for Macs, we probably need to swap endianness?
        // can detect endedness using BitConverter.IsLittleEndian
        public object ReadValueFromBuffer( byte[] buffer, ref int nextposition, Type type )
        {    
            if( type == typeof( bool ) )
            {
                if( buffer[nextposition] == (byte)'T' )
                {
                    nextposition++;
                    return true;
                }
                nextposition++;
                return false;
            }
            else if( type == typeof( string ) )
            {
                short payloadlength = (short)ReadValueFromBuffer( buffer, ref nextposition, typeof( short ) );
                
                string datastring = Encoding.UTF8.GetString( buffer, nextposition, payloadlength );
                nextposition += payloadlength;
                
                return datastring;
            }
            else if( type == typeof( char ) )
            {
                char value = Encoding.ASCII.GetString( buffer, nextposition, 1 )[0];
                nextposition ++;
                return value;
            }
            else if( type == typeof( double ) )
            {
                object result = BitConverter.ToDouble( buffer, nextposition );
                nextposition += 8;
                return result;
            }
            else if( type == typeof( int ) )
            {
                object result = BitConverter.ToInt32( buffer, nextposition );
                nextposition += 4;
                return result;
            }
            else if( type == typeof( short ) )
            {
                object result = BitConverter.ToInt16( buffer, nextposition );
                nextposition += 2;
                return result;
            }
            else if (type.IsArray && type.GetArrayRank() == 1)
            // we handle simple one-dimensional arrays of simple types
            {
                // first we read the size, then the data
                int size = (int)ReadValueFromBuffer(buffer, ref nextposition, typeof(int));
                Type elementtype = type.GetElementType();
               // LogFile.WriteLine("Array " + elementtype.ToString() + "[" + size + "]");
                Array valueasarray = Array.CreateInstance(elementtype, size);
                // shortcut for byte arrays:
                if (type.GetElementType() == typeof(byte))
                {
                    Buffer.BlockCopy(buffer, nextposition, valueasarray, 0, size);
                    nextposition += size;
                }
                else
                {
                    for (int i = 0; i < size; i++)
                    {
                        valueasarray.SetValue(ReadValueFromBuffer(buffer, ref nextposition, elementtype), i);
                    }
                }
                return valueasarray;
            }
            else if (type.IsClass) // unpack public fields from class
            {
                //LogFile.WriteLine("unpack class " + type.Name);
                object newobject = Activator.CreateInstance(type);
                foreach (FieldInfo fieldinfo in type.GetFields())
                {
                 //   if (allowedattributes == null || HasAllowedAttribute(fieldinfo))
                   // {
                        object fieldvalue = ReadValueFromBuffer(buffer, ref nextposition, fieldinfo.FieldType);
                       // LogFile.WriteLine("unpacking " + fieldinfo.Name + " value " + fieldvalue + " ...");
                        fieldinfo.SetValue(newobject, fieldvalue);
                    //}
                }
                foreach (System.Reflection.PropertyInfo propertyinfo in type.GetProperties())
                {
                    //if (allowedattributes == null || HasAllowedAttribute(propertyinfo))
                    //{
                        object fieldvalue = ReadValueFromBuffer(buffer, ref nextposition, propertyinfo.PropertyType);
                       // LogFile.WriteLine("unpacking " + propertyinfo.Name + "  value " + fieldvalue + " ...");
                        propertyinfo.SetValue(newobject, fieldvalue, null);
                    //}
                }
                return newobject;
            }
            else
            {
                throw new Exception("Unknown type: " + type.ToString() );
            }
        }
    }
}
