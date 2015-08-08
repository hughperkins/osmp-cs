// Copyright Hugh Perkins 2004,2005,2006
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
//! \brief mgraphics contains certain wrapper routines around OpenGL/GLUT

using System;
using Tao.OpenGl;
using Tao.FreeGlut;
using MathGl;
using System.Drawing;
using Metaverse.Utility;

namespace OSMP
{
    class GraphicsHelperGl : IGraphicsHelper
    {
        int iWindowWidth = 1024;
        int iWindowHeight = 812;

        bool bCubeDefined = false;
        //bool bConeDefined = false;
        bool bCylinderDefined = false;
        bool bSphereDefined = false;

        //int LISTCONE = 1;
        int LISTCUBE = 2;
        int LISTSPHERE = 3;
        int LISTCYLINDER = 4;

        //! Feedback point buffer for OpenGL feedback, used by mvgraphics.cpp
        /*
        class FeedbackPointBufferItem
        {
            public double type;
            public double x;
            public double y;
            public override string ToString()
            {
                return "<FeedbackPointBufferItem type=\"" + type.ToString() + "\" x=\"" + x.ToString() + "\" y=\"" + y.ToString() + " />";
            }
        }
        */

        Vector2 GetFeedbackPointBufferItem(float[] buffer, int index)
        {
            // each point takes 3 bytes:
            // type, x, y
            int vertexoffset = 3 * index + 1;
            return new Vector2(buffer[vertexoffset], buffer[vertexoffset + 1]);
        }

        Vector2 GetFeedbackLineBufferItem(float[] buffer, int lineindex, int vertexindex)
        {
            // each line takes 5 bytes:
            // type, x1, y1, x2, y2
            int vertexoffset = 5 * lineindex + 1 + vertexindex * 2;
            return new Vector2(buffer[vertexoffset], buffer[vertexoffset + 1]);
        }

        //! Feedback line buffer for OpenGL feedback, used by mvgraphics.cpp
        /*
        class FeedbackLineBufferItem
        {
            public double type;
            public Vector2[] vertices = new Vector2[2];
        }
        */

        public Vector3 GetMouseVector(Vector3 OurPos, Rot OurRot, int mousex, int mousey)
        {
            IRenderer renderer = RendererFactory.GetInstance();

            //Vector3 MouseVectorObserverAxes = new Vector3(-renderer.WindowWidth / 2 + mousex, -renderer.ScreenDistanceScreenCoords, renderer.WindowHeight / 2 - mousey);
            Vector3 MouseVectorObserverAxes = new Vector3(
                renderer.ScreenDistanceScreenCoords,
                renderer.WindowWidth / 2 - mousex,
                renderer.WindowHeight / 2 - mousey);
            //LogFile.WriteLine("MouseVectorObserverAxes: " + MouseVectorObserverAxes);
            MouseVectorObserverAxes.Normalize();
            //LogFile.WriteLine("MouseVectorObserverAxes (normalized): " + MouseVectorObserverAxes);
            Vector3 MouseVectorWorldAxes = MouseVectorObserverAxes * OurRot.Inverse();
            //LogFile.WriteLine("MouseVectorWorldAxes: " + MouseVectorWorldAxes.ToString());
            MouseVectorWorldAxes.Normalize();
            return MouseVectorWorldAxes;
        }

        // we want to find out the length something of 1 world unit takes up in screen pixels when it is fDepth away from us
        // into the screen (along our x axis)
        // we draw a lines in the world, at depth fDepth (in world axes)
        // and we use an OpenGl FeedbackBuffer to find out how long that line is on the screen
        // ( see http://msdn.microsoft.com/library/en-us/opengl/glfunc02_3m42.asp )
        public double GetScalingFrom3DToScreen(double fDepth)
        {
            return RendererFactory.GetInstance().ScreenDistanceScreenCoords / fDepth / RendererFactory.GetInstance().NearClip;

            /*
            int iNumLines = 1;
            // each line uses five floats in the buffer:
            float[] feedbackbuffer = new float[iNumLines * 5];

            int[] viewport = new int[4];
            // This Sets The Array <viewport> To The Size And Location Of The Screen Relative To The Window
            Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);

            Gl.glLoadIdentity();

            RendererFactory.GetInstance().ApplyViewingMatrices();

            // rotate so z axis is up, and x axis is forward
            Gl.glRotatef(90f, 0.0f, 0.0f, 1.0f);
            Gl.glRotatef(90f, 0.0f, 1.0f, 0.0f);

            Gl.glFeedbackBuffer(iNumLines * 5, Gl.GL_2D, feedbackbuffer);
            Gl.glRenderMode(Gl.GL_FEEDBACK);

            Gl.glBegin(Gl.GL_LINES);
            Gl.glVertex3d(fDepth, -0.5, 0);
            Gl.glVertex3d(fDepth, 0.5, 0);
            Gl.glEnd();

            int iNumValues = Gl.glRenderMode(Gl.GL_RENDER);

            // A is distance of vector, at depth fDepth in world, in pixels
            // Test.WriteOut( feedbackbuffer );
            Vector2 VectorA = GetFeedbackLineBufferItem(feedbackbuffer, 0, 1) - GetFeedbackLineBufferItem(feedbackbuffer, 0, 0);
            double A = VectorA.Det();

            return A;
             */
        }

        // not to self: this is ugly, maybe simplify a little/lot?
        public Vector3 GetScreenPos(Vector3 ObserverPos, Rot ObserverRot, Vector3 TargetPos3D)
        {
            float[] feedbackbuffer = new float[1 * 3];

            Gl.glFeedbackBuffer(3, Gl.GL_2D, feedbackbuffer);

            int[] viewport = new int[4];
            // This Sets The Array <viewport> To The Size And Location Of The Screen Relative To The Window
            Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);

            Gl.glPushMatrix();

            Gl.glRenderMode(Gl.GL_FEEDBACK);
            Gl.glLoadIdentity();

            // rotate so z axis is up, and x axis is forward
            Gl.glRotatef(90f, 0.0f, 0.0f, 1.0f);
            Gl.glRotatef(90f, 0.0f, 1.0f, 0.0f);

            Rotate(ObserverRot.Inverse());
            Translate(-ObserverPos);

            Gl.glBegin(Gl.GL_POINTS);
            Gl.glVertex3f((float)TargetPos3D.x, (float)TargetPos3D.y, (float)TargetPos3D.z);
            Gl.glEnd();

            Gl.glRenderMode(Gl.GL_RENDER);

            Gl.glPopMatrix();

            //DEBUG(  "Screencoords of input vertex: " << FeedbackBuffer.Vertices[0].x << " " << FeedbackBuffer.Vertices[0].y ); // DEBUG
            Vector2 screenpoint = GetFeedbackPointBufferItem(feedbackbuffer, 0);
            Vector3 ScreenPos = new Vector3(
                0,
                RendererFactory.GetInstance().WindowWidth - screenpoint.x,
                screenpoint.y
            );

            //DEBUG(  "screenpos: " << ScreenPos ); // DEBUG

            return ScreenPos;
        }

        public void SetColor(double r, double g, double b)
        {
            Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_AMBIENT_AND_DIFFUSE, new float[] { (float)r, (float)g, (float)b, 1.0f });
        }

        public void SetColor(Color color)
        {
            Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_AMBIENT_AND_DIFFUSE, new float[] { (float)color.r, (float)color.g, (float)color.b, 1.0f });
        }

        public void PrintText(string text)
        {
            foreach (char p in text)
            {
                Glut.glutBitmapCharacter(Glut.GLUT_BITMAP_TIMES_ROMAN_24, p);
            }
        }

        public void ScreenPrintText(int x, int y, string text)
        {
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glPushMatrix();
            Gl.glLoadIdentity();
            Gl.glOrtho(0, iWindowWidth, 0, iWindowHeight, -1.0f, 1.0f);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glPushMatrix();
            Gl.glLoadIdentity();
            Gl.glPushAttrib(Gl.GL_DEPTH_TEST);
            Gl.glDisable(Gl.GL_DEPTH_TEST);
            Gl.glRasterPos2i(x, y);
            PrintText(text);
            Gl.glPopAttrib();
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glPopMatrix();
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glPopMatrix();
        }

        public void Rotate(Rot rot)
        {
            double fRotAngle = 0;
            Vector3 vAxis = new Vector3();
            mvMath.Rot2AxisAngle(ref vAxis, ref fRotAngle, rot);
            Gl.glRotatef((float)(fRotAngle / Math.PI * 180), (float)vAxis.x, (float)vAxis.y, (float)vAxis.z);
        }

        public void DrawSquareXYPlane()
        {
            // Test.Debug(  "DrawSquareXYPlanes" ); // Test.Debug
            Gl.glBegin(Gl.GL_LINE_LOOP);
            Gl.glVertex3f(-0.5f, -0.5f, 0f);
            Gl.glVertex3f(-0.5f, 0.5f, 0f);
            Gl.glVertex3f(0.5f, 0.5f, 0f);
            Gl.glVertex3f(0.5f, -0.5f, 0f);
            Gl.glEnd();
        }

        public void DrawParallelSquares(int iNumSlices)
        {
            // Test.Debug(  "DrawParallelSquares" ); // Test.Debug
            Gl.glPushMatrix();
            Gl.glTranslated(0, 0, -0.5);
            double fSpacing = 1.0 / (double)iNumSlices;
            for (int i = 0; i <= iNumSlices; i++)
            {
                DrawSquareXYPlane();
                Gl.glTranslated(0, 0, fSpacing);
            }
            Gl.glPopMatrix();
        }

        public void DrawWireframeBox(int iNumSlices)
        {
            // Test.Debug(  "DrawWireframeBox" ); // Test.Debug
            Gl.glPushMatrix();
            DrawParallelSquares(iNumSlices);
            Gl.glPushMatrix();
            Gl.glRotatef(90f, 1f, 0f, 0f);
            DrawParallelSquares(iNumSlices);

            Gl.glPopMatrix();

            Gl.glRotatef(90f, 0f, 1f, 0f);
            DrawParallelSquares(iNumSlices);

            Gl.glPopMatrix();
        }

        public void DrawCone()
        {
            //if (!bConeDefined)
            //{
              //  Gl.glNewList(LISTCONE, Gl.GL_COMPILE);

                Gl.glPushMatrix();
                Glu.GLUquadric quadratic = Glu.gluNewQuadric();   // Create A Pointer To The Quadric Object
                Glu.gluQuadricNormals(quadratic, Glu.GLU_SMOOTH); // Create Smooth Normals
                Glu.gluQuadricTexture(quadratic, 32);  // Create Texture Coords
                Gl.glTranslated(0.0f, 0.0f, -0.5f);   // Center The Cone
                Glu.gluQuadricOrientation(quadratic, Glu.GLU_OUTSIDE);
                Glu.gluCylinder(quadratic, 0.5f, 0.0f, 1.0f, 32, 32);
                //glTranslatef(0.0f,0.0f,-0.5f);
                Gl.glRotatef(180.0f, 1.0f, 0.0f, 0.0f);
                Glu.gluDisk(quadratic, 0.0f, 0.5f, 32, 32);
                //glRotatef( 180.0f, 1.0f, 0.0f, 0.0f );
                //glTranslatef(0.0f,0.0f,1.0f);
                //gluDisk(quadratic,0.0f,1.0f,32,32);
                Gl.glPopMatrix();

              //  Gl.glEndList();
                //bConeDefined = true;
            //}

            //Gl.glCallList(LISTCONE);
        }

        public void DrawCube()
        {

            if (!bCubeDefined)
            {
                Gl.glNewList(LISTCUBE, Gl.GL_COMPILE);

                Gl.glBegin(Gl.GL_QUADS);
                Gl.glNormal3f(0.0f, 0.0f, 1.0f);
                Gl.glTexCoord2f(0.0f, 0.0f);
                Gl.glVertex3f(1f, 1f, 1f);
                Gl.glTexCoord2f(0.0f, 1.0f);
                Gl.glVertex3f(0f, 1f, 1f);
                Gl.glTexCoord2f(1.0f, 1.0f);
                Gl.glVertex3f(0f, 0f, 1f);
                Gl.glTexCoord2f(1.0f, 0.0f);
                Gl.glVertex3f(1f, 0f, 1f);

                Gl.glNormal3f(0.0F, 0.0F, -1.0F);
                Gl.glTexCoord2f(1.0f, 0.0f);
                Gl.glVertex3f(1f, 1f, 0f);
                Gl.glTexCoord2f(0.0f, 0.0f);
                Gl.glVertex3f(1f, 0f, 0f);
                Gl.glTexCoord2f(0.0f, 1.0f);
                Gl.glVertex3f(0f, 0f, 0f);
                Gl.glTexCoord2f(1.0f, 1.0f);
                Gl.glVertex3f(0f, 1f, 0f);

                Gl.glNormal3f(0.0F, 1.0F, 0.0F);
                Gl.glTexCoord2f(0.0f, 0.0f);
                Gl.glVertex3f(1f, 1f, 1f);
                Gl.glTexCoord2f(0.0f, 1.0f);
                Gl.glVertex3f(1f, 1f, 0f);
                Gl.glTexCoord2f(1.0f, 1.0f);
                Gl.glVertex3f(0f, 1f, 0f);
                Gl.glTexCoord2f(1.0f, 0.0f);
                Gl.glVertex3f(0f, 1f, 1f);

                Gl.glNormal3f(0.0F, -1.0F, 0.0F);
                Gl.glTexCoord2f(0.0f, 1.0f);
                Gl.glVertex3f(0f, 0f, 0f);
                Gl.glTexCoord2f(1.0f, 1.0f);
                Gl.glVertex3f(1f, 0f, 0f);
                Gl.glTexCoord2f(1.0f, 0.0f);
                Gl.glVertex3f(1f, 0f, 1f);
                Gl.glTexCoord2f(0.0f, 0.0f);
                Gl.glVertex3f(0f, 0f, 1f);

                Gl.glNormal3f(1.0F, 0.0F, 0.0F);
                Gl.glTexCoord2f(1.0f, 0.0f);
                Gl.glVertex3f(1f, 1f, 1f);
                Gl.glTexCoord2f(0.0f, 0.0f);
                Gl.glVertex3f(1f, 0f, 1f);
                Gl.glTexCoord2f(0.0f, 1.0f);
                Gl.glVertex3f(1f, 0f, 0f);
                Gl.glTexCoord2f(1.0f, 1.0f);
                Gl.glVertex3f(1f, 1f, 0f);

                Gl.glNormal3f(-1.0F, 0.0F, 0.0F);
                Gl.glTexCoord2f(1.0f, 1.0f);
                Gl.glVertex3f(0f, 0f, 0f);
                Gl.glTexCoord2f(1.0f, 0.0f);
                Gl.glVertex3f(0f, 0f, 1f);
                Gl.glTexCoord2f(0.0f, 0.0f);
                Gl.glVertex3f(0f, 1f, 1f);
                Gl.glTexCoord2f(0.0f, 1.0f);
                Gl.glVertex3f(0f, 1f, 0f);
                Gl.glEnd();
                Gl.glEndList();

                //    Test.Debug(  "cube list stored" ); // Test.Debug
                bCubeDefined = true;
            }

            Gl.glPushMatrix();
            Gl.glTranslated(-0.5, -0.5, -0.5);

            // if( sTextureChecksum != "" )
            // {
            // }

            Gl.glCallList(LISTCUBE);
            Gl.glPopMatrix();
        }

        public void DrawSphere()
        {
            if (!bSphereDefined)
            {
                Gl.glNewList(LISTSPHERE, Gl.GL_COMPILE);

                Glu.GLUquadric quadratic = Glu.gluNewQuadric();   // Create A Pointer To The Quadric Object
                Glu.gluQuadricNormals(quadratic, Glu.GLU_SMOOTH); // Create Smooth Normals
                Glu.gluQuadricTexture(quadratic, 32);  // Create Texture Coords
                Glu.gluSphere(quadratic, 0.5f, 32, 32);
                Gl.glEndList();
                bSphereDefined = true;
            }

            Gl.glCallList(LISTSPHERE);
        }

        public void DrawCylinder()
        {
            if (!bCylinderDefined)
            {
                Gl.glNewList(LISTCYLINDER, Gl.GL_COMPILE);

                Gl.glPushMatrix();
                Glu.GLUquadric quadratic = Glu.gluNewQuadric();   // Create A Pointer To The Quadric Object
                Glu.gluQuadricNormals(quadratic, Glu.GLU_SMOOTH); // Create Smooth Normals
                Glu.gluQuadricTexture(quadratic, 32);  // Create Texture Coords
                Gl.glTranslatef(0.0f, 0.0f, -0.5f);   // Center The Cylinder
                Glu.gluQuadricOrientation(quadratic, Glu.GLU_OUTSIDE);
                Glu.gluCylinder(quadratic, 0.5f, 0.5f, 1.0f, 32, 32);
                //glTranslatef(0.0f,0.0f,-0.5f);
                Gl.glRotatef(180.0f, 1.0f, 0.0f, 0.0f);
                Glu.gluDisk(quadratic, 0.0f, 0.5f, 32, 32);
                Gl.glRotatef(180.0f, 1.0f, 0.0f, 0.0f);
                Gl.glTranslatef(0.0f, 0.0f, 1.0f);
                Glu.gluDisk(quadratic, 0.0f, 0.5f, 32, 32);
                Gl.glPopMatrix();

                Gl.glEndList();
                bCylinderDefined = true;
            }

            Gl.glCallList(LISTCYLINDER);
        }

        // based on http://nehe.gamedev.net
        public void RenderHeightMap(int[,] HeightMap, int iMapSize)     // This Renders The Height Map As Quads
        {
            int X = 0, Y = 0;
            int x, y, z;

            if (HeightMap == null)
            {
                LogFile.WriteLine("Error: no height map data available"); // Test.Debug
                return;
            }

            Gl.glBegin(Gl.GL_QUADS);

            // Test.Debug(  "drawing quads..." ); // Test.Debug
            for (X = 2; X < (iMapSize - 3); X += 1)
            {
                // Test.Debug(  "X " << X ); // Test.Debug
                for (Y = 2; Y < (iMapSize - 3); Y += 1)
                {
                    Vector3 Normal = new Vector3(); ; // = VectorNormals[ X + 128 * Y ];
                    Normal.z = 1;
                    Normal.x = (HeightMap[X + 10, Y] - HeightMap[X, Y]) / 10.0f;
                    Normal.y = (HeightMap[X, Y + 10] - HeightMap[X, Y]) / 10.0f;

                    x = X;
                    y = Y;
                    z = HeightMap[X, Y];
                    Gl.glNormal3d(Normal.x, Normal.y, Normal.z);
                    Gl.glTexCoord2d((double)X / (double)iMapSize, (double)Y / (double)iMapSize);
                    Gl.glVertex3i(x, y, z);

                    x = X + 1;
                    y = Y;
                    z = HeightMap[X + 1, Y];
                    Gl.glNormal3d(Normal.x, Normal.y, Normal.z);
                    Gl.glTexCoord2d((double)(X + 1) / (double)iMapSize, (double)(Y) / (double)iMapSize);
                    Gl.glVertex3i(x, y, z);

                    x = X + 1;
                    y = Y + 1;
                    z = HeightMap[X + 1, Y + 1];
                    Gl.glNormal3d(Normal.x, Normal.y, Normal.z);
                    Gl.glTexCoord2d((double)(X + 1) / (double)iMapSize, (double)(Y + 1) / (double)iMapSize);
                    Gl.glVertex3i(x, y, z);

                    x = X;
                    y = Y + 1;
                    z = HeightMap[X, Y + 1];
                    Gl.glNormal3d(Normal.x, Normal.y, Normal.z);
                    Gl.glTexCoord2d((double)(X) / (double)iMapSize, (double)(Y + 1) / (double)iMapSize);
                    Gl.glVertex3i(x, y, z);
                }
            }
            //Test.Debug(  "Quads done" << X ); // Test.Debug
            Gl.glEnd();
        }

        //int iNextTerrainListNumber = 1001;

        public void RenderTerrain(int[,] HeightMap, int iMapSize)
        {
            // Test.Debug(  "RenderTerrain() start..." ); // Test.Debug
            Gl.glPushMatrix();
            Gl.glTranslated(-0.5, -0.5, -0.5);
            Gl.glScaled(1 / (double)iMapSize, 1 / (double)iMapSize, 1.0f / 256.0f);

            //if( !bTerrainListInitialized )
            // {
            //  iOurTerrainListNumber = iNextTerrainListNumber;
            // iNextTerrainListNumber++;

            //   glNewList(iOurTerrainListNumber, GL_COMPILE);
            // Test.Debug(  "rendering height map..." ); // Test.Debug

            RenderHeightMap(HeightMap, iMapSize);
            // Test.Debug(  "rendering done" ); // Test.Debug
            //   glEndList();

            //  bTerrainListInitialized = true;
            //}

            //glCallList(iOurTerrainListNumber);

            Gl.glPopMatrix();

            //  Test.Debug(  "RenderTerrain() ... done" ); // Test.Debug
        }

        public void Vertex(Vector3 vertex)
        {
            Gl.glVertex3d(vertex.x, vertex.y, vertex.z);
        }

        public void Translate(double x, double y, double z)
        {
            Gl.glTranslated(x, y, z);
        }

        public void Translate(Vector3 pos)
        {
            Gl.glTranslated(pos.x, pos.y, pos.z);
        }

        public void Scale(double x, double y, double z)
        {
            Gl.glScaled(x, y, z);
        }

        public void Scale(Vector3 scale)
        {
            Gl.glScaled(scale.x, scale.y, scale.z);
        }

        public void TexCoord(Vector2 coord)
        {
            Gl.glTexCoord2d(coord.x, coord.y);
        }

        public void Bind2DTexture(int iTextureID)
        {
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, iTextureID);
        }

        public void PopMatrix()
        {
            Gl.glPopMatrix();
        }

        public void PushMatrix()
        {
            Gl.glPushMatrix();
        }

        public void LoadMatrix(double[] matrix)
        {
            Gl.glLoadMatrixd(matrix);
        }

        public void SetMaterialColor(double[] mcolor)
        {
            Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_AMBIENT_AND_DIFFUSE,
                new float[] { (float)mcolor[0], (float)mcolor[1], (float)mcolor[2], (float)mcolor[3] });
        }

        public void SetMaterialColor(Color color)
        {
            Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_AMBIENT_AND_DIFFUSE,
                new float[] { (float)color.r, (float)color.g, (float)color.b, (float)color.a });
        }

        public void DrawWireSphere()
        {
            //glutWireSphere(0.5, 32, 32 );
        }

        public void RasterPos3f(double x, double y, double z)
        {
            Gl.glRasterPos3f((float)x, (float)y, (float)z);
        }

        public void Rotate(double fAngleDegrees, double fX, double fY, double fZ)
        {
            Gl.glRotatef((float)fAngleDegrees, (float)fX, (float)fY, (float)fZ);
        }

        public void DisableTexture2d()
        {
            Gl.glDisable(Gl.GL_TEXTURE_2D);
        }

        public void EnableTexture2d()
        {
            Gl.glEnable(Gl.GL_TEXTURE_2D);
        }

        public void ActiveTexture(int TextureStage)
        {
            switch (TextureStage)
            {
                case 0:
                    Gl.glActiveTextureARB(Gl.GL_TEXTURE0_ARB);
                    break;

                case 1:
                    Gl.glActiveTextureARB(Gl.GL_TEXTURE1_ARB);
                    break;

                case 2:
                    Gl.glActiveTextureARB(Gl.GL_TEXTURE2_ARB);
                    break;

                case 3:
                    Gl.glActiveTextureARB(Gl.GL_TEXTURE3_ARB);
                    break;

                case 4:
                    Gl.glActiveTextureARB(Gl.GL_TEXTURE4_ARB);
                    break;

                case 5:
                    Gl.glActiveTextureARB(Gl.GL_TEXTURE5_ARB);
                    break;

                case 6:
                    Gl.glActiveTextureARB(Gl.GL_TEXTURE6_ARB);
                    break;

                case 7:
                    Gl.glActiveTextureARB(Gl.GL_TEXTURE7_ARB);
                    break;
            }
        }
        public void EnableModulate()
        {
            Gl.glTexEnvi(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_MODULATE);
        }
        public void EnableBlendSrcAlpha()
        {
            Gl.glEnable(Gl.GL_BLEND);
            Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
            CheckError();
        }
        public void Normal(Vector3 normal)
        {
            Gl.glNormal3d(normal.x, normal.y, normal.z);
        }
        public void SetTextureScale(double scale)
        {
            // matrix texture scaling from http://www.kraftwrk.com/multi_texturing.htm
            // Now we want to enter the texture matrix. This will allow us
            // to change the tiling of the detail texture.
            Gl.glMatrixMode(Gl.GL_TEXTURE);
            // Reset the current matrix and apply our chosen scale value
            Gl.glLoadIdentity();
            Gl.glScalef((float)scale, (float)scale, 1);
            // Leave the texture matrix and set us back in the model view matrix
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
        }

        public void ApplyOrtho(int innerwindowwidth, int innerwindowheight, int outerwindowwidth, int outerwindowheight)
        {
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glPushMatrix();
            Gl.glLoadIdentity();
            //Gl.glOrtho(0, windowwidth, windowheight, 0, -1, 1); // we'll just draw the minimap directly onto our display
            Gl.glOrtho(0, innerwindowwidth, innerwindowheight, innerwindowheight - outerwindowheight, -1, 1); // we'll just draw the minimap directly onto our display

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glPushMatrix();
            Gl.glLoadIdentity();
        }

        public void RemoveOrtho()
        {
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glPopMatrix();
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glPopMatrix();
        }

        public void CheckError()
        {
            int error = Gl.glGetError();
            if (error != Gl.GL_NO_ERROR)
            {
                string errortype = "unknown";
                switch (error)
                {
                    case Gl.GL_OUT_OF_MEMORY:
                        errortype = "out of memory";
                        break;

                    case Gl.GL_TABLE_TOO_LARGE:
                        errortype = "table too large";
                        break;

                    case Gl.GL_INVALID_ENUM:
                        errortype = "invalid enum";
                        break;

                    case Gl.GL_INVALID_VALUE:
                        errortype = "invalid value";
                        break;

                    case Gl.GL_INVALID_OPERATION:
                        errortype = "invalid operation";
                        break;

                    case Gl.GL_STACK_OVERFLOW:
                        errortype = "stack overflow";
                        break;

                    case Gl.GL_STACK_UNDERFLOW:
                        errortype = "stack underflow";
                        break;

                }
                throw new Exception( "Gl error: " + error + " " + errortype );
            }
        }
    }
}
