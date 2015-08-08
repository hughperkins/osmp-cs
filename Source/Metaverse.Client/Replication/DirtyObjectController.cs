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
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Metaverse.Utility;

namespace OSMP
{
    // runs on server
    public class DirtyObjectController
    {
        public Dictionary<IPEndPoint, DirtyObjectQueueSingleClient> remoteclientdirtyqueues = new Dictionary<IPEndPoint, DirtyObjectQueueSingleClient>(); // queue by connection
        
        //static RemoteClientController instance = new RemoteClientController();
        //public static RemoteClientController GetInstance(){ return instance; }

        public NetReplicationController netreplicationcontroller;
        public NetworkLevel2Controller network;
        public RpcController rpc;

        public DirtyObjectController( NetReplicationController netreplicationcontroller, NetworkLevel2Controller network, RpcController rpc )
        {
            if (!network.IsServer)
            {
                throw new Exception( "Shouldnt be instantiating dirtyobjectcontroller on client" );
            }
            this.netreplicationcontroller = netreplicationcontroller;
            this.rpc = rpc;
            this.network = network;
            //network = NetworkControllerFactory.GetInstance();
            network.NewConnection += new Level2NewConnectionHandler(network_NewConnection);
            network.Disconnection += new Level2DisconnectionHandler(network_Disconnection);
        }

        public void ReplicateAll(IPEndPoint connection)
        {
            remoteclientdirtyqueues[connection].MarkAllDirty();
        }

        void network_Disconnection(NetworkLevel2Connection net2con, ConnectionInfo connectioninfo)
        {
            remoteclientdirtyqueues.Remove(net2con.connectioninfo.Connection);
        }

        void network_NewConnection(NetworkLevel2Connection net2con, ConnectionInfo connectioninfo)
        {
            LogFile.WriteLine( "isserver: " + net2con.isserver );
            LogFile.WriteLine( "net2con: " + net2con );
            LogFile.WriteLine( "connectioninfo: " + net2con.connectioninfo );
            LogFile.WriteLine( "connection: " + net2con.connectioninfo.Connection );
            remoteclientdirtyqueues.Add( net2con.connectioninfo.Connection, new DirtyObjectQueueSingleClient( this, net2con.connectioninfo.Connection ) );
        }
        public void MarkDirty(IHasReference targetobject, Type[] dirtytypes)
        {
            foreach (DirtyObjectQueueSingleClient remoteclientdirtyqueue in remoteclientdirtyqueues.Values)
            {
                remoteclientdirtyqueue.MarkDirty(targetobject, dirtytypes);
            }
        }
        public void MarkDeleted(int reference, string typename)
        {
            foreach (DirtyObjectQueueSingleClient remoteclientdirtyqueue in remoteclientdirtyqueues.Values)
            {
                remoteclientdirtyqueue.MarkDeleted(reference, typename);
            }
        }

        public void Tick()
        {
            //LogFile.WriteLine("dirtyobjectcontroller.tick");
            foreach (DirtyObjectQueueSingleClient remoteclient in remoteclientdirtyqueues.Values)
            {
                //RemoteClient remoteclient = (RemoteClient)remoteclientobject;
                remoteclient.Tick();
            }
        }
    }
}
