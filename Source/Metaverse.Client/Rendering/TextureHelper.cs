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
using System.Drawing;
using System.Drawing.Imaging;
using Tao.OpenGl;

namespace OSMP
{
    public class TextureHelper
    {
        // based on http://nemerle.org/svn/nemerle/trunk/snippets/opengl/sdlopengl4.n , Ben Humphrey, digiben@gametutorilas.com
        public static int LoadBitmapToOpenGl( Bitmap bitmap )
        {
            // the following lines extract R,G and B values from any bitmap
            const int channels = 4;
            byte[] data = new byte[channels * bitmap.Width * bitmap.Height];
            for( int i = 0; i < bitmap.Height; i++ )          
            {
                for(int j = 0; j < bitmap.Width; j++ )
                {
                    System.Drawing.Color pixel = bitmap.GetPixel ( j, i );
                    int offset = ( i * bitmap.Width + j ) * channels;
                    data[offset + 0] = pixel.R; // in our tImage classes we store r first
                    data[offset + 1] = pixel.G; // then g
                    data[offset + 2] = pixel.B;
                    data[offset + 3] = pixel.B;
                }
            }
            
            int iTextureReference;
            Gl.glGenTextures( 1, out iTextureReference );
            Gl.glBindTexture( Gl.GL_TEXTURE_2D, iTextureReference );
            Glu.gluBuild2DMipmaps(Gl.GL_TEXTURE_2D, Gl.GL_RGBA8, bitmap.Width, bitmap.Height, Gl.GL_RGBA, Gl.GL_UNSIGNED_BYTE, data);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D,Gl.GL_TEXTURE_MAG_FILTER,Gl.GL_LINEAR);    
            
            return iTextureReference;
        }
    }
}
