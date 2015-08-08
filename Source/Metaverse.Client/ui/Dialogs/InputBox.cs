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
    // generic input box for asking questions
    public class InputBox
    {
        public delegate void Callback( string result );
        Callback callback = null;

        [Widget]
        Window inputbox = null;

        [Widget]
        Label promptlabel = null;

        [Widget]
        Entry usertext = null;

        [Widget]
        Button btnok = null;

        [Widget]
        Button btncancel = null;

        public InputBox( string prompt, Callback callback )
        {
            this.callback = callback;

            Glade.XML app = new Glade.XML( EnvironmentHelper.GetExeDirectory() + "/metaverse.client.glade", "inputbox", "" );
            app.Autoconnect( this );

            this.promptlabel.Text = prompt;
            this.inputbox.Title = prompt;

            btncancel.Clicked += new EventHandler( btncancel_Activated );
            btnok.Clicked += new EventHandler( btnok_Activated );
        }

        void btnok_Activated( object sender, EventArgs e )
        {
            LogFile.WriteLine( "ok pressed" );
            inputbox.Destroy();
            callback( usertext.Text );
        }

        void btncancel_Activated( object sender, EventArgs e )
        {
            LogFile.WriteLine( "cancel pressed" );
            inputbox.Destroy();
        }
    }
}
