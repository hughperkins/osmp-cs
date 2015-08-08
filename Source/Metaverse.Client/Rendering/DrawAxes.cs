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
//using Tao.Sdl;
using Tao.OpenGl;
using SdlDotNet;

namespace OSMP
{
    class DrawAxes
    {
        public DrawAxes()
        {
            RendererFactory.GetInstance().WriteNextFrameEvent += new WriteNextFrameCallback(renderer_WriteNextFrameEvent);
        }

        void renderer_WriteNextFrameEvent( Vector3 camerapos)
        {
            Gl.glPushMatrix();
            Gl.glBegin(Gl.GL_LINES);
            GraphicsHelperFactory.GetInstance().SetMaterialColor(new Color(1, 0, 0));
            Gl.glVertex3d(1, 1, 1);
            Gl.glVertex3d(10, 1, 1);

            GraphicsHelperFactory.GetInstance().SetMaterialColor(new Color(0, 1, 0));
            Gl.glVertex3d(1, 1, 1);
            Gl.glVertex3d(1, 10, 1);

            GraphicsHelperFactory.GetInstance().SetMaterialColor(new Color(0, 0, 1));
            Gl.glVertex3d(1, 1, 1);
            Gl.glVertex3d(1, 1, 10);
            Gl.glEnd();
            Gl.glPopMatrix();

        }
    }
}
