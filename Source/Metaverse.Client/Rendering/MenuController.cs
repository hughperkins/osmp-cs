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
//using System.Windows.Forms;

namespace OSMP
{
    public class MenuController
    {
        ArrayList mainmenucommanditems = new ArrayList();
        ArrayList mainmenucallbacks = new ArrayList();
        
       // MainMenu menu;
        
        static MenuController instance = new MenuController();
        public static MenuController GetInstance(){ return instance; }
        
        public void RegisterMainMenu( string[] menupath, MainMenuCallback callback )
        {
         //   menu = RendererFactory.GetInstance().Menu;
            
            //Menu.MenuItemCollection thesemenuitems = menu.MenuItems;
            
            //MenuItem commanditem = MenuHelper.CreateMenuItemInTree( menupath, thesemenuitems );
            //commanditem.Click += new  EventHandler( MainMenuClickHandler );
            
            //mainmenucommanditems.Add( commanditem );
            //mainmenucallbacks.Add( callback );
        }
        
        public void MainMenuClickHandler( object source, EventArgs e )
        {
            for( int i = 0; i < mainmenucallbacks.Count; i++ )
            {
                if( mainmenucommanditems[i] == source )
                {
                    ((MainMenuCallback)mainmenucallbacks[i])();
                }
            }
        }        
    }
}
