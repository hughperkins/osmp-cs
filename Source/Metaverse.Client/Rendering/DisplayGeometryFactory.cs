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

using System;
using System.Collections.Generic;
using System.Text;

    public class DisplayGeometryFactory
    {
        static IDisplayGeometry instance;

        // unfortunately getting display geometry is platform dependent, so
        // we abstract the various methods for doing this here
        public static IDisplayGeometry GetDisplayGeometry()
        {
            if (System.Environment.OSVersion.Platform == PlatformID.Win32Windows ||
                System.Environment.OSVersion.Platform == PlatformID.Win32NT )
            {
                if (instance == null)
                {
                    instance = new DisplayGeometryWindows();
                }
                return instance;
            }
            else
            {
                throw new Exception("IDisplayGeometry not yet implemented for platform " + System.Environment.OSVersion.Platform.ToString());
                // return new LinuxDisplayGeomtry();
                // etc...
            }
        }
    }
