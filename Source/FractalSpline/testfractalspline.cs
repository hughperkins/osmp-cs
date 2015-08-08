// Copyright Hugh Perkins 2005,2006
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

#define BOXES
#define PRISMS 
#define CYLINDERS
#define TUBES
#define RINGS
#define TORUSES

using System;
using System.Threading;
using System.Drawing;

using Tao.Sdl;
using Tao.OpenGl;
using SdlDotNet;
using FractalSpline;

namespace TestFractalSpline
{
    class TestFractalSpline
    {
        int iWindowHeight, iWindowWidth;        
        int mousex, mousey;
        int[] TextureIds = new int[15];
        
        double xoffset, yoffset;
        
        bool LoadGLTextures()           // Load Bitmaps And Convert To Textures
        {        
            for( int i = 0; i <= 9; i++ )
            {
                string Filename = "data\\" + i.ToString() + ".tga";
                Console.WriteLine( "Loading " + Filename + " ... " );
                Bitmap bitmap = DevIL.DevIL.LoadBitmap( Filename );
                if( bitmap == null )
                {
                    Console.WriteLine( "Error loading " + Filename );
                    System.Environment.Exit(1);
                }
                TextureIds[i] = TextureHelper.LoadBitmapToOpenGl( bitmap );
            }
        
            TextureIds[10] = TextureHelper.LoadBitmapToOpenGl( DevIL.DevIL.LoadBitmap("data\\osmpico32.bmp" ) );
            TextureIds[11] = TextureHelper.LoadBitmapToOpenGl( DevIL.DevIL.LoadBitmap("data\\test.jpg" ) );
            TextureIds[12] = TextureHelper.LoadBitmapToOpenGl( DevIL.DevIL.LoadBitmap( "data\\Compressed.tga" ) );
            TextureIds[13] = TextureHelper.LoadBitmapToOpenGl( DevIL.DevIL.LoadBitmap( "data\\Background.tga" ) );
            TextureIds[14] = TextureHelper.LoadBitmapToOpenGl( DevIL.DevIL.LoadBitmap( "data\\USMC.pcx" ) );
        
            Console.WriteLine( "EndLoadGLTextures()" );
            return true;            // Return The Status
        }
        
        IRenderer renderer;
        
        #if BOXES
        FractalSpline.Box mybox;
        FractalSpline.Box mybox2;
        FractalSpline.Box mybox3;
        FractalSpline.Box mybox4;
        #endif

        #if CYLINDERS
        FractalSpline.Cylinder cylinder;
        FractalSpline.Cylinder cylinder2;
        FractalSpline.Cylinder cylinder3;
        #endif
        
        #if PRISMS
        FractalSpline.Prism prism;
        FractalSpline.Prism prism2;
        FractalSpline.Prism prism3;
        #endif
        
        #if TORUSES
        FractalSpline.Torus torus;
        FractalSpline.Torus torus2;
        FractalSpline.Torus torus3;
        #endif
        
        #if RINGS
        FractalSpline.Ring ring;
        FractalSpline.Ring ring2;
        FractalSpline.Ring ring3;
        #endif
        
        #if TUBES
        FractalSpline.Tube tube;
        FractalSpline.Tube tube2;
        FractalSpline.Tube tube3;
        #endif
        
        void SetupWorld()
        {
            renderer = new RendererOpenGl();
            Console.WriteLine( renderer );
        #if BOXES
            mybox = new FractalSpline.Box( RendererOpenGl.GetInstance() );
            mybox2 = new FractalSpline.Box( RendererOpenGl.GetInstance() );
            mybox3 = new FractalSpline.Box( RendererOpenGl.GetInstance() );
            mybox4 = new FractalSpline.Box( RendererOpenGl.GetInstance() );
            
            for ( int i = 0; i < 9; i++ )
            {
                mybox.SetTexture( i, TextureIds[i] );
            }
            mybox.Hollow = 70;
            mybox.CutStart = 15;
            mybox.CutEnd = 185;
            mybox.UpdateTransforms();
            
            mybox2.SetTexture(FractalSpline.Primitive.AllFaces, TextureIds[5] );
            mybox2.CutStart = 20;
            mybox2.CutEnd = 175;
            mybox2.Twist = 30;
            mybox2.Shear = 0.3;
            //mybox2.LevelOfDetail =4;
            mybox2.Hollow = 30;
            mybox2.UpdateTransforms();
            
            mybox3.SetTexture(FractalSpline.Primitive.AllFaces, TextureIds[5] );
            mybox3.TextureOffset = new double[]{ 0.25, 0.25 };
            mybox3.TextureScale = new double[]{ 0.5, 0.5 };
            mybox3.TextureRotate = 80;
            mybox3.Twist = 30;
            mybox3.Shear = 0.3;
            mybox3.UpdateTransforms();

            mybox4.SetTexture(FractalSpline.Primitive.AllFaces, TextureIds[5] );
            mybox4.Hollow = 70;
            mybox4.TextureOffset = new double[]{ 0.25, 0.25 };
            mybox4.TextureScale = new double[]{ 0.5, 0.5 };
            mybox4.TextureRotate = 80;
            mybox4.UpdateTransforms();
            #endif

                #if CYLINDERS
            cylinder = new FractalSpline.Cylinder( RendererOpenGl.GetInstance() );
            cylinder.SetTexture(FractalSpline.Primitive.AllFaces, TextureIds[5] );
            
            cylinder2 = new FractalSpline.Cylinder( RendererOpenGl.GetInstance() );
            cylinder2.SetTexture(FractalSpline.Primitive.AllFaces, TextureIds[5] );
            cylinder2.CutStart = 20;
            cylinder2.CutEnd = 185;
            cylinder2.Twist = 45;
            cylinder2.Shear = 0.3;
            //cylinder2.LevelOfDetail =4;
            cylinder2.Hollow = 30;
            cylinder2.UpdateTransforms();
            
            cylinder3 = new FractalSpline.Cylinder( RendererOpenGl.GetInstance() );
            cylinder3.SetTexture(FractalSpline.Primitive.AllFaces, TextureIds[5] );
            cylinder3.Twist = 30;
            cylinder3.Shear = 0.2;
            cylinder3.TextureRotate = 20;
            cylinder3.UpdateTransforms();
            #endif

            #if PRISMS
            prism = new FractalSpline.Prism( renderer );
            for ( int i = 0; i < 9; i++ )
            {
                prism.SetTexture( i, TextureIds[i] );
            }
            prism.TextureRotate = 10;
            
            prism2 = new FractalSpline.Prism( renderer );
            prism2.CutStart = 100;
            prism2.CutEnd = 175;
            prism2.Twist = 90;
            //prism2.LevelOfDetail =4;
            prism2.Hollow = 30;
            prism2.UpdateTransforms();
            prism2.SetTexture(FractalSpline.Primitive.AllFaces, TextureIds[5] );
            prism2.TextureRotate = 30;

            prism3 = new FractalSpline.Prism( renderer );
            prism3.SetTexture(FractalSpline.Primitive.AllFaces, TextureIds[5] );
            prism3.TextureRotate = 30;
            #endif

            #if TUBES
            Console.WriteLine( "tube");
            tube = new FractalSpline.Tube( renderer );
            tube.SetTexture(FractalSpline.Primitive.AllFaces, TextureIds[5] );
            tube.TextureRotate = 30;
            
            Console.WriteLine( "tube2");
            tube2 = new FractalSpline.Tube( renderer );
            tube2.SetTexture(FractalSpline.Primitive.AllFaces, TextureIds[5] );
            tube2.CutStart = 100;
            tube2.CutEnd = 175;
            tube2.Twist = 90;
            //prism2.LevelOfDetail =4;
            tube2.Hollow = 30;
            tube2.HoleSize = 30;
            tube2.AdvancedCutStart = 10;
            tube2.AdvancedCutEnd = 80;
            tube2.TextureRotate = 30;
            tube2.UpdateTransforms();
            
            Console.WriteLine( "tube3");
            tube3 = new FractalSpline.Tube( renderer );
            tube3.SetTexture(FractalSpline.Primitive.AllFaces, TextureIds[5] );
            tube3.TextureRotate = 30;
            #endif
            
            #if RINGS
            ring = new FractalSpline.Ring( renderer );
            ring.SetTexture(FractalSpline.Primitive.AllFaces, TextureIds[5] );
            ring.TextureRotate = 30;
            
            ring2 = new FractalSpline.Ring( renderer );
            ring2.CutStart = 100;
            ring2.CutEnd = 175;
            ring2.Twist = 90;
            //prism2.LevelOfDetail =4;
            ring2.Hollow = 30;
            ring2.HoleSize = 30;
            ring2.AdvancedCutStart = 10;
            ring2.AdvancedCutEnd = 80;
            ring2.UpdateTransforms();
            ring2.SetTexture(FractalSpline.Primitive.AllFaces, TextureIds[5] );
            ring2.TextureRotate = 30;

            ring3 = new FractalSpline.Ring( renderer );
            ring3.SetTexture(FractalSpline.Primitive.AllFaces, TextureIds[5] );
            ring3.TextureRotate = 30;
            #endif
            
            #if TORUSES
            torus = new FractalSpline.Torus( renderer );
            torus.SetTexture(FractalSpline.Primitive.AllFaces, TextureIds[5] );
            torus.TextureRotate = 30;
            
            torus2 = new FractalSpline.Torus( renderer );
            torus2.CutStart = 100;
            torus2.CutEnd = 175;
            torus2.Twist = 90;
            //prism2.LevelOfDetail =4;
            torus2.Hollow = 30;
            torus2.HoleSize = 30;
            torus2.AdvancedCutStart = 10;
            torus2.AdvancedCutEnd = 80;
            torus2.UpdateTransforms();
            torus2.SetTexture(FractalSpline.Primitive.AllFaces, TextureIds[5] );
            torus2.TextureRotate = 30;
            
            torus3 = new FractalSpline.Torus( renderer );
            torus3.SetTexture(FractalSpline.Primitive.AllFaces, TextureIds[5] );
            torus3.TextureRotate = 30;
            #endif
        }
                
        float zRot = 0;
        float yRot = 0;
        
        int iThisFace = 1;
        int iFrameCount = 0;
        
        void DrawWorld()
        {
            Gl.glPushMatrix();
            
            Gl.glRotatef( zRot, 0, 0, 1 );
            Gl.glRotatef( yRot, 0, 1, 0 );

            Gl.glTranslated( xoffset, yoffset, 0 );
            
            #if BOXES
            Gl.glPushMatrix();
            mybox.RenderSingleFace(iThisFace);
            
            Gl.glTranslated( 1.5, 0, 0 );
            mybox2.Render();
            
            Gl.glTranslated( 1.5, 0, 0 );
            mybox3.Render();
            
            Gl.glTranslated( 1.5, 0, 0 );
            mybox4.Render();
            
            Gl.glPopMatrix();
            Gl.glTranslated( 0, 1.5, 0 );
            #endif
            
            #if PRISMS
            Gl.glPushMatrix();
            prism.Render();
            
            Gl.glTranslated( 1.5, 0, 0 );
            prism2.Render();
            
            Gl.glTranslated( 1.5, 0, 0 );
            prism3.Render();
            
            Gl.glPopMatrix();
            Gl.glTranslated( 0, 1.5, 0 );
            #endif

            #if CYLINDERS
            Gl.glPushMatrix();
            cylinder.Render();
            
            Gl.glTranslated( 1.5, 0, 0 );
            cylinder2.Render();
            
            Gl.glTranslated( 1.5, 0, 0 );
            cylinder3.Render();
            
            Gl.glPopMatrix();
            Gl.glTranslated( 0, 1.5, 0 );
            #endif
            
            #if TUBES
            Gl.glPushMatrix();
            tube.Render();

            Gl.glTranslated( 1.5, 0, 0 );
            tube2.Render();

            Gl.glTranslated( 1.5, 0, 0 );
            tube3.Render();
            
            Gl.glPopMatrix();
            Gl.glTranslated( 0, 1.5, 0 );
            #endif

            #if RINGS
            Gl.glPushMatrix();
            ring.Render();

            Gl.glTranslated( 1.5, 0, 0 );
            ring2.Render();

            Gl.glTranslated( 1.5, 0, 0 );
            ring3.Render();
            
            Gl.glPopMatrix();
            Gl.glTranslated( 0, 1.5, 0 );
            #endif

            #if TORUSES
            Gl.glPushMatrix();
            torus.Render();

            Gl.glTranslated( 1.5, 0, 0 );
            torus2.Render();

            Gl.glTranslated( 1.5, 0, 0 );
            torus3.Render();
            
            Gl.glPopMatrix();
            Gl.glTranslated( 0, 1.5, 0 );
            #endif

            Gl.glPopMatrix();
        }
        
        void SetColor( float r, float g, float b )
        {
            Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_AMBIENT_AND_DIFFUSE, new float[]{ r,g,b,1.0f});
        }
        
        void WriteNextFrame()
        {
            Gl.glClear( Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT );
        
            Gl.glLoadIdentity();
        
            // rotate so z axis is up, and x axis is forward
            Gl.glRotatef( 90f, 0.0f, 0.0f, 1.0f );
            Gl.glRotatef( 90f, 0.0f, 1.0f, 0.0f );
        
            Gl.glRotatef( 0f, 0f, 1f, 0f );
            Gl.glRotatef( 0f, 0f, 0f, 1f );
        
            Gl.glTranslatef( 5f, 0f, 0f );
        
            Gl.glPushMatrix();
            
            DrawWorld();
            
            Gl.glPopMatrix();
        
            Video.GLSwapBuffers( );
        }
        
        void Quit( object source, QuitEventArgs e )
        {
            System.Environment.Exit(0);
        }
        
        void MouseDown( object source, MouseButtonEventArgs e )
        {
            if( e.Button == MouseButton.PrimaryButton )
            {
                mousex = e.X;
                mousey = e.Y;
            }
        }
        
        void MouseMotion( object source, MouseMotionEventArgs e )
        {
            if( e.Button == MouseButton.PrimaryButton )
            {
                zRot += (e.X - mousex);
                yRot += (e.Y - mousey);
                mousex = e.X;
                mousey = e.Y;
            }
        }
        
        void MouseUp( object source, MouseButtonEventArgs e )
        {
        }
        
        bool bleftdown;
        bool brightdown;
        bool bupdown;
        bool bdowndown;
        
        void KeyDown( object source, KeyboardEventArgs e )
        {
            if( (char)e.Key == 'q' || e.Key == Key.Escape )
            {
                System.Environment.Exit(0);
            }
            if( (char)e.Key == 'a' )
            {
                bleftdown = true;
            }
            if( (char)e.Key == 'd' )
            {
                brightdown = true;
            }
            if( (char)e.Key == 'w' )
            {
                bupdown = true;
            }
            if( (char)e.Key == 's' )
            {
                bdowndown = true;
            }
        }
        
        void KeyUp( object source, KeyboardEventArgs e )
        {
            if( (char)e.Key == 'a' )
            {
                bleftdown = false;
            }
            if( (char)e.Key == 'd' )
            {
                brightdown = false;
            }
            if( (char)e.Key == 'w' )
            {
                bupdown = false;
            }
            if( (char)e.Key == 's' )
            {
                bdowndown = false;
            }
        }
        
        void MainLoop()
        {
            while( true )
            {
                if( bleftdown )
                {
                    yoffset -= 0.1;
                }
                else if( brightdown )
                {
                    yoffset += 0.1;
                }
                else if( bupdown )
                {
                    xoffset -= 0.1;
                }
                else if( bdowndown )
                {
                    xoffset += 0.1;
                }
            
                iFrameCount++;
                if( iFrameCount > 50 )
                {
                    iThisFace = ( iThisFace + 1 ) % 10;
                    iFrameCount = 0;
                }
                WriteNextFrame();
                while( Events.Poll() )
                {
                }
            }
        }
        
        public void Go()
        {
            iWindowWidth = 800;
            iWindowHeight = 600;
            
            Console.WriteLine(  "requested window width/height: " + iWindowWidth.ToString() + " " + iWindowHeight.ToString() ); // Console.WriteLine
            
            Video.SetVideoModeWindowOpenGL( iWindowWidth, iWindowHeight );
            //Video.SetVideoModeOpenGL(iWindowWidth, iWindowHeight, FullScreenBitsPerPixel);
            
            Video.WindowCaption = "TestFractalSpline";
            
            Gl.glClearColor(0.0f,0.0f,0.0f,0.0f);
            
            Gl.glEnable(Gl.GL_TEXTURE_2D);
        
            LoadGLTextures();
        
            SetupWorld();
        
            Gl.glShadeModel(Gl.GL_SMOOTH);
        
            Gl.glClearColor(0.0f,0.0f,0.0f,0.0f);
        
            Gl.glEnable(Gl.GL_DEPTH_TEST);
            //Gl.glEnable (Gl.GL_CULL_FACE);
        
            Gl.glEnable(Gl.GL_LIGHTING);
            Gl.glEnable(Gl.GL_LIGHT0);
        
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_AMBIENT, new float[]{0.5f, 0.5f, 0.5f, 1.0f});
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_DIFFUSE, new float[]{0.8f, 0.8f, 0.8f, 1.0f});
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_SPECULAR, new float[]{0.5f, 0.5f, 0.5f, 1.0f});
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_POSITION, new float[]{ -1.5f, 1.0f, -4.0f, 1.0f});
        
            Gl.glLoadIdentity();
        
            Gl.glMatrixMode( Gl.GL_PROJECTION );
            Gl.glLoadIdentity();
            float aspect = (float) iWindowWidth / iWindowHeight;
            Glu.gluPerspective( 45.0f, aspect, 0.5f, 100.0f );
        
            Gl.glMatrixMode( Gl.GL_MODELVIEW );
            Gl.glViewport (0, 0, iWindowWidth, iWindowHeight);
        
            Events.Quit += new QuitEventHandler (this.Quit);
            Events.MouseButtonDown +=  new MouseButtonEventHandler( this.MouseDown );
            Events.MouseMotion +=  new MouseMotionEventHandler( this.MouseMotion );
            Events.MouseButtonUp +=  new MouseButtonEventHandler( this.MouseUp );
            Events.KeyboardDown +=  new KeyboardEventHandler( this.KeyDown );
            Events.KeyboardUp +=  new KeyboardEventHandler( this.KeyUp );
            
            MainLoop();
        }
    }
    
    class EntryPoint
    {
        public static void Main()
        {
            try{
                new TestFractalSpline().Go();
            }catch( Exception e )
            {
                Console.WriteLine( e );
            }
        }
    }    
}    
    
