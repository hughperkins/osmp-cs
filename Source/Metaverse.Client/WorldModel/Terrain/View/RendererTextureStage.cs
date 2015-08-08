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

namespace OSMP
{
    // represents one renderer texture stage, targets terrains
    public class RendererTextureStage
    {
        public RendererTextureStage(MapTextureStageView maptexturestage, int maptexturestagepass, bool UsingMultipass, int mapwidth, int mapheight)
        {
            this.maptexturestage = maptexturestage;
            this.maptexturestagepass = maptexturestagepass;
            this.mapwidth = mapwidth;
            this.mapheight = mapheight;
            this.UsingMultipass = UsingMultipass;
        }
        public void Apply()
        {
            //Console.WriteLine("texturestage apply " + maptexturestagepass + " " + maptexturestage.texture.Filename);
            //Console.WriteLine("TextureStage " + maptexturestagepass + " " + IsFirstStage + " " + maptexturestage.Operation + " " + maptexturestage.texture.Filename);
            maptexturestage.Apply(maptexturestagepass, UsingMultipass, mapwidth, mapheight);
        }
        int mapwidth, mapheight;
        public MapTextureStageView maptexturestage;
        public int maptexturestagepass = 0;
        public bool UsingMultipass = false;
    }
}
