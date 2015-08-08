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
using System.Threading;
using System.Text;
using Metaverse.Utility;

namespace OSMP
{
    public class TestLevel2
    {
        public class Server
        {
            NetworkLevel2Controller net;
            public void Go()
            {
                net = new NetworkLevel2Controller();
                net.ListenAsServer(3456);
                net.NewConnection += new Level2NewConnectionHandler(net_NewConnection);
                net.Disconnection += new Level2DisconnectionHandler(net_Disconnection);
                net.RegisterPacketConsumer('Z', new Level2ReceivedPacketHandler(net_ReceivedPacket));
                //net.ReceivedPacket += new Level2ReceivedPacketHandler(net_ReceivedPacket);
                // net.ListenAsServer( 3457 );
                while (true)
                {
                    net.Tick();
                    Thread.Sleep(50);
                }
            }

            void net_ReceivedPacket(NetworkLevel2Connection net2con, byte[] data, int offset, int length)
            {
                LogFile.WriteLine("TestLevel2Server Received packet: " + Encoding.UTF8.GetString(data, offset, length));
                net.Send(net2con.connectioninfo.Connection, 'Z', Encoding.UTF8.GetBytes("Hello from server"));
            }

            void net_Disconnection(NetworkLevel2Connection net2con, ConnectionInfo connectioninfo)
            {
                LogFile.WriteLine("TestLevel2Server: Client disconnected " + connectioninfo.IPAddress.ToString() + " " + connectioninfo.Port);
            }

            void net_NewConnection(NetworkLevel2Connection net2con, ConnectionInfo connectioninfo)
            {
                LogFile.WriteLine("TestLevel2Server: Client connected " + connectioninfo.IPAddress.ToString() + " " + connectioninfo.Port);
            }

            //void INetPacketHandler.ReceivedPacket(int refnum, object connection, byte[] packetdata, int nextposition)
            //{
            //}
        }

        public class Client
        {
            NetworkLevel2Controller net;
            public void Go()
            {
                net = new NetworkLevel2Controller();
                net.ConnectAsClient("127.0.0.1", 3456);

                net.NewConnection += new Level2NewConnectionHandler(net_NewConnection);
                net.Disconnection += new Level2DisconnectionHandler(net_Disconnection);
                net.RegisterPacketConsumer('Z', new Level2ReceivedPacketHandler(net_ReceivedPacket));
                //net.ReceivedPacket += new Level2ReceivedPacketHandler(net_ReceivedPacket);

                while (true)
                {
                    net.Tick();
                    //net.ConnectionToServer.Send( 'P', Encoding.ASCII.GetBytes( "sample data " + i.ToString() ) );
                    Thread.Sleep(50);
                //    i++;
                }
            }

            void net_ReceivedPacket(NetworkLevel2Connection net2con, byte[] data, int offset, int length)
            {
                LogFile.WriteLine("TestLevel2Client Received packet: " + Encoding.UTF8.GetString(data, offset, length   ));
                net.Send(net2con.connectioninfo.Connection, 'Z', Encoding.UTF8.GetBytes("Test reply from client"));
            }

            void net_Disconnection(NetworkLevel2Connection net2con, ConnectionInfo connectioninfo)
            {
                LogFile.WriteLine("TestLevel2Client: Disconnected from server " + connectioninfo.IPAddress.ToString() + " " + connectioninfo.Port);
            }

            void net_NewConnection(NetworkLevel2Connection net2con, ConnectionInfo connectioninfo)
            {
                LogFile.WriteLine("TestLevel2Client: connected to server " + connectioninfo.IPAddress.ToString() + " " + connectioninfo.Port);
                net.Send( net2con.connectioninfo.Connection, 'Z', Encoding.UTF8.GetBytes("Hello server!") );
            }

            //void INetPacketHandler.ReceivedPacket(int refnum, object connection, byte[] packetdata, int nextposition)
            //{
              //  LogFile.WriteLine("TestLevel2Client Received packet: " + Encoding.UTF8.GetString(data));
            //}
        }

        public static void Go(string[] args)
        {
            try
            {
                bool IsClient = false;
                if (args.GetUpperBound(0) + 1 > 0 && args[0] == "client")
                {
                    IsClient = true;
                }
                if (IsClient)
                {
                    new Client().Go();
                }
                else
                {
                    new Server().Go();
                }
            }
            catch (Exception e)
            {
                LogFile.WriteLine(e);
            }
        }
    }
}

