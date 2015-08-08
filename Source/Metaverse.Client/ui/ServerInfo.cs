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
using System.Net;
using System.Net.Sockets;

using Glade;
using Gtk;
using Metaverse.Utility;

namespace OSMP
{
    public class ServerInfo
    {
        static ServerInfo instance = new ServerInfo();
        public static ServerInfo GetInstance() { return instance; }

        [Widget]
        Entry localipaddressentry = null;

        [Widget]
        Entry localportentry = null;

        [Widget]
        Entry publicportentry = null;

        [Widget]
        Entry publicipaddressentry = null;

        [Widget]
        Entry friendipaddressentry = null;

        [Widget]
        Entry friendportentry = null;

        [Widget]
        //ToggleButton btnholdopen = null;
        Button btnsendping = null;

        [Widget]
        Button btnclose = null;

        [Widget]
        Window serverinfowindow = null;

        ServerInfo()
        {
            if (MetaverseServer.GetInstance().Running)
            {
                ContextMenuController.GetInstance().RegisterPersistentContextMenu(new string[] { "Network", "&Server Info..." }, new ContextMenuHandler(ServerInfoDialog));
                //ContextMenuController.GetInstance().RegisterPersistentContextMenu( new string[] { "Network", "&Private server Info..." }, new ContextMenuHandler( PrivateServerInfoDialog ) );
                MetaverseClient.GetInstance().Tick += new MetaverseClient.TickHandler( ServerInfo_Tick );
            }
        }

        DateTime lastping;
        void ServerInfo_Tick()
        {
            if (holdopendown && DateTime.Now.Subtract( lastping ).TotalMilliseconds > 1000)
            {
                SendPing();
                lastping = DateTime.Now;
            }
        }

        void SendPing()
        {
            if (friendipaddressentry.Text == "" )
            {
                return;
            }
            int friendport = 1234;
            try
            {
                friendport = Convert.ToInt32( friendportentry.Text );
            }
            catch
            {
            }
            MetaverseServer.GetInstance().PingClient( friendipaddressentry.Text, friendport );
        }

        void ServerInfoDialog(object source, ContextMenuArgs e)
        {
            if (serverinfowindow != null)
            {
                serverinfowindow.Destroy();
            }
            Glade.XML app = new Glade.XML( EnvironmentHelper.GetExeDirectory() + "/metaverse.client.glade", "serverinfowindow", "" );
            app.Autoconnect( this );

            btnsendping.Clicked += new EventHandler( btnsendping_Clicked );
            //btnholdopen.Pressed += new EventHandler( btnholdopen_Pressed );
            //btnholdopen.Released += new EventHandler( btnholdopen_Released );
            btnclose.Clicked += new EventHandler( btnclose_Clicked );

            localipaddressentry.Text = MetaverseServer.GetInstance().network.networkimplementation.LocalIPAddress.ToString();
            localportentry.Text = MetaverseServer.GetInstance().ServerPort.ToString();
            STUN stun = new STUN( MetaverseServer.GetInstance().network.networkimplementation, new STUN.GotExternalAddress( STUNResponse ) );
        }

        void btnsendping_Clicked( object sender, EventArgs e )
        {
            SendPing();
        }

        void STUNResponse( IPAddress ipaddress, int port )
        {
            LogFile.WriteLine( "ServerInfo, Stunresponse: " + ipaddress + " " + port );
            publicipaddressentry.Text = ipaddress.ToString();
            publicportentry.Text = port.ToString();
            serverinfowindow.ShowAll();
        }

        bool holdopendown = false;
        void btnholdopen_Released( object sender, EventArgs e )
        {
            holdopendown = false;
        }

        void btnholdopen_Pressed( object sender, EventArgs e )
        {
            holdopendown = true;
            SendPing();
        }

        void btnclose_Clicked( object sender, EventArgs e )
        {
            holdopendown = false;
            serverinfowindow.Destroy();
            serverinfowindow = null;
        }

        //void PrivateServerInfoDialog( object source, ContextMenuArgs e )
        //{
            //DialogHelpers.ShowInfoMessageModal( null, "Server listening on port: " + MetaverseServer.GetInstance().ServerPort );
        //}
    }
}
