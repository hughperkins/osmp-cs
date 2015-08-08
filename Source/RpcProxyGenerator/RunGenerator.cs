using System;
using System.Collections.Generic;
using System.Text;

namespace RpcProxyGenerator
{
    public class RunGenerator
    {
        public void Go()
        {
            string dllname = "NetworkInterfaces.dll";
            new NetworkProxyBuilder().Go( dllname, "OSMP.Testing", "ITestInterface");
            new NetworkProxyBuilder().Go(dllname, "OSMP.NetworkInterfaces", "ILockRpcToClient");
            new NetworkProxyBuilder().Go(dllname, "OSMP.NetworkInterfaces", "ILockRpcToServer");
            new NetworkProxyBuilder().Go(dllname, "OSMP.NetworkInterfaces", "IObjectReplicationClientToServer");
            new NetworkProxyBuilder().Go(dllname, "OSMP.NetworkInterfaces", "IObjectReplicationServerToClient");
            new NetworkProxyBuilder().Go(dllname, "OSMP.NetworkInterfaces", "IWorldControl");
        }
    }
}
