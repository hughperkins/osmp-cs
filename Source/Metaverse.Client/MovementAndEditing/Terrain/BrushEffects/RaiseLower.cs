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
    public class RaiseLower : IBrushEffect
    {
        double speed;

        public RaiseLower()
        {
            speed = Config.GetInstance().HeightEditingSpeed;
        }

        public void ApplyBrush( IBrushShape brushshape, int brushsize, double brushcentre_x, double brushcentre_y, bool israising, double timespanmilliseconds )
        {
            TerrainModel terrain = MetaverseClient.GetInstance().worldstorage.terrainmodel;
            double[,] mesh = terrain.Map;

            int x = (int)(brushcentre_x );
            int y = (int)(brushcentre_y );

            double timemultiplier = timespanmilliseconds * speed;
            int meshsize = mesh.GetUpperBound( 0 ) + 1;
            for (int i = -brushsize; i <= brushsize; i++)
            {
                for (int j = -brushsize; j <= brushsize; j++)
                {
                    int thisx = x + i;
                    int thisy = y + j;
                    if (thisx >= 0 && thisy >= 0 && thisx < meshsize &&
                        thisy < meshsize)
                    {
                        double brushshapecontribution = brushshape.GetStrength( (double)i / brushsize, (double)j / brushsize );
                        if (brushshapecontribution > 0)
                        {
                            double directionmultiplier = 1.0;
                            if (!israising)
                            {
                                directionmultiplier = -1.0;
                            }
                            mesh[thisx, thisy] += (brushshapecontribution * directionmultiplier * timemultiplier);
                            if (mesh[thisx, thisy] >= terrain.MaxHeight)
                            {
                                mesh[thisx, thisy] = terrain.MaxHeight;
                            }
                            else if (mesh[thisx, thisy] < terrain.MinHeight)
                            {
                                mesh[thisx, thisy] = terrain.MinHeight;
                            }
                        }
                    }
                }
            }
            terrain.OnHeightMapInPlaceEdited( x - brushsize, y - brushsize, x + brushsize, y + brushsize );
        }

        public void ShowControlBox( Gtk.VBox labels, Gtk.VBox widgets )
        {
        }

        public bool Repeat
        {
            get
            {
                return true;
            }
        }

        public string Name
        {
            get { return "Raise/lower"; }
        }

        public string Description
        {
            get { return "Leftclick map to raise land, right click to lower"; }
        }
    }
}
