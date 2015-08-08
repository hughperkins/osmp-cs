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
using Tao.OpenGl;
using Metaverse.Utility;

namespace OSMP
{

    //! Manages 3d Rotation editing
    //!
    //! What this module can do:
    //! - Draw Rotation Edit Handles:
    //!    - Call DrawRotEditHandles, passing in the object's scale
    //!
    //! - manage a constrained edit:
    //!   - call InitiateHandleRotEdit, passing in the current mouse x and y, and the axis it is constrained to
    //!   - when the mouse moves, call InteractiveHandleRotEdit, passing in the new mouse coordinates, and the constraint axis
    //!   - the module will update the Rot of the selected object
    //!
    //! - manage a free edit:
    //!   - call InitiateFreeRotEdit, passing in the current mouse x and y
    //!   - when the mouse moves, call InteractiveHandleRotEdit, passing in the new mouse coordinates
    //!   - the module will update the Rot of the selected object
    
    class Editing3dRot
    {
        Editing3d editing3d;
        SelectionModel selectionmodel;
        Camera camera;
        IGraphicsHelper graphics;
        
        double fStartRotationAngle;
           
        public Editing3dRot( Editing3d editing3d )
        {
            this.editing3d = editing3d;
            selectionmodel = SelectionModel.GetInstance();
            camera = Camera.GetInstance();
            graphics = GraphicsHelperFactory.GetInstance();
        }
        
        public void InteractiveFreeEdit( int x, int y )
        {
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
        
            Rot rInverseOurRot = OurRot.Inverse();
        
            double HalfWinWidth = RendererFactory.GetInstance().WindowWidth / 2;
            double HalfWinHeight = RendererFactory.GetInstance().WindowHeight / 2;
        
            double zRoll =  ( x - editing3d.iDragStartX ) / HalfWinWidth;
            double yRoll = - ( y - editing3d.iDragStartY ) / HalfWinHeight;
            double amount = Math.Sqrt( (double)( ( editing3d.iDragStartX - x ) * ( editing3d.iDragStartX - x ) +
                ( editing3d.iDragStartY - y ) * ( editing3d.iDragStartY - y ) ) ) * mvMath.Pi2 / HalfWinHeight;
        
            Vector3 ArcBallAxis = new Vector3( 0, yRoll, zRoll );
            Rot ArcBallRot = mvMath.AxisAngle2Rot( ArcBallAxis, amount );
        
            Rot rTransposedToAv = rInverseOurRot * editing3d.startrot;
            Rot rRotated = ArcBallRot * rTransposedToAv;
            Rot rNewRot = OurRot * rRotated;

            selectionmodel.GetFirstSelectedEntity().rot = rNewRot;
            MetaverseClient.GetInstance().worldstorage.OnModifyEntity(selectionmodel.GetFirstSelectedEntity());
        }
        
        double GetRotationAngleForEntityAndMouse( Vector3 EntityVector3, Rot EntityRot, Axis axis, int mousex, int mousey )
        {
            double fRotationAngle = 0;
            
            bool bRotAngleGot = false;
        
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
                    return 0;
                }
            }
            Rot rInverseOurRot = OurRot.Inverse();
        
            double fDistanceFromUsToEntity = ( EntityVector3 - OurPos ).Det();
            double fScalingFromPosToScreen = graphics.GetScalingFrom3DToScreen( fDistanceFromUsToEntity );
        
            Vector3 ScreenEntityPos = graphics.GetScreenPos( OurPos, OurRot, EntityVector3 );
            Vector3 ScreenMousePos = new Vector3(
                0,
                RendererFactory.GetInstance().WindowWidth - mousex,
                RendererFactory.GetInstance().WindowHeight - mousey );
        
            // mousepoint is a point on the mouseray into the screen, with x = entity.pos.x
            Vector3 ScreenVectorEntityToMousePoint = ScreenMousePos - ScreenEntityPos;
            Vector3 VectorEntityToMousePointObserverAxes = ScreenVectorEntityToMousePoint * ( 1.0f / fScalingFromPosToScreen );
        
            //Test.Debug(  " screenobjectpos: " + ScreenEntityPos + " screenmousepos: " + ScreenMousePos + " objecttomouse: " + ScreenVectorEntityToMouse ); // Test.Debug
        
            Vector3 RotationAxisEntityAxes = axis.ToVector();        
            Rot rInverseEntityRot = EntityRot.Inverse();
        
            Vector3 RotationAxisWorldAxes = RotationAxisEntityAxes * rInverseEntityRot;
            //    Test.Debug(  " RotationAxisWorldAxes " + RotationAxisWorldAxes ); // Test.Debug
        
            Vector3 RotationAxisObserverAxes = RotationAxisWorldAxes * OurRot;
            RotationAxisObserverAxes.Normalize();
        
            double DistanceOfRotationPlaneFromOrigin = 0; // Lets move right up to the object
            // we're going to imagine a ray from the MousePoint going down the XAXIS, away from us
            // we'll intersect this ray with the rotation plane to get the point on the rotation plane
            // where we can consider the mouse to be.
            double fVectorDotRotationAxisObserverAxesWithXAxis = Vector3.DotProduct( RotationAxisObserverAxes, mvMath.XAxis );
            if( Math.Abs( fVectorDotRotationAxisObserverAxesWithXAxis ) > 0.0005 )
            {
                double fDistanceFromMousePointToRotationPlane = ( DistanceOfRotationPlaneFromOrigin - 
                    Vector3.DotProduct( RotationAxisObserverAxes, VectorEntityToMousePointObserverAxes ) )
                        / fVectorDotRotationAxisObserverAxesWithXAxis;
                //  Test.Debug(  " fDistanceFromMousePointToRotationPlane " + fDistanceFromMousePointToRotationPlane ); // Test.Debug
        
                Vector3 VectorMouseClickOnRotationPlaneObserverAxes = new Vector3(
                    fDistanceFromMousePointToRotationPlane,
                    VectorEntityToMousePointObserverAxes.y,
                    VectorEntityToMousePointObserverAxes.z );
                //    Test.Debug(  " VectorMouseClickOnRotationPlaneObserverAxes " + VectorMouseClickOnRotationPlaneObserverAxes ); // Test.Debug
                // We'll rotate this vector into object axes
        
                Vector3 VectorMouseClickOnRotationPlaneWorldAxes = VectorMouseClickOnRotationPlaneObserverAxes * rInverseOurRot;
                Vector3 VectorMouseClickOnRotationPlaneEntityAxes = VectorMouseClickOnRotationPlaneWorldAxes * EntityRot;        
                //     Test.Debug(  " VectorMouseClickOnRotationPlaneEntityAxes " + VectorMouseClickOnRotationPlaneEntityAxes ); // Test.Debug
        
                // now we work out rotation angle
                double fDistanceOfPointFromOrigin;
                if( axis.IsXAxis )
                {
                    fDistanceOfPointFromOrigin = Math.Sqrt( VectorMouseClickOnRotationPlaneEntityAxes.z * VectorMouseClickOnRotationPlaneEntityAxes.z
                                                       + VectorMouseClickOnRotationPlaneEntityAxes.y * VectorMouseClickOnRotationPlaneEntityAxes.y );
                    //         Test.Debug(  "Z axis distnace of point from origin: " + fDistanceOfPointFromOrigin ); // Test.Debug
        
                    if( Math.Abs( fDistanceOfPointFromOrigin ) > 0.0005 )
                    {
                        fRotationAngle = - Math.Asin( VectorMouseClickOnRotationPlaneEntityAxes.y / fDistanceOfPointFromOrigin );
                        if( VectorMouseClickOnRotationPlaneEntityAxes.z < 0 )
                        {
                            fRotationAngle = mvMath.Pi - fRotationAngle;
                        }
                        //             Test.Debug(  "************RotANGLE: " + fRotationAngle ); // Test.Debug
                        bRotAngleGot = true;
                    }
                }
                else if( axis.IsYAxis )
                {
                    fDistanceOfPointFromOrigin = Math.Sqrt( VectorMouseClickOnRotationPlaneEntityAxes.z * VectorMouseClickOnRotationPlaneEntityAxes.z
                                                       + VectorMouseClickOnRotationPlaneEntityAxes.x * VectorMouseClickOnRotationPlaneEntityAxes.x );
                    //         Test.Debug(  "Z axis distnace of point from origin: " + fDistanceOfPointFromOrigin ); // Test.Debug
        
                    if( Math.Abs( fDistanceOfPointFromOrigin ) > 0.0005 )
                    {
                        fRotationAngle = Math.Asin( VectorMouseClickOnRotationPlaneEntityAxes.x / fDistanceOfPointFromOrigin );
                        if( VectorMouseClickOnRotationPlaneEntityAxes.z < 0 )
                        {
                            fRotationAngle = mvMath.Pi - fRotationAngle;
                        }
                        //             Test.Debug(  "************RotANGLE: " + fRotationAngle ); // Test.Debug
                        bRotAngleGot = true;
                    }
                }
                else
                {
                    fDistanceOfPointFromOrigin = Math.Sqrt( VectorMouseClickOnRotationPlaneEntityAxes.x * VectorMouseClickOnRotationPlaneEntityAxes.x
                                                       + VectorMouseClickOnRotationPlaneEntityAxes.y * VectorMouseClickOnRotationPlaneEntityAxes.y );
                    //         Test.Debug(  "Z axis distnace of point from origin: " + fDistanceOfPointFromOrigin ); // Test.Debug
        
                    if( Math.Abs( fDistanceOfPointFromOrigin ) > 0.0005 )
                    {
                        fRotationAngle = Math.Asin( VectorMouseClickOnRotationPlaneEntityAxes.y / fDistanceOfPointFromOrigin );
                        if( VectorMouseClickOnRotationPlaneEntityAxes.x < 0 )
                        {
                            fRotationAngle = mvMath.Pi - fRotationAngle;
                        }
                        //             Test.Debug(  "************RotANGLE: " + fRotationAngle ); // Test.Debug
                        bRotAngleGot = true;
                    }
                }
            }
        
            if( bRotAngleGot )
            {
                //fRotationAngle = fRotAngle;
                return fRotationAngle;
            }
            else
            {
                return 0;
            }
        }
        
        // Maths description:
        //
        // take mousecoordinates, convert to 3d vector in screen coordinates
        // take object coordinates, convert to screen coordinates using OpenGL Feedback mode
        // now we have a vector, in screen coordinates, from the object to the mouse
        // convert this vector into OpenGL observer coordinates by dividing by the scaling factor from OpenGl.screen (see pos handles maths desscdription)
        //
        // now we imagine a line/ray running along our x-axis, in observer coordinates, through the point we we imagined the mouse clicked
        // we put the origin at the position of the object
        // we take the object axis according tot he handle we clicked on (XAXIS, YAXIS or ZAXIS) and multiple it by InverseEntityStartRot to convert from Entity coordinates into world coordinates
        // then we multiple by ObserverRot to convert into observer coordinates/axes
        // we normalize, which gives us the normal of a plane running through the origin perpendicular to the rotation axis, in observer coordinates
        // we use the maths from http://nehe.gamedev.net collision tutorial to intersect our mouseclick ray with this plane
        //
        // now we have a vector from the object to a point on the rotation plane
        // we multiply by EntityStartRot to move into object axes/coordinates
        // so the vector will now be in the x-y, x-z or y-z plane, depending on which handle we are dragging
        // we do a quick asin, trig according to which handle we are dragging,
        // and now we have the rotation angle
        //
        // when we first start dragging, we do this maths and store the start rotation angle
        // then we just find the difference between teh current rotation angle and the start one to get the rotation angle chnage
        // quick axisangle2rot and rotmultiply and now we have our new rot.
        
        public void InteractiveHandleEdit( Axis axis, int mousex, int mousey )
        {
            Entity entity = selectionmodel.GetFirstSelectedEntity();
        
            Vector3 OurPos;
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
            }
            Rot rInverseOurRot = OurRot.Inverse();
        
            if( entity != null )
            {
                double fCurrentRotationAngle = GetRotationAngleForEntityAndMouse( editing3d.startpos, editing3d.startrot, axis, mousex, mousey );
                double fRotateAngleChange = fCurrentRotationAngle - fStartRotationAngle;
                // Test.Debug(  "Rotation angel change: " + fRotateAngleChange ); // Test.Debug
    
                Vector3 RotationAxisEntityAxes = axis.ToVector();
    
                Rot RotationChangeEntityAxes = mvMath.AxisAngle2Rot( RotationAxisEntityAxes, fRotateAngleChange );
                //Test.Debug(  " RotationChangeEntityAxes " + RotationChangeEntityAxes ); // Test.Debug
    
                Rot rInverseStartRot = editing3d.startrot.Inverse();
    
                Rot NewRotation = editing3d.startrot * RotationChangeEntityAxes;
                // Test.Debug(  " NewRotation " + NewRotation ); // Test.Debug
            
                //Test.Debug(  " NewRotation " + NewRotationWorldAxes ); // Test.Debug
                entity.rot = NewRotation;
                MetaverseClient.GetInstance().worldstorage.OnModifyEntity(entity);
            }
        }
        
        public void InitiateFreeEdit( int mousex, int mousey )
        {
            Entity entity = selectionmodel.GetFirstSelectedEntity();
            if( entity != null )
            {
                editing3d.EditingPreliminaries();
            
                editing3d.startrot = entity.rot;
                Test.Debug(  "initializing StartRot " + editing3d.startrot.ToString() ); // Test.Debug
                //    mvKeyboardAndMouse::bDragging = true;
                editing3d.iDragStartX = mousex;
                editing3d.iDragStartY = mousey;
            
                editing3d.currentedittype = Editing3d.EditType.RotFree;
            }
        }
        
        public void InitiateHandleEdit( int mousex, int mousey, Axis axis )
        {
            editing3d.EditingPreliminaries();
        
            Entity entity = selectionmodel.GetFirstSelectedEntity();
        
            if( entity != null )
            {
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
                Rot rInverseOurRot = OurRot.Inverse();
        
                double fCurrentRotationAngle;
        
                editing3d.startpos = entity.pos;
                editing3d.startrot = entity.rot;
                fCurrentRotationAngle = GetRotationAngleForEntityAndMouse( editing3d.startpos, editing3d.startrot, axis, mousex, mousey );
                
                editing3d.iDragStartX = mousex;
                editing3d.iDragStartY = mousey;
                fStartRotationAngle = fCurrentRotationAngle;
                Test.Debug(  "initializing StartRot " + editing3d.startrot.ToString() +  " rotation angle " + fCurrentRotationAngle.ToString() ); // Test.Debug
    
                editing3d.currentaxis = axis;
                editing3d.currentedittype = Editing3d.EditType.RotHandle;
            }
        }
        
        void DrawSelectionHandle()
        {
            graphics.PushMatrix();
        
            graphics.Scale( 1.5, 1.5, 1.5 );
            Glu.GLUquadric quadratic = Glu.gluNewQuadric();   // Create A Pointer To The Quadric Entity
        
            Glu.gluQuadricNormals(quadratic, Glu.GLU_SMOOTH); // Create Smooth Normals
        
            Glu.gluQuadricOrientation( quadratic, Glu.GLU_OUTSIDE );
            graphics.Translate(0.0f,0.0f,-0.025f);   // Center The Cylinder
            Glu.gluCylinder(quadratic,0.5f,0.5f,0.05f,20,3);
        
            Glu.gluQuadricOrientation( quadratic, Glu.GLU_INSIDE );
            Glu.gluCylinder(quadratic,0.48f,0.5f,0.05f,20,3);
        
            Glu.gluQuadricOrientation( quadratic, Glu.GLU_OUTSIDE );
        
            graphics.Rotate( 180.0f, 1.0f, 0.0f, 0.0f );
            Glu.gluDisk(quadratic,0.48f,0.5f,20,3);
        
            graphics.Rotate( 180.0f, 1.0f, 0.0f, 0.0f );
            graphics.Translate(0.0f,0.0f,0.05f);
            Glu.gluDisk(quadratic,0.48f,0.5f,20,3);
        
        
            graphics.PopMatrix();
        }
        
        public void DrawEditHandles( Vector3 entityscale )
        {
            // + x
            graphics.SetMaterialColor( editing3d.GetEditHandleColor( Axis.PosX ) );
            //graphics.SetMaterialColor( new double[]{ 1.0, 0.0, 0.0, 0.5 } );
            RendererFactory.GetPicker3dModel().AddHitTarget( new HitTargetEditHandle( Axis.PosX ) );
            graphics.PushMatrix();
            graphics.Scale( entityscale );
            graphics.Rotate( 90f, 0f, 1f, 0f );
            DrawSelectionHandle();
            graphics.PopMatrix();
        
            // + y
            graphics.SetMaterialColor( editing3d.GetEditHandleColor( Axis.PosY ) );
            RendererFactory.GetPicker3dModel().AddHitTarget( new HitTargetEditHandle( Axis.PosY ) );
            graphics.PushMatrix();
            graphics.Scale( entityscale );
            graphics.Rotate( 90, 1, 0, 0 );
            DrawSelectionHandle();
            graphics.PopMatrix();
        
            // + z
            graphics.SetMaterialColor( editing3d.GetEditHandleColor( Axis.PosZ ) );
            RendererFactory.GetPicker3dModel().AddHitTarget( new HitTargetEditHandle( Axis.PosZ ) );
            graphics.PushMatrix();
            graphics.Scale( entityscale );
            //Gl.glRotatef( 90, 0, 1, 0 );
            DrawSelectionHandle();
            graphics.PopMatrix();
        }
    }
}
