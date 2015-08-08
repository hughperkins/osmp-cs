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
using Glade;
using Gtk;
using Gdk;
using Metaverse.Utility;

namespace OSMP
{
    public class ShowUsersDialog
    {
        static ShowUsersDialog instance = new ShowUsersDialog();
        public static ShowUsersDialog GetInstance() { return instance; }

        [Widget]
        ScrolledWindow usersscrolledwindow = null;

        [Widget]
        TreeView userstreeview = null;

        [Widget]
        Button btnclose = null;

        [Widget]
        Gtk.Window showusersdialog = null;

        ShowUsersDialog()
        {
        }

        public void Show( string[] userlist )
        {
            Console.WriteLine("showusersdialog.Show()");

            if (showusersdialog != null)
            {
                showusersdialog.Destroy();
            }

            Glade.XML app = new Glade.XML( EnvironmentHelper.GetExeDirectory() + "/metaverse.client.glade", "showusersdialog", "" );
            app.Autoconnect( this );

            btnclose.Clicked += new EventHandler( btnclose_Clicked );

            ListStore liststore = new ListStore( typeof( string ) );
            userstreeview.Model = liststore;

            userstreeview.AppendColumn( "User name:", new CellRendererText(), "text", 0 );

            userstreeview.ShowAll();

            foreach (string username in userlist)
            {
                liststore.AppendValues( username );
            }
        }

        void btnclose_Clicked( object sender, EventArgs e )
        {
            Hide();
        }

        public void Hide()
        {
            showusersdialog.Destroy();
            showusersdialog = null;
        }
    }
}
