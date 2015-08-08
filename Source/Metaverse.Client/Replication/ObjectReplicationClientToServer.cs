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
using System.Net;

namespace OSMP
{
	public class ObjectReplicationClientToServer : NetworkInterfaces.IObjectReplicationClientToServer
    {
        IPEndPoint connection;
        public ObjectReplicationClientToServer(IPEndPoint connection) { this.connection = connection; }

        public void ObjectCreated( int remoteclientreference, string typename, int attributebitmap, byte[] entitydata )
        {
            MetaverseServer.GetInstance().netreplicationcontroller.ObjectCreatedRpcClientToServer(connection,
                remoteclientreference, typename, attributebitmap, entitydata );
        }

        public void ObjectModified( int reference, string typename, int attributebitmap, byte[]entity )
        {
            MetaverseServer.GetInstance().netreplicationcontroller.ObjectModifiedRpc(connection,
                reference, typename, attributebitmap, entity);
        }

        public void ObjectDeleted(int reference, string typename)
        {
            MetaverseServer.GetInstance().netreplicationcontroller.ObjectDeletedRpc(connection,
                reference, typename );
        }
    }
	
	
}
