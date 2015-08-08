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
using Metaverse.Utility;

namespace Metaverse.Communication
{
    public enum ChatMessageType
    {
        Error,
        ChannelMessage,
        PrivateMessage,
        Action,
        Notice
    }

    public class IMReceivedArgs : EventArgs
    {
        public ChatMessageType chatmessagetype;
        public string Sender;
        public string MessageText;
        public IMReceivedArgs( ChatMessageType chatmessagetype, string sender, string text )
        {
            this.chatmessagetype = chatmessagetype;
            this.Sender = sender;
            MessageText = text;
        }
    }
    
    public delegate void IMReceivedHandler( object source, IMReceivedArgs e );
    public delegate void WhoCallback( string[] names );
        
    public interface IChat
    {
        event IMReceivedHandler IMReceived;
        void SendChannelMessage( string message );
        void SendPrivateMessage( string targetuser, string message );
        bool Login( string username, string password );
        bool Login( string[] serverlist, int port, string channel, string username, string password );
        void GetUserList( WhoCallback callback );
        void Tick();
    }
}
