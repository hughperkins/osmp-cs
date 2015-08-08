using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace OSMP.Testing
{
    class TestInterface : ITestInterface
    {
        IPEndPoint connection;

        public TestInterface(IPEndPoint connection)
        {
            this.connection = connection;
        }

        public void SayHello()
        {
            TestNetRpc.TestController.GetInstance().SayHello(connection);
        }

        public void SayMessage(string message)
        {
            TestNetRpc.TestController.GetInstance().SayMessage(connection, message);
        }

        public void SendObject(TestClass testobject)
        {
            TestNetRpc.TestController.GetInstance().SendObject(connection, testobject);
        }

    }
}
