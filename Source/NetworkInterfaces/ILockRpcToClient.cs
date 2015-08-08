using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace OSMP.NetworkInterfaces
{
    [AuthorizedRpcInterface]
    public interface ILockRpcToClient
    {
        // send result to ILockConsumer, and add lock to locks
        void LockRequestResponse(IPEndPoint netconnection, IReplicated targetobject, bool LockCreated);

        // if this is one of our locks, signal the ILockConsumer, then mark lock released
        void LockReleased(IPEndPoint netconnection, IReplicated targetobject);
    }
}
