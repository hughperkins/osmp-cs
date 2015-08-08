// Copyright Hugh Perkins 2006
// hughperkins@gmail.com http://manageddreams.com
//
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the
// Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
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
using System.Collections.Generic;
using System.Text;
using Tao.OpenGl;

namespace OSMP
{
    public class RenderableWater
    {
        Vector3 pos;
        Vector2 scale;

        int numsectors = 10;

        public RenderableWater( Vector3 pos, Vector2 scale)
        {
            this.pos = pos;
            this.scale = scale;
            RendererFactory.GetInstance().WriteAlpha += new WriteNextFrameCallback(RenderableWater_WriteNextFrameEvent);
        }

        public Vector2 Scale
        {
            set
            {
                this.scale = value;
            }
        }

        void RenderableWater_WriteNextFrameEvent(Vector3 camerapos)
        {
            double xmultiplier = scale.x / numsectors;
            double ymultiplier = scale.y / numsectors;
            GraphicsHelperGl g = new GraphicsHelperGl();
            g.SetMaterialColor(new double[] { 0, 0.2, 0.8, 0.6 });
            g.EnableBlendSrcAlpha();
            g.EnableModulate();
            g.DisableTexture2d(); // note to self: could add texture??? (or vertex fragment)
            Gl.glDisable( Gl.GL_CULL_FACE ); // water has two sides?
            g.Normal(new Vector3(0, 0, 1));
            for (int x = 0; x < numsectors; x++)
            {
                Gl.glBegin(Gl.GL_TRIANGLE_STRIP);
                for (int y = 0; y <= numsectors; y++)
                {
                    Vector3 posoffset = new Vector3( x * xmultiplier, y * ymultiplier, 0 );
                    Vector3 vertexpos = pos + posoffset;
                    //Console.WriteLine(vertexpos);
                    g.Vertex(vertexpos);
                    vertexpos.x += xmultiplier;
                    //Console.WriteLine(vertexpos);
                    g.Vertex(vertexpos);
                }
                Gl.glEnd();
            }
            Gl.glEnable( Gl.GL_CULL_FACE );
            Gl.glDisable( Gl.GL_BLEND );
            g.SetMaterialColor(new double[] { 1, 1, 1, 1 });
        }
    }
}
