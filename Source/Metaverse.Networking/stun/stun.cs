// Copyright Hugh Perkins 2006
//
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the
// Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURVector3E. See the GNU General Public License for
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
using System.Net;
using System.Net.Sockets;
using net.voxx.stun4cs;
using Metaverse.Utility;

namespace OSMP
{
    // handles getting external address for a connection
    // wraps parts of stun4cs library
    public class STUN
    {
        public delegate void GotExternalAddress( IPAddress ipaddress, int port );
        GotExternalAddress callback;

        INetworkImplementation network;

        public Level1ReceivedPacketHandler packethandler;

        //~STUN()
        //{
          //  network.ReceivedPacket -= packethandler; // not sure if necessary?
        //}

        public STUN( INetworkImplementation network, GotExternalAddress callback )
        {
            LogFile.WriteLine( "STUN " + network.LocalIPAddress + " " + network.LocalPort + " " + callback.Target + " " + callback.Method );

            this.network = network;
            this.callback = callback;

            packethandler = new Level1ReceivedPacketHandler( network_ReceivedPacket );
            network.ReceivedPacket += packethandler;

            byte[] bytes = CreateSTUNBindingRequestPacket();

            network.Send( GetStunServerEndpoint(), bytes );
        }

        IPEndPoint GetStunServerEndpoint()
        {
            IPAddress[] stunserveripaddresses = Dns.GetHostAddresses( StunServerHostname );
            IPEndPoint stunserveripendpoint = new IPEndPoint( stunserveripaddresses[0], StunServerPort );
            return stunserveripendpoint;
        }

        string StunServerHostname
        {
            get
            {
                return Config.GetInstance().coordination.stunserver;
            }
        }

        int StunServerPort
        {
            get
            {
                return 3478;
            }
        }

        /*
        public STUN( int ourport, GotExternalAddress callback )
        {
            LogFile.WriteLine( "STUN( ourport: " + ourport + " callback: " + callback.Target + "." + callback.Method.Name );

            INetworkImplementation level1net = NetworkImplementationFactory.CreateNewInstance();
            level1net.ConnectAsClient( new IPAddress( stunser ), serverinfo.port 

            UdpClient udpclient = new UdpClient( StunServerHostname, StunServerPort );
            byte[] bytes = CreateSTUNBindingRequestPacket();
            udpclient.Send( bytes, bytes.Length );
        }
        */
        byte[] CreateSTUNBindingRequestPacket()
        {
            Request request = MessageFactory.CreateBindingRequest();
            ChangeRequestAttribute changeRequest = (ChangeRequestAttribute)request.GetAttribute( net.voxx.stun4cs.Attribute.CHANGE_REQUEST );
            changeRequest.SetChangeIpFlag( false );
            changeRequest.SetChangePortFlag( false );

            TransactionID transactionID = TransactionID.CreateTransactionID();
            request.SetTransactionID( transactionID.GetTransactionID() );

            byte[] bytes = request.Encode();
            return bytes;
        }

        void network_ReceivedPacket( INetworkImplementation source, ConnectionInfo connectioninfo, byte[] data, int offset, int length )
        {
            try
            {
                //LogFile.WriteLine("STUN network receivedpacket");
                if (length > 2 && data[0] == 1 && data[0] == 1) // bindingresponse
                {
                    LogFile.WriteLine( this.GetType() + "could be binding reponse");
                    Message responsemessage = Message.Decode(data, 0, data.Length);
                    MappedAddressAttribute mappedaddress = (MappedAddressAttribute)responsemessage.GetAttribute( net.voxx.stun4cs.Attribute.MAPPED_ADDRESS );
                    IPAddress ipaddress = new IPAddress( mappedaddress.GetAddressBytes() );
                    int port = mappedaddress.GetAddress().GetPort();
                    Console.WriteLine( ipaddress + " " + port );
                    network.ReceivedPacket -= packethandler;
                    callback( ipaddress, port );
                }
                else if (length > 2 && data[0] == 1 && data[0] == 0x11) // bindingerror response
                {
                    LogFile.WriteLine( this.GetType() + " could be binding error");
                }
            }
            catch( Exception e )
            {
                //LogFile.WriteLine( "STUN networkreceivedpacket " + e);
            }
        }
    }
}
