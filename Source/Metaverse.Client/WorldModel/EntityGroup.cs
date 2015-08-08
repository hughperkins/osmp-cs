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

//! \file
//! \brief Contains all the prim/object classes that can be rezed in the world/sim
//!
//! Contains all the prim / object classes that can be rezed in the world/sim, except Terrain which has its own
//! headerfile (Terrain.h)
//!
//! Entity is the pure virtual base class of all objects
//! there are two main derivative branches currently : Prim and EntityGrouping
//! Prims is stuff you can see, like cubes and spheres (when we implement spheres)
//! EntityGroupings are groups of objects - could be prims or other objectgroupings
//! groupings can be hierarhical
//!
//! Entities are responsible for drawing themselves when called with Draw()
//! similarly, a lot of database stuff is in the objects themselves when its object-specific
//! (data-oriented architecture)
//!

using System;
using System.Collections;
using System.Collections.Generic;
using Metaverse.Utility;

namespace OSMP
{

    //! an EntityGrouping is a group of Entities, be it Cubes, or other EntityGroupings, or a mixture
    public class EntityGroup : Entity
    {
        public List<Entity> children;
        
        public EntityGroup()
        {
            children = new List<Entity>();
        }
            
        public void RemoveChild( Entity childentity )
        {
            if( children.Contains( childentity ) )
            {
                children.Remove( childentity );
            }
        }
        
        public void AddChild( Entity child )
        {
            Test.Debug( "objectgrouping " + this.ToString() + " adding subobject " + child.ToString() );
            children.Add( child );
        }
        
        public void ApplyTransforms()
        {
            IGraphicsHelper graphics = GraphicsHelperFactory.GetInstance();
            if( Parent != null )
            {
                EntityGroup parententity = Parent as EntityGroup;
                parententity.ApplyTransforms();
            }
            graphics.Translate( pos );
            graphics.Rotate(  rot );
        }
        
        public override void Draw()
        {
            //Test.Debug("EntityGroup.Draw()");
            IGraphicsHelper graphics = GraphicsHelperFactory.GetInstance();
            //SelectionModel selectionmodel = SelectionModel.GetInstance();
            Picker3dController picker3dcontroller = Picker3dController.GetInstance();
            if( graphics == null )
            {
                return;
            }
            
            graphics.PushMatrix();
            graphics.Translate( pos );
            
            bool bNeedToPopMatrix = false;
            bool bRotatedToGroupRot = false;
            //for( int iSubEntityRef = 0; iSubEntityRef < iNumSubEntities; iSubEntityRef++ )
            foreach( Entity child in children )
            {
                if( !bRotatedToGroupRot )
                {
                    // dont rotate first prim in elevation for avatars (looks better like this)
                    //if( strcmp( sDeepEntityType, "AVATAR" ) != 0 )
                    if( true ) // Just display all avatars as is for now( we should put this back in though probably)
                    {
                        graphics.Rotate( rot );
                        bRotatedToGroupRot = true;
                    }
                    
                    picker3dcontroller.AddHitTarget( child );
                    child.Draw();
                }
        
                if( bNeedToPopMatrix )
                {
                    graphics.PopMatrix();
                }
            }
            graphics.PopMatrix();
        }
        public override void DrawSelected()
        {
        }
    }
}
