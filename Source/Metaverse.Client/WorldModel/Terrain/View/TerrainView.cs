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
    public class TerrainView
    {
        TerrainModel terrainmodel;

        public RenderableHeightMap renderableheightmap;
        //RenderableAllFeatures renderableallfeatures;
        public RenderableWater renderablewater;
        //public RenderableMinimap renderableminimap;

        //public List<MapTextureStageView> maptexturestageviews = new List<MapTextureStageView>();
        public Dictionary<MapTextureStageModel, MapTextureStageView> mapviewbymapmodel = new Dictionary<MapTextureStageModel, MapTextureStageView>();

        public TerrainView( TerrainModel terrainmodel )
        {
            LogFile.WriteLine( "TerrainView( " + terrainmodel + ")" );

            this.terrainmodel = terrainmodel;

            terrainmodel.TerrainModified += new TerrainModel.TerrainModifiedHandler( terrainmodel_TerrainModified );

            GraphicsHelperGl g = new GraphicsHelperGl();
            g.CheckError();
            foreach (MapTextureStageModel maptexturestagemodel in terrainmodel.texturestages)
            {
                LogFile.WriteLine( "create maptexturestageview for " + maptexturestagemodel );
                mapviewbymapmodel.Add( maptexturestagemodel, new MapTextureStageView( maptexturestagemodel ) );
            }
            g.CheckError();

            renderableheightmap = new RenderableHeightMap( this, terrainmodel, 1, 1 );
            //renderableallfeatures = new RenderableAllFeatures(this);
            // water must be last, otherwise you cant see through it ;-)
            renderablewater = new RenderableWater(new Vector3(), new Vector2(terrainmodel.HeightMapWidth , terrainmodel.HeightMapHeight ));
            // minimap last, covers everything else
            //renderableminimap = new RenderableMinimap(this, renderableheightmap);
        }

        void terrainmodel_TerrainModified()
        {
            Console.WriteLine( "terrainview.terrainmodified" );
            //mapviewbymapmodel.Clear();
            //maptexturestageviews.Clear();
            foreach (MapTextureStageModel maptexturestagemodel in terrainmodel.texturestages)
            {
                if (!mapviewbymapmodel.ContainsKey( maptexturestagemodel ))
                {
                    Console.WriteLine( "creating maptexturestageview for " + maptexturestagemodel );
                    mapviewbymapmodel.Add( maptexturestagemodel, new MapTextureStageView( maptexturestagemodel ) );
                }
            }
            List<MapTextureStageModel> modelsremoved = new List<MapTextureStageModel>();
            foreach (MapTextureStageModel maptexturestagemodel in mapviewbymapmodel.Keys)
            {
                if (!terrainmodel.texturestages.Contains( maptexturestagemodel ))
                {
                    modelsremoved.Add( maptexturestagemodel );
                }
            }
            foreach (MapTextureStageModel maptexturestagemodel in modelsremoved)
            {
                LogFile.WriteLine( "Removing mapview for " + maptexturestagemodel );
                mapviewbymapmodel.Remove( maptexturestagemodel );
            }
            renderablewater.Scale = new Vector2( terrainmodel.HeightMapWidth, terrainmodel.HeightMapHeight );
        }


        public void SetLod(int[] lod)
        {
            renderableheightmap.loddistances = lod;
        }

        public int[] GetLod()
        {
            return renderableheightmap.loddistances;
        }
    }
}
