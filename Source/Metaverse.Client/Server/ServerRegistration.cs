// Copyright Hugh Perkins 2006
// hughperkins at gmail http://hughperkins.com
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
using System.Threading;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using Metaverse.Communication;
using Metaverse.Utility;

namespace OSMP
{
    public class ServerRegistration
    {
        //public string[] serverlist = new string[] { "irc.gamernet.org" };
        //public int port = 6667;
        //public string channel = "#osmp";

        static ServerRegistration instance = new ServerRegistration();
        public static ServerRegistration GetInstance(){ return instance; }

        IChat chat = null;
        public string ircname;

        Config.Coordination coordinationconfig;

        ServerRegistration() // protected to enforce singleton
        {
            coordinationconfig = Config.GetInstance().coordination;

            chat = ChatImplementationFactory.CreateInstance();
            MetaverseServer.GetInstance().Tick += new MetaverseServer.TickHandler(chat.Tick);

            
            chat.IMReceived += new IMReceivedHandler( this.OnRecieveMessage );

            new InputBox( "Please enter a worldname to publish your server to " + coordinationconfig.ircserver + 
                 " irc " + coordinationconfig.ircchannel, new InputBox.Callback( ServernameCallback ) );
        }

        void ServernameCallback( string servername )
        {
            if (servername == "")
            {
                return;
            }

            ircname = "srv_" + servername;

            LogFile.WriteLine( this.GetType() + " ircname will be: " + ircname + " calling STUN..." );
            STUN stun = new STUN( MetaverseServer.GetInstance().network.networkimplementation, new STUN.GotExternalAddress( GotExternalAddress ) );
        }

        IPAddress externaladdress;
        int externalport;

        void GotExternalAddress( IPAddress ipaddres, int port )
        {
            LogFile.WriteLine( "serverregistration using stun info: " + ipaddres + " " + port );
            this.externaladdress = ipaddres;
            this.externalport = port;
            Connect();
        }

        void Connect()
        {
            string[] serverlist = new string[] { coordinationconfig.ircserver };
            int port = coordinationconfig.ircport;
            string channel = coordinationconfig.ircchannel;

            LogFile.WriteLine( "serverregistration connecting to " + coordinationconfig.ircserver + " ..." );
            LogFile.WriteLine( "serverregistration login as " + ircname );
            LogFile.WriteLine( "serverregistration join channel " + channel );
            chat.Login( serverlist, port, channel, ircname, string.Empty );
            LogFile.WriteLine( "serverregistration connected" );

        }
        
        
        public void OnServerRegistration( string nickname, string message )
        {
            LogFile.WriteLine( "serverregistration. received from " + nickname + ": " + message );
            if( message.StartsWith( "QUERY" ))
            {
                SendCommand(nickname, new XmlCommands.ServerInfo(
                    externaladdress, externalport ) );
            }
            else
            {
                try
                {
                    XmlCommands.Command command = XmlCommands.GetInstance().Decode( message );
                    if( command.GetType() == typeof( XmlCommands.PingMe ) )
                    {
                        XmlCommands.PingMe pingmecommand = command as XmlCommands.PingMe;
                        LogFile.WriteLine( "serverregistration received pingme command: " + new IPAddress( pingmecommand.MyIPAddress ) +
                            " " + pingmecommand.Myport );
                        IPEndPoint endpoint = new IPEndPoint( new IPAddress( pingmecommand.MyIPAddress ), pingmecommand.Myport );
                        MetaverseServer.GetInstance().network.networkimplementation.Send( endpoint, new byte[] { 0 } );
                    }
                }
                catch (Exception ex)
                {
                    LogFile.WriteLine( ex );
                }
            }
        }
        
        void SendCommand( string targetnick, XmlCommands.Command command )
        {
            string message = XmlCommands.GetInstance().Encode( command );
            LogFile.WriteLine( message );
            chat.SendPrivateMessage( targetnick, message );
        }

        public void OnRecieveMessage( object source, IMReceivedArgs e ) {
        	
        	if( e.chatmessagetype == ChatMessageType.PrivateMessage ) {
        		OnServerRegistration( e.Sender, e.MessageText );
        	}
        	else if( e.chatmessagetype == ChatMessageType.Error ) {
        		 LogFile.WriteLine( "Error: " + e.MessageText );
	            Thread.Sleep( 10000 );
	            Connect();
        	}
        	
        	return;
        }
        
    }
}
