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
using Metaverse.Communication;
using Metaverse.Utility;

namespace OSMP
{
    // handles sending "QUERY" to server and returning results
    public class QueryServer
    {
        public delegate void GotServerResponse( string servername, XmlCommands.ServerInfo serverinfo );

        IChat chat;
        string targetserver;
        GotServerResponse callback;
        IMReceivedHandler handler;

        public QueryServer()
        {
            chat = MetaverseClient.GetInstance().imimplementation;
            handler = new IMReceivedHandler( imimplementation_IMReceived );
        }

        public void Go( string servername, GotServerResponse callback )
        {
            Console.WriteLine( "queryserver.go" );
            this.targetserver = servername;
            this.callback = callback;
            chat.IMReceived += handler;
            chat.SendPrivateMessage( servername, "QUERY" );
        }

        void imimplementation_IMReceived( object source, IMReceivedArgs e )
        {
            Console.WriteLine( "queryserver, im received " + e.Sender + " " + e.MessageText );
            if (e.Sender == targetserver)
            {
                try
                {
                    XmlCommands.Command command = XmlCommands.GetInstance().Decode( e.MessageText );
                    if( command.GetType() == typeof( XmlCommands.ServerInfo ) )
                    {
                        XmlCommands.ServerInfo serverinfo = command as XmlCommands.ServerInfo;
                        Console.WriteLine( "IRCQueryServer Got server response " + serverinfo );
                        callback( targetserver, serverinfo );
                        chat.IMReceived -= handler;
                    }
                }
                catch( Exception ex )
                {
                    LogFile.WriteLine( ex );
                }
            }
        }
    }
}
