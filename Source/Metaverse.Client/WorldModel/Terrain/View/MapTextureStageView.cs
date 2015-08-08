// Copyright Hugh Perkins 2006
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
using Metaverse.Utility;
using Tao.OpenGl;

namespace OSMP
{
    // represents a single maptexture stage, eg one stage from md3
    // this could represent two opengl texture stages, eg for blend
    public class MapTextureStageView
    {
        public MapTextureStageModel maptexturestagemodel;

        public ITexture splattexture;
        public ITexture blendtexture;

        public MapTextureStageView( MapTextureStageModel maptexturestagemodel )
        {
            LogFile.WriteLine( "MapTextureStageView(" + maptexturestagemodel + ")" );
            this.maptexturestagemodel = maptexturestagemodel;
            splattexture = new GlTexture( maptexturestagemodel.splattexture, false );
            blendtexture = new GlTexture( maptexturestagemodel.blendtexture, true );

            maptexturestagemodel.Changed += new MapTextureStageModel.ChangedHandler( maptexturestagemodel_Changed );
        }

        void maptexturestagemodel_Changed() // note to self: need to differentaite between global change, blend teture, splattexture
        {
            LogFile.WriteLine( "maptexturestageview.Changed" );
            //maptexturestagemodel.splattexture.Save( "out.jpg" ); -> Ok
            splattexture.LoadNewImage( maptexturestagemodel.splattexture, false );
            blendtexture.LoadNewImage( maptexturestagemodel.blendtexture, true );
        }

        public int NumTextureStagesRequired
        {
            get
            {
                if (maptexturestagemodel.Operation == MapTextureStageModel.OperationType.Nop)
                {
                    return 0;
                }
                if (maptexturestagemodel.Operation == MapTextureStageModel.OperationType.Blend)
                {
                    return 2;
                }
                return 1;
            }
        }

        void SetTextureScale(double scale)
        {
            // matrix texture scaling from http://www.kraftwrk.com/multi_texturing.htm
            // Now we want to enter the texture matrix. This will allow us
            // to change the tiling of the detail texture.
            Gl.glMatrixMode(Gl.GL_TEXTURE);
            // Reset the current matrix and apply our chosen scale value
            Gl.glLoadIdentity();
            Gl.glScalef((float)scale, (float)scale, 1);
            // Leave the texture matrix and set us back in the model view matrix
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
        }

        // applies texture stage setup to gl.  texturestagenum will be 0 except for blend
        // where it will be either 0 or 1
        // texture coordinates are handled independently (?)
        // either we're using multipass or we're using multitexture.  multipass still uses 2 multitexture units, but not 4, 6, 8, ...
        public void Apply(int texturestagenum, bool UsingMultipass, int mapwidth, int mapheight)
        {
            //Console.WriteLine( "MapTextureStageView for " + maptexturestagemodel + " " + ((GlTexture)splattexture).GlReference +  " Apply" );
            GlTextureCombine texturecombine;
            GraphicsHelperGl g = new GraphicsHelperGl();
            switch (maptexturestagemodel.Operation)
            {
                case MapTextureStageModel.OperationType.NoTexture:
                    g.DisableTexture2d();
                    g.EnableModulate();
                    break;
                case MapTextureStageModel.OperationType.Add:
                    splattexture.Apply();
                    SetTextureScale(1 / (double)maptexturestagemodel.Tilesize);
                    texturecombine = new GlTextureCombine();
                    texturecombine.Operation = GlTextureCombine.OperationType.Add;
                    texturecombine.Args[0].SetRgbaSource(GlCombineArg.Source.Previous);
                    texturecombine.Args[1].SetRgbaSource(GlCombineArg.Source.Texture);
                    texturecombine.Apply();
                    break;
                case MapTextureStageModel.OperationType.Blend:
                    if (UsingMultipass)
                    {
                        if (texturestagenum == 0)
                        {
                            blendtexture.Apply();
                            SetTextureScale(1 / (double)mapwidth);
                            texturecombine = new GlTextureCombine();
                            texturecombine.Operation = GlTextureCombine.OperationType.Replace;
                            texturecombine.Args[0].SetAlphaSource(GlCombineArg.Source.Texture, GlCombineArg.Operand.Alpha);
                            texturecombine.Apply();
                        }
                        else
                        {
                            splattexture.Apply();
                            SetTextureScale( 1 / (double)maptexturestagemodel.Tilesize );
                            new GraphicsHelperGl().EnableModulate();
                        }
                    }
                    else
                    {
                        if (texturestagenum == 0)
                        {
                            blendtexture.Apply();
                            SetTextureScale(1 / (double)mapwidth);
                            texturecombine = new GlTextureCombine();
                            texturecombine.Operation = GlTextureCombine.OperationType.Replace;
                            texturecombine.Args[0].SetRgbSource(GlCombineArg.Source.Previous, GlCombineArg.Operand.Rgb);
                            texturecombine.Args[0].SetAlphaSource(GlCombineArg.Source.Texture, GlCombineArg.Operand.Alpha);
                            texturecombine.Apply();
                        }
                        else
                        {
                            splattexture.Apply();
                            SetTextureScale( 1 / (double)maptexturestagemodel.Tilesize );
                            texturecombine = new GlTextureCombine();
                            texturecombine.Operation = GlTextureCombine.OperationType.Interpolate;
                            texturecombine.Args[0].SetRgbaSource(GlCombineArg.Source.Previous);
                            texturecombine.Args[1].SetRgbaSource(GlCombineArg.Source.Texture);
                            texturecombine.Args[2].SetRgbSource(GlCombineArg.Source.Previous, GlCombineArg.Operand.Alpha);
                            texturecombine.Args[2].SetAlphaSource(GlCombineArg.Source.Previous, GlCombineArg.Operand.Alpha);
                            texturecombine.Apply();
                        }
                    }
                    break;
                case MapTextureStageModel.OperationType.Multiply:
                    splattexture.Apply();
                    SetTextureScale( 1 / (double)maptexturestagemodel.Tilesize );
                    texturecombine = new GlTextureCombine();
                    texturecombine.Operation = GlTextureCombine.OperationType.Modulate;
                    texturecombine.Args[0].SetRgbaSource(GlCombineArg.Source.Previous);
                    texturecombine.Args[1].SetRgbaSource(GlCombineArg.Source.Texture);
                    texturecombine.Apply();
                    break;
                case MapTextureStageModel.OperationType.Subtract:
                    splattexture.Apply();
                    SetTextureScale( 1 / (double)maptexturestagemodel.Tilesize );
                    texturecombine = new GlTextureCombine();
                    texturecombine.Operation = GlTextureCombine.OperationType.Subtract;
                    texturecombine.Args[0].SetRgbaSource(GlCombineArg.Source.Previous);
                    texturecombine.Args[1].SetRgbaSource(GlCombineArg.Source.Texture);
                    texturecombine.Apply();
                    break;
                case MapTextureStageModel.OperationType.Replace:
                    splattexture.Apply();
                    SetTextureScale( 1 / (double)maptexturestagemodel.Tilesize );
                    texturecombine = new GlTextureCombine();
                    texturecombine.Operation = GlTextureCombine.OperationType.Modulate;
                    texturecombine.Args[0].SetRgbaSource(GlCombineArg.Source.Texture);
                    texturecombine.Args[1].SetRgbaSource(GlCombineArg.Source.Fragment);
                    texturecombine.Apply();
                    break;
            }
        }
    }
}
