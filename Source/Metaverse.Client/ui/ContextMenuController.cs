// Copyright Hugh Perkins 2004,2005,2006
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
using System.Collections;
//using System.Windows.Forms;

namespace OSMP
{
    // Manages the context menu for us
    // This used to do everything, but now it just wraps UIContextMenu
    public class ContextMenuController
    {
        public event ContextMenuHandler ContextMenuPopup;
                
        static ContextMenuController instance = new ContextMenuController();
        public static ContextMenuController GetInstance()
        {
            return instance;
        }

        ContextMenuController()
        {
        }

        // This context menu function is for ContextMenuPopup events
        // It will not persist.  Use this for menu items which are context dependent, like entity properties (doesnt display if no entity selected)
        public void RegisterContextMenu(string[] contextmenupath, ContextMenuHandler callback)
        {
            UIController.GetInstance().contextmenu.RegisterContextMenu(contextmenupath, callback);
        }

        // This context menu function creates a persistent contextmenu.  This is good for things like Quit
        public void RegisterPersistentContextMenu(string[] contextmenupath, ContextMenuHandler callback)
        {
            UIController.GetInstance().contextmenu.RegisterPersistentContextMenu(contextmenupath, callback);
        }

        // hook for uicontextmenu to call event
        public void OnContextMenuPopup(object source, ContextMenuArgs e)
        {
            ContextMenuPopup(source, e);
        }
    }
}
