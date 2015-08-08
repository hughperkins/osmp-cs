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
using Metaverse.Utility;

namespace OSMP
{
    // generic message box, non-modal, has callback
    public class MessageBox
    {
        public delegate void Callback();
        Callback callback = null;

        [Widget]
        Label messagelabel = null;

        [Widget]
        Button btnok = null;

        [Widget]
        Image imageicon = null;

        [Widget]
        Window messagebox = null;

        public enum MessageType
        {
            Info,
            Warning,
            Error
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messagetype"></param>
        /// <param name="message"></param>
        /// <param name="callback">Can be null</param>
        public MessageBox( MessageType messagetype, string title, string message, Callback callback )
        {
            this.callback = callback;

            Glade.XML app = new Glade.XML( EnvironmentHelper.GetExeDirectory() + "/metaverse.client.glade", "messagebox", "" );
            app.Autoconnect( this );
            messagebox.SetIconFromFile(EnvironmentHelper.GetOsmpIconFilepath());

            messagebox.Title = title;
            messagelabel.Text = message;

            switch (messagetype)
            {
                case MessageType.Info:
                    imageicon.SetFromStock( "gtk-dialog-info", IconSize.Dialog );
                    break;
                case MessageType.Warning:
                    imageicon.SetFromStock( "gtk-dialog-warning", IconSize.Dialog );
                    break;
                case MessageType.Error:
                    imageicon.SetFromStock( "gtk-dialog-error", IconSize.Dialog );
                    break;
            }

            btnok.Clicked += new EventHandler( btnok_Activated );
        }

        void btnok_Activated( object sender, EventArgs e )
        {
            LogFile.WriteLine( "ok pressed" );
            messagebox.Destroy();
            if (callback != null)
            {
                callback();
            }
        }
    }
}
