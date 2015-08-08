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
using OSMP.NetworkInterfaces;

namespace OSMP
{
    public interface IReferenceConsumer
    {
        void NewReferenceResponse( Type targettype, int reference );
    }
    
    // interface for rpc calls for ReplicatedObjectReferenceController
    public interface IReplicatedObjectReferenceControllerRpc
    {
    }
    
    public class ReplicatedObjectReferenceController : IReplicatedObjectReferenceControllerRpc
    {
        Hashtable nextreferencebytype = new Hashtable();
        
        Hashtable objectbyreference = new Hashtable();
        Hashtable referencebyobject = new Hashtable();
        
        bool ismaster;
        
        public bool IsMaster{
            get{ return ismaster; }
            set{ ismaster = value; }
        }
        
        int _GetNextReferenceForType( Type targettype )
        {
            if( !nextreferencebytype.Contains( targettype ) )
            {
                nextreferencebytype.Add( targettype, 1 );
            }
            int nextreference = (int)nextreferencebytype[ targettype ];
            nextreferencebytype[ targettype ] = nextreference + 1;
            return nextreference;
        }
        
        public int GetNextReference( IReferenceConsumer requester, Type targettype, IReplicated targetobject )
        {
            HashableWeakReference targetobjectweakreference = new HashableWeakReference( targetobject );
            if( referencebyobject.Contains( targetobjectweakreference ) )
            {
                return (int)referencebyobject[ targetobjectweakreference ];
            }
            
            if( ismaster )
            {
                int nextreference = _GetNextReferenceForType( targettype );
            
                objectbyreference.Add( nextreference, targetobjectweakreference );
                referencebyobject.Add( targetobjectweakreference, nextreference );                
                return nextreference;
            }
            else
            {
                return null;
            }
        }
        
        public void RequestNewReference( IReferenceConsumer requester, Type targettype )
        {
            if( ismaster )
            {
                int reference = _GetNextReferenceForType( targettype );
                requester.NewReferenceResponse( targettype, reference );
            }
            else
            {
                RpcController.GetInstance().NetObject( this ).RequestNewReferenceRpc( null, targettype, new RefToLocalObject( requester ) );
            }
        }

        // This will blindly return a new reference; it is the client's responsibility to ensure this is only applied to a brand-new object        
        [RpcToServer]
        public void RequestNewReferenceRpc( object connection, Type targettype, RefToLocalObject requester )
        {
            int nextreference = _GetNextReferenceForType( targettype );
            RpcController.GetInstance().NetObject( this ).ReferenceResponse( connection, nextreference, targettype, requester );
        }

        // This returns a new reference; it is the client's responsibility to ensure this is only applied to a brand-new object        
        [RpcToRemoteClient]        
        public void ReferenceResponse( object connection, int reference, Type targettype, RefToLocalObject requesterref )
        {
            IReferenceConsumer requester = (IReferenceConsumer)requesterref.Target;
            requester.NewReferenceResponse( targettype, reference );
        }
    }
    
    // note to self: might want to add Tick to remove dead objects (ie weakreference.Target is null)
    public class RefToReplicatedObject
    {        
        bool islocal;
        HashableWeakReference targetobjectweakreference;
        int reference;
        
        public RefToReplicatedObject( IReplicated targetobject )
        {
            islocal = true;
            this.targetobject = targetobject;
            Type targettype = targetobject.GetType();
            reference = replicatedobjectreferencecontroller.GetInstance().GetReference( targettype, targetobject );
        }
        
        int Reference{
            get{
                return reference;
            }
        }
        
        bool IsLocal{
            get{
                return islocal;
            }
        }
        
        Type TargetType{
            get{
                object targetobject = targetobjectweakreference.Target );
                if( targetobject == null )
                {
                    return null;
                }
                return targetobject.GetType();
            }
        }
        
        IReplicated Target{
            get{ return (IReplicated)targetobjectweakreference.Target;
            }
        }
    }
}
