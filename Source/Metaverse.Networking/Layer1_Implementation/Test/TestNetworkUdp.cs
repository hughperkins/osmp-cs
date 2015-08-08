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
using System.Text;
using System.Threading;
using Metaverse.Utility;

namespace OSMP
{
    public class TestNetworkUdp
    {
        public class Server
        {
            INetworkImplementation net;
            public void Go(int port)
            {
                net = NetworkImplementationFactory.CreateNewInstance();
                net.ListenAsServer(port);
                net.ReceivedPacket += new Level1ReceivedPacketHandler(net_ReceivedPacket);
                while (true)
                {
                    net.Tick();
                    Thread.Sleep(50);
                }
            }

            void net_ReceivedPacket( INetworkImplementation source, ConnectionInfo connectioninfo, byte[] data, int offset, int length )
            {
                LogFile.WriteLine("Received packet: " + Encoding.UTF8.GetString(data));
                net.Send(connectioninfo.Connection, Encoding.UTF8.GetBytes( "Hello from server!" ) );
            }
        }

        public class Client
        {
            public void Go(string ipaddress, int port)
            {
                INetworkImplementation net = NetworkImplementationFactory.CreateNewInstance();
                net.ConnectAsClient(ipaddress, port);
                net.ReceivedPacket += new Level1ReceivedPacketHandler(net_ReceivedPacket);
                net.Send(Encoding.UTF8.GetBytes("Hi, this is a test"));
                while (true)
                {
                    net.Tick();
                    Thread.Sleep(50);
                }
            }

            void net_ReceivedPacket( INetworkImplementation source, ConnectionInfo connectioninfo, byte[] data, int offset, int length )
            {
                LogFile.WriteLine( "Received packet: " + Encoding.UTF8.GetString( data ) );
            }
        }

        static string ipaddress = "127.0.0.1";
        static int serverport = 4241;

        public static void Go(string[] args)
        {
            //LogFile.WriteLine(Int32.MaxValue);
            //LogFile.WriteLine((int)DateTime.Now.Ticks);

            try
            {
                bool IsClient = false;
                if (args.GetUpperBound(0) + 1 > 0 && args[0] == "client")
                {
                    IsClient = true;
                }
                if (IsClient)
                {
                    new Client().Go(ipaddress, serverport);
                }
                else
                {
                    new Server().Go(serverport);
                }
            }
            catch (Exception e)
            {
                LogFile.WriteLine(e);
            }
        }
    }
}
