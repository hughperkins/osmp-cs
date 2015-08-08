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
using System.Collections;
using System.Collections.Generic;
using Meebey.SmartIrc4net;
using Metaverse.Utility;

namespace Metaverse.Communication
{
    public class IrcChat : IChat
    {
        //public string[] serverlist = new string[] {"irc.freenode.org"};
        //public string[] serverlist = new string[] { "irc.gamernet.org" };
        //public int port = 6667;
        public string _channel = "";
        
        public event IMReceivedHandler IMReceived;
            
        IrcClient ircclient;
        bool IsConnected;

        List<WhoCallback> whocallbacks = new List<WhoCallback>();

        //static IrcController instance = new IrcController();
        //public static IrcController GetInstance(){ return instance; }
        
        public IrcChat()
        {
            LogFile.WriteLine( this.GetType().ToString() + " IrcController()" );
            ircclient = new IrcClient();
            ircclient.SendDelay = 200;
            ircclient.ActiveChannelSyncing = true; // we use channel sync, means we can use ircclient.GetChannel() and so on
            
            ircclient.OnQueryMessage += new IrcEventHandler(OnQueryMessage);
            ircclient.OnQueryNotice += new IrcEventHandler(OnQueryNotice);
            ircclient.OnQueryAction += new ActionEventHandler(OnQueryAction);
            
            ircclient.OnChannelMessage += new IrcEventHandler(OnChannelMessage);
            ircclient.OnChannelNotice += new IrcEventHandler(OnChannelNotice);
            ircclient.OnChannelAction += new ActionEventHandler(OnChannelAction);

            ircclient.OnWho += new WhoEventHandler( ircclient_OnWho );
            
            ircclient.OnRawMessage += new IrcEventHandler(OnRawMessage);

            ircclient.OnError += new ErrorEventHandler(OnError);
        }

        void ircclient_OnWho( object sender, WhoEventArgs e )
        {
            if (e.Server != "hidden" && e.Channel == _channel )
            {
                Console.WriteLine( "onwho " + e.IsIrcOp + " " + e.IsOp + " " + e.Server + " " + e.Channel + " " + e.Nick );
                wholist.Add( e.Nick );
            }
        }

        void onEndOfWho()
        {
            string[] namestosendarray = wholist.ToArray();
            Console.WriteLine( "end of who " + String.Join( ", ", namestosendarray ) );
            foreach (WhoCallback whocallback in whocallbacks)
            {
                whocallback( namestosendarray );
            }
            whocallbacks.Clear();
        }

        public void Tick()
        {
            ircclient.ListenOnce( false );
        }

        public void GetUserList( WhoCallback callback )
        {
            whocallbacks.Add( callback );
            SendWho();
        }

        string mylogin;
        
        public bool Login( string username, string password )
        {
            mylogin = username;
            LogFile.WriteLine( this.GetType().ToString() + " Login()" );
            //InformClient( "test inform" );
            try
            {
                Config.Coordination coordinationconfig = Config.GetInstance().coordination;
                string[] serverlist = new string[] { coordinationconfig.ircserver };
                int port = coordinationconfig.ircport;
                _channel = coordinationconfig.ircchannel;

                LogFile.WriteLine( "ircchat connecting to " + coordinationconfig.ircserver );
                ircclient.Connect(serverlist, port);
                ircclient.Login(username, username);
                ircclient.RfcJoin(_channel);                
                if( password != "" )
                {
                    ircclient.SendMessage(SendType.Message, "nickserv", "identify " + password );
                }
                IsConnected = true;
            }
            catch (ConnectionException e)
            {
                OnMessage( ChatMessageType.Error, "", "IRC Error: "+e.Message + ". Irc chat will not be available in this session" );
            }
            return IsConnected;
        }
        
         public bool Login( string[] serverlist, int port, string channel, string username, string password )
        {
            mylogin = username;
            LogFile.WriteLine( this.GetType().ToString() + " Login()" );
            //InformClient( "test inform" );
            try
            {
                _channel = channel;

                LogFile.WriteLine( "ircchat connecting to " + serverlist );
                ircclient.Connect(serverlist, port);
                ircclient.Login(username, username);
                ircclient.RfcJoin(_channel);                
                if( password != "" )
                {
                    ircclient.SendMessage(SendType.Message, "nickserv", "identify " + password );
                }
                IsConnected = true;
            }
            catch (ConnectionException e)
            {
                OnMessage( ChatMessageType.Error, "", "IRC Error: "+e.Message + ". Irc chat will not be available in this session" );
            }
            return IsConnected;
        }

        List<string> wholist = new List<string>();
        public void SendWho()
        {
            Console.WriteLine( "SendWho()..." );
            if (ircclient != null)
            {
                wholist.Clear();
                ircclient.WriteLine( "WHO *" );
            }
        }

        public void SendPrivateMessage( string targetuser, string message )
        {
            if (IsConnected && message != "")
            {
                LogFile.WriteLine( "ircchat.sendprivatemessage " + targetuser + " " + message );
                ircclient.SendMessage( SendType.Message, targetuser, message );
            }
        }

        public void SendChannelMessage( string message )
        {
            if( IsConnected && message != "" )
            {
                LogFile.WriteLine( "ircchat.sendchannelmessage " + message );
                ircclient.SendMessage( SendType.Message, _channel, message );
            }
        }
        
        void OnMessage( ChatMessageType chatmessagetype, string sender, string message )
        {
            //LogFile.WriteLine( "informclient: " + message );
            if( IMReceived != null )
            {
                IMReceived( this, new IMReceivedArgs( chatmessagetype, sender, message ) );
            }
        }

        public void OnRawMessage(object sender, IrcEventArgs e)
        {
            //LogFile.WriteLine("OnRawMessage Replycode " + e.Data.ReplyCode.ToString() + " Received: "+e.Data.RawMessage);
            if (e.Data.ReplyCode == ReplyCode.EndOfWho)
            {
                onEndOfWho();
            }
            //InformClient( "*" + e.Data.Nick + "* " + e.Data.Message );
        }
        
        public void OnQueryMessage(object sender, IrcEventArgs e)
        {
            OnMessage(ChatMessageType.PrivateMessage, e.Data.Nick, e.Data.Message );
        }
        public void OnQueryNotice(object sender, IrcEventArgs e)
        {
            OnMessage( ChatMessageType.Notice, e.Data.Nick, e.Data.Message );
        }
        public void OnQueryAction(object sender, ActionEventArgs e)
        {
            OnMessage( ChatMessageType.Action, e.Data.Nick, e.Data.Message );
        }        
        public void OnChannelMessage(object sender, IrcEventArgs e)
        {
            OnMessage( ChatMessageType.ChannelMessage, e.Data.Nick, e.Data.Message );
        }
        public void OnChannelNotice(object sender, IrcEventArgs e)
        {
            OnMessage( ChatMessageType.Notice, e.Data.Nick, e.Data.Message );
        }
        public void OnChannelAction(object sender, ActionEventArgs e)
        {
            OnMessage( ChatMessageType.Action, e.Data.Nick, e.Data.Message );
        }
        public void OnError(object sender, ErrorEventArgs e)
        {
            LogFile.WriteLine( "Error: "+e.ErrorMessage);
            OnMessage( ChatMessageType.Error, e.Data.Nick, e.Data.Message );
            IsConnected = false;
        }
    }
}
