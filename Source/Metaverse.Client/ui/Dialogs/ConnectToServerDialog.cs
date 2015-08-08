// Copyright Hugh Perkins 2006
// hughperkins at gmail http://hughperkins.com
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
using Gtk;
using Glade;
using Metaverse.Utility;

namespace OSMP
{
    // used for choosing a new server
    public class ConnectToServerDialog
    {
        static ConnectToServerDialog instance = new ConnectToServerDialog();
        public static ConnectToServerDialog GetInstance(){ return instance; }

        [Widget]
        Entry entryserveripaddress = null;

        [Widget]
        Entry entryserverport = null;

        [Widget]
        Button btnok = null;

        [Widget]
        Button btncancel = null;

        [Widget]
        Window connecttoserverdialog = null;

        ConnectToServerDialog()
        {
            ContextMenuController.GetInstance().RegisterPersistentContextMenu(new string[] { "Network", "&Connect to server..." }, new ContextMenuHandler(ShowDialog));
        }

        void ShowDialog(object source, ContextMenuArgs e)
        {
            if( connecttoserverdialog != null )
            {
                connecttoserverdialog.Destroy();
            }

            Glade.XML app = new Glade.XML( EnvironmentHelper.GetExeDirectory() + "/metaverse.client.glade", "connecttoserverdialog", "" );
            app.Autoconnect(this);

            entryserveripaddress.Text = "127.0.0.1";
            entryserverport.Text = "2501";

            // note: events are configurable from Glade, but it's possibly easier to control from here
            btnok.Clicked += new EventHandler(btnok_Clicked);
            btncancel.Clicked += new EventHandler(btncancel_Clicked);
        }

        void btncancel_Clicked(object sender, EventArgs e)
        {
            connecttoserverdialog.Destroy();
        }

        void btnok_Clicked(object sender, EventArgs e)
        {
            string ipaddress = entryserveripaddress.Text;
            string portstring = entryserverport.Text;
            if (ipaddress == "") { return; }
            if (portstring == "") { return; }
            int port = 0;
            try
            {
                port = Convert.ToInt32(portstring);
            }
            catch
            {
                return;
            }
            LogFile.WriteLine("ConnectToServerDialog, connecting to " + ipaddress + " " + port);
            MetaverseClient.GetInstance().ConnectToServer(ipaddress, port);
        }
    }
}
