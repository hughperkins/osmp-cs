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
//! \brief This module is responsible for moving one's own avatar around

using System;
using System.Collections;
using Metaverse.Utility;
using System.Xml;

namespace OSMP
{
    public class PlayerMovement
    {
        Vector3 currentvelocity = new Vector3();
        TimeKeeper timekeeper;
            
        Camera camera;
        
        Vector3 WorldBoundingBoxMin;
        Vector3 WorldBoundingBoxMax;

        public Vector3 avatarpos = new Vector3();
        public Rot avatarrot = new Rot();

        public bool bAvatarMoved;  //! If avatar has moved (so it should be synchronized to server)
        
        double fAvatarAcceleration;
        double fAvatarTurnSpeed;  //! turnspeed of avatar (constant)
        double fAvatarMoveSpeed;   //! movespeed of avatar (constant)
        double fVerticalMoveSpeed;
        double fDeceleration;

        public double avatarradius = 2;
        
        public bool kMovingLeft;    //!< set by keyboardandmouse, and used by playermovement to set current player object velocity
        public bool kMovingRight;//!< set by keyboardandmouse, and used by playermovement to set current player object velocity
        public bool kMovingForwards;//!< set by keyboardandmouse, and used by playermovement to set current player object velocity
        public bool kMovingBackwards;//!< set by keyboardandmouse, and used by playermovement to set current player object velocity
        public bool kMovingUpZAxis;//!< set by keyboardandmouse, and used by playermovement to set current player object velocity
        public bool kMovingDownZAxis;//!< set by keyboardandmouse, and used by playermovement to set current player object velocity
        
        public bool bJumping;//!< set by keyboardandmouse, and used by playermovement to set current player object velocity
        
        public double avatarzrot;
        public double avataryrot;    //!< avatar z and y rot, used to get avatar rotation
        
        static PlayerMovement instance = new PlayerMovement();
        public static PlayerMovement GetInstance()
        {
            return instance;
        }
        
        public void MoveLeft( string command, bool down )
        {
            Test.WriteOut("playermovement.moveleft");
            kMovingLeft = down;
        }
        public void MoveRight(string command, bool down)
        {
            kMovingRight = down;
        }
        public void MoveForwards(string command, bool down)
        {
            kMovingForwards = down;
        }
        public void MoveBackwards(string command, bool down)
        {
            kMovingBackwards = down;
        }
        public void MoveUp(string command, bool down)
        {
            kMovingUpZAxis = down;
        }
        public void MoveDown(string command, bool down)
        {
            kMovingDownZAxis = down;
        }
        public PlayerMovement()
        {
            Test.Debug("instantiating PlayerMovement()");
            
            Config config = Config.GetInstance();
            //XmlElement minnode = (XmlElement)config.clientconfig.SelectSingleNode("worldboundingboxmin");
            //XmlElement maxnode =(XmlElement) config.clientconfig.SelectSingleNode("worldboundingboxmax");
            WorldBoundingBoxMin = new Vector3(0,0,config.mingroundheight );
            WorldBoundingBoxMax = new Vector3( config.world_xsize, config.world_ysize, config.ceiling );
            Test.WriteOut( WorldBoundingBoxMin );
            Test.WriteOut( WorldBoundingBoxMax );
            
            XmlElement movementnode = (XmlElement)config.clientconfig.SelectSingleNode("movement");
            fAvatarAcceleration = Convert.ToDouble( movementnode.GetAttribute("acceleration") );
            fAvatarTurnSpeed = Convert.ToDouble( movementnode.GetAttribute("turnspeed") );
            fAvatarMoveSpeed = Convert.ToDouble( movementnode.GetAttribute("movespeed") );
            fVerticalMoveSpeed = Convert.ToDouble( movementnode.GetAttribute("verticalmovespeed") );
            fDeceleration = Convert.ToDouble( movementnode.GetAttribute("deceleration") );

            camera = Camera.GetInstance();
            //KeyFilterComboKeys keyfiltercombokeys = KeyFilterComboKeys.GetInstance();

            CommandCombos.GetInstance().RegisterAtLeastCommand(
                "moveleft", new KeyCommandHandler(MoveLeft));
            CommandCombos.GetInstance().RegisterAtLeastCommand(
                "moveright", new KeyCommandHandler(MoveRight));
            CommandCombos.GetInstance().RegisterAtLeastCommand(
                "movebackwards", new KeyCommandHandler(MoveBackwards));
            CommandCombos.GetInstance().RegisterAtLeastCommand(
                "moveforwards", new KeyCommandHandler(MoveForwards));
            CommandCombos.GetInstance().RegisterAtLeastCommand(
                "moveup", new KeyCommandHandler(MoveUp));
            CommandCombos.GetInstance().RegisterAtLeastCommand(
                "movedown", new KeyCommandHandler(MoveDown));

            ViewerState.GetInstance().StateChanged += new ViewerState.StateChangedHandler(PlayerMovement_StateChanged);
            MouseCache.GetInstance().MouseMove += new MouseMoveHandler(PlayerMovement_MouseMove);
            MetaverseClient.GetInstance().worldstorage.terrainmodel.TerrainModified += new TerrainModel.TerrainModifiedHandler( terrainmodel_TerrainModified );

            timekeeper = new TimeKeeper();
            
            Test.Debug("PlayerMovement instantiated");
        }

        void terrainmodel_TerrainModified()
        {
            TerrainModel terrainmodel = MetaverseClient.GetInstance().worldstorage.terrainmodel;
            WorldBoundingBoxMin = new Vector3( 0, 0, terrainmodel.MinHeight );
            WorldBoundingBoxMax = new Vector3( terrainmodel.MapWidth, terrainmodel.MapHeight, terrainmodel.MaxHeight );
            LogFile.WriteLine( "PlayerMovement, new boundingbox " + WorldBoundingBoxMin + " < (x,y,z0 < " + WorldBoundingBoxMax );
        }

        void PlayerMovement_StateChanged(ViewerState.ViewerStateEnum neweditstate, ViewerState.ViewerStateEnum newviewstate)
        {
            //bcapturing = down;

            if (newviewstate == ViewerState.ViewerStateEnum.MouseLook)
            {
                LogFile.WriteLine("PlayerMovement.ActivateMouseLook");
                istartmousex = MouseCache.GetInstance().MouseX;
                istartmousey = MouseCache.GetInstance().MouseY;
                startavatarzrot = avatarzrot;
                startavataryrot = avataryrot;
            }
            else
            {
            }
        }

        //bool bcapturing = false; // if someone else filtered our MouseDown (ie SelectionModel, Camera,etc...), we shouldnt be processing MouseMove
        //bool _bcapturing;

        int istartmousex;
        int istartmousey;
        double startavatarzrot;
        double startavataryrot;

        void PlayerMovement_MouseMove()
        {
            //LogFile.WriteLine("PlayerMovement.MouseMove " + bcapturing);
            //if ( InMouseMoveDrag &&
             //   ViewerState.GetInstance().CurrentViewerState == ViewerState.ViewerStateEnum.MouseLook)
            if (ViewerState.GetInstance().CurrentViewerState == ViewerState.ViewerStateEnum.MouseLook)
            {
                avatarzrot = startavatarzrot - (double)(MouseCache.GetInstance().MouseX - istartmousex) * fAvatarTurnSpeed;
                avataryrot = Math.Min(Math.Max(startavataryrot + (double)(MouseCache.GetInstance().MouseY - istartmousey) * fAvatarTurnSpeed, -90), 90);
                UpdateAvatarObjectRotAndPos();
            }
        }

        //public void MouseDown(string command, bool down)
        //{
            //Test.Debug("Playermovement MouseDown " + e.ToString() );
            //LogFile.WriteLine("PlayerMovement.MouseDown " + down + " " + bcapturing);
          //  if (ViewerState.GetInstance().CurrentViewerState == ViewerState.ViewerStateEnum.MouseLook)
//            {
  //              InMouseMoveDrag = down;
    //        }
      //      else
        //    {
          //      InMouseMoveDrag = false;
            //}
        //}

        //public void MouseMove( object source, MouseEventArgs e )
        //{
            //Test.Debug("Playermovement MouseMove " + e.ToString() );
          //  if( _bcapturing )
            //{
              //  avatarzrot = startavatarzrot - (double)( e.X - istartmousex ) * fAvatarTurnSpeed;
                //avataryrot = Math.Min( Math.Max( startavataryrot + (double)( e.Y - istartmousey ) * fAvatarTurnSpeed, - 90 ), 90 );
                //UpdateAvatarObjectRotAndPos();
            //}
        //}

        //public void MouseUp( object source, MouseEventArgs e )
        //{
            //Test.Debug("Playermovement MouseUp " + e.ToString() );
            //bcapturing = false;
          //  _bcapturing = false;
        //}

        public void UpdateAvatarObjectRotAndPos()
        {
            avatarrot = mvMath.AxisAngle2Rot( mvMath.ZAxis, ( avatarzrot * Math.PI / 180 ) );
            avatarrot *= mvMath.AxisAngle2Rot( mvMath.YAxis, avataryrot * Math.PI / 180 );
            
            Entity avatar = MetaverseClient.GetInstance().myavatar;
        
            if( avatar != null )
            {
                avatar.pos = avatarpos;        
                avatar.rot = avatarrot;
            }
        }
        
        public void MovePlayer()
        {
            double fRight = 0.0;
            double fUp = 0.0;
            double fForward = 0.0;
        
            if( kMovingLeft )
            {
                fRight -= 1.0;
                bAvatarMoved = true;
            }
            if( kMovingRight )
            {
                fRight += 1.0;
                bAvatarMoved = true;
            }
            if( kMovingForwards )
            {
                bAvatarMoved = true;
                fForward += 1.0;
            }
            if( kMovingBackwards )
            {
                fForward -= 1.0;
                bAvatarMoved = true;
            }
            if( kMovingUpZAxis )
            {
                fUp += 1.0;
                bAvatarMoved = true;
            }
            if( kMovingDownZAxis )
            {
                fUp -= 1.0;
                bAvatarMoved = true;
            }
        
            double fTimeSlotMultiplier = (double)timekeeper.ElapsedTime / 100; // PLACEHOLDER
            if( bAvatarMoved )
            {
                switch( camera.viewpoint )
                {
                    case Camera.Viewpoint.MouseLook:
                    //case Camera.Viewpoint.BehindPlayer:
                        Vector3 accelerationavaxes = new Vector3( fForward, - fRight, 0 )  * fTimeSlotMultiplier * fAvatarAcceleration;
                        Vector3 accelerationvectorworldaxes = accelerationavaxes * MetaverseClient.GetInstance().myavatar.rot.Inverse();
                    
                        accelerationvectorworldaxes.z = fUp * fAvatarAcceleration * fTimeSlotMultiplier;
                        
                        currentvelocity = currentvelocity / ( 1 + fTimeSlotMultiplier * fDeceleration ) + accelerationvectorworldaxes;
                        currentvelocity.x = Math.Max( Math.Min( currentvelocity.x, fAvatarMoveSpeed ), -fAvatarMoveSpeed );
                        currentvelocity.y = Math.Max( Math.Min( currentvelocity.y, fAvatarMoveSpeed ), -fAvatarMoveSpeed );
                        currentvelocity.z = Math.Max( Math.Min( currentvelocity.z, fVerticalMoveSpeed ), -fVerticalMoveSpeed );
                    
                        avatarpos = avatarpos + currentvelocity * fTimeSlotMultiplier;
                        
                        avatarpos.x = Math.Max( avatarpos.x, WorldBoundingBoxMin.x );
                        avatarpos.y = Math.Max( avatarpos.y, WorldBoundingBoxMin.y );
                        avatarpos.z = Math.Max( avatarpos.z, WorldBoundingBoxMin.z );
                        
                        avatarpos.x = Math.Min( avatarpos.x, WorldBoundingBoxMax.x );
                        avatarpos.y = Math.Min( avatarpos.y, WorldBoundingBoxMax.y );
                        avatarpos.z = Math.Min( avatarpos.z, WorldBoundingBoxMax.z );

                        avatarpos.z = Math.Max( avatarpos.z, MetaverseClient.GetInstance().worldstorage.terrainmodel.Map[
                            (int)avatarpos.x, (int)avatarpos.y] + avatarradius );
                                
                        break;
        
                    //case Camera.Viewpoint.ThirdParty:
                      //  camera.fThirdPartyViewZoom += fForward;
                        //camera.fThirdPartyViewRotate += fRight * 3.0;
                        //break;
                }
                UpdateAvatarObjectRotAndPos();
            }
        }
    }
}
