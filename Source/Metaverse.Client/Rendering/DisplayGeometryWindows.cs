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
using System.Runtime.InteropServices;
using Tao.OpenGl;
using SdlDotNet;
using Tao.Sdl;
using Metaverse.Utility;

    public class DisplayGeometryWindows : IDisplayGeometry
    {
        public int WindowWidth {
            get
            {
                return windowwidth;
            }
        }
        public int WindowHeight
        {
            get
            {
                return windowheight;
            }
        }

        int windowwidth;
        int windowheight;

        [StructLayout( LayoutKind.Sequential     )]
        public struct Rect
        {
            public int left, top, right, bottom;
        }
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetClientRect(int hWnd, ref Rect rect );

        public DisplayGeometryWindows()
        {
            Sdl.SDL_SysWMinfo_Windows info;
            Tao.Sdl.Sdl.SDL_GetWMInfo(out info);
            LogFile.WriteLine(info.window);
            Rect rect = new Rect();
            GetClientRect(info.window, ref rect);
            LogFile.WriteLine("error code: " + Marshal.GetLastWin32Error());
            LogFile.WriteLine(rect.top + " " + rect.bottom + " " + rect.right + " " + rect.left );
            windowwidth = rect.right - rect.left;
            windowheight = rect.bottom - rect.top;
        }
    }
