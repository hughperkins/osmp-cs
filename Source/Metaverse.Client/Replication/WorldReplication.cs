using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace OSMP
{
    public class WorldControl : NetworkInterfaces.IWorldControl
    {
        IPEndPoint connection;
        public WorldControl(IPEndPoint connection)
        {
            this.connection = connection;
        }

        public void RequestResendWorld()
        {
            MetaverseServer.GetInstance().worldreplication.ResendWorld(connection);
        }
    }

    public class WorldReplication
    {
        public WorldReplication()
        {
        }

        public void ResendWorld(IPEndPoint connection)
        {
            MetaverseServer.GetInstance().netreplicationcontroller.dirtyobjectcontroller.ReplicateAll(connection);
        }
    }
}
