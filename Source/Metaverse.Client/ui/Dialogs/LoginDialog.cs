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
    public class LoginDialog
    {
        [Widget]
        Label welcomelabel = null;

        [Widget]
        Entry entryusername = null;

        [Widget]
        Entry entrypassword = null;

        //[Widget]
        //Button btnok = null;

        [Widget]
        Window loginwindow = null;

        public void Destroy()
        {
            loginwindow.Destroy();
        }

        public string Login
        {
            get
            {
                return entryusername.Text;
            }
        }

        public string Password
        {
            get
            {
                return entrypassword.Text;
            }
        }

        void on_btnok_clicked(object o, EventArgs e)
        {
            loginwindow.Destroy();
            parent.Login(Login, Password);
        }

        UserChatDialog parent;

        public LoginDialog( UserChatDialog parent )
        {
            this.parent = parent;

            Glade.XML app = new Glade.XML( EnvironmentHelper.GetExeDirectory() + "/metaverse.client.glade", "loginwindow", "" );
            app.Autoconnect(this);

            entrypassword.Activated += new EventHandler(entrypassword_Activated);
            entryusername.Activated += new EventHandler(entryusername_Activated);
        }

        void entryusername_Activated(object sender, EventArgs e)
        {
            on_btnok_clicked(sender, e);
        }

        void entrypassword_Activated(object sender, EventArgs e)
        {
            on_btnok_clicked(sender, e);
        }
    }
}
