// Created by Hugh Perkins 2006
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
    /// <summary>
    /// handles brush registration etc
    /// </summary>
    public class BrushShapeController
    {
        static BrushShapeController instance = new BrushShapeController();
        public static BrushShapeController GetInstance() { return instance; }

        /// <summary>
        /// Use Register to add to this list
        /// </summary>
        public Dictionary<Type, IBrushShape> brushshapes = new Dictionary<Type, IBrushShape>();

        public BrushShapeController()
        {
            RendererFactory.GetInstance().WriteNextFrameEvent += new WriteNextFrameCallback( BrushShapeController_WriteNextFrameEvent );
        }

        void BrushShapeController_WriteNextFrameEvent( Vector3 camerapos )
        {
            if (ViewerState.GetInstance().CurrentViewerState == ViewerState.ViewerStateEnum.Terrain)
            {
                if (CurrentEditBrush.GetInstance().BrushShape != null)
                {
                    if (CurrentEditBrush.GetInstance().BrushEffect.Repeat)
                    {
                        Vector3 intersectpos = EditingHelper.GetIntersectPoint();
                        if (intersectpos == null)
                        {
                            return;
                        }

                        Gl.glDisable(Gl.GL_LIGHTING);
                        Gl.glColor3ub(0, 255, 200);
                        CurrentEditBrush.GetInstance().BrushShape.Render(intersectpos);
                        Gl.glColor3ub(255, 255, 255);
                        Gl.glEnable(Gl.GL_LIGHTING);
                    }
                }
            }
        }

        public void Register( IBrushShape brushshape )
        {
            Console.WriteLine(this.GetType() + " registering " + brushshape );
            this.brushshapes.Add( brushshape.GetType(), brushshape );
            if (CurrentEditBrush.GetInstance().BrushShape == null)
            {
                CurrentEditBrush.GetInstance().BrushShape = brushshape;
            }
            MainTerrainWindow.GetInstance().AddBrushShape( brushshape.Name, brushshape.Description, brushshape );
        }
    }
}
