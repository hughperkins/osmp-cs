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

namespace OSMP
{
    // represents a single maptexture stage, eg one stage from md3
    // this could represent two opengl texture stages, eg for blend
    public class MapTextureStageModel
    {
        public delegate void ChangedHandler();
        public event ChangedHandler Changed;

        public enum OperationType
        {
            NoTexture,  // hack to allow map to run with no texturestages. note to self: cleanup
            Blend,
            Add,
            Multiply,
            Subtract,
            Replace,
            Nop
        };

        [Replicate]
        public OperationType Operation = OperationType.NoTexture;

        [Replicate]
        public string SplatTextureRelativeFilename
        {
            get
            {
                if (splattexturefilename == "" || splattexturefilename == null )
                {
                    return "";
                }
                return ProjectFileController.GetInstance().GetRelativePath( new Uri( splattexturefilename ) );
            }
            set
            {
                if (value != "")
                {
                    string fullfilepath = ProjectFileController.GetInstance().GetFullPath( value ).LocalPath;
                    LoadSplatTextureFromFile( fullfilepath );
                }
                else
                {
                    splattexturefilename = "";
                }
            }
        }
        [Replicate]
        public string BlendTextureRelativeFilename
        {
            get
            {
                if (blendtexturefilename != null && blendtexturefilename != "" )
                {
                    return ProjectFileController.GetInstance().GetRelativePath( new Uri( blendtexturefilename ) );
                }
                else
                {
                    return "";
                }
            }
            set
            {
                if (value != "")
                {
                    string fullfilepath = ProjectFileController.GetInstance().GetFullPath( value ).LocalPath;
                    LoadBlendTextureFromFile( value );
                }
                else
                {
                    blendtexturefilename = "";
                }
            }
        }

        public string SplatTextureFilename
        {
            get
            {
                return splattexturefilename;
            }
            set
            {
                LoadSplatTextureFromFile( value );
            }
        }

        public string BlendTextureFilename
        {
            get
            {
                return blendtexturefilename;
            }
            set
            {
                LoadBlendTextureFromFile( value );
            }
        }

        string splattexturefilename;
        string blendtexturefilename;

        public ImageWrapper splattexture;
        public ImageWrapper blendtexture;

        [Replicate]
        public int Tilesize;

        void CreateBlankTexture()
        {
            LogFile.WriteLine("CreateBlankTexture()");
            splattexture = new ImageWrapper( 1, 1 );
            splattexture.SetPixel( 0, 0, 255, 255, 255, 255 );
            //splattexture.Save( "blanksplat.jpg" );
            LogFile.WriteLine( "...done" );
        }

        void CreateBlankBlendTexture()
        {
            LogFile.WriteLine( "CreateBlankBlendTexture()" );
            blendtexture = new ImageWrapper( 256, 256 );
            //blendtexture.Save( "blankblend.jpg" );
            LogFile.WriteLine( "...done" );
        }

        // defaults to no texture
        public MapTextureStageModel()
        {
            this.Operation = OperationType.Replace;
            this.Tilesize = 50;
            CreateBlankTexture();
            CreateBlankBlendTexture();
        }

        public MapTextureStageModel(OperationType operation)
        {
            LogFile.WriteLine("MapTextureStage(), operation");
            this.Operation = operation;
            this.Tilesize = 50;
            CreateBlankTexture();
            CreateBlankBlendTexture();
        }
        
        public MapTextureStageModel(OperationType operation, int Tilesize, ImageWrapper splattexture )
        {
            LogFile.WriteLine("MapTextureStage(), single stages");
            this.Operation = operation;
            this.splattexture = splattexture;
            this.Tilesize = Tilesize;
            CreateBlankBlendTexture();
        }

        // blend needs two textures
        public MapTextureStageModel( OperationType operation, int Tilesize, ImageWrapper splattexture, ImageWrapper blendtexture )
        {
            LogFile.WriteLine("MapTextureStage(), two stages");
            this.Operation = operation;
            this.splattexture = splattexture;
            this.blendtexture = blendtexture;
            this.Tilesize = Tilesize;
        }

        /// <summary>
        /// Fire Changed event
        /// </summary>
        public void onChanged()
        {
            if (Changed != null)
            {
                Changed();
            }
        }

        public void LoadSplatTextureFromFile( string filepath )
        {
            splattexturefilename = filepath;
            splattexture = new ImageWrapper( filepath );
            //splattexture.Save( "newsplat.jpg" );
            onChanged();
        }

        public void LoadBlendTextureFromFile( string filepath )
        {
            blendtexturefilename = filepath;
            blendtexture = new ImageWrapper( filepath ); // note to self: Is this right???
            onChanged();
        }

        public void SaveBlendTextureToFile( string filepath )
        {
            blendtexturefilename = filepath;
            blendtexture.Save( filepath ); // note to self: Is this right???
        }

        // determines if this stage affects the map coordinates specified
        // returns true always, except for Blend
        public bool Affects(int mapx, int mapy, int mapwidth, int mapheight)
        {
            //Console.WriteLine("Affects");
            if (Operation == OperationType.Nop)
            {
              //  Console.WriteLine("return false: Nop");
                return false;
            }
            if (Operation != OperationType.Blend)
            {
                //Console.WriteLine("return true: !Blend");
                return true;
            }
            int texturex = (blendtexture.Width * mapx) / mapwidth;
            int texturey = (blendtexture.Height * mapy) / mapheight;
            //int texturex = ( blendtexture.AlphaData.GetUpperBound(0) * mapx ) / mapwidth;
            //int texturey = (blendtexture.AlphaData.GetUpperBound(1) * mapy) / mapheight;
            try
            {
                if( blendtexture.GetRed( texturex, texturey ) > 0 )
                //if (blendtexture.AlphaData[texturex, texturey] > 0)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                throw new Exception("texturex: " + texturex + " " + texturey + " mapx " + mapx + " mapy " + mapy + " mapwidth " + mapwidth + " " + mapheight);
            }
            return false;
        }

        public override string ToString()
        {
            return "MapTextureStageModel: " + this.splattexturefilename + " " + this.blendtexturefilename + " " + this.Operation + " " + this.Tilesize;
        }
    }
}
