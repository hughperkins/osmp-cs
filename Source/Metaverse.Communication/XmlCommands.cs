// Copyright Hugh Perkins 2006
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
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Net;

namespace OSMP
{
    public class XmlCommands
    {
        // use singleton so only create a single xmlserializer
        static XmlCommands instance = new XmlCommands();
        public static XmlCommands GetInstance() { return instance; }

        XmlCommands()
        {
            xmlserializer = new XmlSerializer( typeof( Command ),
                new Type[] { 
                    typeof( ServerInfo ),
                    typeof( PingMe )
                } );
        }

        public string Encode( Command cmd )
        {
            StringWriter stringwriter = new StringWriter();
            xmlserializer.Serialize( stringwriter, cmd );
            string message = stringwriter.ToString().Replace( "\n", "" ).Replace( "\r", "" );
            stringwriter.Close();
            return message;
        }

        public Command Decode( string message )
        {
            try
            {
                StringReader stringreader = new StringReader( message );
                return xmlserializer.Deserialize( stringreader ) as Command;
            }
            catch
            {
                return null;
            }
        }

        XmlSerializer xmlserializer;

        public class Command
        {
        }

        public class ServerInfo : Command
        {
            public byte[] IPAddress;
            public int port;

            public ServerInfo(){}
            public ServerInfo( IPAddress ipaddress, int port )
            {
                this.IPAddress = ipaddress.GetAddressBytes();
                this.port = port;
            }
            public override string ToString()
            {
                return "ServerInfo: " + new IPAddress( IPAddress ).ToString() + " " + port;
            }
        }

        public class PingMe : Command
        {
            public byte[] MyIPAddress;
            public int Myport;

            public PingMe(){}
            public PingMe( IPAddress MyIPAddress, int Myport )
            {
                this.MyIPAddress = MyIPAddress.GetAddressBytes();
                this.Myport = Myport;
            }
            public override string ToString()
            {
                return "ClientInfo: " + new IPAddress( MyIPAddress ).ToString() + " " + Myport;
            }
        }
    }
}
