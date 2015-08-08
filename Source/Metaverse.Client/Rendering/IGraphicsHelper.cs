// Copyright Hugh Perkins 2004,2005,2006
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

//! \file
//! \brief mvGraphicsInterface is a pure interface class for mvGraphics

//! mvGraphicsInterface is a pure interface class for mvGraphics

//! mgraphics contains certain wrapper routines around OpenGL/GLUT
//! for the moment, it contains routines to display text on the screen
//! and to rotate to a passed in quaternion (OpenGL expects axis/angle)
//!
//! This is the abstract pure interface class, to reduce link dependencies
//!
//! See mvGraphics (mvGraphics.h) for documentation

using System;

namespace OSMP
{
    interface IGraphicsHelper
    {
        /// <summary>
        /// throws exception if gl error has occurred
        /// </summary>
        void CheckError();

        void PrintText(string text);
        void ScreenPrintText(int x, int y, string text);
        Vector3 GetMouseVector(Vector3 OurPos, Rot rOurRot, int imousex, int imousey);
        double GetScalingFrom3DToScreen(double fDepth);
        Vector3 GetScreenPos(Vector3 ObserverPos, Rot ObserverRot, Vector3 TargetPos3D);

        void DrawWireframeBox(int iNumSlices);
        void DrawCone();
        void DrawCube();
        void DrawSphere();
        void DrawCylinder();
        void DrawWireSphere();
        void DrawSquareXYPlane();
        void DrawParallelSquares(int iNumSlices);

        void RenderHeightMap(int[,] HeightMap, int iMapSize);
        void RenderTerrain(int[,] HeightMap, int iMapSize);

        void Vertex(Vector3 vertex);

        void Translate(double x, double y, double z);
        void Translate(Vector3 pos);

        void Rotate(double fAngleDegrees, double fX, double fY, double fZ);
        void Rotate(Rot rot);

        void Scale(double x, double y, double z);
        void Scale(Vector3 scale);

        void Bind2DTexture(int iTextureID);
        void PopMatrix();
        void PushMatrix();
        //void SetColor( double r, double g, double b );
        //void SetColor( Color color );
        void SetMaterialColor(double[] mcolor);
        void SetMaterialColor(Color color);
        void RasterPos3f(double x, double y, double z);

        void LoadMatrix(double[] matrix);
    };
}
