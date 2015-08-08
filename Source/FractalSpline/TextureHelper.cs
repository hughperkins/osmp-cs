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

namespace TestFractalSpline
{
    public class TextureHelper
    {
        // based on http://nemerle.org/svn/nemerle/trunk/snippets/opengl/sdlopengl4.n , Ben Humphrey, digiben@gametutorilas.com
        public static int LoadBitmapToOpenGl( Bitmap bitmap )
        {
            // the following lines extract R,G and B values from any bitmap
            byte[]data = new byte[ 3 * bitmap.Width * bitmap.Height ];
            const int channels = 3;
            for( int i = 0; i < bitmap.Height; i++ )          
            {
                for(int j = 0; j < bitmap.Width; j++ )
                {
                    Color pixel = bitmap.GetPixel ( j, i );
                    int offset = ( i * bitmap.Width + j ) * channels;
                    data[offset + 0] = pixel.R; // in our tImage classes we store r first
                    data[offset + 1] = pixel.G; // then g
                    data[offset + 2] = pixel.B;
                    // (for bmps - three channels only)
                }
            }
            
            int iTextureReference;
            Gl.glGenTextures( 1, out iTextureReference );
            
            // Now that we have a reference for the texture, we need to bind the texture
            // to tell OpenGL this is the reference that we are assigning the bitmap data too.
            // The first parameter tells OpenGL we want are using a 2D texture, while the
            // second parameter passes in the reference we are going to assign the texture too.
            // We will use this function later to tell OpenGL we want to use this texture to texture map.
            
            // Bind the texture to the texture arrays index and init the texture
            Gl.glBindTexture( Gl.GL_TEXTURE_2D, iTextureReference );
            
//            	glTexImage2D(GL_PROXY_TEXTURE_2D, 0, Texture.bpp / 8, Texture.width, Texture.height, 0, Texture.type, GL_UNSIGNED_BYTE, NULL);
//	glGetTexLevelParameteriv(GL_PROXY_TEXTURE_2D, 0, GL_TEXTURE_INTERNAL_FORMAT, &format);

//	glTexImage2D(GL_TEXTURE_2D, 0, Texture.bpp / 8, Texture.width, Texture.height, 0, Texture.type, GL_UNSIGNED_BYTE, Texture.imageData);
//	glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_LINEAR);
	//glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MAG_FILTER,GL_LINEAR);
            
            // Now comes the important part, we actually pass in all the data from the bitmap to
            // create the texture. Here is what the parameters mean in gluBuild2DMipmaps():
            // (We want a 2D texture, 3 channels (RGB), bitmap width, bitmap height, It's an RGB format,
            //  the data is stored as unsigned bytes, and the actuall pixel data);
            
            // What is a Mip map?  Mip maps are a bunch of scaled pictures from the original.  This makes
            // it look better when we are near and farther away from the texture map.  It chooses the
            // best looking scaled size depending on where the camera is according to the texture map.
            // Otherwise, if we didn't use mip maps, it would scale the original UP and down which would
            // look not so good when we got far away or up close, it would look pixelated.
            
            // Build Mipmaps (builds different versions of the picture for distances - looks better)
            // Glu.gluBuild2DMipmaps(Gl.GL_TEXTURE_2D, 3, bitmap.Width, bitmap.Height, Gl.GL_RGB, Gl.GL_UNSIGNED_BYTE, data);
                  //Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGB, bitmap.Width, bitmap.Height, -1, Gl.GL_RGB, Gl.GL_UNSIGNED_BYTE, data);
            //Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGB, 32, 32, 0, Gl.GL_RGB, Gl.GL_UNSIGNED_BYTE, data);
            
            // Lastly, we need to tell OpenGL the quality of our texture map.  GL_LINEAR_MIPMAP_LINEAR
            // is the smoothest.  GL_LINEAR_MIPMAP_NEAREST is faster than GL_LINEAR_MIPMAP_LINEAR, 
            // but looks blochy and pixilated.  Good for slower computers though.  Read more about 
            // the MIN and MAG filters at the bottom of main.cpp
            //      glTexEnvi (GL_TEXTURE_ENV, GL_TEXTURE_ENV_MODE, GL_DECAL);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D,Gl.GL_TEXTURE_MAG_FILTER,Gl.GL_LINEAR);    
            // glTexParameteri(GL_TEXTURE_2D,GL_TEXTURE_MIN_FILTER,GL_NEAREST);
            
            return iTextureReference;
        }
    }
}
