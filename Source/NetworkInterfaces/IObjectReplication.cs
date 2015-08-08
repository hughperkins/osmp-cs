using System;
using System.Collections.Generic;
using System.Text;

namespace OSMP.NetworkInterfaces
{
    [AuthorizedRpcInterface]
    public interface IObjectReplicationClientToServer
    {
        void ObjectCreated( int remoteclientreference, string entitytypename, int attributebitmap, byte[] entitydata);
        void ObjectModified(int reference, string entitytypename, int attributebitmap, byte[] entitydata);
        void ObjectDeleted(int reference, string entitytypename);
        //void RequestReferences(int numberreferences); // request a block of reference numbers
    }

    [AuthorizedRpcInterface]
    public interface IObjectReplicationServerToClient
    {
        void ObjectCreatedServerToCreatorClient(int clientreference, int globalreference);
        void ObjectCreated(int reference, string entitytypename, int attributebitmap, byte[] entitydata);
        void ObjectModified(int reference, string entitytypename, int attributebitmap, byte[] entitydata);
        void ObjectDeleted( int reference, string entitytypename );
        //svoid RequestReferencesResponse(int startreference, int lastreference);// series of new reference numbers that client can use, ie for creating new objects/prims
    }
}
