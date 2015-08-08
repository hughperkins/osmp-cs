// Copyright Hugh Perkins 2006
// hughperkins@gmail.com http://manageddreams.com
//
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License version 2 as published by the
// Free Software Foundation;
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
using System.Threading;
using Gtk;
using Glade;
using Metaverse.Utility;

namespace OSMP
{
    class LodDialog
    {
        [Widget]
        HScale lod1 = null;

        [Widget]
        HScale lod2 = null;

        [Widget]
        HScale lod3 = null;

        [Widget]
        HScale lod4 = null;

        [Widget]
        HScale lod5 = null;

        [Widget]
        HScale lod6 = null;

        [Widget]
        Window levelofdetaildialog = null;
        
        // Get rid of compiler warnings
        void KillWarnings()
        {
            on_lod1_value_changed( null, null );
            on_lod2_value_changed( null, null );
            on_lod3_value_changed( null, null );
            on_lod4_value_changed( null, null );
            on_lod5_value_changed( null, null );
            on_lod6_value_changed( null, null );
            KillWarnings();
        }

        void on_lod1_value_changed(object o, EventArgs e)
        {
            ChangeLods();
        }

        void on_lod2_value_changed(object o, EventArgs e)
        {
            ChangeLods();
        }

        void on_lod3_value_changed(object o, EventArgs e)
        {
            ChangeLods();
        }

        void on_lod4_value_changed(object o, EventArgs e)
        {
            ChangeLods();
        }

        void on_lod5_value_changed(object o, EventArgs e)
        {
            ChangeLods();
        }

        void on_lod6_value_changed(object o, EventArgs e)
        {
            ChangeLods();
        }

        void ChangeLods()
        {
            int lod1value = (int)lod1.Value;
            int lod2value = (int)lod2.Value;
            int lod3value = (int)lod3.Value;
            int lod4value = (int)lod4.Value;
            int lod5value = (int)lod5.Value;
            int lod6value = (int)lod6.Value;

            List<int> lodlist = new List<int>();
            lodlist.Add(lod1value);
            lodlist.Add(lod2value);
            lodlist.Add(lod3value);
            lodlist.Add(lod4value);
            lodlist.Add(lod5value);
            lodlist.Add(lod6value);
            MetaverseClient.GetInstance().worldview.terrainview.SetLod(lodlist.ToArray());
        }

        public void on_okbutton_clicked(object o, EventArgs e)
        {
            ChangeLods();
            levelofdetaildialog.Destroy();
        }

        public LodDialog()
        {
            Glade.XML app = new Glade.XML( EnvironmentHelper.GetExeDirectory() + "/TerrainEditing.glade", "levelofdetaildialog", "" );
            app.Autoconnect(this);

            int[] lod = MetaverseClient.GetInstance().worldview.terrainview.GetLod();
            lod1.Value = lod[0];
            //LogFile.WriteLine( lod[0] + " " + lod1.Value);
            lod2.Value = lod[1];
            //LogFile.WriteLine(lod[1] + " " + lod2.Value);
            lod3.Value = lod[2];
            //LogFile.WriteLine(lod[2] + " " + lod3.Value);
            lod4.Value = lod[3];
            lod5.Value = lod[4];
            lod6.Value = lod[5];
        }
    }
}

