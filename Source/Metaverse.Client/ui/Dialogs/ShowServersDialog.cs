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
using System.Net;
using Glade;
using Gtk;
using Metaverse.Utility;
using Metaverse.Communication;

namespace OSMP
{
    public class ShowServersDialog
    {
        string servernameprefix = "srv_";

        static ShowServersDialog instance = new ShowServersDialog();
        public static ShowServersDialog GetInstance() { return instance; }

        [Widget]
        ScrolledWindow serversscrolledwindow = null;

        [Widget]
        TreeView serverstreeview = null;

        [Widget]
        Button btngetinfo = null;

        [Widget]
        Button btnconnect = null;

        [Widget]
        Button btnclose = null;

        [Widget]
        Gtk.Window availableserversdialog = null;

        QueryServer queryserver = new QueryServer();

        ShowServersDialog()
        {
        }

        public void Show()
        {
            MetaverseClient.GetInstance().imimplementation.GetUserList( new WhoCallback( ShowServersCallback ) );
            //ShowServersCallback( new string[] { "srv_antartic", "srv_iceland" } );
        }

        ListStore liststore;

        void ShowServersCallback( string[] whoresults )
        {
            if (availableserversdialog != null)
            {
                availableserversdialog.Destroy();
            }

            List<string> serverlist = new List<string>();
            foreach (string name in whoresults)
            {
                if (name.StartsWith( servernameprefix ))
                {
                    serverlist.Add( name.Substring( servernameprefix.Length ) );
                }
            }

            Glade.XML app = new Glade.XML( EnvironmentHelper.GetExeDirectory() + "/metaverse.client.glade", "availableserversdialog", "" );
            app.Autoconnect( this );

            btnclose.Clicked += new EventHandler( btnclose_Clicked );
            btnconnect.Clicked += new EventHandler( btnconnect_Clicked );
            btngetinfo.Clicked += new EventHandler( btngetinfo_Clicked );

            liststore = new ListStore( typeof( string ) );
            serverstreeview.Model = liststore;

            serverstreeview.AppendColumn( "Server:", new CellRendererText(), "text", 0 );

            serverstreeview.ShowAll();

            foreach (string name in whoresults)
            {
                if (name.StartsWith( servernameprefix ))
                {
                    string worldname = name.Substring( servernameprefix.Length );
                    liststore.AppendValues( worldname );
                }
            }
        }

        string GetSelectedServer()
        {
            TreePath[] selectedtreepaths = serverstreeview.Selection.GetSelectedRows();
            if (selectedtreepaths.Length > 0)
            {
                TreeIter treeiter;
                liststore.GetIter( out treeiter, selectedtreepaths[0] );
                string servername = servernameprefix + (string)liststore.GetValue( treeiter, 0 );
                Console.WriteLine( "server selected: " + servername );
                return servername;
            }
            return "";
        }

        void btngetinfo_Clicked( object sender, EventArgs e )
        {
            string servername = GetSelectedServer();
            if( servername != "" )
            {
                queryserver.Go( servername, new QueryServer.GotServerResponse( GotServerInfo ) );
            }
        }

        void btnconnect_Clicked( object sender, EventArgs e )
        {
            string servername = GetSelectedServer();
            if (servername != "")
            {
                queryserver.Go( servername, new QueryServer.GotServerResponse( GotServerInfo_nowconnect ) );
            }
        }

        string servername;

        void GotServerInfo_nowconnect( string servername, XmlCommands.ServerInfo serverinfo )
        {
            LogFile.WriteLine( "showserversdialog.GotServerInfo_nowconnect, got serverinfo: " + serverinfo );
            MetaverseClient.GetInstance().network.NewConnection += new Level2NewConnectionHandler( network_NewConnection );
            this.servername = servername;
            MetaverseClient.GetInstance().ConnectToServer(
                new IPAddress( serverinfo.IPAddress ).ToString(), serverinfo.port );
        }

        void network_NewConnection( NetworkLevel2Connection net2con, ConnectionInfo connectioninfo )
        {
            LogFile.WriteLine( "showserverdialog.network_newconnection()" );
            new STUN( net2con.networkimplementation, new STUN.GotExternalAddress( STUNResponseForOwnInterface ) );
        }

        void STUNResponseForOwnInterface( IPAddress ipaddress, int port )
        {
            LogFile.WriteLine( "showserverdialog.STUNResponseForOwnInterface( " + ipaddress + " " + port + " )" );
            MetaverseClient.GetInstance().imimplementation.SendPrivateMessage( servername,
                XmlCommands.GetInstance().Encode( new XmlCommands.PingMe( ipaddress, port ) ) );
        }

        void GotServerInfo( string servername, XmlCommands.ServerInfo serverinfo )
        {
            LogFile.WriteLine( "showserversdialog, got serverinfo: " + serverinfo );
            new RemoteServerInfoDialog( servername, serverinfo );
        }

        void btnclose_Clicked( object sender, EventArgs e )
        {
            Hide();
        }

        public void Hide()
        {
            availableserversdialog.Destroy();
            availableserversdialog = null;
        }
    }
}
