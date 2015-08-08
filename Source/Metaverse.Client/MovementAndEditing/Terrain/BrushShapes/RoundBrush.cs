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
using Gtk;

namespace OSMP
{
    public class RoundBrush : IBrushShape
    {
        public RoundBrush()
        {
        }

        public double GetStrength(double x, double y)
        {
            double distance = Math.Sqrt(x * x + y * y);
            if (distance <= coresizevalue / 100.0 )
            {
                return 1;
            }
            double falloffradius = 1 - coresizevalue / 100;
            double distanceintofalloffradius = distance - coresizevalue / 100;
            return 1 - distanceintofalloffradius / falloffradius;
        }

        HScale coresize;
        double coresizevalue = 0;

        public void ShowControlBox( Gtk.VBox labels, Gtk.VBox widgets )
        {
            Tooltips tooltips = new Tooltips();

            coresize = new HScale( 0, 100, 5 );
            coresize.Value = coresizevalue;
            coresize.ValueChanged += new EventHandler( coresize_ValueChanged );
            tooltips.SetTip( coresize, "Radius of core.  Core has no fall-off", "Radius of core.  Inside core, brush has full effect; outside of core, brush effect falls off towards the edge." );
            tooltips.Enable();

            labels.PackEnd( new Label( "Brush core radius:" ) );
            widgets.PackEnd( coresize );
            labels.ShowAll();
            widgets.ShowAll();
        }

        void coresize_ValueChanged( object sender, EventArgs e )
        {
            coresizevalue = coresize.Value;
            Console.WriteLine( "coresize changed: " + coresizevalue );
        }

        public void Render( Vector3 intersectpos  )
        {
            double radius = CurrentEditBrush.GetInstance().BrushSize;
            int segments = 32;
            double anglestep = Math.PI * 2 / segments;
            TerrainModel terrain = MetaverseClient.GetInstance().worldstorage.terrainmodel;
            double x, y, z;
            // outer radius
            double displayradius = radius ;
            Gl.glBegin( Gl.GL_LINE_LOOP );
            for (double angle = 0; angle < Math.PI * 2; angle += anglestep)
            {
                x = intersectpos.x + displayradius * Math.Sin( angle );
                y = intersectpos.y + displayradius * Math.Cos( angle );
                x = Math.Max( 0, x );
                x = Math.Min( x, terrain.MapWidth );
                y = Math.Max( 0, y );
                y = Math.Min( y, terrain.MapHeight );
                z = terrain.Map[(int)(x),
                    (int)(y)];
                z = Math.Max( 0.1, z ); // make sure visible over sea
                Gl.glVertex3d( x, y, z );
            }
            Gl.glEnd();
            // core radius
            displayradius = coresizevalue / 100 * radius;
            Gl.glBegin( Gl.GL_LINE_LOOP );
            for (double angle = 0; angle < Math.PI * 2; angle += anglestep)
            {
                x = intersectpos.x + displayradius * Math.Sin( angle );
                y = intersectpos.y + displayradius * Math.Cos( angle );
                x = Math.Max( 0, x );
                x = Math.Min( x, terrain.MapWidth );
                y = Math.Max( 0, y );
                y = Math.Min( y, terrain.MapHeight );
                z = terrain.Map[(int)(x),
                    (int)(y)];
                z = Math.Max( 0.1, z ); // make sure visible over sea
                Gl.glVertex3d( x, y, z );
            }
            Gl.glEnd();
        }

        public string Name
        {
            get
            {
                return "Round brush";
            }
        }

        public string Description
        {
            get
            {
                return "Round brush";
            }
        }
    }
}
