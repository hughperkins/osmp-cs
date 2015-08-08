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
using System.Collections.Generic;
using System.Text;
using System.IO;
using Gtk;
using Metaverse.Utility;
    // note: all of these run modally at the moment and will cause disconnection of all clients most likely
    // we should probably shift these to non-modal dialogs

    public class DialogHelpers
    {
        public static void ShowErrorMessageModal( Window window, string message )
        {
            Dialog dialog = new MessageDialog( window, DialogFlags.DestroyWithParent | DialogFlags.Modal,
                MessageType.Error, ButtonsType.Ok, message );
            dialog.Run();
            dialog.Hide();
        }
        public static void ShowWarningMessageModal( Window window, string message )
        {
            Dialog dialog = new MessageDialog( window, DialogFlags.DestroyWithParent | DialogFlags.Modal,
                MessageType.Warning, ButtonsType.Ok, message );
            dialog.Run();
            dialog.Hide();
        }
        public static void ShowInfoMessageModal( Window window, string message )
        {
            Dialog dialog = new MessageDialog( window, DialogFlags.DestroyWithParent | DialogFlags.Modal,
                MessageType.Info, ButtonsType.Ok, message );
            dialog.Run();
            dialog.Hide();
        }

        static string lastdirectorypath = "";
        public static string GetFilePath( string prompt, string defaultfilename )
        {
            using (FileSelection dialog = new FileSelection( prompt ))
            {
                dialog.Filename = Path.Combine( lastdirectorypath, defaultfilename );
                ResponseType response = (ResponseType)dialog.Run();
                dialog.Hide();
                if (response == ResponseType.Ok)
                {
                    LogFile.WriteLine( "got filepath: " + dialog.Filename );
                    lastdirectorypath = Path.GetDirectoryName( dialog.Filename );
                    return dialog.Filename;
                }
                else
                {
                    LogFile.WriteLine( "Cancel pressed" );
                    return "";
                }
            }
        }

        public static OSMP.Color GetColor()
        {
            ColorSelectionDialog colorselectiondialog = new ColorSelectionDialog( "Choose color:" );
            ResponseType response = (ResponseType)colorselectiondialog.Run();
            //colorselectiondialog.

            OSMP.Color newcolor = null;
            if (response == ResponseType.Ok)
            {
                LogFile.WriteLine( colorselectiondialog );
                LogFile.WriteLine( colorselectiondialog.ColorSelection );
                LogFile.WriteLine( colorselectiondialog.ColorSelection.CurrentColor.Red.ToString() + " " +
                colorselectiondialog.ColorSelection.CurrentColor.Green.ToString() + " " +
                    colorselectiondialog.ColorSelection.CurrentColor.Blue.ToString() );
                Gdk.Color newgtkcolor = colorselectiondialog.ColorSelection.CurrentColor;
                newcolor = new OSMP.Color( newgtkcolor.Red / (double)65536,
                    newgtkcolor.Green / (double)65536,
                    newgtkcolor.Blue / (double)65536 );
            }
            else
            {
                LogFile.WriteLine( "cancel pressed" );
            }

            colorselectiondialog.Hide();
            return newcolor;
        }
    }
