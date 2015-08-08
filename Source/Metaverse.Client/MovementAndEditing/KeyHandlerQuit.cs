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
using Gtk;
using Glade;
using Metaverse.Utility;

namespace OSMP
{
    // example keyboard and menu plugin
    // This class provides a Quit functionality via the Escape key, context menu, and main menu
    public class KeyHandlerQuit
    {
        static KeyHandlerQuit instance = new KeyHandlerQuit(); // instantiate handler
        public static KeyHandlerQuit GetInstance()
        {
            return instance;
        }
        
        public KeyHandlerQuit()
        {
            LogFile.WriteLine("instantiating keyhandlerquit" );
            CommandCombos.GetInstance().RegisterAtLeastCommand("quit", new KeyCommandHandler(QuitKeyDown));
            //KeyFilterComboKeys keyfiltercombokeys = KeyFilterComboKeys.GetInstance();
            //keyfiltercombokeys.RegisterCombo( new string[]{"quit"},null, new KeyComboHandler( QuitKeyDown ) );
            
           //RendererFactory.GetInstance().RegisterContextMenu(new string[]{ "Quit","&Quit" }, new ContextMenuCallback( ContextMenuQuit ) );
            ContextMenuController.GetInstance().RegisterPersistentContextMenu(new string[]{ "Quit","&Quit" }, new ContextMenuHandler( ContextMenuQuit ) );
            MenuController.GetInstance().RegisterMainMenu(new string[]{ "&File","&Quit" }, new MainMenuCallback( Quit ) );
        }
        public void Quit()
        {
            LogFile.WriteLine("Shutdown from " + this.GetType().ToString() );
            //System.Environment.FailFast( "quit" );
            //SdlDotNet.Video.Close();
            //Tao.Sdl.Sdl.SDL_Quit();
            //Tao.DevIl.Il.ilShutDown();
            //MetaverseClient.GetInstance().chatcontroller.logindialog.Destroy();
            //while (Gtk.Application.EventsPending())
            //{
              //Gtk.Application.RunIteration( false );
            //}
            //try
            //{
              //  Gtk.Application.Quit();
            //}
            //catch
            //{
            //}
            //MetaverseClient.GetInstance().Shutdown();
            System.Environment.Exit( 0 );
        }
        public void ContextMenuQuit( object source, ContextMenuArgs e )
        {
            Quit();
        }
        public void QuitKeyDown( string command, bool down )
        {
            if (down)
            {
                Quit();
            }
        }
    }
}
