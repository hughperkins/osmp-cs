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
using System.IO;
using Metaverse.Utility;

namespace OSMP
{
    // example keyboard and menu plugin
    // This class provides a Quit functionality via the Escape key, context menu, and main menu
    public class HelpAbout
    {
        static HelpAbout instance = new HelpAbout(); // instantiate handler
        public static HelpAbout GetInstance()
        {
            return instance;
        }
        
        public HelpAbout()
        {
            LogFile.WriteLine("instantiating HelpAbout" );
            //MenuController.GetInstance().RegisterMainMenu(new string[]{ "&Help","&About..." }, new MainMenuCallback( About ) );
            ContextMenuController.GetInstance().RegisterPersistentContextMenu(new string[] { "&Help", "&About..." }, new ContextMenuHandler(About));
        }
        public void About( object source, ContextMenuArgs e)
        {
            //DialogHelpers.ShowInfoMessageModal(null,
              //  "OSMP C# written by Hugh Perkins hughperkins@gmail.com"  + Environment.NewLine +
                //"Website at http://metaverse.sf.net by Zenaphex" + Environment.NewLine +
                //"Forums designed by Nick Merrill" + Environment.NewLine +
                //"OSMP C# based on original C++ version written by Hugh Perkins and contributed to by Jack Didgeridoo, Christopher Omega, Jorge Lima, and Carnildo" + Environment.NewLine +
                //Environment.NewLine +
                //"OSMP C# compilation date/time: " + EnvironmentHelper.GetCompilationDateTime()
            //);
            new MessageBox(MessageBox.MessageType.Info, "About OSMP",
                "OSMP C# written by Hugh Perkins hughperkins@gmail.com" + Environment.NewLine +
                "Website at http://metaverse.sf.net by Zenaphex" + Environment.NewLine +
                "Forums designed by Nick Merrill" + Environment.NewLine +
                "OSMP C# based on original C++ version written by Hugh Perkins and contributed to by Jack Didgeridoo, Christopher Omega, Jorge Lima, and Carnildo" + Environment.NewLine +
                Environment.NewLine +
                "OSMP C# compilation date/time: " + EnvironmentHelper.GetCompilationDateTime(),
                null);
        }
    }
}
