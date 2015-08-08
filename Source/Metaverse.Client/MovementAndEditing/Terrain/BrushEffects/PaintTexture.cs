// Created by Hugh Perkins 2006
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
using Metaverse.Utility;

namespace OSMP
{
    public class PaintTexture : IBrushEffect
    {
        double speed;

        public PaintTexture()
        {
            speed = Config.GetInstance().HeightEditingSpeed * 20;
        }

        MapTextureStageModel maptexturestage = null;
        ImageWrapper thistexture = null;
        int texturewidth;
        int textureheight;
        double[,] alphadata = null;

        void LoadTextureData( MapTextureStageModel maptexturestage )
        {
            LogFile.WriteLine( "PaintTexture.LoadTextureData( " + maptexturestage );
            thistexture = maptexturestage.blendtexture;
            if (thistexture != null)
            {
                texturewidth = thistexture.Width;
                textureheight = thistexture.Height;
                LogFile.WriteLine( "edittexture width " + texturewidth + " height " + textureheight );
                alphadata = new double[texturewidth, textureheight];
                for (int x = 0; x < texturewidth; x++)
                {
                    for (int y = 0; y < textureheight; y++)
                    {
                        alphadata[x, y] = thistexture.GetRed( x, y );
                    }
                }
            }
        }

        public void SetCurrentEditTexture( MapTextureStageModel maptexturestage )
        {
            this.maptexturestage = maptexturestage;
            LoadTextureData( maptexturestage );
        }

        public void ApplyBrush( IBrushShape brushshape, int brushsize, double brushcentrex, double brushcentrey, bool israising, double milliseconds )
        {
            if ( thistexture != null && maptexturestage != null )
            {
                double timemultiplier = milliseconds * speed;

                double directionmultiplier = 1.0;
                if (!israising)
                {
                    directionmultiplier = -1.0;
                }

                int mapx = (int)(brushcentrex );
                int mapy = (int)(brushcentrey );
                int mapwidth = MetaverseClient.GetInstance().worldstorage.terrainmodel.HeightMapWidth - 1;
                int mapheight = MetaverseClient.GetInstance().worldstorage.terrainmodel.HeightMapHeight - 1;
                int texturex = (int)(texturewidth * mapx / mapwidth);
                int texturey = (int)(textureheight * mapy / mapheight);
                int texturebrushwidth = (int)(texturewidth * brushsize / mapwidth);
                int texturebrushheight = (int)(textureheight * brushsize / mapheight);
                for (int i = -texturebrushwidth; i <= texturebrushwidth; i++)
                {
                    for (int j = -texturebrushheight; j <= texturebrushheight; j++)
                    {
                        double brushshapecontribution = brushshape.GetStrength( (double)i / texturebrushwidth,
                           (double)j / texturebrushheight );
                        if (brushshapecontribution > 0)
                        {
                            int thisx = texturex + i;
                            int thisy = texturey + j;
                            //      Console.WriteLine(thisx + " " + thisy);
                            if (thisx >= 0 && thisy >= 0 && thisx < texturewidth &&
                                thisy < textureheight)
                            {
                                // we update our double array then set the int array iwthin ITexture itself
                                //LogFile.WriteLine( speed + " " + directionmultiplier + " " + timemultiplier + " " + brushshapecontribution );
                                alphadata[thisx, thisy] += speed * directionmultiplier * timemultiplier * brushshapecontribution;
                                if (alphadata[thisx, thisy] >= 255)
                                {
                                    alphadata[thisx, thisy] = 255;
                                }
                                else if (alphadata[thisx, thisy] < 0)
                                {
                                    alphadata[thisx, thisy] = 0;
                                }
                                thistexture.SetPixel( thisx, thisy, (byte)alphadata[thisx, thisy], (byte)alphadata[thisx, thisy], (byte)alphadata[thisx, thisy], 255 );
                                //LogFile.WriteLine( "setting pixel " + thisx + " " + thisy + " to " + (byte)alphadata[thisx, thisy] );
                                //thistexture.AlphaData[thisx, thisy] = (byte)alphadata[thisx, thisy];
                                //  Console.WriteLine(thisx + " " + thisy + " " + (byte)alphadata[thisx, thisy]);
                            }
                        }
                    }
                }

                thistexture.Save( "editedblend.jpg" );

                //thistexture.ReloadAlpha();
                maptexturestage.onChanged();
                //thistexture.Modified = true;
                MetaverseClient.GetInstance().worldstorage.terrainmodel.OnBlendMapInPlaceEdited( maptexturestage, mapx - brushsize, mapy - brushsize, mapx + brushsize, mapy + brushsize );
            }
        }

        public void ShowControlBox( Gtk.VBox labels, Gtk.VBox widgets )
        {
        }

        public string Name
        {
            get { return "Paint texture"; }
        }

        public string Description
        {
            get { return "left-click map to paint texture, right click to remove"; }
        }

        public bool Repeat
        {
            get
            {
                return true;
            }
        }
    }
}
