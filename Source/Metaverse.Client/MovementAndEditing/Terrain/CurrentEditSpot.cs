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
using Tao.OpenGl;

namespace OSMP
{
    public class CurrentEditSpot
    {
        public CurrentEditSpot()
        {
            RendererFactory.GetInstance().WriteNextFrameEvent += new WriteNextFrameCallback(CurrentEditSpot_WriteNextFrameEvent);
            //Terrain.GetInstance().renderableminimap.Render += new RenderableMinimap.RenderHandler(renderableminimap_Render);
        }

        void renderableminimap_Render(int minimapleft, int minimaptop, int minimapwidth, int minimapheight)
        {
            //Console.WriteLine("renderableminimap_Render");
            Vector3 intersectpoint = EditingHelper.GetIntersectPoint();
            if (intersectpoint != null)
            {
                Vector2 minimappos = new Vector2(intersectpoint.x * minimapwidth / MetaverseClient.GetInstance().worldstorage.terrainmodel.MapWidth,
                    intersectpoint.y * minimapheight / MetaverseClient.GetInstance().worldstorage.terrainmodel.MapHeight);

                //Console.WriteLine("minmappos: " + minimappos);
                //double distancefromcamera = (intersectpoint - camerapos).Det();
                GraphicsHelperFactory.GetInstance().SetMaterialColor(new Color(0, 0, 1));
                Gl.glPushMatrix();
                Gl.glDisable(Gl.GL_LIGHTING);
                Gl.glDisable(Gl.GL_CULL_FACE);
                Gl.glColor3ub(0, 0, 255);
                Gl.glBegin(Gl.GL_POINTS);
                Gl.glVertex2d(minimapleft + minimappos.x, minimaptop + minimappos.y);
                Gl.glEnd();
                Gl.glEnable(Gl.GL_CULL_FACE);
                Gl.glEnable(Gl.GL_LIGHTING);
                Gl.glColor3ub(255, 255, 255);
                //Gl.glTranslated(intersectpoint.x, intersectpoint.y, intersectpoint.z);
                //Gl.glScaled(0.01 * distancefromcamera, 0.01 * distancefromcamera, 0.01 * distancefromcamera);
                //GraphicsHelperFactory.GetInstance().DrawSphere();
                Gl.glPopMatrix();
            }
        }

        void CurrentEditSpot_WriteNextFrameEvent( Vector3 camerapos)
        {
            if (ViewerState.GetInstance().CurrentViewerState != ViewerState.ViewerStateEnum.Terrain)
            {
                return;
            }
            Vector3 intersectpoint = EditingHelper.GetIntersectPoint();
            if (intersectpoint != null)
            {
                double distancefromcamera = (intersectpoint - camerapos).Det();
                GraphicsHelperFactory.GetInstance().SetMaterialColor(new Color(0, 0, 1));
                Gl.glPushMatrix();
                Gl.glTranslated( intersectpoint.x, intersectpoint.y, intersectpoint.z + 0.01 * distancefromcamera );
                Gl.glScaled(0.01 * distancefromcamera, 0.01 * distancefromcamera, 0.01 * distancefromcamera);
                GraphicsHelperFactory.GetInstance().DrawSphere();
                Gl.glPopMatrix();
            }
        }
    }
}
