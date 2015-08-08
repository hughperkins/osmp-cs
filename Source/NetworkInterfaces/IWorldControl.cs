using System;
using System.Collections.Generic;
using System.Text;

namespace OSMP.NetworkInterfaces
{
    [AuthorizedRpcInterface]
    public interface IWorldControl
    {
        // from client to server; request whole world to be resent
        // eg following reconnect
        void RequestResendWorld();

        // client to server; possibly need some sort of security here ;-)
        //void ResetWorld();

        // server to client
        // startreference specifies minimum reference for objects/prims
        // any objects/prims with reference numbers lower than startreference 
        // are assumed to be old and should be deleted/ignore by client
        //void ResetWorld( int minimumreference );
    }
}
