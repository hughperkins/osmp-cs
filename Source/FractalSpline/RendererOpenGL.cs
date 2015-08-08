// Copyright Hugh Perkins 2005,2006
// hughperkins at gmail http://hughperkins.com
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

using Tao.OpenGl;

namespace FractalSpline
{
    public class RendererOpenGl : IRenderer
    {
        static RendererOpenGl instance = new RendererOpenGl();
        public static RendererOpenGl GetInstance()
        {
            return instance;
        }
        
        public void AddVertex( double x, double y, double z )
        {
            Gl.glVertex3f( (float)x,(float)y,(float)z );
        }
        public void SetNormal( double x, double y, double z )
        {
            Gl.glNormal3f( (float)x,(float)y, (float)z );
        }
        public void SetTextureCoord( double u, double v )
        {
            Gl.glTexCoord2f( (float)u,(float)v );
        }
        public void StartTriangle()
        {
            Gl.glBegin( Gl.GL_TRIANGLES );
        }
        public void EndTriangle()
        {
            Gl.glEnd();
        }
        public void SetTextureId( int iTexture )
        {
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, iTexture );
        }
        public void SetColor( double r, double g, double b )
        {
            Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_AMBIENT_AND_DIFFUSE, new float[]{ (float)r,(float)g,(float)b,1.0f} );   
        }
    }
} // namespace FractalSpline

