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
using System.Collections;
using System.Collections.Generic;
using System.Text; // for Encoding
using System.Net;
using Metaverse.Utility;

namespace OSMP
{
    public interface INetPacketHandler
    {
        void ReceivedPacket(int refnum, IPEndPoint connection, byte[] packetdata, int nextposition);
    }
    
    public delegate void Level2NewConnectionHandler( NetworkLevel2Connection net2con, ConnectionInfo connectioninfo );
    public delegate void Level2DisconnectionHandler( NetworkLevel2Connection net2con, ConnectionInfo connectioninfo );
    public delegate void Level2ReceivedPacketHandler(NetworkLevel2Connection net2con, byte[] data, int offset, int length);

    // This is responsible for handling:
    // - packet reference number (DONE)
    // - generate, confirm shared key; spoof rejection (DONE)
    // - duplicate detection (DONE)
    // - request and process packet re-send
    public class NetworkLevel2Controller
    {
        public event Level2NewConnectionHandler NewConnection;
        public event Level2DisconnectionHandler Disconnection;
        public event Level2ReceivedPacketHandler ReceivedPacket; 

        public INetworkImplementation networkimplementation;
        // MyRand rand = new MyRand( (int)System.DateTime.Now.Ticks );  // initialize seed to ticks
        public bool IsServer;
        
        Dictionary<object,NetworkLevel2Connection> connections = new Dictionary<object,NetworkLevel2Connection>(); // used by server
        NetworkLevel2Connection connectiontoserver; // used by client
        //bool connectionopen = false; // used by client
                
        //static NetworkModel instance = new NetworkModel();
        //public static NetworkModel GetInstance(){ return instance; }

        public NetworkLevel2Controller()
        {
            this.networkimplementation = NetworkImplementationFactory.CreateNewInstance();
            networkimplementation.ReceivedPacket += new Level1ReceivedPacketHandler( ReceivedPacketHandler );
            //networkimplementation.NewConnection += new Level1NewConnectionHandler(networkimplementation_NewConnection);
            networkimplementation.Disconnection += new Level1DisconnectionHandler( networkimplementation_Disconnection );
        }

        //void networkimplementation_NewConnection(INetworkImplementation source, ConnectionInfo connectioninfo)
        //{
          //  if (!IsServer)
//            {
  //              if( NewConnection != null )
    //            {
      //              NewConnection(connectiontoserver, connectiontoserver.connectioninfo);
        //        }
          //  }
        //}
        
        public NetworkLevel2Connection ConnectionToServer{
            get{
                return connectiontoserver;
            }
        }

        public void ConnectAsClient( string ipaddress, int port )
        {
            LogFile.WriteLine("NetworkController.  Connect as client to " + ipaddress + " " + port);
            IsServer = false;
            connections = null;
//            connectionopen = false;
            connectiontoserver = new NetworkLevel2Connection( this, new ConnectionInfo(null, System.Net.IPAddress.Parse( ipaddress ), port ), false );
            networkimplementation.ConnectAsClient( ipaddress, port );
        }
        
        public void ListenAsServer( int port )
        {
            LogFile.WriteLine("NetworkController.  Listen as server port " + port);
            IsServer = true;
            connectiontoserver = null;
            connections = new Dictionary<object,NetworkLevel2Connection>();
            networkimplementation.ListenAsServer( port );
        }
        
        public void ReceivedPacketHandler( INetworkImplementation source, ConnectionInfo connectioninfo, byte[] data, int offset, int length )
        {
            object connection = connectioninfo.Connection;

            //LogFile.WriteLine("Connection: "  + connection);
            if (IsServer)
            {
                if (!connections.ContainsKey(connection))
                {
                    NetworkLevel2Connection networklevel2connection = new NetworkLevel2Connection(this, connectioninfo, IsServer);
                    connections.Add(connection, networklevel2connection);
                }
                connections[connection].ReceivedPacketHandler(connectioninfo, data, offset, length);
            }
            else
            {
                connectiontoserver.ReceivedPacketHandler(connectioninfo, data, offset, length);
            }
        }
        
        public void Tick()
        {
            networkimplementation.Tick();
            if( IsServer )
            {
                foreach ( NetworkLevel2Connection networkmodelconnection in connections.Values)
                {
                    //NetworkLevel2Connection networkmodelconnection = dictionaryentry.Value as NetworkLevel2Connection;
                    networkmodelconnection.Tick();
                }
            }
            else
            {
                connectiontoserver.Tick();
            }
        }

        public void OnConnectionValidated(NetworkLevel2Connection netcon)
        {
            LogFile.WriteLine("Networklevel2controller new connection: " + netcon.connectioninfo.IPAddress + " " + netcon.connectioninfo.Port);
            if (NewConnection != null)
            {
                NewConnection(netcon, netcon.connectioninfo);
            }
        }
        
        public void networkimplementation_Disconnection( INetworkImplementation source, OSMP.ConnectionInfo connectioninfo )
        {
            if( IsServer && connections.ContainsKey( connectioninfo.Connection ) )
            {
                if (Disconnection != null)
                {
                    Disconnection(connections[connectioninfo.Connection], connectioninfo);
                }
                connections[connectioninfo.Connection]._Disconnect();
                connections.Remove(connectioninfo.Connection);
            }
        }

        public void Send( char packettype, byte[] data)
        {
            Send( null, packettype, data, 0, data.Length);
        }

        public void Send(IPEndPoint connection, char packettype, byte[] data)
        {
            Send(connection, packettype, data, 0, data.Length);
        }

        public void Send(IPEndPoint connection, char packettype, byte[] data, int offset, int length)
        {
            //LogFile.WriteLine("netcontroller Send isserver " + IsServer );
            if (IsServer)
            {
                connections[connection].Send(packettype, data, offset, length);
            }
            else
            {
                connectiontoserver.Send(packettype, data, offset, length);
            }
        }

        public void OnPacketReceived(char packettype, NetworkLevel2Connection net2con, byte[] data, int offset, int length )
        {
            if( packetconsumers.ContainsKey( packettype ) )
            {
                packetconsumers[ packettype ]( net2con, data, offset, length );
            }
        }

        public Dictionary<char, Level2ReceivedPacketHandler> packetconsumers = new Dictionary<char, Level2ReceivedPacketHandler>();
        public void RegisterPacketConsumer(char packettype, Level2ReceivedPacketHandler packethandler)
        {
            if (!packetconsumers.ContainsKey(packettype))
            {
                packetconsumers.Add(packettype, packethandler);
            }
            else
            {
                throw new Exception("packettype " + packettype + " already used by " + packetconsumers[packettype]);
            }
        }
    }
}
