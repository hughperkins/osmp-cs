// Copyright Hugh Perkins 2004,2005,2006
// hughperkins at gmail http://hughperkins.com
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

//! \file
//! \brief This module carries out OpenGL initialization and manages drawing of the world on demand

using System;
using System.Collections;
using System.Runtime.InteropServices;
using Metaverse.Utility;
using Tao.Sdl;
using Tao.OpenGl;
using SdlDotNet;

namespace OSMP
{
    // This creates the window into which we render, and also the chat panel
    // provides a Menu, a ContextMenu, and event callbacks for key, mouse, contextmenu
    //
    // Note to self: would be nice to have some sort of publisher/subscriber pattern for creating panels and rendering areas
    public class RendererSdl : IRenderer
    {
        static RendererSdl instance = new RendererSdl();
        public static RendererSdl GetInstance() { return instance; }

        public event WriteNextFrameCallback WriteNextFrameEvent;
        public event WriteNextFrameCallback WriteAlpha;
        public event PreDrawCallback PreDrawEvent;
        public event TickHandler Tick;

        public event MouseMotionEventHandler MouseMotion;
        public event MouseButtonEventHandler MouseDown;
        public event MouseButtonEventHandler MouseUp;
        public event KeyboardEventHandler KeyUp;
        public event KeyboardEventHandler KeyDown;

        RendererSdl() { } // protected constructor, enforce Singleton

        //int FullScreenBitsPerPixel = 32;

        int iWindowWidth, iWindowHeight;
        public int OuterWindowWidth{ get{ return iWindowWidth; } }
        public int OuterWindowHeight{ get{ return iWindowHeight; } }

        string WindowName = "OSMP";

        int innerwidth;
        int innerheight;
        public int WindowWidth
        {
            get
            {
                return innerwidth;
            }
        }

        public int WindowHeight
        {
            get
            {
                return innerheight;
            }
        }

        public void DrawWorld()
        {
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);

            Gl.glLoadIdentity();

            new GraphicsHelperGl().CheckError();
            if (PreDrawEvent != null)
            {
                PreDrawEvent();
            }
            new GraphicsHelperGl().CheckError();

            // note to self: move lighting to subscriber object?
            float[] ambientLight = new float[] { 0.4f, 0.4f, 0.4f, 1.0f };
            float[] diffuseLight = new float[] { 0.6f, 0.6f, 0.6f, 1.0f };
            float[] specularLight = new float[] { 0.2f, 0.2f, 0.2f, 1.0f };
            float[] position = new float[] { -1.0f, 0.2f, -0.4f, 1.0f };

            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_AMBIENT, ambientLight);
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_DIFFUSE, diffuseLight);
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_SPECULAR, specularLight);
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_POSITION, position);

            Vector3 camerapos = Camera.GetInstance().CameraPos;
            new GraphicsHelperGl().CheckError();
            if (WriteNextFrameEvent != null)
            {
                //LogFile.WriteLine("writenextframe");
                WriteNextFrameEvent(camerapos);
            }

            new GraphicsHelperGl().CheckError();
            if (WriteAlpha != null)
            {
                WriteAlpha( camerapos );
            }
            new GraphicsHelperGl().CheckError();

            // rotate so z axis is up, and x axis is forward
            //Gl.glRotatef( 90f, 0.0f, 0.0f, 1.0f );
            //Gl.glRotatef( 90f, 0.0f, 1.0f, 0.0f );

            //Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_POSITION, new float[] { -100.5f, 100.0f, 100.0f, 1.0f });

            //     DEBUG(  "...disp done" ); // DEBUG
        }

        //! called to draw what we see in window, once a frame.
        //! calls drawworld
        void WriteNextFrame()
        {
            DrawWorld();
            Video.GLSwapBuffers();
        }

        public void ApplyViewingMatrices()
        {
            new GraphicsHelperGl().CheckError();
            if (PreDrawEvent != null)
            {
                PreDrawEvent();
            }
            new GraphicsHelperGl().CheckError();
        }

        bool IsRunning = true;
        public void Shutdown()
        {
            IsRunning = false;
        }

        void MainLoop()
        {
            while (IsRunning)
            {
                //try
                //{
                    WriteNextFrame();
                    // LogFile.WriteLine(".");
                    // Thread.Sleep(100);
                    while (Events.Poll())
                    {
                    }
                    if (Tick != null)
                    {
                        Tick();
                    }
                //}
                //catch (Exception e)
               // {
                 //   LogFile.WriteLine(e.ToString());
               // }
            }
        }

        public int MaxTexelUnits;
        public bool Multitexturing = true;

        public void Init()
        {
            LogFile.WriteLine("init sdl");

            // int bpp = pixelFormat.BitsPerPixel;
            /*
                    Sdl.SDL_GL_SetAttribute( Sdl.SDL_GL_RED_SIZE, 5 );
                    Sdl.SDL_GL_SetAttribute( Sdl.SDL_GL_GREEN_SIZE, 5 );
                    Sdl.SDL_GL_SetAttribute( Sdl.SDL_GL_BLUE_SIZE, 5 );
                    Sdl.SDL_GL_SetAttribute( Sdl.SDL_GL_DEPTH_SIZE, 16 );
                    Sdl.SDL_GL_SetAttribute( Sdl.SDL_GL_DOUBLEBUFFER, 1 );

                    Sdl.SDL_WM_SetIcon(Sdl.SDL_LoadBMP("osmpico32.bmp"), null );
                    if( Sdl.SDL_SetVideoMode( iWindowWidth, iWindowHeight, bpp, Sdl.SDL_OPENGL | Sdl.SDL_HWSURFACE | Sdl.SDL_DOUBLEBUF ) == IntPtr.Zero )
                    {
                        LogFile.WriteLine(  "Video mode set failed: " + Sdl.SDL_GetError().ToString() ); // Console.WriteLine
                        return;
                    }

                    Sdl.SDL_WM_SetCaption( WindowName, "" );
            */
            //iWindowWidth = SdlDotNet.Video.Screen.Width;// Sdl.GetScreenWidth();
            //iWindowHeight = SdlDotNet.Video.Screen.Height; // Sdl.GetScreenHeight();

            System.Drawing.Icon icon = new System.Drawing.Icon( EnvironmentHelper.GetExeDirectory() + "/Metaverse.ico");
            SdlDotNet.Video.WindowIcon(icon);

            iWindowWidth = Config.GetInstance().windowwidth;
            iWindowHeight = Config.GetInstance().windowheight;

            LogFile.WriteLine("requested window width/height: " + iWindowWidth.ToString() + " " + OuterWindowHeight.ToString()); // Console.WriteLine

            Video.SetVideoModeWindowOpenGL( iWindowWidth, iWindowHeight );
            //Video.SetVideoModeOpenGL(iWindowWidth, WindowHeight, FullScreenBitsPerPixel);

            LogFile.WriteLine(Video.WindowHandle.ToString());

            IDisplayGeometry displaygeometry = DisplayGeometryFactory.GetDisplayGeometry();
            innerwidth = displaygeometry.WindowWidth;
            innerheight = displaygeometry.WindowHeight;

            string extensions = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(Gl.glGetString(Gl.GL_EXTENSIONS));
            LogFile.WriteLine(extensions);

            if (extensions.IndexOf("GL_ARB_multitexture") >= 0)
            {
                GlExtensionLoader.LoadExtension("GL_ARB_multitexture");           // Is Multitexturing Supported?
                Gl.glGetIntegerv(Gl.GL_MAX_TEXTURE_UNITS_ARB, out MaxTexelUnits);
                LogFile.WriteLine("max texel units: " + MaxTexelUnits);
                Multitexturing = true;
            }

            LogFile.WriteLine(Marshal.PtrToStringAnsi(Gl.glGetString(Gl.GL_VERSION))); 

            Video.WindowCaption = WindowName;

            Gl.glClearColor(0.0f, 0.0f, 0.0f, 0.0f);

            Gl.glEnable(Gl.GL_DEPTH_TEST);
            //Gl.glEnable(Gl.GL_TEXTURE_2D);
            //Gl.glEnable (Gl.GL_CULL_FACE);

            Gl.glEnable(Gl.GL_LIGHTING);
            Gl.glEnable(Gl.GL_LIGHT0);

            Tao.DevIl.Ilut.ilutRenderer(Tao.DevIl.Ilut.ILUT_OPENGL);

            //Gl.glShadeModel(Gl.GL_SMOOTH);

            Reshape(iWindowWidth, iWindowHeight);

             float[] ambientLight = new float[]{0.4f, 0.4f, 0.4f, 1.0f};
             float[] diffuseLight =new float[]{0.6f, 0.6f, 0.6f, 1.0f};
             float[] specularLight = new float[]{0.2f, 0.2f, 0.2f, 1.0f};
             float[] position = new float[]{-1.0f, 0.2f, -0.4f, 1.0f};

             Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_AMBIENT, ambientLight);
             Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_DIFFUSE, diffuseLight);
             Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_SPECULAR, specularLight);
             Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_POSITION, position);

            /*

             Gl.glLoadIdentity();

             Gl.glMatrixMode( Gl.GL_PROJECTION );
             Gl.glLoadIdentity();
             float aspect = (float)iWindowWidth / (float)iWindowHeight;
             Glu.gluPerspective( 45.0, aspect, 0.5, 100.0 );

             Gl.glMatrixMode( Gl.GL_MODELVIEW );
             Gl.glViewport (0, 0, iWindowWidth, iWindowHeight);
        
             */
            Events.Quit += new QuitEventHandler(this.Quit);
            Events.MouseMotion += new MouseMotionEventHandler(this._MouseMotion);
            Events.MouseButtonDown += new MouseButtonEventHandler(Events_MouseButtonDown);
            Events.MouseButtonUp += new MouseButtonEventHandler(Events_MouseButtonUp);
            Events.KeyboardDown += new KeyboardEventHandler(this._KeyDown);
            Events.KeyboardUp += new KeyboardEventHandler(this._KeyUp);
        }

        void Events_MouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (MouseUp != null)
            {
                MouseUp(sender, e);
            }
        }

        void Events_MouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (MouseDown != null)
            {
                MouseDown(sender, e);
            }
        }

        public void StartMainLoop()
        {
            MainLoop();
        }

        //public void Shutdown()
        //{
          //  System.Environment.Exit(0);
        //}

        void Quit(object source, QuitEventArgs e)
        {
            Shutdown();
        }

        void _MouseMotion(object source, MouseMotionEventArgs e)
        {
            if (MouseMotion != null)
            {
                MouseMotion(source, e);
            }
        }

        void _KeyDown(object source, KeyboardEventArgs e)
        {
            //if ((char)e.Key == 'q')
            //{
             //   System.Environment.Exit(0);
            //}
            if( KeyDown != null )
            {
                KeyDown( source, e );
            }
        }

        void _KeyUp(object source, KeyboardEventArgs e)
        {
            if (KeyUp != null)
            {
                KeyUp(source, e);
            }
        }

        int _ScreenDistanceScreenCoords;
        double _FieldOfView;
        double _FarClip = 1500;
        double _NearClip = 1.5;

        public int ScreenDistanceScreenCoords{ get { return _ScreenDistanceScreenCoords; } }
        public double FieldOfView { get { return _FieldOfView; } }
        public double FarClip{ get { return _FarClip; } }
        public double NearClip { get { return _NearClip; } }

        private void Reshape(int width, int height)
        {
            Gl.glViewport(0, 0, width, height);
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            Glu.gluPerspective(60.0, (float)width / (float)height, _NearClip, _FarClip);
            _FieldOfView = 60.0;
            _ScreenDistanceScreenCoords = (int)( (double)height / 2 / Math.Tan( 60.0 / 2 * Math.PI / 180 ) );
            LogFile.WriteLine( "_ScreenDistanceScreenCoords: " + _ScreenDistanceScreenCoords );
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
            Glu.gluLookAt(0.0, 0.0, 3.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0);
            //Gl.glShadeModel(Gl.GL_FLAT;
            //Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glPolygonMode(Gl.GL_BACK, Gl.GL_LINE);
        }
    }
}    
