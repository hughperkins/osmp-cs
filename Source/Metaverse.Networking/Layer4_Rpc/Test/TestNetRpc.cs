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
using System.Threading;
using System.Net;
using Metaverse.Utility;

namespace OSMP
{
    public class TestNetRpc
    {
        public class TestController
        {
            static TestController instance = new TestController();
            public static TestController GetInstance() { return instance; }

            public RpcController rpc;

            TestController()
            {

            }

            public void SayHello(IPEndPoint connection)
            {
                LogFile.WriteLine("Hello!");
                Testing.TestInterface_ClientProxy testinterfaceclientproxy = new OSMP.Testing.TestInterface_ClientProxy( rpc, connection );
                testinterfaceclientproxy.SayMessage("Hello sent via SayMessage!");
            }

            public void SayMessage(IPEndPoint connection, string message)
            {
                LogFile.WriteLine("Message: " + message);
                Testing.TestInterface_ClientProxy testinterfaceclientproxy = new OSMP.Testing.TestInterface_ClientProxy(rpc, connection);
                Testing.TestClass testobject = new OSMP.Testing.TestClass();
                testobject.name = "blue river";
                testobject.indexes = new int[0];
                testobject.childclass = new OSMP.Testing.ChildClass();
                testinterfaceclientproxy.SendObject(testobject);
            }

            public void SendObject(IPEndPoint connection, Testing.TestClass testobject)
            {
                LogFile.WriteLine("testobject name: " + testobject.name);
            }
        }

        class TestNetRpcClient
        {
            NetworkLevel2Controller network;
            string serveraddress = "127.0.0.1";
            int serverport = 3000;

            public void Go()
            {
                network = new NetworkLevel2Controller();
                network.ConnectAsClient(serveraddress, serverport);
                network.NewConnection += new Level2NewConnectionHandler(network_NewConnection);

                while (true)
                {
                    network.Tick();
                    Thread.Sleep(50);
                }
            }

            void network_NewConnection(NetworkLevel2Connection net2con, ConnectionInfo connectioninfo)
            {
                LogFile.WriteLine("Testnetrpc , new connection " + connectioninfo);
                RpcController rpc = new RpcController(network);
                TestController.GetInstance().rpc = rpc;
                Testing.TestInterface_ClientProxy testinterface_clientproxy = new OSMP.Testing.TestInterface_ClientProxy(rpc, null);
                testinterface_clientproxy.SayHello();
            }
        }

        class TestNetRpcServer
        {
            NetworkLevel2Controller network;
            int serverport = 3000;

            public void Go()
            {
                network = new NetworkLevel2Controller();
                network.ListenAsServer(serverport);
                network.NewConnection += new Level2NewConnectionHandler(network_NewConnection);

                RpcController rpc = new RpcController(network);
                TestController.GetInstance().rpc = rpc;

                while (true)
                {
                    network.Tick();
                    Thread.Sleep(50);
                }
                    //Testing.TestInterface_ClientProxy testinterface_clientproxy = new OSMP.Testing.TestInterface_ClientProxy( rpc, 
            }

            void network_NewConnection(NetworkLevel2Connection net2con, ConnectionInfo connectioninfo)
            {
                LogFile.WriteLine("TestNetRpcServer, new connection: " + net2con.connectioninfo);
            }
        }

        public static void Go(string[] args)
        {
            bool IsServer = true;
            if (args.GetUpperBound(0) + 1 > 0 && args[args.GetUpperBound(0)] == "client")
            {
                IsServer = false;
            }

            try
            {
                if (IsServer)
                {
                    new TestNetRpcServer().Go();
                }
                else
                {
                    new TestNetRpcClient().Go();
                }
            }
            catch (Exception e)
            {
                LogFile.WriteLine(e);
            }
        }
    }
}

