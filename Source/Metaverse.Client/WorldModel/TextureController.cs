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
using System.Collections.Generic;
using System.Net;
using Tao.DevIl;
using System.IO;
using Tao.OpenGl;
using Metaverse.Utility;

namespace OSMP
{
    public class TextureController
    {
        // This is responsible for loading a texture progressively
        // we can add an extra thread into this class to progressively change the OpenGL texture as the texture is loading
        // note to self: currently only support File Uris.
        class TextureProxy
        {
            int CreateGraphicsEngineId()
            {
                Gl.glGenTextures( 1, out idingraphicsengine );
                return idingraphicsengine;
            }

            // from http://svn.sourceforge.net/viewvc/boogame/trunk/BooGame/src/Texture.cs?view=markup
            int NextPowerOfTwo(int n)
            {
                double power = 0;
                while (n > Math.Pow(2.0, power))
                    power++;
                return (int)Math.Pow(2.0, power);
            }

            void LoadTexture(byte[] bytes)
            {
                LogFile.WriteLine( "loading texture to opengl, bytescount = " + bytes.Length );
                // from http://svn.sourceforge.net/viewvc/boogame/trunk/BooGame/src/Texture.cs?view=markup
                int ilImage;
                Il.ilGenImages(1, out ilImage);
                Il.ilBindImage(ilImage);
                if (!Il.ilLoadL(Il.IL_TYPE_UNKNOWN, bytes, bytes.Length))
                    throw new Exception("Failed to load image.");
                if (!Il.ilConvertImage(Il.IL_RGBA, Il.IL_UNSIGNED_BYTE))
                    throw new Exception("Failed to convert image.");
                int m_BytesPerPixel = Il.ilGetInteger(Il.IL_IMAGE_BPP);
                int m_Width = Il.ilGetInteger(Il.IL_IMAGE_WIDTH);
                int m_Height = Il.ilGetInteger(Il.IL_IMAGE_HEIGHT);
                int m_Format = Il.ilGetInteger(Il.IL_IMAGE_FORMAT);
                int m_Depth = Il.ilGetInteger(Il.IL_IMAGE_DEPTH);
                LogFile.WriteLine( "size: " + m_Width + " x " + m_Height + " depth " + m_Depth + " bytesperpixel " + m_BytesPerPixel );

                int m_TextureWidth = NextPowerOfTwo(m_Width);
                int m_TextureHeight = NextPowerOfTwo(m_Height);
                if ((m_TextureWidth != m_Width) || (m_TextureHeight != m_Height))
                    Ilu.iluEnlargeCanvas(m_TextureWidth, m_TextureHeight, m_Depth);
                //Ilu.iluFlipImage();

                Gl.glBindTexture( Gl.GL_TEXTURE_2D, idingraphicsengine );
                Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST);
                Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_NEAREST);
                Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, m_BytesPerPixel, m_TextureWidth,
                    m_TextureHeight, 0, m_Format, Gl.GL_UNSIGNED_BYTE, Il.ilGetData());

                Il.ilDeleteImages(1, ref ilImage);
            }

            byte[] _LoadUri( Uri uri )
            {
                byte[] bytes = null;
                if (uri.IsFile)
                {
                    LogFile.WriteLine( "local path: " + uri.LocalPath );
                    FileStream fs = new FileStream( uri.LocalPath, FileMode.Open );
                    //bytes = StreamHelper.ReadFully( fs, fs.Length );
                    bytes = new byte[fs.Length];
                    fs.Read( bytes, 0, (int)fs.Length );
                    fs.Close();
                }
                else
                {
                    HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create( uri );
                    HttpWebResponse httpwebresponse = (HttpWebResponse)myReq.GetResponse();
                    Stream stream = httpwebresponse.GetResponseStream();
                    int length = (int)httpwebresponse.ContentLength;
                    LogFile.WriteLine( length );
                    bytes = StreamHelper.ReadFully( stream, length );
                    //bytes = new byte[ length ];
                    //stream.Read( bytes, 0, length );
                    stream.Close();
                    httpwebresponse.Close();
                }
                return bytes;
            }

            delegate byte[] LoadUriDelegate( Uri uri );
            LoadUriDelegate loaduridelegate;

            Uri uri;
            bool isloaded = false;
            int idingraphicsengine = 0;
            IAsyncResult asyncresult = null;

            void LoadUri(Uri uri)
            {
                LogFile.WriteLine("Loading image " + uri);
                loaduridelegate = new LoadUriDelegate( _LoadUri );
                asyncresult = loaduridelegate.BeginInvoke( uri, null, null );
                CheckHowLoadingIsGoing();
            }

            void CheckHowLoadingIsGoing()
            {
                if (asyncresult.IsCompleted)
                {
                    LogFile.WriteLine( "image " + uri + " loaded, adding to opengl..." );
                    byte[] bytes = loaduridelegate.EndInvoke( asyncresult );
                    LoadTexture( bytes );
                    isloaded = true;
                }
            }

            public int IdInGraphicsEngine { get { return idingraphicsengine; } } // since this doesnt have a set, this doesnt get saved to xml by serializer, which is correct behavior

            public bool IsLoaded
            {
                get { return isloaded; }
            }

            public TextureProxy() // for XmlSerializer Usage
            {
            }

            public TextureProxy(Uri uri)
            {
                LogFile.WriteLine( "new TextureProxy for " + uri );
                this.uri = uri;
                idingraphicsengine = CreateGraphicsEngineId();
                LogFile.WriteLine( "id: " + idingraphicsengine );
                LoadUri( uri );
            }

            public void Tick()
            {
                if (!isloaded)
                {
                    CheckHowLoadingIsGoing();
                }
            }
        }

        Dictionary<Uri,TextureProxy> TextureProxies = new Dictionary<Uri,TextureProxy>();
        
        static TextureController instance = new TextureController();
        public static TextureController GetInstance(){ return instance; }
        
        public TextureController()
        {
            Il.ilInit();
            MetaverseClient.GetInstance().Tick += new MetaverseClient.TickHandler( TextureController_Tick );
        }

        void TextureController_Tick()
        {
            foreach (TextureProxy proxy in TextureProxies.Values)
            {
                if (!proxy.IsLoaded)
                {
                    proxy.Tick();
                }
            }
        }

        public int LoadUri( Uri uri )
        {
            LogFile.WriteLine( "TextureController.LoadUri " + uri );
            if( !TextureProxies.ContainsKey( uri ) )
            {
                TextureProxies.Add( uri, new TextureProxy( uri ) );
            }
            return TextureProxies[ uri ].IdInGraphicsEngine;
        }
    }
}
