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
using System.Xml;

namespace OSMP
{
    //! Base class for all prims and objects in the world.  Virtual: cant be instantiated itself
    public abstract class Entity : IHasProperties, IHasReference
    {
        public int Reference { get { return iReference; } set { iReference = value; } }

        [Replicate]
        public int iReference;            //!< fundamental unique identifier for the object.  Created by dbinterface.  Unique.  (LIke a guiid)
        [Replicate]
        public int iParentReference;   //!< reference for object's parent, or 0 if object is a toplevel object
        [Replicate]
        public int iOwnerReference;       //!< iReference of owner/creator of object
            
        public Entity Parent = null;
        [Replicate]
        public string name = "New entity";   //!< Object's name

        [Replicate, Movement]
        public Vector3 pos = new Vector3();                   //!< position of object in sim/world
        [Replicate, Movement]
        public Rot rot = new Rot();                   //!< rotation of object

        [Replicate]
        public Vector3 scale = new Vector3();
            
        //public string sScriptReference; //!< md5 checksum of assigned script
           
// Ditching these, since ditching physics ;-) and also they take up bandwidth
// We can always put some of these back later if we really want...        
        //public bool bPhysicsEnabled;  //!< Physics enabled
        //public bool bPhantomEnabled;  //!< Phantom enabled
        //public bool bGravityEnabled;  //!< Gravity enabled
        //public bool bTerrainEnabled;  //!< Terrain enabled 
        //public Vector3 vVelocity = new Vector3();      //!< Current linear velocity
        //public Vector3 vAngularVelocity = new Vector3();      //!< Current angular velocity
        //public Vector3 vLocalForce = new Vector3();      //!< Current local linear force spontaneously acting on object
        //public Vector3 vLocalTorque = new Vector3();      //!< Current local rotational torque spontaneously acting on object
        
        public void SetName( string name ){ this.name = name; }
        
        //public override static Entity operator=( Entity ent1 )
        //{
         //   return new Entity();
       // }
        
        // used to get a list of properties of this class, eg for properties dialog
        public virtual void RegisterProperties( IPropertyController propertycontroller )
        {
            propertycontroller.RegisterStringProperty( "Name", name, 64, new SetStringPropertyHandler( SetName ) );            
        } 
        
        public abstract void Draw();                                                     //!< Draws this object to OpenGL/GLUT, including applying textures, rotating, and translating (leaves OpenGL Matrices how they were prior to function call)
        public abstract void DrawSelected();
        public override string ToString()
        {
            return "<object iReference=\"" + iReference.ToString() + "\" name=\"" + name + "\" />";
        }
    }
}
