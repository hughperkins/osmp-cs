// Copyright Hugh Perkins 2004,2005,2006
//
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License version 2 as published by the
// Free Software Foundation;
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
using System.Collections;
using Metaverse.Utility;

namespace OSMP
{
    public class EntityCreationProperties
    {
        public Vector3 pos;
        public Rot rot;
        public Vector3 scale;

        public EntityCreationProperties(int screenx, int screeny)
        {
            LogFile.WriteLine("create entity screen pos : " + screenx + " " + screeny);

            IGraphicsHelper graphics = GraphicsHelperFactory.GetInstance();

            Camera camera = Camera.GetInstance();
            Vector3 mousevector = graphics.GetMouseVector(
                camera.CameraPos, camera.CameraRot, screenx, screeny);
            pos = camera.CameraPos + 3.0 * mousevector.Normalize();
            LogFile.WriteLine("mousevector: " + mousevector);

            rot = new Rot();
            scale = new Vector3(1, 1, 1);
        }
        public void WriteToEntity( Entity entity, string name )
        {
            entity.pos = pos;
            entity.rot = rot;
            entity.scale = scale;
            entity.name = "New " + name;
        }
    }
}

