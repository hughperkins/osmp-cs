// Copyright Hugh Perkins 2006
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
using System.Windows.Forms; // yah, not good, should use GTK really

namespace Setup
{
    // most of this stuff is going to be platform specific
    // so we'll split stuff into linux, win32, common, etc

    // we assume setup.exe is launched from directory containing metaverse.client.exe

    // for now we just register things in place as necessary, eg osmp:// handler on windows
    public class Setup
    {
        static void Main( string[] args )
        {
            try
            {
                new Common().Go();
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                        new Win32().Go();
                        break;

                    case PlatformID.Unix:
                        new Linux().Go();
                        break;

                    default:
                        throw new Exception( "unknown platform: " + Environment.OSVersion.Platform );
                }
                MessageBox.Show( "Setup complete.  Please run Osmp from the Osmp group in the start menu." );
            }
            catch (Exception e)
            {
                MessageBox.Show( "Unfortunately there was an error: " + e );
            }
        }
    }
}
