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
    // represents one renderer pass, targets terrains
    public class RendererPass
    {
        public int numstages;
        public List<RendererTextureStage> texturestages = new List<RendererTextureStage>();
        GraphicsHelperGl g;
        public RendererPass(int maxtexels)
        {
            g = new GraphicsHelperGl();
            //texturestages = new TextureStage[maxtexels];
            this.maxtexels = maxtexels;
        }
        public void AddStage(RendererTextureStage stage)
        {
            texturestages.Add(stage);
            numstages++;
        }
        int maxtexels;
        public void Apply()
        {
            //Console.WriteLine("rendererpass");
            int i;
            //Console.WriteLine("rendererpass.apply " + numstages + " stages");
            for (i = numstages; i < maxtexels; i++)
            {
                g.ActiveTexture(i);
                g.DisableTexture2d();
            }
            i = 0;
            foreach (RendererTextureStage texturestage in texturestages)
            {
                g.ActiveTexture(i);
                g.EnableTexture2d();
                texturestage.Apply();
                i++;
            }
        }
    }
}
