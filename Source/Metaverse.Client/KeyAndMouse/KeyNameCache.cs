// Copyright Hugh Perkins 2006
// hughperkins@gmail.com http://manageddreams.com
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
using SdlDotNet;
using Metaverse.Utility;

namespace OSMP
{
    // caches the names of keys, eg "shift", "leftmousebutton", "f1", "a"
    // includes mousebuttons
    public class KeyNameCache
    {
        public delegate void KeyDownHandler(string keyname);
        public delegate void KeyUpHandler(string keyname);

        public event KeyDownHandler KeyDown;
        public event KeyUpHandler KeyUp;

        static KeyNameCache instance = new KeyNameCache();
        public static KeyNameCache GetInstance() { return instance; }

        public List<string> keynamesdown = new List<string>();

        public KeyNameCache()
        {
            SdlKeyCache.GetInstance().KeyDown += new SdlDotNet.KeyboardEventHandler(KeyNameCache_KeyDown);
            SdlKeyCache.GetInstance().KeyUp += new SdlDotNet.KeyboardEventHandler(KeyNameCache_KeyUp);
            MouseCache.GetInstance().MouseDown += new SdlDotNet.MouseButtonEventHandler(KeyNameCache_MouseDown);
            MouseCache.GetInstance().MouseUp += new SdlDotNet.MouseButtonEventHandler(KeyNameCache_MouseUp);
        }

        string MouseEventToKeyName(MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.PrimaryButton)
            {
                return "leftmousebutton";
            }
            if (e.Button == MouseButton.MiddleButton)
            {
                return "middlemousebutton";
            }
            if (e.Button == MouseButton.SecondaryButton)
            {
                return "rightmousebutton";
            }
            return "";
        }

        public string KeyCodeToKeyName(SdlDotNet.KeyboardEventArgs e)
        {
            string skeyname = e.Key.ToString().ToLower();
            if (skeyname.IndexOf("shift") >= 0)
            {
                skeyname = "shift";
            }
            else if (skeyname.IndexOf("control") >= 0)
            {
                skeyname = "ctrl";
            }
            else if (skeyname.IndexOf("alt") >= 0)
            {
                skeyname = "alt";
            }
            //LogFile.WriteLine(skeyname);
            return skeyname;
        }

        void HandleKeyDown(string keyname)
        {
            if (!keynamesdown.Contains(keyname))
            {
                keynamesdown.Add(keyname);
                if (KeyDown != null)
                {
                    LogFile.WriteLine("down: " + keyname);
                    KeyDown(keyname);
                }
            }
        }

        void HandleKeyUp(string keyname)
        {
            if (keynamesdown.Contains(keyname))
            {
                keynamesdown.Remove(keyname);
                if (KeyUp != null)
                {
                    LogFile.WriteLine("up: " + keyname);
                    KeyUp(keyname);
                }
            }
        }

        void KeyNameCache_MouseDown(object sender, SdlDotNet.MouseButtonEventArgs e)
        {
            Console.WriteLine("KeyNameCache_MouseDown");
            string mousebuttonname = MouseEventToKeyName(e);
            if (mousebuttonname != "")
            {
                HandleKeyDown(mousebuttonname);
            }
        }

        void KeyNameCache_MouseUp(object sender, SdlDotNet.MouseButtonEventArgs e)
        {
            Console.WriteLine("KeyNameCache_MouseUp");
            string mousebuttonname = MouseEventToKeyName(e);
            if (mousebuttonname != "")
            {
                HandleKeyUp(mousebuttonname);
            }
        }

        void KeyNameCache_KeyDown(object sender, SdlDotNet.KeyboardEventArgs e)
        {
            string thiskeyname = KeyCodeToKeyName(e);
            HandleKeyDown(thiskeyname);
        }

        void KeyNameCache_KeyUp(object sender, SdlDotNet.KeyboardEventArgs e)
        {
            string thiskeyname = KeyCodeToKeyName(e);
            HandleKeyUp(thiskeyname);
        }
    }
}
