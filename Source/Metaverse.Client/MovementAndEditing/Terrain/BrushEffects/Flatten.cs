// Created by Hugh Perkins 2006
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
using Metaverse.Utility;

namespace OSMP
{
    public class Flatten : IBrushEffect
    {
        double speed;

        public Flatten()
        {
            speed = Config.GetInstance().HeightEditingSpeed;
        }

        public void ApplyBrush( IBrushShape brushshape, int brushsize, double brushcentre_x, double brushcentre_y, bool israising, double milliseconds )
        {
            TerrainModel terrain = MetaverseClient.GetInstance().worldstorage.terrainmodel;
            double[,] mesh = terrain.Map;

            int x = (int)( brushcentre_x );
            int y = (int)(brushcentre_y );

            double timemultiplier = milliseconds * speed;
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
                            mesh[thisx, thisy] = mesh[thisx, thisy] + (mesh[x, y] - mesh[thisx, thisy]) * brushshapecontribution * timemultiplier / 50;
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
            get { return "Flatten"; }
        }

        public string Description
        {
            get { return "Flatten land inside brush"; }
        }
    }
}
