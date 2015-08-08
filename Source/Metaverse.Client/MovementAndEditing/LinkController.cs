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

using System;
using System.Collections;

namespace OSMP
{
    // This will contain anything to do with linking
    // For now, we're just dumping stuff in that was polluting mvWorldStorage.cpp / WorldModel.cs.
    public class LinkController
    {
        /*
            public Entity GetTopLevelParentReference( int iEntityReference )
            {
                Entity entity = GetEntityByReference( iEntityReference );
                if( entity != null )
                {
                    if( entity.iParentReference == 0 )
                    {
                        return entity.iReference;
                    }
                    else
                    {
                        return GetTopLevelParentReference( entity.iParentReference );
                    }
                }
                else
                {
                    return -1;
                }
            }
            
            public bool IsLastPrimInGroup( int iGroupReference )
            {
                Test.Debug(  "IsLastPrimInGroup()" ); // Test.Debug
            
                Entity entity = GetEntityByReference( iGroupReference );
            
                if( entity == null )
                {
                    Test.Debug(  "IsLastPrimInGroup() parentobject is null" ); // Test.Debug
                    return false;
                }
             
                if( entity.GetType() != typeof( EntityGroup ) )
                {
                    Test.Debug(  "IsLastPrimInGroup() WARNING: function called for non-objectgroup" ); // Test.Debug
                    return false;
                }
            
                EntityGroup entitygroup = (EntityGroup)entity;
                if( entitygroup.iNumSubEntities > 1 )
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            */
        void CrossReferenceParentIfNecessary( int SubEntityArrayPos )
        {
            int ParentEntityArrayPos;
        
            Test.Debug(  "CrossReferenceParentIfNecessary() Getting parent reference ..." );
            int iParentReference = entities[ SubEntityArrayPos ].iParentReference;
            ParentEntityArrayPos = GetArrayNumForEntityReference( iParentReference );
            if( ParentEntityArrayPos != -1 )
            {
                ( (EntityGroup)entities[ ParentEntityArrayPos ] ).AddSubEntity( entities[ SubEntityArrayPos ] );
            }
            else
            {
                Test.Debug(  "No parent found at moment for object iReference " + entities[ SubEntityArrayPos ].iReference.ToString() );
            }
        }
        public void CrossReferenceChildrenIfNecessary( EntityGroup group )
        {
            int iArrayPos = 0;
            for( iArrayPos = 0; iArrayPos < iNumEntities; iArrayPos++ )
            {
                Entity childentity = entities[ iArrayPos ];
                if( childentity.iParentReference == group.iReference )
                {
                    Test.Debug(  "Child of iReference " + childentity.iParentReference.ToString() + " is " + childentity.iReference.ToString() ); // Test.Debug
                    group.AddSubEntity( childentity );
                }
            }
        }

        public void UnlinkChildren( EntityGroup group )
        {
            Vector3 ParentPos = group.pos;
            Rot ParentRot = group.rot;
            Rot InverseParentRot= ParentRot.Inverse();
        
            for( int i = 0; i < group.iNumSubEntities; i++ )
            {
                Test.Debug(  "unlinking child " + group.SubEntities[i].ToString() +
                        " group pos " + group.pos.ToString() ); // Test.Debug
        
                // Entity childentity = GetEntityByReference( group.SubEntityReferences[i] );
                Entity childentity = (Entity)group.SubEntities[i];
        
                Rot OldChildRot = childentity.rot;
                Rot NewChildRot = ParentRot * OldChildRot;
        
                Vector3 OldChildPos = childentity.pos;
                Vector3 GroupAxesVectorFromParentToChild = OldChildPos;
                Vector3 GlobalAxesVectorFromParentToChild = GroupAxesVectorFromParentToChild * InverseParentRot;
                //MultiplyVectorByRot( GlobalAxesVectorFromParentToChild, InverseParentRot, GroupAxesVectorFromParentToChild );
                Vector3 NewChildPos = GlobalAxesVectorFromParentToChild + ParentPos;
        
                childentity.iParentReference = 0;
                childentity.pos = NewChildPos;
                childentity.rot = NewChildRot;
                Test.Debug(  "child after unlinking: " + childentity.pos.ToString() + " " + childentity.rot.ToString() ); // Test.Debug
        
                group.SubEntities[i] = null;
            }
            group.iNumSubEntities = 0;
        }
    }
}
