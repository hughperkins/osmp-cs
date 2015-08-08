// Copyright Hugh Perkins 2004,2005,2006
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

// WorldModel contains the entities in the world and the terrain,everythingin the world really
// it has events that fire when something changes
// if you change something, be sure to call the appropriate on<event> function to notify
// WorldModel to fire the event
//
// rendering is handled by WorldView

using System;
using System.Collections;
using System.Collections.Generic;
using Metaverse.Utility;

namespace OSMP
{ 
    public class CreateEntityEventArgs : EventArgs
    {
        public Entity entity;
        public CreateEntityEventArgs( Entity entity )
        {
            this.entity = entity;
        }
    }

    public class DeleteEntityEventArgs : EventArgs
    {
        public Entity entity;
        public DeleteEntityEventArgs( Entity entity )
        {
            this.entity = entity;
        }
    }

    public class ModifyEntityEventArgs : EventArgs
    {
        public Entity oldEntity;
        public Entity newEntity;
        public ModifyEntityEventArgs( Entity oldentity, Entity newentity )
        {
            this.oldEntity = oldentity;
            this.newEntity = newentity;
        }
    }

    public delegate bool BeforeCreateHandler( object source, CreateEntityEventArgs e );
    public delegate bool BeforeDeleteHandler( object source, DeleteEntityEventArgs e );
    public delegate bool BeforeModifyHandler( object source, ModifyEntityEventArgs e );
    
    public delegate void AfterCreateHandler( object source, CreateEntityEventArgs e );
    public delegate void AfterDeleteHandler( object source, DeleteEntityEventArgs e );
    public delegate void AfterModifyHandler( object source, ModifyEntityEventArgs e );

    public delegate void ClearHandler( object source );
    
    public class WorldModel : IReplicatedObjectController
    {
        // from IReplicatedObjectController
        public event ObjectCreatedHandler ObjectCreated;
        public event ObjectModifiedHandler ObjectModified;
        public event ObjectDeletedHandler ObjectDeleted;

        // for filtering etc
        public event BeforeCreateHandler BeforeCreate;
        public event BeforeDeleteHandler BeforeDelete;
        public event BeforeModifyHandler BeforeModify;

        // for filtering etc
        public event AfterCreateHandler AfterCreate;
        public event AfterDeleteHandler AfterDelete;
        public event AfterModifyHandler AfterModify;

        public event ClearHandler ClearEvent;

        public TerrainModel terrainmodel;
        public int worldversion = 0; // version of world; resetting world increments version number
        public List<Entity> entities = new List<Entity>();
        public Dictionary<int, Entity> entitybyreference = new Dictionary<int, Entity>();

        bool IReplicatedObjectController.HasEntityForReference(int reference)
        {
            foreach (Entity entity in entities)
            {
                if (entity.iReference == reference)
                {
                    return true;
                }
            }
            return false;
        }

        IHasReference IReplicatedObjectController.GetEntity(int reference)
        {
            if( entitybyreference.ContainsKey( reference ) )
            {
                return entitybyreference[ reference ];
            }
            return null;
        }

        void IReplicatedObjectController.AssignGlobalReference(IHasReference entitytoassign, int globalreference)
        {
            // first check we dont already have an entity with same reference, but not same object
            // if we do it is duplicate, so delete
            foreach (Entity entity in entities)
            {
               if (entity.iReference == globalreference && entity != entitytoassign )
               {
                   LogFile.WriteLine("removed duplicate " + globalreference);
                   entities.Remove(entity);
                   entitybyreference.Remove(globalreference);
                   break;
               }
            }
            if (entitybyreference.ContainsKey(entitytoassign.Reference))
            {
                entitybyreference.Remove(entitytoassign.Reference);
            }
            entitytoassign.Reference = globalreference;
            entitybyreference.Add(entitytoassign.Reference, entitytoassign as Entity);
        }
            
        //static WorldModel instance = new WorldModel(); // This is for deserialization (eg load a world from xml).  Not for normal use.
        //public static WorldModel GetInstance()
        //{
          //  return instance;
        //}

        NetReplicationController netreplicationcontroller;

        public WorldModel( NetReplicationController netreplicationcontroller )
        {
            this.netreplicationcontroller = netreplicationcontroller;
            entities = new List<Entity>();
            netreplicationcontroller.RegisterReplicatedObjectController(this, typeof(Entity));
            terrainmodel = new TerrainModel();
        }
        
        // incoming event from NetReplicationController:
        void IReplicatedObjectController.ReplicatedObjectCreated(object notifier, ObjectCreatedArgs e)
        {
            LogFile.WriteLine("WorldModel ReplicatedObjectCreated " + e.TargetObject);
            Entity newentity = e.TargetObject as Entity;
            entities.Add( newentity );
            entitybyreference.Add(newentity.iReference, newentity);
            //if(this.ObjectCreated != null )
            //{
              //  ObjectCreated( this, new ObjectCreatedArgs( DateTime.Now, newentity ) );
            //}
            if (AfterCreate != null)
            {
                AfterCreate(this, new CreateEntityEventArgs(newentity));
            }
        }
        
        // incoming event from NetReplicationController:
        void IReplicatedObjectController.ReplicatedObjectModified(object notifier, ObjectModifiedArgs e)
        {
            LogFile.WriteLine("WorldModel ReplicatedObjectModified " + e.TargetObject);
        }
        
        // incoming event from NetReplicationController:
        void IReplicatedObjectController.ReplicatedObjectDeleted(object notifier, ObjectDeletedArgs e)
        {
            LogFile.WriteLine("WorldModel ReplicatedObjectDeleted " + e.Reference + " "  + e.typename);
            Entity entity = GetEntityByReference(e.Reference);
            if (entity != null)
            {
                entities.Remove(entity);
                entitybyreference.Remove(e.Reference);
            }
        }
        
        public Entity GetEntityByReference( int iReference )
        {
            if (entitybyreference.ContainsKey(iReference))
            {
                return entitybyreference[iReference];
            }
            return null;
        }

        // call this to signal worldmodel that you modified an entity
        public void OnModifyEntity(Entity entity)
        {
            if (ObjectModified != null)
            {
                ObjectModified( this, new ObjectModifiedArgs( DateTime.Now, entity, new Type[]{ typeof(Replicate)}) );
            }
        }
        
        // fires off events, then does the actual delete
        public bool DeleteEntity( Entity entity )
        {
            bool bDeleteApproved = true;
            if( BeforeDelete != null )
            {
                foreach( BeforeDeleteHandler beforedeletecallback in BeforeDelete.GetInvocationList() )
                {
                    if( !beforedeletecallback( this, new DeleteEntityEventArgs( entity ) ) )
                    {
                        bDeleteApproved = false;
                    }
                }
            }
            
            if( !bDeleteApproved )
            {
                return false;
            }

            if (ObjectDeleted != null)
            {
                ObjectDeleted(this, new ObjectDeletedArgs(DateTime.Now, entity.iReference, entity.GetType().ToString() ));
            }
            entitybyreference.Remove(entity.iReference);
            entities.Remove(entity);
            
            if( AfterDelete != null )
            {
                AfterDelete( this, new DeleteEntityEventArgs( entity ) );
            }
            
            return true;
        }
        
        // fires events, then calls __AddEntity to do the actual add, if not cancelled by an event callback
        public bool AddEntity( Entity entity )
        {
            bool bAddApproved = true;
            if( BeforeCreate != null )
            {
                foreach( BeforeCreateHandler beforecreatecallback in BeforeCreate.GetInvocationList() )
                {
                    if( !beforecreatecallback(  this, new CreateEntityEventArgs( entity ) ) )
                    {
                        bAddApproved = false;
                    }
                }
            }
            
            if( !bAddApproved )
            {
                return false;
            }
            
            entities.Add( entity );
            if (entity.iReference != 0)
            {
                entitybyreference.Add(entity.iReference, entity);
            }

            if (ObjectCreated != null)
            {
                //LogFile.WriteLine(entity);
                ObjectCreated( this, new ObjectCreatedArgs( DateTime.Now, entity ) );
            }

            if( AfterCreate != null )
            {
                AfterCreate(this, new CreateEntityEventArgs(entity));
            }
            
            return true;
        }

        public override string ToString()
        {
            return "WorldModel: " + entities.Count + " entities. " + terrainmodel;
        }
        
        public void DumpWorld()
        {
            for( int i = 0; i < entities.Count; i++ )
            {
                Test.Debug( i.ToString() + ": " + entities[i].ToString() );
            }
        }
        
        public void Clear()
        {
            if( ClearEvent != null )
            {
                ClearEvent( this );
            }
            entities.Clear();
            entitybyreference.Clear();
        }
    }
}

