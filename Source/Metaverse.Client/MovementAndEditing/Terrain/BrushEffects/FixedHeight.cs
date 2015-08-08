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
using Gtk;
using Metaverse.Utility;

namespace OSMP
{
    public class FixedHeight : IBrushEffect
    {
        double minheight;
        double maxheight;

        double speed;

        public FixedHeight()
        {
            speed = Config.GetInstance().HeightEditingSpeed;
            MetaverseClient.GetInstance().worldstorage.terrainmodel.TerrainModified += new TerrainModel.TerrainModifiedHandler( FixedHeight_TerrainModified );
        }

        void FixedHeight_TerrainModified()
        {
            minheight = MetaverseClient.GetInstance().worldstorage.terrainmodel.MinHeight;
            maxheight = MetaverseClient.GetInstance().worldstorage.terrainmodel.MaxHeight;
            LogFile.WriteLine( "FixedHeight_terrainmodified " + minheight + " " + maxheight );
            if (heightscale != null)
            {
                heightscale.SetRange( minheight, maxheight );
                lastheight = Math.Min( maxheight, lastheight );
                lastheight = Math.Max( minheight, lastheight );
            }
        }

        public void ApplyBrush( IBrushShape brushshape, int brushsize, double brushcentrex, double brushcentrey, bool israising, double milliseconds )
        {
            if (heightscale == null)
            {
                return;
            }
            double targetheight = heightscale.Value;
            Console.WriteLine( "height scale value: " + targetheight );
            int x = (int)( brushcentrex );
            int y = (int)(brushcentrey );

            for (int i = -brushsize; i <= brushsize; i++)
            {
                for (int j = -brushsize; j <= brushsize; j++)
                {
                    double brushcontribution = brushshape.GetStrength(
                        (double)i / brushsize, (double)j / brushsize );
                    if (brushcontribution > 0)
                    {
                        int thisx = x + i;
                        int thisy = y + j;
                        if (thisx >= 0 && thisy >= 0 &&
                            thisx < MetaverseClient.GetInstance().worldstorage.terrainmodel.HeightMapWidth &&
                            thisy < MetaverseClient.GetInstance().worldstorage.terrainmodel.HeightMapHeight)
                        {
                            double oldheight = MetaverseClient.GetInstance().worldstorage.terrainmodel.Map[thisx, thisy];
                            double newheight = oldheight + (targetheight - oldheight) *
                                speed * milliseconds / 50 * brushcontribution;
                            MetaverseClient.GetInstance().worldstorage.terrainmodel.Map[thisx, thisy] = newheight;
                        }
                    }
                }
            }
            MetaverseClient.GetInstance().worldstorage.terrainmodel.OnHeightMapInPlaceEdited( x - brushsize, y - brushsize, x + brushsize, y + brushsize );
        }

        public string Name
        {
            get { return "Set height"; }
        }

        public string Description
        {
            get { return "Sets terrain to predefined height"; }
        }

        public bool Repeat { get { return true; } }

        HScale heightscale = null;

        double lastheight = 0;

        public void ShowControlBox( Gtk.VBox labels, Gtk.VBox widgets )
        {
            Label label = new Label( "Target height:" );

            heightscale = new HScale( minheight, maxheight, (maxheight - minheight) / 20 );
            heightscale.Value = lastheight;
            heightscale.ValueChanged += new EventHandler( heightscale_ValueChanged );

            labels.PackEnd( label );
            widgets.PackEnd( heightscale );
            labels.ShowAll();
            widgets.ShowAll();
        }

        void heightscale_ValueChanged( object sender, EventArgs e )
        {
            Console.WriteLine( "slider moved: " + heightscale.Value );
            lastheight = heightscale.Value;
        }
    }
}
