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
using Tao.OpenGl;

namespace OSMP
{
    public class SquareBrush : IBrushShape
    {
        public double GetStrength( double x, double y )
        {
            double distance = Math.Max( Math.Abs( x ), Math.Abs( y ) );
            return 1 - distance;
        }

        public void ShowControlBox( Gtk.VBox labels, Gtk.VBox widgets ) { }

        public void Render( Vector3 intersectpos )
        {
            double radius = CurrentEditBrush.GetInstance().BrushSize;
            double displayradius = radius;

            TerrainModel terrain = MetaverseClient.GetInstance().worldstorage.terrainmodel;

            int numsegments = 16;

            // list four corner of square, in order
            List<Vector2> corners = new List<Vector2>(); 
            corners.Add( new Vector2( -1, -1 ) );
            corners.Add( new Vector2( -1, 1 ) );
            corners.Add( new Vector2( 1, 1 ) );
            corners.Add( new Vector2( 1, -1 ) );

            Gl.glBegin( Gl.GL_LINE_LOOP );
            for (int i = 0; i < 4; i++ )
            {
                Vector2 startcorner = corners[i];
                Vector2 endcorner = corners[ ( i + 1 ) % 4 ];
                double startx = intersectpos.x + startcorner.x * displayradius;
                double starty = intersectpos.y + startcorner.y * displayradius;
                startx = Math.Max( 0, startx );
                startx = Math.Min( startx, terrain.MapWidth );
                starty = Math.Max( 0, starty );
                starty = Math.Min( starty, terrain.MapHeight );

                double endx = intersectpos.x + endcorner.x * displayradius;
                double endy = intersectpos.y + endcorner.y * displayradius;
                endx = Math.Max( 0, endx );
                endx = Math.Min( endx, terrain.MapWidth );
                endy = Math.Max( 0, endy );
                endy = Math.Min( endy, terrain.MapHeight );

                for (int segment = 0; segment < numsegments; segment++)
                {
                    double x = startx + (endx - startx) * segment / numsegments;
                    double y = starty + (endy - starty) * segment / numsegments;
                    double z = MetaverseClient.GetInstance().worldstorage.terrainmodel.Map[(int)(x),
                        (int)(y)];
                    z = Math.Max( 0.1, z ); // make sure visible over sea
                    Gl.glVertex3d( x, y, z );
                }
            }
            Gl.glEnd();
        }

        public string Name
        {
            get
            {
                return "Square brush";
            }
        }

        public string Description
        {
            get
            {
                return "Square brush";
            }
        }
    }
}
