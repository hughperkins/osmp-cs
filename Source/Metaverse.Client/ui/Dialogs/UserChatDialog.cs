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
//using System.Drawing;
using Gtk;
using Glade;
using Metaverse.Communication;
using Metaverse.Utility;

namespace OSMP
{
    public class UserChatDialog
    {
        //Panel IMPanel;

        [Widget]
        Gtk.Label chathistory = null;

        [Widget]
        Entry chatentry = null;

        [Widget]
        Window chatwindow = null;

        [Widget]
        ScrolledWindow chathistoryscrolledwindow = null;

        [Widget]
        Viewport chathistoryviewport = null;

        [Widget]
        Button btnshowusers = null;

        [Widget]
        Button btnshowservers = null;
        
        //static ChatController instance = new ChatController();
        //public static ChatController GetInstance(){ return instance; }

        //ChatWindow chatwindow;

        public LoginDialog logindialog;

        IChat imimplementation;

        string mylogin;

        public UserChatDialog()
        {
            imimplementation = MetaverseClient.GetInstance().imimplementation;
            logindialog = new LoginDialog( this );
            //Login( "hugh", "" );
        }

        public void Login(string username, string password)
        {
            LogFile.WriteLine(this.GetType().ToString() + " trying to login as " + username + " ... ");
            this.mylogin = username;
            imimplementation.Login( username, password);
            //logindialog.Destroy();

            Glade.XML app = new Glade.XML( EnvironmentHelper.GetExeDirectory() + "/metaverse.client.glade", "chatwindow", "");
            app.Autoconnect(this);

            imimplementation.IMReceived += new IMReceivedHandler( MessageReceived );

            CommandCombos.GetInstance().RegisterAtLeastCommand(
                "activatechat", new KeyCommandHandler(EnterChat));

            btnshowusers.Clicked += new EventHandler( btnshowusers_Clicked );
            btnshowservers.Clicked += new EventHandler( btnshowservers_Clicked );
        }

        void btnshowservers_Clicked( object sender, EventArgs e )
        {
            Console.WriteLine( "showservers clicked" );
            ShowServersDialog.GetInstance().Show();
        }

        void ShowUsersCallback( string[] usernames )
        {
            Console.WriteLine( "showuserscallback" );
            List<string> userlist = new List<string>();
            foreach (string name in usernames)
            {
                if (!name.StartsWith( "srv_" ))
                {
                    userlist.Add( name );
                }
            }
            ShowUsersDialog.GetInstance().Show( userlist.ToArray() );
        }

        void btnshowusers_Clicked( object sender, EventArgs e )
        {
            Console.WriteLine( "showusers clicked" );
            imimplementation.GetUserList( new WhoCallback( ShowUsersCallback ) );
        }

        void on_btnSend_clicked(object o, EventArgs e)
        {
            LogFile.WriteLine("send clicked");
            if (chatentry.Text != "")
            {
                SendMessage( chatentry.Text );
                //imimplementation.SendChannelMessage( chatentry.Text );
                chatentry.Text = "";
            }
        }

        void SendMessage( string message )
        {
            if ( message != "")
            {
                string[] splitmessage = message.Split( new char[] { ' ' } );
                if (splitmessage[0].Substring( 0, 1 ) == "/")
                {
                    string command = splitmessage[0].ToLower();

                    if (command == "/msg")
                    {
                        if (splitmessage.GetUpperBound( 0 ) >= 2)
                        {
                            string target = splitmessage[1];
                            string messagetosend = message.Substring( (splitmessage[0] + " " + splitmessage[1] + " ").Length );
                            imimplementation.SendPrivateMessage( target, messagetosend );
                            AppendMessage( "-> " + target + " " + messagetosend );
                        }
                    }
                    else if (command == "/who")
                    {
                        imimplementation.GetUserList( new WhoCallback( GotWhoResponse ) );
                    }
                    else
                    {
                        AppendMessage( "Unknown command: " + splitmessage[0] );
                    }
                }
                else
                {
                    imimplementation.SendChannelMessage( message );
                    AppendMessage( "<" + mylogin + ">" + message );
                }
            }
        }

        void GotWhoResponse( string[] namelist )
        {
            AppendMessage( "Logged on users:" );
            foreach (string name in namelist)
            {
                AppendMessage( "   " + name );
            }
        }

        public void EnterChat( string command, bool down )
        {
            if( down )
            {
                LogFile.WriteLine("EnterChat");
                //chatentry.Focus();
                chatwindow.RootWindow.Hide();
                chatwindow.RootWindow.Show();
            }
        }

        void AppendMessage( string messageline )
        {
            chathistory.Text += Environment.NewLine + messageline;
            chathistoryviewport.Vadjustment.Value = chathistoryviewport.Vadjustment.Upper;
        }
        
        //int numlines = 0;
        void MessageReceived( object source, IMReceivedArgs e )
        {
            string messagetodisplay = MessageToDisplayLine( e );
            LogFile.WriteLine("message received: " + messagetodisplay );
            AppendMessage( messagetodisplay );
        }

        string MessageToDisplayLine( IMReceivedArgs e )
        {
            switch( e.chatmessagetype )
            {
                case ChatMessageType.Action:
                    return "*" + e.Sender + "* " + e.MessageText;

                case ChatMessageType.ChannelMessage:
                    return "<" + e.Sender + "> " + e.MessageText;

                case ChatMessageType.Error:
                    return e.MessageText;

                case ChatMessageType.Notice:
                    return "-" + e.Sender + "-" + e.MessageText;

                case ChatMessageType.PrivateMessage:
                    return "*" + e.Sender + "*" + e.MessageText;
            }
            return "";
        }
    }
}
