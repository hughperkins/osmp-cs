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
    public class EditingHelper
    {
        /// <summary>
        /// Return current mouse intersect point to x-y plane on map, in display coordinates
        /// </summary>
        /// <returns></returns>
        public static Vector3 GetIntersectPoint()
        {
            // intersect mousevector with x-z plane.
            TerrainModel terrain = MetaverseClient.GetInstance().worldstorage.terrainmodel;
            Vector3 mousevector = GraphicsHelperFactory.GetInstance().GetMouseVector(
                Camera.GetInstance().CameraPos, 
                Camera.GetInstance().CameraRot, 
                MouseCache.GetInstance().MouseX,
                MouseCache.GetInstance().MouseY );
            Vector3 camerapos = Camera.GetInstance().CameraPos;
            int width = terrain.HeightMapWidth;
            int height = terrain.HeightMapHeight;
            //Vector3 planenormal = mvMath.ZAxis;
            mousevector.Normalize();
            if (mousevector.z < -0.0005)
            {
                //Vector3 intersectionpoint = camerapos + mousevector * (Vector3.DotProduct(camerapos, planenormal) + 0) /
                //  (Vector3.DotProduct(mousevector, planenormal));
                Vector3 intersectpoint = camerapos - mousevector * (camerapos.z / mousevector.z);
                //Console.WriteLine("intersection: " + intersectionpoint.ToString());
                double heightmapx = intersectpoint.x;
                double heightmapy = intersectpoint.y;
                if (heightmapx >= 0 && heightmapy >= 0 &&
                    heightmapx < width && heightmapy < height)
                {
                    intersectpoint.z = terrain.Map[(int)heightmapx, (int)heightmapy];
                    return intersectpoint;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                //                Console.WriteLine("no intersection");
                return null;
            }
        }
    }
}
