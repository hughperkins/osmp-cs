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
//! \brief Handles camera movement on client, such as pans, orbits etc

using System;
using System.Collections;
using MathGl;
using SdlDotNet;
using Metaverse.Utility;

namespace OSMP
{
    public class Camera
    {
        public enum CameraMoveType
        {
             None,
             AltZoom,
             Orbit,
             Pan
        }
        
        public enum Viewpoint
        {
            MouseLook
            //BehindPlayer,
            //ThirdParty
        }
        
        public Viewpoint viewpoint = Viewpoint.MouseLook;
    
        CameraMoveType CurrentMove; //!< current movetype, eg PAN or ORBIT
        
        public bool bRoamingCameraEnabled;  //!< if camera is enabled (otherwise, just normal avatar view)
        public Vector3 RoamingCameraPos = new Vector3();  //!< position of camera
        public Rot RoamingCameraRot = new Rot();  //!< rotation of camera

        public Vector3 CameraPos
        {
            get
            {
                UpdateCamera();
                return camerapos;
            }
        }
        public Rot CameraRot
        {
            get
            {
                UpdateCamera();
                return camerarot;
            }
        }

        Vector3 camerapos;
        Rot camerarot;
            
        int iDragStartx;
        int iDragStarty;
        
        Vector3 AltZoomCentrePos = new Vector3();
    
        double fAltZoomStartRotateZAxis;
        double fAltZoomStartRotateYAxis;
        double fAltZoomStartZoom;
    
        double fAltZoomRotateZAxis;
        double fAltZoomRotateYAxis;
        double fAltZoomZoom;
        
        public double fThirdPartyViewZoom;  //!< used in third party view (f9 twice).  Distance from avatar
        public double fThirdPartyViewRotate;   //!< used in third party view (f9 twice).  Rotation around avatar        
        
        static Camera instance = new Camera();
        public static Camera GetInstance()
        {
            return instance;
        }

        //const string CMD_ZOOM = "camerazoom";
        //const string CMD_PAN = "camerapan";
        //const string CMD_ORBIT = "cameraorbit";

        public Camera()
        {
            MouseCache mousefiltermousecache = MouseCache.GetInstance();

            ViewerState.GetInstance().StateChanged += new ViewerState.StateChangedHandler(Camera_StateChanged);

          //  CommandCombos.GetInstance().RegisterCommandGroup(
            //    new string[]{ CMD_ZOOM, CMD_PAN, CMD_ORBIT }, new KeyCommandHandler(CameraModeHandler));
            //CommandCombos.GetInstance().RegisterCommand(
              //  "cameraorbit", new KeyCommandHandler(CameraModeOrbitHandler));
            //CommandCombos.GetInstance().RegisterCommand(
              //  "camerapan", new KeyCommandHandler(CamerModePanHandler));
            CommandCombos.GetInstance().RegisterAtLeastCommand(
                "toggleviewpoint", new KeyCommandHandler(ToggleViewpointHandler));
            CommandCombos.GetInstance().RegisterAtLeastCommand(
                "leftmousebutton", new KeyCommandHandler(MouseDown));
            MouseCache.GetInstance().MouseMove += new MouseMoveHandler(Camera_MouseMove);
              
            RendererFactory.GetInstance().PreDrawEvent += new PreDrawCallback(Camera_PreDrawEvent);
        }

        void Camera_StateChanged(ViewerState.ViewerStateEnum neweditstate, ViewerState.ViewerStateEnum newviewstate)
        {
            switch (newviewstate)
            {
                case ViewerState.ViewerStateEnum.CameraOrbit:
                    CurrentMove = CameraMoveType.Orbit;
                    break;

                case ViewerState.ViewerStateEnum.CameraPan:
                    CurrentMove = CameraMoveType.Pan;
                    break;

                case ViewerState.ViewerStateEnum.CameraZoom:
                    CurrentMove = CameraMoveType.AltZoom;
                    break;

                default:
                    CurrentMove = CameraMoveType.Pan;
                    break;
            }
        }

        void Camera_PreDrawEvent()
        {
            ApplyCamera();
        }

        public void ToggleViewpointHandler(string command, bool down)
        {
            if( down )
            {
                Test.Debug(  "toggling viewpoint..." ); // Test.Debug
                // viewpoint = (Viewpoint)(( (int)viewpoint + 1 ) % 3 );
                viewpoint = (Viewpoint)(( (int)viewpoint + 1 ) % 1 );  // disactivating third viewpoint for now (since we cant see avatar at moment...)
            }
        }
        
        public void MouseDown( string command, bool down )
        {
            //if (ViewerState.GetInstance().CurrentViewState == ViewerState.ViewerStateEnum.RoamingCamera)
            //{
                switch (CurrentMove)
                {
                    case CameraMoveType.AltZoom:
                        InitiateZoomCamera(MouseCache.GetInstance().MouseX, MouseCache.GetInstance().MouseY);
                        break;
                    case CameraMoveType.Orbit:
                        InitiateOrbitCamera(MouseCache.GetInstance().MouseX, MouseCache.GetInstance().MouseY);
                        break;
                    case CameraMoveType.Pan:
                        InitiatePanCamera(MouseCache.GetInstance().MouseX, MouseCache.GetInstance().MouseY);
                        break;
                }
            //}
        }
        void Camera_MouseMove()
        {
            //if (ViewerState.GetInstance().CurrentViewState == ViewerState.ViewerStateEnum.RoamingCamera)
            //{
                switch (CurrentMove)
                {
                    case CameraMoveType.AltZoom:
                        UpdateAltZoomCamera(MouseCache.GetInstance().MouseX, MouseCache.GetInstance().MouseY);
                        break;
                    case CameraMoveType.Orbit:
                        UpdateOrbitCamera(MouseCache.GetInstance().MouseX, MouseCache.GetInstance().MouseY);
                        break;
                    case CameraMoveType.Pan:
                        UpdatePanCamera(MouseCache.GetInstance().MouseX, MouseCache.GetInstance().MouseY);
                        break;
                }
            //}
        }
    
        public void InitiateOrbitSlashAltZoom( int imousex, int imousey, CameraMoveType eMoveType )
        {
        }
        
        public void GetCurrentCameraFromAltZoomCamera()
        {
            Rot RotationAboutZAxis =  mvMath.AxisAngle2Rot( mvMath.ZAxis, mvMath.Pi - fAltZoomRotateZAxis );
            Rot RotationAboutYAxis = mvMath.AxisAngle2Rot( mvMath.YAxis, fAltZoomRotateYAxis );
        
            Rot CombinedZYRotation = RotationAboutZAxis * RotationAboutYAxis;
        
            double DeltaZ = fAltZoomZoom * Math.Sin( fAltZoomRotateYAxis );
            RoamingCameraPos.z = AltZoomCentrePos.z + DeltaZ;
        
            double DistanceInXYPlane = fAltZoomZoom * Math.Cos( fAltZoomRotateYAxis );
            RoamingCameraPos.x = AltZoomCentrePos.x + DistanceInXYPlane * Math.Cos( fAltZoomRotateZAxis );
            RoamingCameraPos.y = AltZoomCentrePos.y - DistanceInXYPlane * Math.Sin( fAltZoomRotateZAxis );
        
            RoamingCameraRot = CombinedZYRotation;
        }
        
        public void UpdatePanCamera( int imousex, int imousey )
        {
        }
        
        public void UpdateOrbitCamera( int imousex, int imousey )
        {
            if( CurrentMove == CameraMoveType.Orbit )
            {
                fAltZoomRotateZAxis = (double)( imousex - iDragStartx ) / 20.0f / 10.0f + fAltZoomStartRotateZAxis;
                fAltZoomRotateYAxis = (double)( imousey - iDragStarty ) / 20.0f / 10.0f + fAltZoomStartRotateYAxis;
            }
            else if( CurrentMove == CameraMoveType.AltZoom )
            {
                fAltZoomStartRotateZAxis = fAltZoomRotateZAxis;
                fAltZoomStartRotateYAxis = fAltZoomRotateYAxis;
                fAltZoomStartZoom = fAltZoomZoom;
        
                iDragStartx = imousex;
                iDragStarty = imousey;
                CurrentMove = CameraMoveType.Orbit;
            }
            else if( CurrentMove == CameraMoveType.None )
            {}
        
            GetCurrentCameraFromAltZoomCamera();
        }
        
        public void UpdateAltZoomCamera( int imousex, int imousey )
        {
            if( CurrentMove == CameraMoveType.AltZoom )
            {
                // Test.Debug(  " updatealtzoom " << imousex << " " << imousey ); // Test.Debug
                fAltZoomRotateZAxis = (double)( imousex - iDragStartx ) / 20.0f / 10.0f + fAltZoomStartRotateZAxis;
                fAltZoomZoom = (double)( imousey - iDragStarty ) / 20.0f  + fAltZoomStartZoom;
            }
            else if( CurrentMove == CameraMoveType.Orbit )
            {
                fAltZoomStartRotateZAxis = fAltZoomRotateZAxis;
                fAltZoomStartRotateYAxis = fAltZoomRotateYAxis;
                fAltZoomStartZoom = fAltZoomZoom;
        
                iDragStartx = imousex;
                iDragStarty = imousey;
                CurrentMove = CameraMoveType.AltZoom;
            }
            else if( CurrentMove == CameraMoveType.None )
            {
            }
        
            GetCurrentCameraFromAltZoomCamera();
        }
        
        public void InitiatePanCamera( int imousex, int imousey )
        {
            CurrentMove = CameraMoveType.Pan;
            iDragStartx = imousex;
            iDragStarty = imousey;
        }
        
        public void InitiateOrbitCamera( int imousex, int imousey )
        {
            InitiateOrbitSlashAltZoom( imousex, imousey, CameraMoveType.Orbit );
        }
        
        public void InitiateZoomCamera( int imousex, int imousey )
        {
            InitiateOrbitSlashAltZoom( imousex, imousey, CameraMoveType.AltZoom );
        }
        
        public void CancelRoamingCamera()
        {
            bRoamingCameraEnabled = false;
        }
        
        public void CameraMoveDone()
        {
            CurrentMove = CameraMoveType.None;
        }

        void UpdateCamera()
        {
            PlayerMovement playermovement = PlayerMovement.GetInstance();

            if (bRoamingCameraEnabled)
            {
                camerapos = RoamingCameraPos;
                camerarot = RoamingCameraRot;
            }
            else if (viewpoint == Viewpoint.MouseLook)
            {
                camerapos = playermovement.avatarpos;
                camerarot = 
                    mvMath.AxisAngle2Rot(mvMath.ZAxis, playermovement.avatarzrot * Math.PI / 180) *
                    mvMath.AxisAngle2Rot(mvMath.YAxis, playermovement.avataryrot * Math.PI / 180)
                    ;
                //cameramatrix.applyRotate(-playermovement.avataryrot, 0f, 1f, 0f);
                //cameramatrix.applyRotate(-playermovement.avatarzrot, 0f, 0f, 1f);
                //cameramatrix.applyTranslate(-playermovement.avatarpos.x, -playermovement.avatarpos.y, -playermovement.avatarpos.z);
            }
                /*
            else if (viewpoint == Viewpoint.BehindPlayer)
            {
                cameramatrix.applyRotate(-18f, 0f, 1f, 0f);

                // Vector3 V = new Vector3( 0, playermovement.avataryrot * mvMath.PiOver180, playermovement.avatarzrot * mvMath.PiOver180 );

                cameramatrix.applyTranslate(3.0f, 0.0f, -1.0f);

                cameramatrix.applyRotate(-(float)playermovement.avataryrot, 0f, 1f, 0f);
                cameramatrix.applyRotate(-(float)playermovement.avatarzrot, 0f, 0f, 1f);

                cameramatrix.applyTranslate(-playermovement.avatarpos.x, -playermovement.avatarpos.y, -playermovement.avatarpos.z);
            }
            else if (viewpoint == Viewpoint.ThirdParty)
            {
                cameramatrix.applyRotate(-18f, 0f, 1f, 0f);
                cameramatrix.applyRotate(-90f, 0f, 0f, 1f);

                cameramatrix.applyTranslate(0.0, -fThirdPartyViewZoom, fThirdPartyViewZoom / 3.0);
                cameramatrix.applyRotate(-fThirdPartyViewRotate, 0f, 0f, 1f);
                cameramatrix.applyTranslate(-playermovement.avatarpos.x, -playermovement.avatarpos.y, -playermovement.avatarpos.z);
            }
            */
        }
        
        public void ApplyCamera()
        {
            UpdateCamera();

            // rotate so z axis is up, and x axis is forward
            
            PlayerMovement playermovement = PlayerMovement.GetInstance();
            
            GraphicsHelperGl g = new GraphicsHelperGl();

            g.Rotate(90, 0, 0, 1);
            g.Rotate(90, 0, 1, 0);

            Rot inversecamerarot = camerarot.Inverse();
            //inversecamerarot.Inverse();
            g.Rotate(inversecamerarot);
            //g.Rotate(camerarot);

            g.Translate(-camerapos);
        }
        
        
    }    
}
