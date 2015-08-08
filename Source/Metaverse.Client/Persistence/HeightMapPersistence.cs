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
using System.IO;
using Metaverse.Utility;

namespace OSMP
{
    public class HeightMapPersistence
    {
        static HeightMapPersistence instance = new HeightMapPersistence();
        public static HeightMapPersistence GetInstance() { return instance; }

        HeightMapPersistence()
        {
            //KeyFilterConfigMappingsFactory.GetInstance().RegisterCommand("saveheightmap", new KeyCommandHandler(SaveHandler));
            //KeyFilterConfigMappingsFactory.GetInstance().RegisterCommand("loadheightmap", new KeyCommandHandler(LoadHandler));
        }

        void NewHeightMap(int heightmapwidth, int heightmapheight)
        {
            TerrainModel terrain = MetaverseClient.GetInstance().worldstorage.terrainmodel;
            terrain.HeightMapWidth = heightmapwidth;
            terrain.HeightMapHeight = heightmapheight;
            terrain.Map = new double[terrain.HeightMapWidth, terrain.HeightMapHeight];
            terrain.HeightmapFilename = "";
            terrain.OnTerrainModified();
        }

        //public void Load()
        //{
          //  Load(Config.GetInstance().defaultHeightMapFilename);
        //}

        public void Load(string filename)
        {
            //Bitmap bitmap = Bitmap.FromFile(filename) as Bitmap;
            //Bitmap bitmap = DevIL.DevIL.LoadBitmap(filename);
            ImageWrapper image = new ImageWrapper( filename );
            int width = image.Width;
            int height = image.Height;
            MetaverseClient.GetInstance().worldstorage.terrainmodel.HeightMapWidth = width;
            MetaverseClient.GetInstance().worldstorage.terrainmodel.HeightMapHeight = height;
            MetaverseClient.GetInstance().worldstorage.terrainmodel.Map = new double[width, height];
            LogFile.WriteLine("loaded bitmap " + width + " x " + height);
            double minheight = Config.GetInstance().mingroundheight;
            double maxheight = Config.GetInstance().maxgroundheight;
            double heightmultiplier = ( maxheight - minheight ) / 255;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    MetaverseClient.GetInstance().worldstorage.terrainmodel.Map[i, j] = (float)( minheight + 
                        heightmultiplier * 
                        image.GetBlue(i, j) );
                }
            }
            MetaverseClient.GetInstance().worldstorage.terrainmodel.HeightmapFilename = filename;
            MetaverseClient.GetInstance().worldstorage.terrainmodel.OnTerrainModified();
            MainTerrainWindow.GetInstance().InfoMessage("Heightmap loaded");
        }

        public void Save()
        {
            if (MetaverseClient.GetInstance().worldstorage.terrainmodel.HeightmapFilename != "")
            {
                Save(MetaverseClient.GetInstance().worldstorage.terrainmodel.HeightmapFilename);
            }
            //else
            //{
              //  Save(Config.GetInstance().defaultHeightMapFilename);
            //}
        }

        public void Save(string filename)
        {
            double[,] mesh = MetaverseClient.GetInstance().worldstorage.terrainmodel.Map;
            int width = mesh.GetUpperBound(0) + 1;
            int height = mesh.GetUpperBound(1) + 1;
            ImageWrapper image = new ImageWrapper( width, height );
            //Bitmap bitmap = new Bitmap( width, height, PixelFormat.Format24bppRgb );
            //Graphics g = Graphics.FromImage(bitmap);
            //Pen[] pens = new Pen[256];
            //for (int i = 0; i < 256; i++)
            //{
              //  pens[i] = new Pen(System.Drawing.Color.FromArgb(i, i, i));
            //}
            double minheight = Config.GetInstance().mingroundheight;
            double maxheight = Config.GetInstance().maxgroundheight;
            double heightmultiplier = 255 / (maxheight - minheight);
            for (int i = 0; i < width; i++)
            {
                for( int j = 0; j < height; j++ )
                {
                    int normalizedmeshvalue = (int)( (mesh[i, j] - minheight) * heightmultiplier );
                    normalizedmeshvalue = Math.Max( 0,normalizedmeshvalue );
                    normalizedmeshvalue = Math.Min( 255,normalizedmeshvalue );
                    byte normalizedmeshvaluebyte = (byte)normalizedmeshvalue;
                    image.SetPixel(i, j, normalizedmeshvaluebyte, normalizedmeshvaluebyte, normalizedmeshvaluebyte, 255);
                    //g.DrawRectangle(pens[ normalizedmeshvalue ], i, j, 1, 1);
                }
            }
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            image.Save(filename);
            //DevIL.DevIL.SaveBitmap(filename, bitmap);
            //bitmap.Save(filename, ImageFormat.Bmp);
            MetaverseClient.GetInstance().worldstorage.terrainmodel.HeightmapFilename = filename;
            MetaverseClient.GetInstance().worldstorage.terrainmodel.OnTerrainModified();
            MainTerrainWindow.GetInstance().InfoMessage("Heightmap saved");
        }

        //void SaveHandler(string command, bool down)
        //{
          //  if (down)
            //{
              //  Save();
            //}
        //}

        //void LoadHandler(string command, bool down)
        //{
          //  if (down)
            //{
              //  Load();
            //}
        //}
    }
}
