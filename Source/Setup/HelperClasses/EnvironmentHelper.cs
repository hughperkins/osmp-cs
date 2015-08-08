﻿// Copyright Hugh Perkins 2006
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
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.IO;

    public class EnvironmentHelper
    {
        public static string GetExeFilename()
        {
            return System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
        }
        public static string GetExeDirectory()
        {
            return Path.GetDirectoryName( System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName );
        }
        public static bool IsMonoRuntime
        {
            get
            {
                return typeof( object ).Assembly.GetType( "System.MonoType" ) != null;
            }
        }
        public static string GetClrDirectory()
        {
            if (!IsMonoRuntime)
            {
                return RuntimeEnvironment.GetRuntimeDirectory();
            }
            else
            {
                if (System.Environment.OSVersion.Platform != PlatformID.Unix)
                {
                    return RuntimeEnvironment.GetRuntimeDirectory() + "\\..\\..\\..\\bin";
                }
                else
                {
                    return RuntimeEnvironment.GetRuntimeDirectory(); // no idea what to do with this...
                }
            }
        }
    }
