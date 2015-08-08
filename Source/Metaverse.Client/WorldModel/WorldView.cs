// Copyright Hugh Perkins 2004,2005,2006
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
using Tao.OpenGl;
using Metaverse.Utility;

namespace OSMP
{
    // responsable for drawing the worldmodel into the 3d view
    // mostly the entities can render themselves; some tweaking and transforms via IGraphicsHelper
    // we use SelectionView to draw selection highlighting
    public class WorldView : IRenderable
    {
        WorldModel worldmodel;
        IGraphicsHelper graphics;
        public TerrainView terrainview;

        Vector2[] LandCoords = new Vector2[ 1000 ];  //!< coordinates of hardcoded green plateau (?)
        
        //static WorldView instance = new WorldView();
        //public static WorldView GetInstance(){ return instance; }

        public class HitTargetLandCoord : HitTarget
        {
            public int iLandCoordIndex;
            public HitTargetLandCoord( int iLandCoordIndex )
            {
                this.iLandCoordIndex = iLandCoordIndex;
            }
        };
                
        public WorldView( WorldModel worldmodel )
        {
            LogFile.WriteLine( "WorldView(" + worldmodel + ")" );
            this.worldmodel = worldmodel;
            graphics = GraphicsHelperFactory.GetInstance();
            terrainview = new TerrainView( worldmodel.terrainmodel );
            RendererFactory.GetInstance().WriteNextFrameEvent += new WriteNextFrameCallback(WorldView_WriteNextFrameEvent);
        }

        void WorldView_WriteNextFrameEvent(Vector3 camerapos)
        {
            Render();
        }

        //! Draws hardcoded platern that we start over
        //! This is a temporary function since land will be migrated to the databse
        void DrawLandscape()
        {
            IPicker3dModel picker3dmodel = RendererFactory.GetPicker3dModel();
            
            int i, iy;
    
            // Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_AMBIENT_AND_DIFFUSE, new float[]{(float)rand.GetRandomFloat(0,1), 0.7f, 0.2f, 1.0f});
            Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_AMBIENT_AND_DIFFUSE, new float[]{0.7f, 0.7f, 0.2f, 1.0f});
    
            int iLandCoord = 0;
            for( iy = -10; iy <=10; iy++ )
            {
                for( i = -10; i <= 10; i++ )
                {                    
                    LandCoords[ iLandCoord ] = new Vector2( -10.0F + i * 1.0F, -10.0F + iy * 1.0F );
                    picker3dmodel.AddHitTarget( new HitTargetLandCoord( iLandCoord ) );
    
                    graphics.PushMatrix();
    
                    graphics.Translate( -10.0F + i * 1.0F, -10.0F + iy * 1.0F, MetaverseClient.GetInstance().fHeight - 0.5 );
                    graphics.DrawCube();
    
                    graphics.PopMatrix();
                    iLandCoord++;
                }
            }
        }
                
        //! Draws all non-hardcoded objects in world - including avatars - into the 3D world of OpenGL
        void DrawEntities()
        {
            //Test.Debug("drawobjects");
            Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_AMBIENT_AND_DIFFUSE, new float[]{1.0f, 0.0f, 0.5f, 1.0f});

            Gl.glEnable( Gl.GL_TEXTURE_2D );

            Avatar myavatar = MetaverseClient.GetInstance().myavatar;
            
            Picker3dController picker3dcontroller = Picker3dController.GetInstance();

            for( int i = 0; i < worldmodel.entities.Count; i++ )
            {
                if( worldmodel.entities[i].iParentReference == 0 )
                {
                    // dont draw own avatar in mouselook mode
                    if( worldmodel.entities[i] != myavatar )
                    {
                        //LogFile.WriteLine("render entity " + i);
                        Gl.glRasterPos3f((float)worldmodel.entities[i].pos.x, (float)worldmodel.entities[i].pos.y, (float)worldmodel.entities[i].pos.z);
                        picker3dcontroller.AddHitTarget( worldmodel.entities[i] );
                        worldmodel.entities[i].Draw();
                        picker3dcontroller.EndHitTarget();
                        graphics.Bind2DTexture( 0 );    
                    }
                    else
                    {
                        Gl.glRasterPos3f( (float)worldmodel.entities[i].pos.x, (float)worldmodel.entities[i].pos.y, (float)worldmodel.entities[i].pos.z);
                        picker3dcontroller.AddHitTarget( worldmodel.entities[i] );
                        worldmodel.entities[i].Draw();
                        picker3dcontroller.EndHitTarget();
                        graphics.Bind2DTexture(0);
                    }
                }
            }
        }
        
        //void DrawLights()
        //{
          //  Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_POSITION, new float[]{-100.5f, 100.0f, 100.0f, 1.0f});            
        //}
        
        public void Render()
        {
            //DrawLights();
            //LogFile.WriteLine("render");
            //DrawLandscape();
            // note to self: add Terrain perhaps?
            DrawEntities();
            SelectionView.GetInstance().Render();
        }
    }
}
