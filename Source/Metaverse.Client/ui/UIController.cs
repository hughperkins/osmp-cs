// Copyright Hugh Perkins 2004,2005,2006
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
    // This initializes the GTK UI, and manages the GTK thread
    // This will replace Windows.Forms
    // For now it doesnt do much; this will increase with time
    // GTK runs in same thread as renderer, which helps give a fluid appearance
    public class UIController
    {
        static UIController instance = new UIController();
        public static UIController GetInstance() { return instance; }

        public UIContextMenu contextmenu;

        UIController()
        {
            MetaverseClient.GetInstance().Tick += new MetaverseClient.TickHandler(UIController_Tick);
            Application.Init();

            contextmenu = new UIContextMenu();

            new MessageBox(MessageBox.MessageType.Info, "Movement", "Middle mouse button to mouselook, asdf or arrowkeys to move, e and c to fly", null);
        }

        void UIController_Tick()
        {
            try
            {
                while (Application.EventsPending())
                {
                    Application.RunIteration(false);
                }
            }
            catch (Exception e)
            {
                LogFile.WriteLine(e.ToString());
            }
        }
    }
}
