// Copyright Hugh Perkins 2006
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
using Tao.OpenGl;

namespace OSMP
{
    // converts picker3dmodel results to an entity
    // dependencies:
    // - picker3dmodel (from RendererFactory.GetInstance().GetPicker3dModel();
    // - worldmodel
    public class Picker3dController
    {
        IPicker3dModel picker3dmodel;
        
        public class SinglePrimFaceDrawer : IRenderable
        {
            public Prim prim;
            public SinglePrimFaceDrawer( Prim prim )
            {
                this.prim = prim;
            }
            public void Render()
            {
                //LogFile.WriteLine("singlefacedrawer.render");
                IRenderer renderer = RendererFactory.GetInstance();
                Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
                Gl.glLoadIdentity();
                renderer.ApplyViewingMatrices();
                for( int i = 0; i < prim.NumFaces; i++ )
                {
                  //  LogFile.WriteLine("face " + i);
                    HitTarget thishittarget = new HitTargetEntityFace( prim, i );
                    //LogFile.WriteLine( "Renderering " + thishittarget.ToString() );
                    Picker3dModelGl.GetInstance().AddHitTarget( thishittarget );
                    if( prim.Parent != null )
                    {
                        ( prim.Parent as EntityGroup ).ApplyTransforms();
                    }
                    prim.RenderSingleFace( i );
                }
            }
        }
        
        public class HitTargetEntity : HitTarget
        {
            public Entity entity;
            public HitTargetEntity( Entity entity )
            {
                this.entity = entity;
            }        
            public override string ToString()
            {
                return "HitTargetEntity entity=" + entity.ToString();
            }
        }
        
        public class HitTargetEntityFace : HitTargetEntity
        {
            public int FaceNumber;
            public HitTargetEntityFace( Entity entity, int FaceNumber ) : base( entity )
            {
                this.FaceNumber = FaceNumber;
            }
            public override string ToString()
            {
                return "HitTargetEntityFace entity=" + entity.ToString() + " face=" + FaceNumber.ToString();
            }
        }
    
        static Picker3dController instance = new Picker3dController();
        public static Picker3dController GetInstance()
        {
            return instance;
        }
        
        public Picker3dController()
        {
            //picker3dmodel = RendererFactory.GetInstance().GetPicker3dModel();
            picker3dmodel = Picker3dModelGl.GetInstance();
        }
        
        public void AddHitTarget( Entity entity )
        {
            picker3dmodel.AddHitTarget( new HitTargetEntity( entity ) );
        }

        /// <summary>
        /// optional, calls Gl.GlBindName(0);
        /// </summary>
        public void EndHitTarget()
        {
            picker3dmodel.EndHitTarget();
        }
        
        public Entity GetClickedEntity( int iMouseX, int iMouseY )
        {
            HitTarget hittarget = picker3dmodel.GetClickedHitTarget( iMouseX, iMouseY );
        
            if( hittarget != null )
            {
                if( hittarget is HitTargetEntity )
                {
                    // Test.Debug(  "selected has reference " + hittarget.iForeignReference.ToString() );
                    return ((HitTargetEntity)hittarget).entity;
                }
            }
        
            return null;
        }      
        
        // we run another selection, with only a single prim, making each face a single pick target
        public int GetClickedFace( Prim prim, int iMouseX, int iMouseY )
        {
            //LogFile.WriteLine("picker3dcontroller.getclickedface " + prim + " " +  iMouseX + " " + iMouseY);
            HitTarget hittarget = picker3dmodel.GetClickedHitTarget( new SinglePrimFaceDrawer( prim as Prim ), iMouseX, iMouseY );
            if( hittarget == null || !( hittarget is HitTargetEntityFace ) )
            {
                return 0;
            }
            //LogFile.WriteLine( "result " + hittarget.ToString() );
            return ( hittarget as HitTargetEntityFace ).FaceNumber;
        }            
    }
}
