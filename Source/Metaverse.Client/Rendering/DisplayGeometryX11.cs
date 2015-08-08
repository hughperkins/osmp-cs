using System;
using System.Collections.Generic;
using System.Text;
using Tao.Sdl;

namespace OSMP
{
    public class DisplayGeometryX11 : IDisplayGeometry
    {
        public int WindowWidth {
            get
            {
                // placeholder till we find a better way
                return RendererSdl.GetInstance().OuterWindowWidth;
            }
        }
        public int WindowHeight
        {
            get
            {
                // placeholder till we find a better way
                return RendererSdl.GetInstance().OuterWindowHeight;
            }
        }

        //int windowwidth;
        //int windowheight;

        //[StructLayout( LayoutKind.Sequential     )]
        //public struct Rect
        //{
          //  public int left, top, right, bottom;
        //}
        //[DllImport("xfree86.so", SetLastError = true)]
        //static extern bool GetClientRect(int hWnd, ref Rect rect );

        public DisplayGeometryX11()
        {
         //   Sdl.SDL_SysWMinfo_Unix info;
           // Tao.Sdl.Sdl.SDL_GetWMInfo(out info);
            //LogFile.WriteLine(info);
            //Rect rect = new Rect();
            //GetClientRect(info.window, ref rect);
            //LogFile.WriteLine("error code: " + Marshal.GetLastWin32Error());
            //LogFile.WriteLine(rect.top + " " + rect.bottom + " " + rect.right + " " + rect.left );
            //windowwidth = rect.right - rect.left;
            //windowheight = rect.bottom - rect.top;
        }
    }
}
