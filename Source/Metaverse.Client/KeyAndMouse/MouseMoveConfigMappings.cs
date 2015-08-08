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
using Metaverse.Utility;

// applies the mappings in mousemoveconfigs/mousemove, in the config file
// provides a way of obtaining values for mouse movement, that are configurable via the config file
// example usage: GetVertical("cameratranslate")  The config file could map this to mouse up-down movement, or to the mouse scroll wheel, or to mouse sideways movement
namespace OSMP
{
    public class MouseMoveConfigMappings
    {
        static MouseMoveConfigMappings instance = new MouseMoveConfigMappings();
        public static MouseMoveConfigMappings GetInstance() { return instance; }

        Dictionary<string, MouseMoveConfig> MouseMoveConfigsByName;
        MouseCache mousefiltermousecache;

        Config config;

        MouseMoveConfigMappings() // protected constructor, to enforce singleton
        {
            MouseMoveConfigsByName = Config.GetInstance().MouseMoveConfigsByName;
            mousefiltermousecache = MouseCache.GetInstance();
            config = Config.GetInstance();
        }

        int GetAxis(string axisname, bool invert)
        {
            int value = 0;
            if (axisname == "mouseupdown")
            {
                value = mousefiltermousecache.MouseY;
            }
            if (axisname == "mousesideways")
            {
                value = mousefiltermousecache.MouseX;
            }
            if (axisname == "mousescroll")
            {
                value = mousefiltermousecache.Scroll * config.mousescrollmultiplier;
            }
            if (invert)
            {
                value = -value;
            }
            return value;
        }

        public int GetVertical(string mousemovename)
        {
            if (!MouseMoveConfigsByName.ContainsKey(mousemovename))
            {
                return 0;
            }
            MouseMoveConfig mousemoveconfig = MouseMoveConfigsByName[mousemovename];
            return GetAxis(mousemoveconfig.VerticalAxis, mousemoveconfig.InvertVertical);
        }

        public int GetHorizontal(string mousemovename)
        {
            if (!MouseMoveConfigsByName.ContainsKey(mousemovename))
            {
                return 0;
            }
            MouseMoveConfig mousemoveconfig = MouseMoveConfigsByName[mousemovename];
            return GetAxis(mousemoveconfig.HorizontalAxis, false);
        }

        public int GetZoom(string mousemovename)
        {
            if (!MouseMoveConfigsByName.ContainsKey(mousemovename))
            {
                return 0;
            }
            MouseMoveConfig mousemoveconfig = MouseMoveConfigsByName[mousemovename];
            return GetAxis(mousemoveconfig.Zoom, mousemoveconfig.InvertScroll);
        }

        public Vector3 GetMouseStateVector(string mousemovename)
        {
            if (!MouseMoveConfigsByName.ContainsKey(mousemovename))
            {
                return null;
            }
            MouseMoveConfig mousemoveconfig = MouseMoveConfigsByName[mousemovename];
            //bool invert = mousemoveconfig.Invert;
            return new Vector3(GetAxis(mousemoveconfig.HorizontalAxis, false), 
                GetAxis(mousemoveconfig.VerticalAxis, mousemoveconfig.InvertVertical),
                GetAxis(mousemoveconfig.Zoom, mousemoveconfig.InvertScroll));
        }
    }
}
