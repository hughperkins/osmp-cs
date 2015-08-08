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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Reflection;
using Metaverse.Utility;

namespace OSMP
{
    // handles udp underlying layer, including:
    // - receiving packets and generating a ReceivedPacket event
    // - generating concept of connection/disconnection
    // - generating keepalives
    //
    // things like packet reference number, duplicate detection and so on are handled by higher-level classes,
    // because they are not udp specific
    //
    // keepalive, connection setup/tear down
    // ===================================
    //
    // Udp packets have no intrinsic connection, but obviously servers have a concept of client connections, so we need to generate this concept somehow
    // Whenever we've received a packet from an alleged host in the last ConnectionTimeOutSeconds seconds, we consider that host to be connected
    // If no packets in ConnectionTimeOutSeconds seconds, that host is considered disconnected
    // Note that this can be manipulated by spoofing, so this might need to be tweaked in the future.
    // 
    public class NetworkImplementationUdp : INetworkImplementation
    {
        public event Level1NetworkChangeStateHandler NetworkChangeState;
        
        public event Level1NewConnectionHandler NewConnection;
        public event Level1DisconnectionHandler Disconnection;
        public event Level1ReceivedPacketHandler ReceivedPacket; 
        
        Dictionary<IPEndPoint,Level1ConnectionInfo> connections = new Dictionary<IPEndPoint,Level1ConnectionInfo>(); // used by server
        Level1ConnectionInfo connectiontoserver = new Level1ConnectionInfo( null, new ConnectionInfo( null, null, 0 ) ); // used by client
        
        public int ConnectionTimeOutSeconds = 30;
        public int KeepaliveIntervalSeconds = 5;
        
        class Level1ConnectionInfo
        {
            public IPEndPoint EndPoint;
            public ConnectionInfo connectioninfo;
            public DateTime LastTimestamp;
            public DateTime LastOutgoingPacketTime;
                
            public Level1ConnectionInfo( IPEndPoint EndPoint, ConnectionInfo connectioninfo )
            {
                this.EndPoint = EndPoint;
                this.connectioninfo = connectioninfo;
                LastTimestamp = DateTime.Now;
                LastOutgoingPacketTime = DateTime.Now;
            }
            public void UpdateLastOutgoingPacketTime()
            {
                LastOutgoingPacketTime = DateTime.Now;
            }
        }

        public IPAddress GetIPAddressForConnection(IPEndPoint connection)
        {
            return connection.Address;
        }

        public int GetPortForConnection(IPEndPoint connection)
        {
            return connection.Port;
        }

        Socket GetUdpClientUnderlyingSocket( UdpClient udpclient )
        {
            foreach ( PropertyInfo propertyinfo in typeof( UdpClient ).GetProperties( 
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance ))
            {
                if (propertyinfo.Name == "Client")
                {
                    return propertyinfo.GetValue( udpclient, null ) as Socket;
                }
            }
            throw new Exception( "reflection issue" );
        }

        public IPAddress LocalIPAddress
        {
            get
            {
                Socket udpclientsocket = GetUdpClientUnderlyingSocket( udpclient );
                return ( udpclientsocket.LocalEndPoint as IPEndPoint ).Address;
            }
        }

        public int LocalPort
        {
            get
            {
                Socket udpclientsocket = GetUdpClientUnderlyingSocket( udpclient );
                return (udpclientsocket.LocalEndPoint as IPEndPoint).Port;
            }
        }
        
        bool isserver;
        int serverport = 3456;
        string serveraddress = "127.0.0.1";
        
        UdpClient udpclient;
        
        public NetworkImplementationUdp()
        {
        }
        
        public void ConnectAsClient( string ipaddress, int port )
        {
            //Shutdown();
            serveraddress = ipaddress;
            serverport = port;
            isserver = false;
            Init();
            if( NetworkChangeState != null )
            {
                NetworkChangeState( this, isserver );
            }
        }
        
        public void ListenAsServer( int port )
        {
            //Shutdown();            
            serverport = port;
            isserver = true;
            Init();
            if( NetworkChangeState != null )
            {
                NetworkChangeState( this, isserver );
            }
        }
        
        public int ServerPort
        {
            get{ return serverport;}
        }
        public string ServerAddress
        {
            get{ return serveraddress;}
        }
        public bool IsServer
        {
            get{ return isserver;}
        }

        IPEndPoint remoteserverendpoint = null;

        void Init()
        {
            if( isserver )
            {
                udpclient = new UdpClient( ServerPort );
            }
            else
            {
                //udpclient = new UdpClient( serveraddress, ServerPort );
                IPAddress[] ipaddresses = Dns.GetHostAddresses( serveraddress );
                remoteserverendpoint = new IPEndPoint( ipaddresses[0], serverport );
                udpclient = new UdpClient();
            }

            receivedelegate = new ReceiveDelegate(udpclient.Receive);
            asyncresult = null;
        }

        // process any received packets        
        public void Tick()
        {
            ProcessReceivedPackets();
            CheckDisconnections();
            SendKeepalives();
        }

        delegate byte[] ReceiveDelegate( ref IPEndPoint endpoint );
        ReceiveDelegate receivedelegate;
        IAsyncResult asyncresult;
        IPEndPoint endpoint = new IPEndPoint( IPAddress.Any, 0 );

        public void ProcessReceivedPackets()
        {
            try
            {
                if (asyncresult == null)
                {
                    asyncresult = receivedelegate.BeginInvoke( ref endpoint, null, null );
                }
                while (asyncresult.IsCompleted)
                {
                    Byte[] receiveddata = receivedelegate.EndInvoke( ref endpoint, asyncresult );
                    try
                    {
                        //LogFile.WriteLine( Name + " received:" + Encoding.UTF8.GetString( receiveddata, 0, receiveddata.Length ) );
                     //   LogFile.WriteLine( Name + " received package length " + receiveddata.Length );
                        ProcessReceivedPacket( endpoint, receiveddata );
                    }
                    catch (Exception e)
                    {
                        LogFile.WriteLine( e );
                    }
                    asyncresult = receivedelegate.BeginInvoke( ref endpoint, null, null );
                }
            }
            catch (Exception e)
            {
                LogFile.WriteLine( e );
                asyncresult = receivedelegate.BeginInvoke( ref endpoint, null, null );
            }
        }

        string Name
        {
            get
            {
                if (isserver) { return "server"; } else { return "client"; }
            }
        }

        // sends received packets up stack
        void ProcessReceivedPacket(IPEndPoint endpoint, byte[] packetdata)
        {
            if (!connections.ContainsKey(endpoint))
            {
                LogFile.WriteLine( Name + ": level 1 newconnection: " + endpoint.ToString());
                connections.Add(endpoint, new Level1ConnectionInfo(endpoint, new ConnectionInfo(endpoint, endpoint.Address, endpoint.Port)));
            }
            Level1ConnectionInfo level1connectioninfo = connections[endpoint] as Level1ConnectionInfo;
            level1connectioninfo.LastTimestamp = DateTime.Now;
          //  LogFile.WriteLine( Name + " updating timestamp for connection " + level1connectioninfo );

            if (ReceivedPacket != null && packetdata.Length > 1)
            {
                ReceivedPacket(this, level1connectioninfo.connectioninfo, packetdata, 0, packetdata.GetLength(0));
            }
        }

        void SendKeepalive()
        {
            Send( new byte[]{} );
        }
        
        public void Send(IPEndPoint connection, byte[] data, int length)
        {
            if (connection != null)
            //if (isserver)
            {
                if (isserver)
                {
                    LogFile.WriteLine( "server layer1net.send( " + connection + " " + length );
                }
                else
                {
                    LogFile.WriteLine( "client layer1net.send( " + connection + " " + length );
                }
                if (connections.ContainsKey( connection ))
                {
                    connections[connection].UpdateLastOutgoingPacketTime();
                }
                try
                {
                    udpclient.Send( data, data.Length, connection );
                }
                catch( Exception e )
                {
                    LogFile.WriteLine( e );
                    Init();
                }
            }
            else
            {
                LogFile.WriteLine( "client layer1net.send( " + remoteserverendpoint + " " + length );
                connectiontoserver.UpdateLastOutgoingPacketTime();
                Send( remoteserverendpoint, data, length );
            }
        }

        public void Send(IPEndPoint connection, byte[] data)
        {
            Send(connection, data, data.Length);
        }

        // for client
        public void Send( byte[] data, int length )
        {
            Send( null, data, length );
           // LogFile.WriteLine( "send( data, " + length + " )" );
            //connectiontoserver.UpdateLastOutgoingPacketTime();
            //udpclient.Send( remote data , length );
        }

        public void Send(byte[] data)
        {
            Send(data, data.Length);
        }

        void SendKeepalives()
        {
            if( isserver )
            {
                foreach( KeyValuePair<IPEndPoint,Level1ConnectionInfo> kvp in connections )
                {
                    IPEndPoint connection = kvp.Key;
                    Level1ConnectionInfo connectioninfo = kvp.Value;
                    if( (int)DateTime.Now.Subtract( connectioninfo.LastOutgoingPacketTime ).TotalMilliseconds > ( KeepaliveIntervalSeconds * 1000 ) )
                    {
                      //  LogFile.WriteLine("sending keepalive to " + connection.ToString() );
                        Send( connection, new byte[]{0} );
                        connectioninfo.UpdateLastOutgoingPacketTime();
                    }
                }
            }
            else
            {
                Level1ConnectionInfo connectioninfo = connectiontoserver;
                if( (int)DateTime.Now.Subtract( connectioninfo.LastOutgoingPacketTime ).TotalMilliseconds > ( KeepaliveIntervalSeconds * 1000 ) )
                {
                    //LogFile.WriteLine("sending keepalive to server" );
                    Send( new byte[]{0} );
                    connectioninfo.UpdateLastOutgoingPacketTime();
                }
            }
        }

        DateTime lastdisconnectioncheck;
        void CheckDisconnections()
        {
            if (DateTime.Now.Subtract( lastdisconnectioncheck ).TotalMilliseconds < 1000)
            {
                return;
            }
            lastdisconnectioncheck = DateTime.Now;

            List<IPEndPoint> disconnected = new List<IPEndPoint>();
            //LogFile.WriteLine( Name + " Checking disconnections:" );
            foreach( KeyValuePair<IPEndPoint,Level1ConnectionInfo> entry in connections )
            {
                IPEndPoint connection = entry.Key;
                Level1ConnectionInfo connectioninfo = (Level1ConnectionInfo)entry.Value;
                int timesincelastpacket = (int)( DateTime.Now.Subtract( connectioninfo.LastTimestamp ).TotalMilliseconds );
              //  LogFile.WriteLine( Name + " level1connection " + connectioninfo + " time since last packet: " + timesincelastpacket );
                if (timesincelastpacket > (ConnectionTimeOutSeconds * 1000))
                {
                    disconnected.Add( connection );
                }
            }
            for (int i = 0; i < disconnected.Count; i++)
            {
                IPEndPoint connection = disconnected[i];
                Level1ConnectionInfo connectioninfo = (Level1ConnectionInfo)connections[connection];
                if (Disconnection != null)
                {
                    Disconnection( this, connectioninfo.connectioninfo );
                }
                LogFile.WriteLine( Name + ": level 1 disconnection: " + connectioninfo.EndPoint.ToString() );
                connections.Remove( connection );
            }
        }
    }
}
