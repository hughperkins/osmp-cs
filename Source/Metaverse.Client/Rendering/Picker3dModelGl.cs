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
using System.Collections;
using Tao.OpenGl;
using System.Runtime.InteropServices;
using Metaverse.Utility;

namespace OSMP
{
    public delegate void DrawWorldHandler();
    
    //class AdditionalGlWrap
    //{
      //  [DllImport ("glselectwrap.dll", CharSet=CharSet.Auto )]
        //public static extern void glCreateSelectBuffer();
            
        //[DllImport ("glselectwrap.dll", CharSet=CharSet.Auto )]
        //public static extern int glGetNearestBufferName( int numhits );
    //}
    
    // responsible for picking, which in OpenGl means essentially using a glSelect buffer to decide what you clicked on.
    // This is the OpenGl specific class; you can derive others from IPicker3dModel
    // You can get an instance of this class at runtime by doing RendererFactory().GetInstance().GetPicker3dModel();
    //
    // name = 0 is reserved, means no name
    public class Picker3dModelGl : IPicker3dModel
    {
        static Picker3dModelGl instance = new Picker3dModelGl();
        public static IPicker3dModel GetInstance() { return instance; }

        ArrayList hittargets = new ArrayList();  // ArrayList is not really fast; might consider using a normal array
        
        bool bAddingNames; // we set this to true if we are adding names to hittargets, otherwise to false to gain speed
        
        const int selectbuffersize = 4096;
        int[] selectbuffer;
        GCHandle selectbufferhandle;

        void CreateSelectBuffer()
        {
            selectbuffer = new int[selectbuffersize];
            selectbufferhandle = GCHandle.Alloc(selectbuffer, GCHandleType.Pinned);
            Gl.glSelectBuffer(selectbuffersize, selectbuffer);
        }

        void FreeSelectBuffer()
        {
            selectbufferhandle.Free();
        }

        int GetNearestBufferName(int inumhits)
        {
            int bestdepth = 0;
            int bestpick = -1;
            for (int i = 0; i < inumhits; i++)
            {
                //int thisitem = Marshal.ReadInt32(selectbufferptr, (i * 4 + 3) * 4);
                //int thisdepth = Marshal.ReadInt32(selectbufferptr, (i * 4 + 1) * 4);
                int thisitem = selectbuffer[i * 4 + 3];
                if (thisitem > 0)
                {
                    int thisdepth = selectbuffer[i * 4 + 1];
                    if (thisdepth < bestdepth || bestpick == -1)
                    {
                        //  cout << "new best depth: " << bestdepth << " pick: " << thisitem << endl;
                        bestdepth = thisdepth;
                        bestpick = (int)thisitem;
                    }
                }
            }
            // cout<< "best hit: " << bestpick << endl;
            return bestpick;

        }

        public void AddHitTarget( HitTarget hittarget )
        {
            if( bAddingNames )
            {
                //Test.Debug("adding name " + hittarget.ToString() );
                hittargets.Add( hittarget );
                Gl.glLoadName( hittargets.Count );  // note: this isnt quite the index; it is index + 1
            }
        }

        /// <summary>
        /// calls Gl.glLoadName(0);
        /// </summary>
        public void EndHitTarget()
        {
            if (bAddingNames)
            {
                Gl.glLoadName(0);
            }
        }
        
        public class WorldDrawer : IRenderable
        {
            public void Render()
            {
                RendererFactory.GetInstance().DrawWorld();
            }
        }
        
        public HitTarget GetClickedHitTarget( int MouseX, int MouseY )
        {
            return GetClickedHitTarget( new WorldDrawer(), MouseX, MouseY );
        }

        // dependencies:
        // - RendererFactory.GetInstance()
        // - Tao.OpenGl        
        public HitTarget GetClickedHitTarget( IRenderable renderable, int MouseX, int MouseY )
        {
            GraphicsHelperGl g = new GraphicsHelperGl();
            g.CheckError();

            IRenderer renderer = RendererFactory.GetInstance();
            ArrayList results = new ArrayList();
            
            int[] viewport = new int[ 4 ];
            Gl.glGetIntegerv( Gl.GL_VIEWPORT, viewport );
            g.CheckError();
            CreateSelectBuffer();
            g.CheckError();
        
            // This Creates A Matrix That Will Zoom Up To A Small Portion Of The Screen, Where The Mouse Is.
            Gl.glMatrixMode( Gl.GL_PROJECTION );
            g.CheckError();
            Gl.glPushMatrix();   // save old matrix, we restore it at end         
            g.CheckError();
            Gl.glLoadIdentity();
            g.CheckError();
            Glu.gluPickMatrix( (float)MouseX, (float)(renderer.WindowHeight - MouseY), 1.0f, 1.0f, viewport );
            g.CheckError();
            Glu.gluPerspective( renderer.FieldOfView, (float)renderer.WindowWidth / (float)renderer.WindowHeight, renderer.NearClip, renderer.FarClip );
            g.CheckError();
            
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            g.CheckError();
            
            Gl.glRenderMode( Gl.GL_SELECT );
            g.CheckError();
            Gl.glInitNames();
            g.CheckError();
            Gl.glPushName( 0 );            // Push one entry onto the stack; we will use LoadName to change this value throughout the rendering

            g.CheckError();
            bAddingNames = true;
            hittargets = new ArrayList();
            renderable.Render();            
            bAddingNames = false;

            // return projection matrix to normal
            Gl.glMatrixMode( Gl.GL_PROJECTION );
            Gl.glPopMatrix();
            Gl.glMatrixMode( Gl.GL_MODELVIEW );

            int iNumHits = Gl.glRenderMode( Gl.GL_RENDER );

            int hitname = GetNearestBufferName( iNumHits );
            LogFile.WriteLine( "hitname: " + hitname.ToString() );
            if( hitname == -1 || hitname == 0 )
            {
                LogFile.WriteLine("not in buffer");
                return null;
            }

            FreeSelectBuffer();
            new GraphicsHelperGl().CheckError();

            if (hittargets.Count == 0)
            {
                return null;
            }

            if (iNumHits == 0)
            {
                //LogFile.WriteLine("no hits");
                return null;
            }

            //LogFile.WriteLine(hittargets[hitname - 1]);
            return (HitTarget)hittargets[ hitname - 1 ];
        }        
    }
}
