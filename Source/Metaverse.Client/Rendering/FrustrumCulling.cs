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
using Tao.OpenGl;

namespace OSMP
{
    // see http://www.lighthouse3d.com/opengl/viewfrustum/index.php?defvf
    // for good explanation
    public class FrustrumCulling
    {
        static FrustrumCulling instance = new FrustrumCulling();
        public static FrustrumCulling GetInstance() { return instance; }

        public Vector3 camerapos;
        public Rot camerarot;
        public Vector3 viewray;
        public Vector3 right;
        public Vector3 up;

        public double HNear;
        public double HFar;
        public double VNear;
        public double VFar;

        double nearclip;
        double farclip;

        public Vector3 fc, nc, ftl, ftr, fbl, fbr, ntl, ntr, nbl, nbr;
        public Plane[] planes = new Plane[6];

        public FrustrumCulling()
        {
            //RendererSdl.GetInstance().WriteNextFrameEvent += new WriteNextFrameCallback(FrustrumCulling_WriteNextFrameEvent);
            RendererFactory.GetInstance().PreDrawEvent += new PreDrawCallback(FrustrumCulling_PreDrawEvent);
            //SetupFrustrum();
        }

        void SetupFrustrum()
        {
            //LogFile.WriteLine("setup frustrum");

            camerapos = Camera.GetInstance().CameraPos;
            camerarot = Camera.GetInstance().CameraRot;
            Rot inversecamerarot = camerarot.Inverse();
            //viewray = -mvMath.YAxis * inversecamerarot;
            //viewray.Normalize();
            //right = mvMath.XAxis * inversecamerarot;
            //up = mvMath.ZAxis * inversecamerarot;
            //right.Normalize();
            //up.Normalize();

            viewray = mvMath.XAxis * inversecamerarot;
            viewray.Normalize();
            right = - mvMath.YAxis * inversecamerarot;
            up = mvMath.ZAxis * inversecamerarot;
            right.Normalize();
            up.Normalize();

            nearclip = RendererFactory.GetInstance().NearClip;
            farclip = RendererFactory.GetInstance().FarClip;
            VNear = 2 * Math.Tan(RendererFactory.GetInstance().FieldOfView / 2 * Math.PI / 180) * nearclip;
            VFar = VNear * farclip / nearclip;
            HNear = VNear * (double)RendererFactory.GetInstance().OuterWindowWidth / RendererFactory.GetInstance().OuterWindowHeight;
            HFar = HNear * farclip / nearclip;

            //Console.WriteLine( "clips: " + nearclip + " " + farclip + " " + VNear + " " + VFar + " " + HNear + " " + HFar );

            fc = camerapos + viewray * farclip;
            ftl = fc + (up * VFar / 2) - (right * HFar / 2);
            ftr = fc + (up * VFar / 2) + (right * HFar / 2);
            fbl = fc - (up * VFar / 2) - (right * HFar / 2);
            fbr = fc - (up * VFar / 2) + (right * HFar / 2);

            nc = camerapos + viewray * nearclip;

            ntl = nc + (up * VNear / 2) - (right * HNear / 2);
            ntr = nc + (up * VNear / 2) + (right * HNear / 2);
            nbl = nc - (up * VNear / 2) - (right * HNear / 2);
            nbr = nc - (up * VNear / 2) + (right * HNear / 2);

            // note: all normals point outwards
            planes[0] = new Plane(-viewray, nc);
            planes[1] = new Plane(viewray, fc);

            Vector3 vectoralongplane;
            Vector3 normal;

            vectoralongplane = (ntr - camerapos).Normalize();
            normal = (up * vectoralongplane).Normalize();
            planes[2] = new Plane( - normal, camerapos);

            vectoralongplane = (nbr - camerapos).Normalize();
            normal = (right * vectoralongplane).Normalize();
            planes[3] = new Plane(- normal, camerapos);

            vectoralongplane = (nbl - camerapos).Normalize();
            normal = -(up * vectoralongplane).Normalize();
            planes[4] = new Plane(- normal, camerapos);

            vectoralongplane = (ntl - camerapos).Normalize();
            normal = -(right * vectoralongplane).Normalize();
            planes[5] = new Plane(- normal, camerapos);
        }

        //public FrustrumCulling( Vector3 camerapos, Rot camerarot, float nearclip, float farclip)
        void FrustrumCulling_PreDrawEvent()
        {
            SetupFrustrum();
        }

        // for testing only
        void FrustrumCulling_WriteNextFrameEvent(Vector3 camerapos)
        {
            SetupFrustrum();

            IGraphicsHelper g = GraphicsHelperFactory.GetInstance();
            
            g.SetMaterialColor(new Color(1, 1, 0));
            Gl.glBegin(Gl.GL_LINES);
            g.Vertex( camerapos + viewray * nearclip * 1.1 + HNear / 2 * 0.95 * right + VNear / 2 * 0.95 * up );
            g.Vertex( camerapos + viewray * nearclip * 1.1 - HNear / 2 * 0.95 * right + VNear / 2 * 0.95 * up );
            Gl.glEnd();
            Gl.glBegin(Gl.GL_LINES);
            g.Vertex( camerapos + viewray * nearclip * 1.1 - HNear / 2 * 0.95 * right - VNear / 2 * 0.95 * up );
            g.Vertex( camerapos + viewray * nearclip * 1.1 + HNear / 2 * 0.95 * right - VNear / 2 * 0.95 * up );
            Gl.glEnd();
             

            g.SetMaterialColor(new Color(1, 0, 1));
            Gl.glBegin(Gl.GL_LINES);
            g.Vertex(camerapos + viewray * farclip * 0.95 + HFar / 2 * 0.95 * right + VFar / 2 * 0.9 * up);
            g.Vertex( camerapos + viewray * farclip * 0.95 - HFar / 2 * 0.95 * right + VFar / 2 * 0.9 * up );
            Gl.glEnd();
            Gl.glBegin(Gl.GL_LINES);
            g.Vertex( camerapos + viewray * farclip * 0.95 - HFar / 2 * 0.95 * right - VFar / 2 * 0.9 * up );
            g.Vertex( camerapos + viewray * farclip * 0.95 + HFar / 2 * 0.95 * right - VFar / 2 * 0.9 * up );
            Gl.glEnd();

            /*
            g.SetMaterialColor(new Color(1, 0, 0));
            Gl.glBegin(Gl.GL_LINES);
            g.Vertex(viewray * 10);
            g.Vertex(viewray * 100);
            Gl.glEnd();

            g.SetMaterialColor(new Color(0, 1, 0));
            Gl.glBegin(Gl.GL_LINES);
            g.Vertex(viewray * 10);
            g.Vertex(viewray * 10 + right * 100);
            Gl.glEnd();

            g.SetMaterialColor(new Color(0, 0, 1));
            Gl.glBegin(Gl.GL_LINES);
            g.Vertex(viewray * 10);
            g.Vertex(viewray * 10 + up * 100);
            Gl.glEnd();

            g.SetMaterialColor(new Color(0, 1, 1));
            for (int i = 0; i < 6; i++)
            {
                Gl.glBegin(Gl.GL_LINES);
                g.Vertex(planes[i].point + viewray * 100);
                g.Vertex(planes[i].point + planes[i].normalizednormal * 100 + viewray * 100);
                Gl.glEnd();
            }
             */

            //Console.WriteLine( camerapos + " " + viewray );
            //Console.WriteLine( IsInsideFrustum( camerapos + viewray * nearclip * 1.5, 0 ) );
            //Console.WriteLine( IsInsideFrustum( camerapos + viewray * farclip * 0.9, 0 ) );
            //System.Environment.Exit( 0 );
        }

        public bool IsInsideFrustum(Vector3 centrepos, double boundingradius)
        {
            //LogFile.WriteLine("IsInsideFrustrum " + centrepos + " " + boundingradius);
            foreach (Plane plane in planes)
            {
                double distance = plane.GetDistance(centrepos);
              //  Console.WriteLine( "plane " + plane + " distance: " + distance );
                if (distance > boundingradius)
                {
                    //System.Environment.Exit(0);
                    return false;
                }
            }
            //Console.WriteLine( "IsInsideFrustrum " + centrepos + " " + boundingradius );
            return true;
        }
    }
}
