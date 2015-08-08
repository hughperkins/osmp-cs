// *** This is a generated file; if you want to change it, please change the generator or the appropriate interface file
// 
// This file was generated by NetworkProxyBuilder, by Hugh Perkins hughperkins@gmail.com http://manageddreams.com
// 

using System;
using System.Net;

namespace OSMP.NetworkInterfaces
{
public class WorldControl_ClientProxy : OSMP.NetworkInterfaces.IWorldControl
{
   RpcController rpc;
   IPEndPoint connection;

   public WorldControl_ClientProxy( RpcController rpc, IPEndPoint connection )
   {
      this.rpc = rpc;
      this.connection = connection;
   }
   public void RequestResendWorld(  )
   {
      rpc.SendRpc( connection, "OSMP.NetworkInterfaces.IWorldControl", "RequestResendWorld",  new object[]{  } );
   }
}
}