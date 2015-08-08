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
using System.Net;

namespace OSMP
{
    public class ConnectionInfo
    {
        public IPEndPoint Connection; // used by underlying implementation
        public IPAddress IPAddress;
        public int Port;
        public ConnectionInfo() { }
        public ConnectionInfo(IPEndPoint Connection, IPAddress IPAddress, int Port)
        {
            this.Connection = Connection;
            this.IPAddress = IPAddress;
            this.Port = Port;
        }
        public override string ToString()
        {
            return "ConnectionInfo IPAddress " + IPAddress + " port " + Port;
        }
    }

    public delegate void Level1NetworkChangeStateHandler( INetworkImplementation source, bool isserver);
    public delegate void Level1NewConnectionHandler(INetworkImplementation source, ConnectionInfo connectioninfo);
    public delegate void Level1DisconnectionHandler(INetworkImplementation source, ConnectionInfo connectioninfo);
    public delegate void Level1ReceivedPacketHandler(INetworkImplementation source, ConnectionInfo connectioninfo, byte[] data, int offset, int length );

    // network implementation is responsible for sending pre-formatted/serialized data across the underlying network    
    // it could be udp or tcp, or somethign else, so we use a factory to select the specific type of INetworkImplementation that we want
    public interface INetworkImplementation
    {
        event Level1NetworkChangeStateHandler NetworkChangeState;
        
        event Level1NewConnectionHandler NewConnection;
        event Level1DisconnectionHandler Disconnection;
        event Level1ReceivedPacketHandler ReceivedPacket;

        IPAddress GetIPAddressForConnection(IPEndPoint connection);
        int GetPortForConnection(IPEndPoint connection);

        IPAddress LocalIPAddress{ get; }
        int LocalPort { get; }

        bool IsServer{get;}
        int ServerPort{get;}
        string ServerAddress{get;}

        void ConnectAsClient( string ipaddress, int port );
        void ListenAsServer( int port );
                
        //void Start();
        //void Shutdown();
        
        void Tick();
        /// <summary>
        /// Send from client to server
        /// </summary>
        /// <param name="data"></param>
        void Send(byte[] data );
        void Send(byte[] data, int count);
        /// <summary>
        /// Send from server to client (need to identify which client using conneciton object
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="data"></param>
        void Send(IPEndPoint connection, byte[] data);
        void Send(IPEndPoint connection, byte[] data, int length);
    }
}
