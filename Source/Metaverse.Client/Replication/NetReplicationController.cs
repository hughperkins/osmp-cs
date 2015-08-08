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
using System.Collections.Generic;
using System.Text;
using System.Net;
using Metaverse.Utility;

namespace OSMP
{
    public class ObjectCreatedArgs : EventArgs
    {
        public DateTime TimeStamp;
        public IHasReference TargetObject;
        public ObjectCreatedArgs(DateTime timestamp, IHasReference TargetObject)
        {
            TimeStamp = timestamp;
            this.TargetObject = TargetObject;
        }
    }
    
    public class ObjectModifiedArgs : EventArgs
    {
        public DateTime TimeStamp;
        public IHasReference TargetObject;
        public Type[] modificationtypeattributes;
        public ObjectModifiedArgs(DateTime timestamp, IHasReference TargetObject, Type[]modificationtypeattributes)
        {
            TimeStamp = timestamp;
            this.TargetObject = TargetObject;
            this.modificationtypeattributes = modificationtypeattributes;
        }
    }
    
    public class ObjectDeletedArgs : EventArgs
    {
        public DateTime TimeStamp;
        public int Reference;
        public string typename;
        public ObjectDeletedArgs(DateTime timestamp, int Reference, string typename )
        {
            TimeStamp = timestamp;
            this.Reference = Reference;
            this.typename = typename;
        }
    }
    
    public delegate void ObjectCreatedHandler( object source, ObjectCreatedArgs e );
    public delegate void ObjectModifiedHandler( object source, ObjectModifiedArgs e );
    public delegate void ObjectDeletedHandler( object source, ObjectDeletedArgs e );
        
    // derive from this on a replicated object controller that wants to create replication references itself
    // this will be the case forworldmodel/controller for example, in order to allow replicated object caching
    // note to self: perhaps caching can be done by the replicationcontroller???
    // so, do we really need this???
    public interface IHasReference
    {
        int Reference { get; set; }
    }
    
    // derive from ths on any classes that manage replicated objects
    // for example, worldmodel(worldcontroller?) derives from this
    // you also need to get the replicatedobjectcaller to call NetReplicationController.GetInstance().RegisterReplicatedObjectController()
    public interface IReplicatedObjectController
    {
        event ObjectCreatedHandler ObjectCreated;
        event ObjectModifiedHandler ObjectModified;
        event ObjectDeletedHandler ObjectDeleted;

        void ReplicatedObjectCreated( object notifier, ObjectCreatedArgs e );
        void ReplicatedObjectModified( object notifier, ObjectModifiedArgs e );
        void ReplicatedObjectDeleted( object notifier, ObjectDeletedArgs e );

        bool HasEntityForReference(int reference);
        void AssignGlobalReference( IHasReference entity, int globalreference );
        IHasReference GetEntity(int reference);
    }

    public interface IReferenceGenerator
    {
        int GenerateReference();
    }
    
    

    // on client, receives replications from network, creates or modifies the object, if it's a new object,  and creates events for this
    // doesnt delete the object if a deletion is received, since theres no way to delete it; instead notifies observers to delete their own reference of it
    //
    // on server, sends out objects as appropriate
    public class NetReplicationController
    {
        public RpcController rpc;
        public NetObjectReferenceController referencecontroller;
        public DirtyObjectController dirtyobjectcontroller;

        bool ismaster = false;
        Dictionary<Type,IReplicatedObjectController> objectcontrollers = new Dictionary<Type,IReplicatedObjectController>();
        Dictionary<int, IHasReference> tempreferences = new Dictionary<int, IHasReference>();
        
        //static NetReplicationController instance = new NetReplicationController();
        //public static NetReplicationController GetInstance(){ return instance; }
        
        public NetReplicationController( RpcController rpc )
        {
            this.rpc = rpc;
            referencecontroller = new NetObjectReferenceController();
            if (rpc.isserver)
            {
                dirtyobjectcontroller = new DirtyObjectController( this, rpc.network, rpc );
            }
        }
        
        public bool IsMaster
        {
            get{ return ismaster; }
            set{
                ismaster = value;
                LogFile.WriteLine( this.GetType().ToString() + " ismaster set to " + ismaster.ToString() );
            }
        }
        
        // called by object replication controllers, who typically pass themselves in as the first argument,
        // and the type of the entity they manage, and want to receive events for
        // for now, only one controller can register for each type
        // note that for class hierarchies, only the base type needs to be registered
        public void RegisterReplicatedObjectController( IReplicatedObjectController controller, Type managedtype )
        {
            if( objectcontrollers.ContainsKey( managedtype ) && objectcontrollers[ managedtype ] != controller )
            {
                throw new Exception( "Error: a replicated type can only be registered by a single replicatedobjectcontroller.  " + 
                    "Duplicated type: " + managedtype.ToString() + " by " + controller.ToString() + " conflicts with " + objectcontrollers[ managedtype ].ToString() );
            }
            if( !objectcontrollers.ContainsKey( managedtype ) )
            {
                objectcontrollers.Add( managedtype, controller );
                controller.ObjectCreated += new ObjectCreatedHandler(controller_ObjectCreated);
                controller.ObjectDeleted += new ObjectDeletedHandler(controller_ObjectDeleted);
                controller.ObjectModified += new ObjectModifiedHandler(controller_ObjectModified);
            }
        }

        // on server, called from dirtyobjectqueuesingleclient
        public void ReplicateSingleEntityToSingleClient(IPEndPoint connection, IHasReference entity, Type[] dirtyattributes)
        {
            LogFile.WriteLine("ReplicateSingleEntityToSingleClient " + entity.GetType() + " to " + connection);
            int bitmap = new ReplicateAttributeHelper().TypeArrayToBitmap(dirtyattributes);

            byte[] entitydata = new byte[4096];
            int nextposition = 0;
            new BinaryPacker().PackObjectUsingSpecifiedAttributes(entitydata, ref nextposition,
                entity, dirtyattributes);

            byte[] entitydatatotransmit = new byte[nextposition];
            Buffer.BlockCopy(entitydata, 0, entitydatatotransmit, 0, nextposition);

            //LogFile.WriteLine(Encoding.UTF8.GetString(entitydatatotransmit));

            new NetworkInterfaces.ObjectReplicationServerToClient_ClientProxy(
                rpc, connection).ObjectModified(
                entity.Reference, entity.GetType().ToString(), bitmap, entitydatatotransmit);
        }

        public void ReplicateDeletionToSingleClient(IPEndPoint connection, int reference, string typename)
        {
            LogFile.WriteLine("ReplicateDeletionToSingleClient " + reference + " to " + connection);
            new NetworkInterfaces.ObjectReplicationServerToClient_ClientProxy(rpc, connection)
                .ObjectDeleted(reference, typename );
        }

        IReplicatedObjectController GetControllerForType(Type type)
        {
            foreach (Type replicatedtype in objectcontrollers.Keys)
            {
                if ( type.IsSubclassOf( replicatedtype ))
                {
                    return objectcontrollers[replicatedtype];
                }
            }
            return null;
        }

        IReplicatedObjectController GetControllerForObject(IHasReference entity)
        {
            foreach (Type replicatedtype in objectcontrollers.Keys)
            {
                if (replicatedtype.IsInstanceOfType(entity))
                {
                    return objectcontrollers[replicatedtype];
                }
            }
            return null;
        }

        // events for incoming changes from object controllers
        void controller_ObjectModified(object source, ObjectModifiedArgs e)
        {
            LogFile.WriteLine("netreplicationcontroller controller_ObjectModified " + e.TargetObject.GetType());
            if (this.rpc.IsServer)
            {
                //LogFile.WriteLine("controller_ObjectCreated() " + e.TargetObject);
                //NetworkInterfaces.ObjectReplicationServerToClient_ClientProxy objectreplicationproxy = new OSMP.NetworkInterfaces.ObjectReplicationServerToClient_ClientProxy( rpc, 
                // handled by something like DirtyCacheController
                dirtyobjectcontroller.MarkDirty(e.TargetObject, e.modificationtypeattributes );
            }
            else
            {
                int bitmap = new ReplicateAttributeHelper().TypeArrayToBitmap(e.modificationtypeattributes);

                byte[] entitydata = new byte[4096];
                int nextposition = 0;
                new BinaryPacker().PackObjectUsingSpecifiedAttributes(entitydata, ref nextposition,
                    e.TargetObject, e.modificationtypeattributes);

                byte[] entitydatatotransmit = new byte[nextposition];
                Buffer.BlockCopy(entitydata, 0, entitydatatotransmit, 0, nextposition);

                //LogFile.WriteLine(Encoding.UTF8.GetString(entitydatatotransmit));

                NetworkInterfaces.ObjectReplicationClientToServer_ClientProxy objectreplicationproxy = new OSMP.NetworkInterfaces.ObjectReplicationClientToServer_ClientProxy(rpc, null);
                objectreplicationproxy.ObjectModified(e.TargetObject.Reference, e.TargetObject.GetType().ToString(), bitmap, entitydatatotransmit);
            }
        }

        void controller_ObjectDeleted(object source, ObjectDeletedArgs e)
        {
            if (rpc.isserver)
            {
                dirtyobjectcontroller.MarkDeleted(e.Reference, e.typename);
            }
            else
            {
                new NetworkInterfaces.ObjectReplicationClientToServer_ClientProxy(rpc, null).ObjectDeleted(
                    e.Reference, e.typename);
            }
        }

        void controller_ObjectCreated(object source, ObjectCreatedArgs e)
        {
            if( this.rpc.IsServer )
            {
                LogFile.WriteLine("netreplicationcontroller,server ObjectCreated " + e.TargetObject.GetType());
                //LogFile.WriteLine("controller_ObjectCreated() " + e.TargetObject);
                //NetworkInterfaces.ObjectReplicationServerToClient_ClientProxy objectreplicationproxy = new OSMP.NetworkInterfaces.ObjectReplicationServerToClient_ClientProxy( rpc, 
                // handled by something like DirtyCacheController
                dirtyobjectcontroller.MarkDirty(e.TargetObject, new Type[] { typeof(Replicate) });
            }
            else
            {
                LogFile.WriteLine("netreplicationcontroller,client ObjectCreated " + e.TargetObject.GetType());
                int tempreference = GenerateTempReference();
                tempreferences.Add( tempreference, e.TargetObject );

                Type[] AttributeTypeList = new Type[] { typeof(Replicate) };
                int bitmap = new ReplicateAttributeHelper().TypeArrayToBitmap( AttributeTypeList );

                byte[] entitydata = new byte[4096];
                int nextposition = 0;
                new BinaryPacker().PackObjectUsingSpecifiedAttributes(entitydata, ref nextposition,
                    e.TargetObject, AttributeTypeList);

                byte[]entitydatatotransmit = new byte[ nextposition ];
                Buffer.BlockCopy( entitydata,0, entitydatatotransmit, 0, nextposition );

                //LogFile.WriteLine(Encoding.UTF8.GetString(entitydatatotransmit));

                NetworkInterfaces.ObjectReplicationClientToServer_ClientProxy objectreplicationproxy = new OSMP.NetworkInterfaces.ObjectReplicationClientToServer_ClientProxy(rpc, null);
                objectreplicationproxy.ObjectCreated(tempreference, e.TargetObject.GetType().ToString(), bitmap, entitydatatotransmit );
            }
        }

        int nextreference = 1;
        int GenerateTempReference()
        {
            nextreference++;
            return nextreference - 1;
        }

        // incoming rpc calls
        // ===================

        public void ObjectCreatedRpcClientToServer(IPEndPoint connection, int remoteclientreference, string typename, int attributebitmap, byte[] entitydata )
        {
            LogFile.WriteLine("ObjectCreatedRpcClientToServer " + typename );

            //LogFile.WriteLine(Encoding.UTF8.GetString(entitydata));

            // note to self: should probably make a whitelist of allowed typenames, maybe via primitive class registration
            Type newtype = Type.GetType(typename);
            IHasReference newobject = (IHasReference) Activator.CreateInstance(newtype);

            IReplicatedObjectController replicatedobjectcontroller = GetControllerForObject( newobject );

            List<Type> AttributeTypeList = new ReplicateAttributeHelper().BitmapToAttributeTypeArray(attributebitmap);

            int nextposition = 0;
            new BinaryPacker().UnpackIntoObjectUsingSpecifiedAttributes(entitydata, ref nextposition,
                newobject, new Type[] { typeof(Replicate) });
            LogFile.WriteLine("server received replicated object: " + newobject.GetType());

            int newobjectreference = 0;
            if (replicatedobjectcontroller != null && replicatedobjectcontroller is IReferenceGenerator)
            {
                newobjectreference = (replicatedobjectcontroller as IReferenceGenerator).GenerateReference();
            }
            else
            {
                newobjectreference = nextreference;
                nextreference++;
            }

            LogFile.WriteLine("New object reference: " + newobjectreference);
            if( newobject is IHasReference )
            {
                ( newobject as IHasReference ).Reference = newobjectreference;
            }

            if (replicatedobjectcontroller != null)
            {
                replicatedobjectcontroller.ReplicatedObjectCreated(this,
        new ObjectCreatedArgs(DateTime.Now, (IHasReference)newobject)); // note to self: untested cast
            }


            new OSMP.NetworkInterfaces.ObjectReplicationServerToClient_ClientProxy( rpc, connection 
                ).ObjectCreatedServerToCreatorClient(
                remoteclientreference, newobjectreference );

            dirtyobjectcontroller.MarkDirty(newobject, new Type[] { typeof(Replicate) });
        }

        public void ObjectCreatedRpcServerToCreatorClient(IPEndPoint connection, int clientreference, int globalreference)
        {
            LogFile.WriteLine("ObjectCreatedRpcServerToCreatorClient " + clientreference + " -> " + globalreference);
            IHasReference thisobject = this.tempreferences[clientreference];
            //if (thisobject is IHasReference)
            //{
                IReplicatedObjectController replicatedobjectcontroller = GetControllerForObject(thisobject);
                if (replicatedobjectcontroller == null)
                {
                    LogFile.WriteLine("Warning, no replicatedobjectcontroller for type " + thisobject.GetType());
                    return;
                }
                replicatedobjectcontroller.AssignGlobalReference(thisobject, globalreference);
                //(thisobject as IHasReference).Reference = globalreference;
            //}
            tempreferences.Remove(clientreference);
        }

        public void ObjectCreatedRpcServerToClient(IPEndPoint connection, int reference, string typename, int attributebitmap, byte[] entitydata)
        {
            LogFile.WriteLine("ObjectCreatedRpcServerToClient " + typename);

            Type newtype = Type.GetType(typename);
            IHasReference newobject = (IHasReference)Activator.CreateInstance(newtype); // note to self: insecure

            IReplicatedObjectController replicatedobjectcontroller = null;
            foreach (Type replicatedtype in objectcontrollers.Keys)
            {
                if (replicatedtype.IsInstanceOfType(newobject))
                {
                    replicatedobjectcontroller = objectcontrollers[replicatedtype];
                }
            }
            if (replicatedobjectcontroller == null)
            {
                LogFile.WriteLine("Error: no controller for received type " + typename);
                return;
            }

            List<Type> AttributeTypeList = new ReplicateAttributeHelper().BitmapToAttributeTypeArray(attributebitmap);

            int nextposition = 0;
            new BinaryPacker().UnpackIntoObjectUsingSpecifiedAttributes(entitydata, ref nextposition,
                newobject, new Type[] { typeof(Replicate) });

            if (!replicatedobjectcontroller.HasEntityForReference(newobject.Reference))
            {
                LogFile.WriteLine("client received replicated object: " + newobject);
                replicatedobjectcontroller.ReplicatedObjectCreated(this,
        new ObjectCreatedArgs(DateTime.Now, newobject)); // note to self: untested cast
            }
        }

        public void ObjectModifiedRpc(IPEndPoint connection, int reference, string typename, int attributebitmap, byte[] entity)
        {
            if (rpc.isserver)
            {
                Type modifiedobjecttype = Type.GetType(typename);

                IReplicatedObjectController replicatedobjectcontroller = GetControllerForType(modifiedobjecttype);
                if (replicatedobjectcontroller == null)
                {
                    LogFile.WriteLine("ObjectModifiedRpc. Error: no controller for received type " + typename);
                    return;
                }

                List<Type> AttributeTypeList = new ReplicateAttributeHelper().BitmapToAttributeTypeArray(attributebitmap);

                IHasReference thisobject = replicatedobjectcontroller.GetEntity(reference);
                if (thisobject == null)
                {
                    LogFile.WriteLine("ObjectModifiedRpc received for unknown object " + reference);
                    return;
                }
                int nextposition = 0;
                new BinaryPacker().UnpackIntoObjectUsingSpecifiedAttributes(entity, ref nextposition,
                    thisobject, new Type[] { typeof(Replicate) });

                LogFile.WriteLine("server received replicated modified object: " + thisobject);
                replicatedobjectcontroller.ReplicatedObjectModified(this,
                    new ObjectModifiedArgs(DateTime.Now, thisobject, AttributeTypeList.ToArray()));
                dirtyobjectcontroller.MarkDirty(thisobject, AttributeTypeList.ToArray());
            }
            else
            {
                LogFile.WriteLine("ObjectModifiedRpcServerToClient " + typename);

                Type modifiedobjecttype = Type.GetType(typename);

                IReplicatedObjectController replicatedobjectcontroller = GetControllerForType(modifiedobjecttype);
                if (replicatedobjectcontroller == null)
                {
                    LogFile.WriteLine("ObjectModifiedRpc. Error: no controller for received type " + typename);
                    return;
                }

                List<Type> AttributeTypeList = new ReplicateAttributeHelper().BitmapToAttributeTypeArray(attributebitmap);

                IHasReference thisobject = replicatedobjectcontroller.GetEntity(reference);
                bool objectisnew = false;
                if (thisobject == null)
                {
                    LogFile.WriteLine("Creating new object");
                    thisobject = Activator.CreateInstance(modifiedobjecttype) as IHasReference;
                    objectisnew = true;
                }
                int nextposition = 0;
                new BinaryPacker().UnpackIntoObjectUsingSpecifiedAttributes(entity, ref nextposition,
                    thisobject, new Type[] { typeof(Replicate) });

                LogFile.WriteLine("client received replicated modified object: " + thisobject);
                if (objectisnew)
                {
                    replicatedobjectcontroller.ReplicatedObjectCreated(this,
                        new ObjectCreatedArgs(DateTime.Now, thisobject));
                }
                else
                {
                    replicatedobjectcontroller.ReplicatedObjectModified(this,
                        new ObjectModifiedArgs(DateTime.Now, thisobject, AttributeTypeList.ToArray()));
                }
            }
        }

        public void ObjectDeletedRpc(IPEndPoint connection, int reference, string typename)
        {
            Type type = Type.GetType( typename );
            IReplicatedObjectController replicatedobjectcontroller = GetControllerForType(type);
            if (replicatedobjectcontroller == null)
            {
                LogFile.WriteLine("Warning: no replicatedobjectcontroller found for type " + typename);
                return;
            }

            if (rpc.isserver)
            {
                LogFile.WriteLine("ObjectDeletedRpc, server " + reference);
                dirtyobjectcontroller.MarkDeleted(reference, typename);
            }
            else
            {
                LogFile.WriteLine("ObjectDeletedRpc, client " + reference);
                replicatedobjectcontroller.ReplicatedObjectDeleted(this, new ObjectDeletedArgs(DateTime.Now, reference, typename));
            }
        }        

        // tick is going to prepare/send some packets to replicate dirty/new objects
        public void Tick()
        {
            if (rpc.isserver)
            {
                dirtyobjectcontroller.Tick();
            }
        }
    }
}
