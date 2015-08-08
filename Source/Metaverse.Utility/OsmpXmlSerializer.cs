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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Reflection;
using System.IO;

namespace OSMP
{
    // does something similar to xmlserializer but:
    // - precise control over how it works
    // - only serializes fields with [Replicate] attribute
    // - can serialize both fields and properties
    // - throws exception at serialization if no default constructor, or if get or set is missing
    //   from property
    public class OsmpXmlSerializer
    {
        static OsmpXmlSerializer instance = new OsmpXmlSerializer();
        public static OsmpXmlSerializer GetInstance() { return instance; }

        Dictionary<string, Type> allowedtypes = new Dictionary<string, Type>(); // type by type.name

        public OsmpXmlSerializer()
        {
        }

        public void RegisterType( Type type )
        {
            if (!allowedtypes.ContainsKey( type.Name ))
            {
                allowedtypes.Add( type.Name, type );
            }
        }

        void SerializeMember( XmlWriter xmlwriter, string name, Type membertype, object membervalue )
        {
            Console.WriteLine( "   SerializeMember " + name + " " + membertype + " " + membervalue );
            if (membervalue == null)
            {
                return;
            }
            //XmlElement childelement = XmlHelper.AddChild( targetelement, name );
            xmlwriter.WriteStartElement( name );
            if (membertype.IsPrimitive || membertype == typeof( string ) )
            {
                xmlwriter.WriteValue( membervalue.ToString() );
            }
            else if ( membertype.IsEnum)
            {
                //xmlwriter.WriteValue( Convert.ToInt32( membervalue ).ToString() );
                xmlwriter.WriteValue( membervalue.ToString() );
            }
            else if (membertype.IsArray)
            {
                Console.WriteLine( "array" );
                Array objectarray = membervalue as Array;
                foreach (object subobject in objectarray)
                {
                    Console.WriteLine( subobject + " " + subobject.GetType() );
                    xmlwriter.WriteStartElement( membertype.GetElementType().Name );
                    if (membertype.GetElementType().IsPrimitive || membertype.GetElementType() == typeof( string ))
                    {
                        if (subobject != null)
                        {
                            xmlwriter.WriteValue( subobject.ToString() );
                        }
                    }
                    else if (membertype.GetElementType().IsClass)
                    {
                        _Serialize( xmlwriter, membertype.GetElementType(), subobject );
                    }
                    xmlwriter.WriteEndElement();
                }
            }
            else if (membertype.IsClass)
            {
                _Serialize( xmlwriter, membertype, membervalue );
            }
            else
            {
                throw new Exception( "unhandled type " + membertype );
            }
            xmlwriter.WriteEndElement();
        }

        public void Serialize( StringWriter stringwriter, object o )
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;
            settings.Indent = true;

            XmlWriter xmlwriter = XmlWriter.Create( stringwriter, settings );
            xmlwriter.WriteStartElement( o.GetType().Name );
            _Serialize( xmlwriter, o.GetType(), o );
            xmlwriter.WriteEndElement();
            xmlwriter.Close();
        }

        void _Serialize( XmlWriter xmlwriter, Type basetype, object o )
        {
            string basetypename = basetype.Name;
            Console.WriteLine( "_Serialize " + o.GetType() );
            if (!allowedtypes.ContainsKey( o.GetType().Name ))
            {
                throw new Exception( o.GetType().Name + " not in allowedtypes list.  Please register this class with " + this.GetType().Name );
            }
            if (o.GetType().GetConstructor( new Type[] { } ) == null)
            {
                throw new Exception( o.GetType().ToString() + " has no default constructor. Please add one then try serializing again." );
            }
            if (basetypename != o.GetType().Name)
            {
                xmlwriter.WriteAttributeString( "type", o.GetType().Name );
            }
            foreach (PropertyInfo propertyinfo in o.GetType().GetProperties())
            {
                if (propertyinfo.GetCustomAttributes( typeof( Replicate ), false ).GetLength(0) > 0)
                {
                    object propertyvalue = propertyinfo.GetValue( o, null );
                    string name = propertyinfo.Name;
                    Type membertype = propertyinfo.PropertyType;
                    if (!propertyinfo.CanRead)
                    {
                        throw new Exception( "property " + propertyinfo.Name + " of " + o.GetType() + " is missing get accessor" );
                    }
                    if (!propertyinfo.CanWrite)
                    {
                        throw new Exception( "property " + propertyinfo.Name + " of " + o.GetType() + " is missing set accessor" );
                    }
                    SerializeMember( xmlwriter, name, membertype, propertyvalue );
                }
            }
            foreach (FieldInfo fieldinfo in o.GetType().GetFields())
            {
                if (fieldinfo.GetCustomAttributes( typeof( Replicate ), false ).GetLength( 0 ) > 0)
                {
                    object membervalue = fieldinfo.GetValue( o );
                    string name = fieldinfo.Name;
                    Type membertype = fieldinfo.FieldType;
                    SerializeMember( xmlwriter, name, membertype, membervalue );
                }
            }
        }

        public object Deserialize( StringReader stringreader )
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            //settings. = Encoding.UTF8;
            //settings.Indent = true;

            putbacknode = false;
            XmlReader xmlreader = XmlReader.Create( stringreader, settings );
            xmlreader.MoveToContent();

            // reader is right at start, hasnt read anything
            object result = _Deserialize( xmlreader, xmlreader.Name );
            xmlreader.Close();
            return result;
        }

        bool ReadNode( XmlReader xmlreader )
        {
            //Console.WriteLine( "Reading node..." );
            if (putbacknode)
            {
                Console.WriteLine( "read putback=true " + xmlreader.Depth + " " + xmlreader.Name );
                putbacknode = false;
                return true;
            }
            bool result = xmlreader.Read();
            if (!result)
            {
                Console.WriteLine( "reader finished" );
                return false;
            }
            xmlreader.MoveToContent();
            Console.WriteLine( "Read node: " + xmlreader.Depth + " " + xmlreader.Name + " " + xmlreader.IsStartElement() );
            while (!xmlreader.IsStartElement())
            {
                result = xmlreader.Read();
                if (!result)
                {
                    Console.WriteLine( "reader finished" );
                    return false;
                }
                xmlreader.MoveToContent();
                Console.WriteLine( "Read node: " + xmlreader.Depth + " " + xmlreader.Name + " " + xmlreader.IsStartElement() );
            }
            return true;
        }

        bool putbacknode = false;

        // reader is right at start of object ,hasnt read anything
        object _Deserialize( XmlReader xmlreader, string typename )
        {
            Console.WriteLine( "_Deserialize " + xmlreader.LocalName + " " + typename );
            //string typename = xmlreader.Name;
            Console.WriteLine( typename );
            string typeattribute = xmlreader.GetAttribute( "type" );
            if (typeattribute != null)
            {
              //  Console.WriteLine( "[" + typeattribute + "]" );
                typename = typeattribute;
            }
            //Console.WriteLine( "type: " + typename );
            if (!allowedtypes.ContainsKey( typename ))
            {
                throw new Exception( "illegal type: [" + typename + "]" );
            }
            Type newtype = allowedtypes[ typename ];
            Console.WriteLine( newtype );
            object newobject = Activator.CreateInstance( newtype );
            int thisdepth = xmlreader.Depth;
            string thisname = xmlreader.Name;
            Console.WriteLine( "thisdepth: " + thisdepth );
            while ( ReadNode( xmlreader ))
            {
                if (xmlreader.Depth <= thisdepth)
                {
                    Console.WriteLine( "done with this object" );
                    //if (xmlreader.Name != thisname)
                    //{
                      //  Console.WriteLine( "putback " + xmlreader.Name + " " + thisname );
                        putbacknode = true;
                    //}
                    return newobject;
                }
                string fieldname = xmlreader.Name;
                Console.WriteLine( "fieldname: " + fieldname );
                MemberInfo[] memberinfos = newtype.GetMember( fieldname );
                if (memberinfos == null)
                {
                    throw new Exception( "Field " + fieldname + " not found in type " + newtype );
                }
                MemberInfo memberinfo = memberinfos[0];
                if (memberinfo.MemberType == MemberTypes.Field)
                {
                    FieldInfo fieldinfo = memberinfo as FieldInfo;
                    object membervalue = DeserializeMember( xmlreader,
                        fieldinfo.FieldType );
                    if (membervalue != null)
                    {
                        Console.WriteLine( fieldname + " = " + membervalue );
                        fieldinfo.SetValue( newobject, membervalue );
                    }
                }
                if (memberinfo.MemberType == MemberTypes.Property)
                {
                    PropertyInfo propertyinfo = memberinfo as PropertyInfo;
                    object membervalue = DeserializeMember( xmlreader,
                        propertyinfo.PropertyType );
                    if (membervalue != null)
                    {
                        Console.WriteLine( fieldname + " = " + membervalue );
                        propertyinfo.SetValue( newobject, membervalue, null );
                    }
                }
                if (xmlreader.Depth <= thisdepth)
                {
                    //Console.WriteLine( "done with this object" );
                    //if (xmlreader.Name != thisname)
                    //{
                      //  Console.WriteLine( "putback " + xmlreader.Name + " " + thisname );
                        putbacknode = true;
                    //}
                    return newobject;
                }
            }
            //Console.WriteLine( "_Deserialize <<<" );
            return newobject;
        }

        // reader has read first node in object
        // when returns, must not have read past end of object
        object DeserializeMember( XmlReader xmlreader, Type membertype )
        {
            Console.WriteLine( "_DeserializeMember " + xmlreader.LocalName + " " + membertype.Name );

            if (membertype == typeof( int ) )
            {
                return xmlreader.ReadElementContentAsInt();
            }
            if (membertype == typeof( double ))
            {
                return xmlreader.ReadElementContentAsDouble();
            }
            if (membertype == typeof( bool ))
            {
                return Convert.ToBoolean( xmlreader.ReadElementContentAsString() );
            }
            if (membertype == typeof( string ))
            {
                return xmlreader.ReadElementContentAsString();
            }
            if (membertype.IsEnum)
            {
                string enumvalue = xmlreader.ReadElementContentAsString();
                return Enum.Parse( membertype, enumvalue );
            }
            if (membertype.IsArray)
            {
                ArrayList arraylist = new ArrayList();
                int thisdepth = xmlreader.Depth;
                string thisname = xmlreader.Name;
                Console.WriteLine( "thisdepth: " + thisdepth + " thisname: " + thisname );
                while (ReadNode(xmlreader))
                {
                    if (xmlreader.Depth <= thisdepth)
                    {
                        if (xmlreader.Name != thisname)
                        {
                            Console.WriteLine( " xmlreader.name: " + xmlreader.Name + " " + thisname );
                            putbacknode = true;
                        }
                        break;
                    }
                    object arrayitem = null;
                    if (membertype.GetElementType().IsPrimitive || membertype.GetElementType() == typeof( string ))
                    {
                        arrayitem = DeserializeMember( xmlreader, membertype.GetElementType() );
                    }
                    else
                    {
                        arrayitem = _Deserialize( xmlreader, membertype.GetElementType().Name );
                    }
                    arraylist.Add( arrayitem );
                    Console.WriteLine( "array item: " + arrayitem );
                    if (xmlreader.Depth <= thisdepth)
                    {
                        if (xmlreader.Name != thisname)
                        {
                            Console.WriteLine( " xmlreader.name: " + xmlreader.Name + " " + thisname );
                            putbacknode = true;
                        }
                        break;
                    }
                }
                Console.WriteLine( "done with this array" );
                object arrayresult = arraylist.ToArray( membertype.GetElementType() );
                //Console.WriteLine( arrayresult );
                return arrayresult;
            }
            if (membertype.IsClass)
            {
                object deserializedobject = _Deserialize( xmlreader, membertype.Name );
                Console.WriteLine( "deserialized object: " + deserializedobject );
                return deserializedobject;
            }
            throw new Exception("Unimplemented membertype: " + membertype );
        }
    }
}
