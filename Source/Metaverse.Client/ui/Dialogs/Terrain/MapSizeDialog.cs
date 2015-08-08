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
    class MapSizeDialog
    {
        public delegate void DoneCallback(object source);
        DoneCallback callback;

        public int width;
        public int height;

        [Widget]
        Combo widthentry = null;

        [Widget]
        Combo heightentry = null;

        [Widget]
        RadioButton radioScale = null;

        [Widget]
        Window mapsizedialog = null;

        void on_okbutton_clicked(object o, EventArgs e)
        {
            width = Convert.ToInt32(widthentry.Entry.Text);
            height = Convert.ToInt32(heightentry.Entry.Text);
            mapsizedialog.Destroy();
            if (callback == null)
            {
                int mapwidth = width * 64;
                int mapheight = height * 64;
                MetaverseClient.GetInstance().worldstorage.terrainmodel.ChangeMapSize( mapwidth, mapheight, radioScale.Active );
                // CommandQueueFactory.FromUI.Enqueue(new CmdMapSizeChange(width, height));
            }
            else
            {
                callback(this);
            }
        }
        void Init()
        {
            int width = (MetaverseClient.GetInstance().worldstorage.terrainmodel.HeightMapWidth - 1) / 64;
            int height = (MetaverseClient.GetInstance().worldstorage.terrainmodel.HeightMapHeight - 1) / 64;
            widthentry.Entry.Text = width.ToString();
            heightentry.Entry.Text = height.ToString();
        }
        public MapSizeDialog()
        {
            Glade.XML app = new Glade.XML( EnvironmentHelper.GetExeDirectory() + "/TerrainEditing.glade", "mapsizedialog", "" );
            app.Autoconnect(this);
            Init();
        }
        public MapSizeDialog(DoneCallback callback)
        {
            this.callback = callback;
            Glade.XML app = new Glade.XML( EnvironmentHelper.GetExeDirectory() + "/TerrainEditing.glade", "mapsizedialog", "" );
            app.Autoconnect(this);
            Init();
        }
    }
    /*
class HeightMapSizeDialog
{
    public delegate void DoneCallback(object source);

    public int width;
    public int height;

    Window window = null;
    Combo widthcombo = null;
    Combo heightcombo = null;
    Button button;

    DoneCallback source;
     * 
    public HeightMapSizeDialog(DoneCallback source)
    {
        this.source = source;

        window = new Window("Heightmap width and height:");
        window.SetDefaultSize(200, 100);

        Table table = new Table(3, 2, false);
        table.BorderWidth = 20;
        table.RowSpacing = 20;
        table.ColumnSpacing = 20;
        window.Add(table);

        table.Attach(new Label("Width:"), 0, 1, 0, 1);
        table.Attach(new Label("Height:"), 0, 1, 1, 2);

        widthcombo = new Combo();
        widthcombo.PopdownStrings = new string[] { "4", "8", "12", "16", "20", "24", "28", "32" };
        table.Attach(widthcombo, 1, 2, 0, 1);

        heightcombo = new Combo();
        heightcombo.PopdownStrings = new string[] { "4", "8", "12", "16", "20", "24", "28", "32" };
        table.Attach(heightcombo, 1, 2, 1, 2);

        Button button = new Button(Stock.Ok);
        button.Clicked += new EventHandler(OnOkClicked);
        button.CanDefault = true;

        table.Attach(button, 1, 2, 2, 3);

        window.Modal = true;
        window.ShowAll();
    }

    void OnOkClicked(object o, EventArgs args)
    {
        Console.WriteLine("Ok clicked");
        Console.WriteLine(widthcombo.Entry.Text);
        width = Convert.ToInt32(widthcombo.Entry.Text);
        height = Convert.ToInt32(heightcombo.Entry.Text);
        window.Hide();
        source(this);
    }
}
    */
}
