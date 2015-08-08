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
using System.Xml.Serialization;
using Metaverse.Utility;

namespace OSMP
{
    // assumptions:
    // x-axis runs north-south, from south to north
    // y-axis runs east-west, from east to west
    // heightmapheight means northsouth mapsize
    // heightmapwidth means eastwest mapsize
    // display dimensions are 1 heightmap unit = 1 opengl unit
    public class TerrainModel
    {
        public delegate void TerrainModifiedHandler();
        //public delegate void FeatureAddedHandler(Unit unit, int x, int y);
        //public delegate void FeatureRemovedHandler(Unit unit, int x, int y);
        public delegate void HeightmapInPlaceEditedHandler(int xleft, int ytop, int xright, int ybottom);
        public delegate void BlendmapInPlaceEditedHandler(MapTextureStageModel maptexturestage, int xleft, int ytop, int xright, int ybottom);

        public event TerrainModifiedHandler TerrainModified;  // any change to terrain, heightmap, mapblendtexture except for
                                                              // heightmap in-place edited, or blend texture in-placed edited
        public event HeightmapInPlaceEditedHandler HeightmapInPlaceEdited;// we edited the heightmap, highfrequency change
        public event BlendmapInPlaceEditedHandler BlendmapInPlaceEdited;  // we edied the blendmap, highfrequency change
        //public event FeatureAddedHandler FeatureAdded;
        //public event FeatureRemovedHandler FeatureRemoved;

        //static TerrainModel instance = new TerrainModel();
        //public static TerrainModel GetInstance() { return instance; }

        public string Sm3Filename = "";
        public TdfParser tdfparser;

        [Replicate]
        public string HeightmapRelativeFilename
        {
            get
            {
                if (HeightmapFilename == "" || HeightmapFilename == null )
                {
                    return "";
                }
                return ProjectFileController.GetInstance().GetRelativePath( new Uri( HeightmapFilename ) );
            }
            set
            {
                if (value != "")
                {
                    HeightmapFilename = ProjectFileController.GetInstance().GetFullPath( value ).LocalPath;
                }
                else
                {
                    HeightmapFilename = "";
                }
            }
        }

        public string HeightmapFilename = "";

        public int HeightMapWidth
        {
            get { return MapWidth + 1; }
            set { MapWidth = value - 1; }
        }
        public int HeightMapHeight
        {
            get{ return MapHeight + 1; }
            set{ MapHeight = value - 1; }
        }
        //public int HeightMapWidth = 1025;
        //public int HeightMapHeight = 1025;
        public double[,] Map;

        [Replicate]
        public double MinHeight = 0;

        [Replicate]
        public double MaxHeight = 255;

        /// <summary>
        /// mapwidth, not including 1 pixel border on heightmap
        /// </summary>
        [Replicate]
        public int MapWidth;
        /// <summary>
        /// mapheight, not including 1 pixel border on heightmap
        /// </summary>
        [Replicate]
        public int MapHeight;
        //public Unit[,] FeatureMap; // assume max one feature per position.  Seems reasonable?

        public List<MapTextureStageModel> texturestages = new List<MapTextureStageModel>();

        [Replicate]
        public MapTextureStageModel[] texturestagesarray
        {
            get
            {
                return texturestages.ToArray();
            }
            set
            {
                texturestages.Clear();
                foreach (MapTextureStageModel texturestage in value)
                {
                    texturestages.Add( texturestage );
                }
            }
        }

        public TerrainModel()
        {
            MapWidth = Config.GetInstance().world_xsize;
            MapHeight = Config.GetInstance().world_ysize;
            MinHeight = Config.GetInstance().mingroundheight;
            MaxHeight = Config.GetInstance().maxgroundheight;
            Map = new double[HeightMapWidth, HeightMapHeight];
            //FeatureMap = new Unit[MapEastWestSize, MapNorthSouthSize];

            InitMap( MinHeight );
            LogFile.WriteLine("HeightMap() " + HeightMapWidth + " " + HeightMapHeight);

            texturestages = new List<MapTextureStageModel>();
            texturestages.Add( new MapTextureStageModel() );

            //texturestages = Sm3Persistence.GetInstance().LoadTextureStages(TdfParser.FromFile("maps/Whakamatunga_Riri.sm3").RootSection.SubSection("map/terrain"));
            //Sm3Persistence.GetInstance().LoadHeightMap(TdfParser.FromFile("maps/Whakamatunga_Riri.sm3").RootSection.SubSection("map/terrain"));

            //OnTerrainModified();
        }

        // scale or clip defined by Scale
        // mapsize is not including the +1 pixel border for heightmaps
        public void ChangeMapSize(int newmapsizex, int newmapsizey, bool Scale)
        {
            double[,] newmap = new double[newmapsizex + 1, newmapsizey + 1];
            for (int x = 0; x < newmapsizex + 1; x++)
            {
                for (int y = 0; y < newmapsizey + 1; y++)
                {
                    if (Scale)
                    {
                        int oldx = (x * HeightMapWidth) / (newmapsizex + 1);
                        int oldy = (y * HeightMapHeight) / (newmapsizey + 1);
                        newmap[x, y] = Map[oldx, oldy];
                    }
                    else
                    {
                        if (x < HeightMapWidth && y < HeightMapHeight)
                        {
                            newmap[x, y] = Map[x, y];
                        }
                        else
                        {
                            newmap[x, y] = MinHeight;
                        }
                    }
                }
            }
            Map = newmap;
            HeightMapWidth = newmapsizex + 1;
            HeightMapHeight = newmapsizey + 1;
            OnTerrainModified();
        }

        // scale or clip defined by Scale
        public void ChangeHeightScale(double minheight, double maxheight, bool Scale)
        {
            if (Scale)
            {
                //double offset = minheight - this.MinHeight;
                double multiplier = (maxheight - minheight) / (this.MaxHeight - this.MinHeight);
                for (int x = 0; x < HeightMapWidth; x++)
                {
                    for (int y = 0; y < HeightMapHeight; y++)
                    {
                        Map[x, y] = (Map[x, y] - this.MinHeight) * multiplier + minheight;
                    }
                }
            }
            else
            {
                for (int x = 0; x < HeightMapWidth; x++)
                {
                    for (int y = 0; y < HeightMapHeight; y++)
                    {
                        Map[x, y] = Math.Max(minheight, Math.Min(maxheight, Map[x, y]));
                    }
                }
            }
            this.MinHeight = minheight;
            this.MaxHeight = maxheight;
            OnTerrainModified();
        }

        public void NewMap()
        {
            MapWidth = Config.GetInstance().world_xsize;
            MapHeight = Config.GetInstance().world_ysize;
            MinHeight = Config.GetInstance().mingroundheight;
            MaxHeight = Config.GetInstance().maxgroundheight;
            Map = new double[HeightMapWidth, HeightMapHeight];
            InitMap(MinHeight);
            LogFile.WriteLine("HeightMap() " + HeightMapWidth + " " + HeightMapHeight);

            texturestages.Clear();
            texturestages.Add( new MapTextureStageModel() );
            // FeatureMap = new Unit[MapEastWestSize, MapNorthSouthSize];

            OnTerrainModified();
        }

        public void InitMap( double height )
        {
            for (int x = 0; x < HeightMapWidth; x++)
            {
                for (int y = 0; y < HeightMapHeight; y++)
                {
                    Map[x, y] = height;
                }
            }
        }

        // in-place blendmaptexture editing
        public void OnBlendMapInPlaceEdited( MapTextureStageModel maptexturestage, int xleft, int ytop, int xright, int ybottom ) // probably could be a little more specific...
        {
            //renderableheightmap.MapTexturesModified(Math.Max(0, xleft), Math.Max(0, ytop), Math.Min(HeightMapWidth - 1, xright), Math.Min(HeightMapHeight - 1, ybottom));
            if (BlendmapInPlaceEdited != null)
            {
                BlendmapInPlaceEdited(maptexturestage, Math.Max(0, xleft), Math.Max(0, ytop), Math.Min(HeightMapWidth - 2, xright), Math.Min(HeightMapHeight - 2, ybottom));
            }
        }

        // in-place heightmap editing
        public void OnHeightMapInPlaceEdited(int xleft, int ytop, int xright, int ybottom)
        {
            if (HeightmapInPlaceEdited != null)
            {
                HeightmapInPlaceEdited(Math.Max(0, xleft), Math.Max(0, ytop), Math.Min(HeightMapWidth - 2, xright), Math.Min(HeightMapHeight - 2, ybottom));
            }
            // renderablewater.Scale = new Vector2(HeightMapWidth * SquareSize, HeightMapHeight * SquareSize);
        }

        // anything not covered by other handlers
        public void OnTerrainModified()
        {
            LogFile.WriteLine( "TerrainModel.OnTerrainModified()" );
            if (TerrainModified != null)
            {
                TerrainModified();
            }
        }

        public override string ToString()
        {
            string result = "TerrainModel: " + HeightmapFilename + " " + this.MapHeight + " " + this.MapWidth + " " + this.MinHeight + " " + this.MaxHeight;
            foreach( MapTextureStageModel maptexturestagemodel in this.texturestages )
            {
                result += maptexturestagemodel + ", ";
            }
            return result;
        }
    }
}
