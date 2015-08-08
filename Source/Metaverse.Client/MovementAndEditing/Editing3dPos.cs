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
//! \brief Manages 3d Position editing

using System;
using System.Collections;
using Metaverse.Utility;

namespace OSMP
{
    public class Editing3dPos
    {
        Editing3d editing3d;
        SelectionModel selectionmodel;
        Camera camera;
        IGraphicsHelper graphics;
        
      //  static Editing3dPos instance = new Editing3dPos();
     //   public static Editing3dPos GetInstance()
      //  {
      //      return instance;
      //  }
        
        public Editing3dPos( Editing3d editing3d )
        {
            this.editing3d = editing3d;
            
            selectionmodel = SelectionModel.GetInstance();
            camera = Camera.GetInstance();
            graphics = GraphicsHelperFactory.GetInstance();
           // editing3d = Editing3d.GetInstance();
        }
        
        // When the movement of the mouse is perpendicular to the selected axis, there are two cases to consider:
        // - the selected axis is in the plane of the screen -> there should be no resulting movement
        // - the selected axis is normal to the screen -> the movement should be infinite
        //
        // Let's call the entity's selected axis in world axes "handleaxis"
        // Let's call the mouse move vector, in world axes "mousemove"
        // So, first we need to project handleaxis onto the screen, and project mousemove onto the unit vector of this, 
        // This gives us a mouse vector, mousemove2 that is immune to the first case above (" - the selected axis is in the plane of the screen -> there should be no resulting movement")
        //
        // Now, we project unit(handleaxis) onto unit(mousemove2)
        // This will give a number <= 1.
        // We will divide |mousemove2| by this value to get the distance that our entity moves along its axis
        // Now it is easy to get the position change by multiplying this quantity by unit(handleaxis)
        //
        // Note that most of the above was in avatar axes (not entity axes or world axes)
        //
        public void InteractiveHandleEdit( Axis axis, int mousex, int mousey )
        {
            Entity entity = selectionmodel.GetFirstSelectedEntity();
            if( entity == null )
            {
                return;
            }
                    
            Vector3 ourpos;
            Rot ourrot;
        
            if( camera.bRoamingCameraEnabled )
            {
                ourpos = camera.RoamingCameraPos;
                ourrot = camera.RoamingCameraRot;
            }
            else
            {
                Avatar ouravatar = MetaverseClient.GetInstance().myavatar;
                if( ouravatar != null )
                {
                    ourpos = ouravatar.pos;
                    ourrot = ouravatar.rot;
                }
                else
                {
                    return;
                }
            }
                    
            // what is the scaling from screen pixels to world pixels, at the distance of the object from us
            // obviously this is only approximate for nearish objects, which is most objects...
            double fDistanceFromUsToObject = ( entity.pos - ourpos ).Det();            
            double fScalingFromPosToScreen = graphics.GetScalingFrom3DToScreen( fDistanceFromUsToObject );
    
            // Create a 3d vector represeting our mouse drag, in avatar coordinates, in screen pixels
            Vector3 mousemovescreenaxes = new Vector3( 0, 
                - (double)( mousex - editing3d.iDragStartX ),
                - (double)( mousey - editing3d.iDragStartY ) );
            
            // transform into a 3d vector, in avatar coordinates, in "world units"
            Vector3 mousemoveavaxes = mousemovescreenaxes * ( 1 / fScalingFromPosToScreen );
            
            // Get handleaxis in avatar axes:
            Vector3 handleaxisentityaxes = axis.ToVector();
            Vector3 handleaxisworldaxes = handleaxisentityaxes * entity.rot.Inverse();
            Vector3 handleaxisavaxes = handleaxisworldaxes * ourrot;

            // we project our handleaxis onto the screen, then project our mousemove onto this
            // to get mousemove2 (see function description for more info)
            Vector3 handleaxisprojectedtoscreen = new Vector3( 0, handleaxisavaxes.y, handleaxisavaxes.z );
            Vector3 mousemove2 = handleaxisprojectedtoscreen.Unit() * Vector3.DotProduct( mousemoveavaxes, handleaxisprojectedtoscreen.Unit() );
            
            // now we are going to find the ratio between our mousemovement size and how far we need to move along the handleaxis
            double entitymovetomousemoveratio = Vector3.DotProduct( handleaxisavaxes.Unit(), mousemove2.Unit() );
            // This gives us the ratio between mouse move distance and entity move distance, now we can calculate how far the entity moves:
            
            Vector3 entitymoveworldaxes = new Vector3();
            if( Math.Abs( entitymovetomousemoveratio ) > 0.05 )  // forbid infinite moves...
            {
                entitymoveworldaxes = ( mousemove2.Det() / entitymovetomousemoveratio ) * handleaxisworldaxes.Unit();
            }
            
            entity.pos = editing3d.startpos + entitymoveworldaxes;
            //Test.Debug(  "pobjectpos: " + entity.pos ); // Test.Debug
            MetaverseClient.GetInstance().worldstorage.OnModifyEntity(entity);
        }
        
        public void InteractiveFreeEdit( bool bAltAxes, int x, int y )
        {
            Entity entity = selectionmodel.GetFirstSelectedEntity();
            if( entity == null )
            {
                return;
            }
                        
            Vector3 OurPos;
            Rot OurRot;
        
            if( camera.bRoamingCameraEnabled )
            {
                OurPos = camera.RoamingCameraPos;
                OurRot = camera.RoamingCameraRot;
            }
            else
            {
                Avatar ouravatar = MetaverseClient.GetInstance().myavatar;
                if( ouravatar != null )
                {
                    OurPos = ouravatar.pos;
                    OurRot = ouravatar.rot;
                }
                else
                {
                    return;
                }
            }
        
            double HalfWinWidth = RendererFactory.GetInstance().WindowWidth / 2;
            double HalfWinHeight = RendererFactory.GetInstance().WindowHeight / 2;
        
            Vector3 translatevector = new Vector3();
            if( bAltAxes )
            {
                translatevector = new Vector3(
                    - ( ( double )( x - editing3d.iDragStartX)) / HalfWinWidth * 3.0,
                    - ( ( double )( y - editing3d.iDragStartY)) / HalfWinWidth * 3.0,
                    0
                );
            }
            else
            {
                translatevector = new Vector3(
                    0,
                    - ((double)(x - editing3d.iDragStartX)) / HalfWinWidth * 3.0,
                    - ((double)(y - editing3d.iDragStartY)) / HalfWinWidth * 3.0
                );
            }
        
            Vector3 PosChangeWorldAxes = translatevector * OurRot.Inverse();
            entity.pos = editing3d.startpos + PosChangeWorldAxes;
            MetaverseClient.GetInstance().worldstorage.OnModifyEntity(entity);
        }
        
        void DrawSingleEditHandle( Vector3 entityscale, Vector3 handlescale, Axis handleaxis )
        {
            graphics.PushMatrix();
            
            graphics.SetMaterialColor( editing3d.GetEditHandleColor( handleaxis ) );
            RendererFactory.GetPicker3dModel().AddHitTarget( new HitTargetEditHandle( new Axis( handleaxis ) ) );
            double fTranslateAmount = ( handleaxis.GetAxisComponentIgnoreAxisDirection( entityscale ) + handleaxis.GetAxisComponentIgnoreAxisDirection( handlescale ) ) / 2;
            graphics.Translate( fTranslateAmount * handleaxis.ToVector() );
            graphics.Scale( handlescale );

            graphics.Rotate( handleaxis.ToRot() );
            
            graphics.Rotate( 90, 0, 1, 0 );
            
            graphics.DrawCone();
            RendererFactory.GetPicker3dModel().EndHitTarget();
            graphics.PopMatrix();
        }
        
        public void DrawEditHandles( Vector3 entityscale, double distance )
        {
            graphics.SetMaterialColor( new double[]{ 1, 0, 0, 0.5 } );
        
            //double averageobjectdimension = ( scaletouse.x + scaletouse.y + scaletouse.z ) / 3;
            Vector3 handlescale = new Vector3( 0.4, 0.4, 0.4 ) * ( distance / 10.0 );
            //LogFile.WriteLine("editing3dpos drawedithandles " + entityscale + " " + distance + " " + handlescale );
        
            DrawSingleEditHandle( entityscale, handlescale, Axis.PosX );
            DrawSingleEditHandle( entityscale, handlescale, Axis.NegX );
            DrawSingleEditHandle( entityscale, handlescale, Axis.PosY );
            DrawSingleEditHandle( entityscale, handlescale, Axis.NegY );
            DrawSingleEditHandle( entityscale, handlescale, Axis.PosZ );
            DrawSingleEditHandle( entityscale, handlescale, Axis.NegZ );
        }
        
        public void InitiateHandleEdit( int mousex, int mousey, Axis axistype )
        {
            editing3d.EditingPreliminaries();
        
            Entity entity = selectionmodel.GetFirstSelectedEntity();
            if( entity == null )
            {
                return;
            }
                    
            editing3d.iDragStartX = mousex;
            editing3d.iDragStartY = mousey;
            editing3d.startpos = entity.pos;
            Test.Debug(  "initializing startpos " + editing3d.startpos ); // Test.Debug
    
            editing3d.currentaxis = axistype;
            editing3d.currentedittype = Editing3d.EditType.PosHandle;
        }
        
        public void InitiateFreeEdit( int mousex, int mousey )
        {
            Entity entity = selectionmodel.GetFirstSelectedEntity();
            if( entity == null )
            {
                return;
            }
            
            editing3d.iDragStartX = mousex;
            editing3d.iDragStartY = mousey;
            
            editing3d.startpos = entity.pos;
            
            Test.Debug(  "initializing startpos " + editing3d.startpos ); // Test.Debug
    
            editing3d.currentedittype = Editing3d.EditType.PosFree;
        }
    }
}
        
