// Copyright Hugh Perkins 2006
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
using System.Net;
using Metaverse.Utility;

namespace OSMP
{
    public class RemoteServerInfoDialog
    {
        [Widget]
        Entry nameentry = null;

        [Widget]
        Entry ipaddressentry = null;

        [Widget]
        Entry portentry = null;

        [Widget]
        Button btnclose = null;

        [Widget]
        Window remoteserverinfodialog = null;

        public RemoteServerInfoDialog( string servername, XmlCommands.ServerInfo serverinfo )
        {
            Glade.XML app = new Glade.XML( EnvironmentHelper.GetExeDirectory() + "/metaverse.client.glade", "remoteserverinfodialog", "" );
            app.Autoconnect( this );

            btnclose.Clicked += new EventHandler( btnclose_Clicked );

            nameentry.Text = servername;
            ipaddressentry.Text = new IPAddress( serverinfo.IPAddress ).ToString();
            portentry.Text = serverinfo.port.ToString();
        }

        void btnclose_Clicked( object sender, EventArgs e )
        {
            remoteserverinfodialog.Destroy();
        }
    }
}
