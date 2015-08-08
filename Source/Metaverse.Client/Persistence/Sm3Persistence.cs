// Copyright Hugh Perkins 2006
// hughperkins at gmail http://hughperkins.com
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
using System.IO;
using Metaverse.Utility;

namespace OSMP
{
    public class Sm3Persistence
    {
        static Sm3Persistence instance = new Sm3Persistence();
        public static Sm3Persistence GetInstance() { return instance; }

        TerrainModel terrain;

        public Sm3Persistence()
        {
            terrain = MetaverseClient.GetInstance().worldstorage.terrainmodel;
        }

        //TdfParser tdfparser = null;

        public void NewSm3()
        {
            //terrain.tdfparser = null;
            //terrain.Sm3Filename = "";
            terrain.NewMap();
            //terrain.OnTerrainModified();
        }

        public void SaveSm3(string filename)
        {
        }

        public void LoadSm3(string filename)
        {
            terrain.tdfparser = TdfParser.FromFile(filename);
            TdfParser.Section terrainsection = terrain.tdfparser.RootSection.SubSection("map/terrain");
            string tdfdirectory = Path.GetDirectoryName(Path.GetDirectoryName(filename));
            LoadTextureStages( tdfdirectory, terrainsection );
            LoadHeightMap(tdfdirectory, terrainsection);
            terrain.OnTerrainModified();
            MainTerrainWindow.GetInstance().InfoMessage("SM3 load completed");
        }

        void LoadHeightMap( string sm3directory, TdfParser.Section terrainsection)
        {
            TerrainModel terrainmodel = MetaverseClient.GetInstance().worldstorage.terrainmodel;

            string filename = Path.Combine( sm3directory, terrainsection.GetStringValue("heightmap") );
            double heightoffset = terrainsection.GetDoubleValue("heightoffset");
            double heightscale = terrainsection.GetDoubleValue("heightscale");
            LogFile.WriteLine("heightoffset: " + heightoffset + " heightscale " + heightscale);
            terrainmodel.MinHeight = heightoffset;
            terrainmodel.MaxHeight = heightoffset + heightscale; // I guess???

            ImageWrapper image = new ImageWrapper( filename );
            //Bitmap bitmap = DevIL.DevIL.LoadBitmap(filename);
            int width = image.Width;
            int height = image.Height;
            terrainmodel.HeightMapWidth = width;
            terrainmodel.HeightMapHeight = height;
            terrainmodel.Map = new double[width, height];
            LogFile.WriteLine("loaded bitmap " + width + " x " + height);
            double minheight = terrainmodel.MinHeight;
            double maxheight = terrainmodel.MaxHeight;
            double heightmultiplier = (maxheight - minheight) / 255;
            LogFile.WriteLine("heightmultiplier: " + heightmultiplier + " minheight: " + minheight);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    terrainmodel.Map[i, j] = 
                        (float)(minheight + heightmultiplier * 
                        image.GetBlue(i,j) );
                }
            }
            terrain.HeightmapFilename = filename;
        }

        List<MapTextureStageModel> LoadTextureStages(string sm3directory, TdfParser.Section terrainsection)
        {
            int numstages = terrainsection.GetIntValue("numtexturestages");
            List<MapTextureStageModel> stages = new List<MapTextureStageModel>();
            TerrainModel terrainmodel = MetaverseClient.GetInstance().worldstorage.terrainmodel;
            for (int i = 0; i < numstages; i++)
            {
                TdfParser.Section texstagesection = terrainsection.SubSection("texstage" + i);
                string texturename = texstagesection.GetStringValue("source");
                string blendertexturename = texstagesection.GetStringValue("blender");
                string operation = texstagesection.GetStringValue("operation").ToLower();

                int tilesize;
                ImageWrapper splattexture = LoadSplatTexture( sm3directory, terrainsection, texturename, out tilesize );
                if (operation == "blend")
                {
                    ImageWrapper blendtexture = LoadBlendTexture( sm3directory, terrainsection, blendertexturename);
                    stages.Add( new MapTextureStageModel(MapTextureStageModel.OperationType.Blend, tilesize, splattexture, blendtexture) );
                }
                else // todo: add other operations
                {
                    stages.Add( new MapTextureStageModel(MapTextureStageModel.OperationType.Replace, tilesize, splattexture ) );
                }
            }
            terrainmodel.texturestages = stages;
            return stages;
        }

        ImageWrapper LoadSplatTexture( string sm3directory, TdfParser.Section terrainsection, string texturesectionname, out int tilesize )
        {
            TdfParser.Section texturesection = terrainsection.SubSection(texturesectionname);
            string texturename = Path.Combine( sm3directory, texturesection.GetStringValue("file") );
            LogFile.WriteLine(texturename);
            tilesize = texturesection.GetIntValue("tilesize");
            ImageWrapper splattexture = new ImageWrapper( texturename );
            return splattexture;
            //return GlTexture.FromFile(texturename);
        }

        ImageWrapper LoadBlendTexture( string sm3directory, TdfParser.Section terrainsection, string texturesectionname )
        {
            TdfParser.Section texturesection = terrainsection.SubSection(texturesectionname);
            string texturename = Path.Combine( sm3directory, texturesection.GetStringValue("file") );
            LogFile.WriteLine(texturename);
            return new ImageWrapper( texturename );
            //return GlTexture.FromAlphamapFile(texturename);
        }
    }
}
