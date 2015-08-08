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

//! \file
//! \brief Manages 3d Scale editing

namespace OSMP
{
    public class Editing3dScale
    {
        Editing3d editing3d;
        Camera camera;
        IGraphicsHelper graphics;
        SelectionModel selectionmodel;
        
        public Editing3dScale( Editing3d editing3d )
        {
            this.editing3d = editing3d;
            
            camera = Camera.GetInstance();
            graphics = GraphicsHelperFactory.GetInstance();
            selectionmodel = SelectionModel.GetInstance();
        }
        
        // see description of function by same name in Editing3dPos
        public void InteractiveHandleEdit( Axis axis, int mousex, int mousey )
        {
            Entity entity = selectionmodel.GetFirstSelectedEntity();    

            if( entity == null )
            {
                return;
            }
            
            Vector3 ourpos = null;
            Rot ourrot = null;
        
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
            handleaxisentityaxes.x = Math.Abs( handleaxisentityaxes.x );
            handleaxisentityaxes.y = Math.Abs( handleaxisentityaxes.y );
            handleaxisentityaxes.z = Math.Abs( handleaxisentityaxes.z );
            Vector3 handleaxisworldaxes = handleaxisentityaxes * entity.rot.Inverse();
            Vector3 handleaxisavaxes = handleaxisworldaxes * ourrot;

            // we project our handleaxis onto the screen, then project our mousemove onto this
            // to get mousemove2 (see function description for more info)
            Vector3 handleaxisprojectedtoscreen = new Vector3( 0, handleaxisavaxes.y, handleaxisavaxes.z );
            Vector3 mousemove2 = handleaxisprojectedtoscreen.Unit() * Vector3.DotProduct( mousemoveavaxes, handleaxisprojectedtoscreen.Unit() );
            
            // now we are going to find the ratio between our mousemovement size and how far we need to move along the handleaxis
            double entitymovetomousemoveratio = Vector3.DotProduct( handleaxisavaxes.Unit(), mousemove2.Unit() );
            // This gives us the ratio between mouse move distance and entity move distance, now we can calculate the change in entity scale:
            
            if( Math.Abs( entitymovetomousemoveratio ) < 0.05 ) // prevent infinite scaling..
            {
                return;
            }
            Vector3 scalechange = ( mousemove2.Det() / entitymovetomousemoveratio ) * handleaxisentityaxes;
            Vector3 newscale = null;
            if( axis.IsPositiveAxis )
            {
                newscale = editing3d.startscale + scalechange;
            }
            else
            {
                newscale = editing3d.startscale - scalechange;
            }
            newscale.x = Math.Max( 0.05, newscale.x );
            newscale.y = Math.Max( 0.05, newscale.y );
            newscale.z = Math.Max( 0.05, newscale.z );
    
            Vector3 finalscalechange = newscale - editing3d.startscale;
            Vector3 finalscalechangeworldaxes = finalscalechange * entity.rot.Inverse();
            
            if( axis.IsPositiveAxis )
            {
                entity.pos = editing3d.startpos + ( finalscalechangeworldaxes ) / 2;
            }
            else
            {
                entity.pos = editing3d.startpos - ( finalscalechangeworldaxes ) / 2;
            }
            
            // scale is defined in entity local axes, so no need to transform into world axes
            entity.scale = newscale;
            MetaverseClient.GetInstance().worldstorage.OnModifyEntity(entity);
        }
        
        public void InteractiveFreeEdit( int mousex, int mousey )
        {
            // DEBUG(  "InteractiveHandleScaleEdit" ); // DEBUG
            Entity entity = selectionmodel.GetFirstSelectedEntity();
        
            Vector3 OurPos = null;
            Rot OurRot = null;
        
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
        
            if( entity != null )
            {
                double fDistanceFromUsToEntity = ( entity.pos - OurPos ).Det();
                double fScalingFromPosToScreen = graphics.GetScalingFrom3DToScreen( fDistanceFromUsToEntity );
    
                Vector3 ScreenMouseVector = new Vector3(
                    0,
                    - ( (double)(mousex - editing3d.iDragStartX) ),
                    - ( (double)( mousey - editing3d.iDragStartY ) )
                );
    
                Vector3 PosMouseVectorAvAxes = ScreenMouseVector * ( 1 / fScalingFromPosToScreen );
    
                Vector3 PosMouseVectorWorldAxes = PosMouseVectorAvAxes * OurRot.Inverse();
                Vector3 PosMouseVectorEntityAxes = PosMouseVectorWorldAxes * entity.rot;
                //   DEBUG(  "screen vector: " << ScreenMouseVector << " PosMouseVectorAvAxes " << PosMouseVectorAvAxes <<
                //      " posmousevectorworldaxes: " << PosMouseVectorWorldAxes << " PosMouseVectorEntityAxes " << PosMouseVectorEntityAxes ); // DEBUG
    
                Vector3 vScaleChangeVectorEntityAxes = PosMouseVectorEntityAxes;        
                Vector3 vNewScale = editing3d.startscale + vScaleChangeVectorEntityAxes;
    
                if( vNewScale.x < 0.05 )
                {
                    vNewScale.x = 0.05;
                }
                else if( vNewScale.y < 0.05 )
                {
                    vNewScale.y = 0.05;
                }
                else if( vNewScale.z < 0.05 )
                {
                    vNewScale.z = 0.05;
                }
    
                //        Vector3 vTranslate;
                //        vTranslate = ( Vector3( vNewScale ) - Vector3( editing3d.startscale ) ) * 0.5f;        
                entity.scale = vNewScale;
                MetaverseClient.GetInstance().worldstorage.OnModifyEntity(entity);
            }
            //   DEBUG(  "InteractiveHandleScaleEdit done" ); // DEBUG
        }
        
        public void _InteractiveFreeScaleEdit_Old( bool bAltAxes, int x, int y )
        {
            Vector3 OurPos = null;
            Rot OurRot = null;
        
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
        
            Rot rInverseOurRot = OurRot.Inverse();
        
            Entity entity = selectionmodel.GetFirstSelectedEntity();
        
            // DEBUG(  "interactive scale edit objectype=[" << World.GetEntity( iSelectedArrayNum ).EntityType << "]" ); // DEBUG
            double HalfWinWidth = RendererFactory.GetInstance().WindowWidth / 2;
            double HalfWinHeight = RendererFactory.GetInstance().WindowHeight / 2;
    
            Vector3 ScaleAvAxes = null;
            if( bAltAxes )
            {
                ScaleAvAxes = new Vector3( 
                    - ((double)(y - editing3d.iDragStartY)) / HalfWinWidth * 1.0,
                    ((double)(x - editing3d.iDragStartX)) / HalfWinWidth * 1.0,
                    0
                );
            }
            else
            {
                ScaleAvAxes = new Vector3( 
                        0,
                    ((double)(x - editing3d.iDragStartX)) / HalfWinWidth * 1.0,
                    - ((double)(y - editing3d.iDragStartY)) / HalfWinWidth * 1.0
                );
            }
    
            Vector3 CurrentScaleWorldAxes = editing3d.startscale * entity.rot.Inverse();
            Vector3 CurrentScaleAvatarAxes = CurrentScaleWorldAxes * entity.rot;
    
            Vector3 ScaleNewScaleAvAxes = new Vector3(
                ( 1 + ScaleAvAxes.x ) * CurrentScaleAvatarAxes.x,
                ( 1 + ScaleAvAxes.y ) * CurrentScaleAvatarAxes.y,
                ( 1 + ScaleAvAxes.z ) * CurrentScaleAvatarAxes.z
            );
    
            Vector3 NewScaleWorldAxes = ScaleNewScaleAvAxes * rInverseOurRot;
            Vector3 NewScaleSeenByPrim = NewScaleWorldAxes * entity.rot;
    
            if( NewScaleSeenByPrim.x < 0 )
            {
                NewScaleSeenByPrim.x = 0.05;
            }
            if( NewScaleSeenByPrim.y < 0 )
            {
                NewScaleSeenByPrim.y = 0.05;
            }
            if( NewScaleSeenByPrim.z < 0 )
            {
                NewScaleSeenByPrim.z = 0.05;
            }
    
            entity.scale = NewScaleSeenByPrim;
            // DEBUG(  "setting new scale " << NewScaleSeenByPrim ); // DEBUG
        }
        
        public void InitiateFreeEdit(int mousex, int mousey )
        {
            editing3d.EditingPreliminaries();
        
            Entity entity = selectionmodel.GetFirstSelectedEntity();
        
            if( entity != null )
            {
                editing3d.iDragStartX = mousex;
                editing3d.iDragStartY = mousey;
                editing3d.startscale = entity.scale;
                editing3d.currentedittype = Editing3d.EditType.ScaleFree;
            }
            //DEBUG(  "initializing StartPos " << editing3d.startscale ); // DEBUG
        }
        
        public void InitiateHandleEdit( int mousex, int mousey, Axis axis )
        {
            editing3d.EditingPreliminaries();
        
            Entity entity = selectionmodel.GetFirstSelectedEntity();
        
            if( entity != null )
            {
                editing3d.iDragStartX = mousex;
                editing3d.iDragStartY = mousey;
                editing3d.startscale = entity.scale;
                editing3d.startpos = entity.pos;
                //DEBUG(  "initializing editing3d.startscale " << editing3d.startscale ); // DEBUG
    
                editing3d.currentaxis = axis;
                editing3d.currentedittype = Editing3d.EditType.ScaleHandle;
            }
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
            
            graphics.DrawCube();
            
            graphics.PopMatrix();
        }
        
        public void DrawEditHandles( Vector3 entityscale, double distance )
        {
            Vector3 handlescale = new Vector3( 0.18, 0.18, 0.18 ) * ( distance / 10d);
        
            DrawSingleEditHandle( entityscale, handlescale, Axis.PosX );
            DrawSingleEditHandle( entityscale, handlescale, Axis.NegX );
            DrawSingleEditHandle( entityscale, handlescale, Axis.PosY );
            DrawSingleEditHandle( entityscale, handlescale, Axis.NegY );
            DrawSingleEditHandle( entityscale, handlescale, Axis.PosZ );
            DrawSingleEditHandle( entityscale, handlescale, Axis.NegZ );
        }
    }
}
