// Copyright Hugh Perkins 2006
// hughperkins at gmail http://hughperkins.com
//
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the
// Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURVector3E. See the GNU General Public License for
//  more details.
//
// You should have received a copy of the GNU General Public License along
// with this program in the file licence.txt; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-
// 1307 USA
// You can find the licence also on the web at:
// http://www.opensource.org/licenses/gpl-license.php
//
// ======================================================================================
//

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using Metaverse.Utility;

namespace OSMP
{
    public class DumpLogfile
    {
        static DumpLogfile instance = new DumpLogfile();
        public static DumpLogfile GetInstance() { return instance; }

        DumpLogfile()
        {
            ContextMenuController.GetInstance().RegisterPersistentContextMenu( new string[] { "Help", "Dump Logfile..." },
                new ContextMenuHandler( _DumpLogfile ) );
        }

        void _DumpLogfile( object source, EventArgs e )
        {
            string errorlogpath = EnvironmentHelper.GetExeDirectory() + "/logfile.log";
            StreamWriter sw = new StreamWriter( errorlogpath, false );
            sw.WriteLine( LogFile.GetInstance().logfilecontents );
            sw.Close();

            if (System.Environment.OSVersion.Platform != PlatformID.Unix)
            {
                ProcessStartInfo psi = new ProcessStartInfo( "notepad.exe", errorlogpath );
                psi.UseShellExecute = true;
                Process process = new Process();
                process.StartInfo = psi;
                process.Start();
            }
            else
            {
                // do something on linux
            }
        }
    }
}
