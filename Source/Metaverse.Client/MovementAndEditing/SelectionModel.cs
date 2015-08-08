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
//! \brief mvSelection handles object selection within the 3d renderer environment

using System;
using System.Collections;
using System.Collections.Generic;
using Tao.OpenGl;
using Metaverse.Utility;

namespace OSMP
{
    // manages entity selection
    // controlled by SelectionController (or similar)
    // Observers can receive ChangedEvent events ( SelectionModel.GetInstance().ChangedEvent += new ChangedHandler( mymethodnamehere ); )
    public class SelectionModel
    {
        static SelectionModel instance = new SelectionModel();
        public static SelectionModel GetInstance()
        {
            return instance;
        }
        
        public class ChangedEventArgs : EventArgs
        {
            public int NumSelected;
            public ChangedEventArgs( int iNumSelected )
            {
                this.NumSelected = iNumSelected;
            }
        }
        
        WorldModel worldstorage;

        public List<Entity> SelectedObjects = new List<Entity>();
        
        public delegate void ChangedHandler( object source, ChangedEventArgs e );
        public event ChangedHandler ChangedEvent;
        
        public SelectionModel()
        {
            Test.Debug("Instantiating SelectionModel");

            worldstorage = MetaverseClient.GetInstance().worldstorage;
            worldstorage.ClearEvent += new ClearHandler( this.ClearHandler );
            worldstorage.AfterDelete += new AfterDeleteHandler( this.DeleteEntityHandler );        
        }
        
        void DeleteEntityHandler( object source, DeleteEntityEventArgs e )
        {
            if( SelectedObjects.Contains( e.entity ) )
            {
                SelectedObjects.Remove( e.entity );
                NotifyObservers();
            }
        }
        
        void ClearHandler( object source )
        {
            Clear();
        }
        
        public Entity GetFirstSelectedEntity()
        {
            if( SelectedObjects.Count > 0 )
            {
                return SelectedObjects[0];
            }
            else
            {
                return null;
            }
        }
        
        public void RemoveFromSelectedObjects( Entity entity )
        {
            SelectedObjects.Remove( entity );
            NotifyObservers();
        }
        
        public bool IsPartOfAvatar( Entity entity )
        {
         //   Test.Debug(  "IsPartOfAvatar() " + entity.ToString() ); // Test.Debug
            // Entity entity = worldstorage.entities[ iarrayreference ];
            if( entity != null )
            {
                //Test.Debug(  "object type: " + p_Object.sDeepObjectType.ToString() + " " + p_Object.iReference.ToString() + " parentref: " + p_Object.iParentReference.ToString() ); // Test.Debug;
                if( entity.Parent == null )
                {
                    // Test.Debug(  "IsPartOfAvatar() doing comparison " + ( entity == MetaverseClient.GetInstance().iMyReference ).ToString() + " " + entity.iReference.ToString() + " " + MetaverseClient.GetInstance().iMyReference.ToString() );
                    return( entity == MetaverseClient.GetInstance().myavatar );
                }
                else
                {
                //    Test.Debug(  "IsPartOfAvatar() looking up parent " + entity.Parent.ToString() ); // Test.Debug
                    return IsPartOfAvatar( entity.Parent );
                }
            }
            else
            {
                Test.Debug(  "no object found!" ); // Test.Debug
                return false;
            }
        }
        
        public void ToggleObjectInSelection( Entity thisentity, bool bSelectParentObject )
        {
           // Test.Debug(  "trying to toggle selection for object refrenence " + thisentity.ToString() );
            //Entity thisentity = worldstorage.GetEntityByReference( iReference );
            if( thisentity != null )
            {
                if( thisentity.Parent != null && bSelectParentObject )
                {
                  //  Test.Debug(  "This object has a parent " + thisentity.Parent.ToString() + " trying that..." );
                    ToggleObjectInSelection( thisentity.Parent, true );
                }
                else
                {
                    // check it's not an avatar, or, if it is, that its us :-O
                    if( thisentity.GetType() != typeof( Avatar ) || thisentity == MetaverseClient.GetInstance().myavatar )
                    {
                        if( SelectedObjects.Contains( thisentity ) )
                        {
                            SelectedObjects.Remove( thisentity );
                            NotifyObservers();
                            Test.Debug( "Removing object from selection" );
                        }
                        else
                        {
                            Test.Debug( "Adding object to selection" );
                            SelectedObjects.Add( thisentity );
                            NotifyObservers();
                            Test.Debug( "object added" );
                        }
                    }
                }
            }
        }
        
        public bool IsSelected( Entity entity )
        {
            //Test.Debug(  "IsSelected()" ); // Test.Debug
            return SelectedObjects.Contains( entity );
        }

        public void ToggleClickedInSelection( bool bSelectParentObject, int iMouseX, int iMouseY )
        {
            Entity entity;
            if( bSelectParentObject )
            {
                entity = GetClickedTopLevelEntity( iMouseX, iMouseY );
            }
            else
            {
                entity = Picker3dController.GetInstance().GetClickedEntity( iMouseX, iMouseY );
            }
        
            if( entity != null )
            {
               // Test.Debug(  "selected is " + entity.ToString() );
                ToggleObjectInSelection( entity, bSelectParentObject );
            }
        }
        
        // Note to self: change to accessor
        public int GetNumSelected()
        {
            return SelectedObjects.Count;
        }
        
        public Entity GetClickedTopLevelEntity( int iMouseX, int iMouseY )
        {
            Entity clickedentity = Picker3dController.GetInstance().GetClickedEntity( iMouseX, iMouseY );
            if( clickedentity == null )
            {
                return null;
            }
            while( clickedentity.Parent != null )
            {
                clickedentity = clickedentity.Parent;
            }
            return clickedentity;
        }
        void NotifyObservers()
        {
            if( ChangedEvent != null )
            {
                ChangedEvent( this, new ChangedEventArgs( SelectedObjects.Count ) );
            }
        }
   
        public void Clear()
        {
            SelectedObjects.Clear();
            NotifyObservers();
        }
    }
}    
