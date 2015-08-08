using System;
using System.Collections.Generic;
using System.Text;
using Tao.OpenGl;

namespace OSMP
{
    // renders the minimap
    public class RenderableMinimap
    {
        TerrainModel parent;

        public delegate void RenderHandler( int minimapleft, int minimaptop, int minimapwidth, int minimapheight );
        /// <summary>
        /// Called on render, *** we are in Ortho mode on entry to this call ***
        /// </summary>
        public event RenderHandler Render;

        int minimapsize = 128; // get this from config perhaps?  Note: this represents largest dimension, out of width or height, for minimap
                               // *** should be a power of 2 *** (at least for now).  This is so minimap texture caching works correctly
        int minimapx = 10;
        int minimapy = 10;

        int numchunks = 16; // determines quality of normal rendering

        double viewdirectionlinelength = 5; // note to self: this will be replaced by true frustrum later

        RenderableHeightMap renderableheightmap;

        int minimaptexture;

        public RenderableMinimap(TerrainModel parent, RenderableHeightMap renderableheightmap ) // note could upgrade this to use Isomething later, but Terrain works for now
        {
            this.parent = parent;
            this.renderableheightmap = renderableheightmap;

            Gl.glGenTextures(1, out minimaptexture);

            RendererSdl.GetInstance().WriteNextFrameEvent += new WriteNextFrameCallback(RenderableMinimap_WriteNextFrameEvent);
        }

        DateTime LastMinimapUpdate;
        void RenderableMinimap_WriteNextFrameEvent(Vector3 camerapos)
        {
            //Console.WriteLine( "RenderableMinimap_WriteNextFrameEvent" );
            GetDimensions();

            GraphicsHelperGl g = new GraphicsHelperGl();
            g.CheckError();
            //LogFile.WriteLine( windowwidth + " " + windowheight + " " + RendererSdl.GetInstance().OuterWindowWidth + " " + RendererSdl.GetInstance().OuterWindowHeight );
            g.ApplyOrtho( windowwidth, windowheight, RendererSdl.GetInstance().OuterWindowWidth, RendererSdl.GetInstance().OuterWindowHeight );
            g.CheckError();
            DrawMinimap();
            g.CheckError();
            DrawFrustrum(camerapos);
            g.CheckError();
            if (Render != null)
            {
                Render(minimapx, minimapy, minimapwidth, minimapheight);
            }
            g.CheckError();
            g.RemoveOrtho();
            g.CheckError();
        }

        int windowwidth;
        int windowheight;
        int mapwidth;
        int mapheight;
        int minimapwidth;
        int minimapheight;

        void GetDimensions()
        {
            TerrainModel terrain = MetaverseClient.GetInstance().worldstorage.terrainmodel;

            windowwidth = RendererFactory.GetInstance().WindowWidth;
            windowheight = RendererFactory.GetInstance().WindowHeight;

            mapwidth = terrain.MapWidth;
            mapheight = terrain.MapHeight;

            double mapheightwidthratio = terrain.MapHeight / terrain.MapWidth;
            minimapwidth = 0;
            minimapheight = 0;
            if (mapheightwidthratio > 1)
            {
                minimapheight = minimapsize;
                minimapwidth = (int)(minimapsize / mapheightwidthratio);
            }
            else
            {
                minimapwidth = minimapsize;
                minimapheight = (int)(minimapsize * mapheightwidthratio);
            }
        }

        // note to self: move this to subscriber?
        void DrawFrustrum(Vector3 camerapos)
        {
            FrustrumCulling frustrum = FrustrumCulling.GetInstance();
            Vector2 cameraposonmap = new Vector2(camerapos.x * minimapwidth / mapwidth,
                camerapos.y * minimapheight / mapheight ); // ignore z
            Vector2 direction = new Vector2(frustrum.viewray.x, frustrum.viewray.y); // ignore z
            Gl.glDisable(Gl.GL_LIGHTING);
            Gl.glDisable(Gl.GL_CULL_FACE);
            GraphicsHelperGl g = new GraphicsHelperGl();
            g.DisableTexture2d();
            //g.SetColor(0, 0, 1);
            //g.SetMaterialColor(new Color(0, 0, 1));
            Gl.glColor3ub(0, 255, 0);
            Gl.glDepthFunc(Gl.GL_ALWAYS);

            Gl.glBegin(Gl.GL_LINES);
            Gl.glVertex2d(minimapx + cameraposonmap.x, minimapy + cameraposonmap.y);
            Gl.glVertex2d(minimapx + cameraposonmap.x + direction.x * viewdirectionlinelength, minimapy + cameraposonmap.y + direction.y * viewdirectionlinelength);
            Gl.glEnd();

            Gl.glEnable(Gl.GL_LIGHTING);
            Gl.glDepthFunc(Gl.GL_LEQUAL);
            Gl.glColor3ub(255, 255, 255);
        }

        // note to self: move this to subscriber?
        void DrawMinimap()
        {
            TerrainModel terrain = MetaverseClient.GetInstance().worldstorage.terrainmodel;

            if (DateTime.Now.Subtract(LastMinimapUpdate).TotalMilliseconds > 1000)
            //if( true )
            {
                List<RendererPass> rendererpasses = new List<RendererPass>();
                bool multipass = true; // force multipass for now for simplicity
                int maxtexels = RendererSdl.GetInstance().MaxTexelUnits;
                if (multipass)
                {
                    for (int i = 0; i < terrain.texturestages.Count; i++)
                    {
                        MapTextureStageModel maptexturestage = terrain.texturestages[i];
                        int numtexturestagesrequired = maptexturestage.NumTextureStagesRequired;
                        if (numtexturestagesrequired > 0) // exclude Nops
                        {
                            RendererPass rendererpass = new RendererPass(maxtexels);
                            for (int j = 0; j < maptexturestage.NumTextureStagesRequired; j++)
                            {
                                rendererpass.AddStage(new RendererTextureStage(maptexturestage, j, true, mapwidth, mapheight));
                            }
                            rendererpasses.Add(rendererpass);
                        }
                    }
                }

                GraphicsHelperGl g = new GraphicsHelperGl();

                //g.ApplyOrtho(windowwidth, windowheight, RendererSdl.GetInstance().OuterWindowWidth, RendererSdl.GetInstance().OuterWindowHeight);

                g.EnableBlendSrcAlpha();
                Gl.glDepthFunc(Gl.GL_LEQUAL);

                int chunkwidth = minimapwidth / numchunks;
                int chunkheight = minimapheight / numchunks;

                float[] ambientLight = new float[] { 0.4f, 0.4f, 0.4f, 1.0f };
                float[] diffuseLight = new float[] { 0.6f, 0.6f, 0.6f, 1.0f };
                float[] specularLight = new float[] { 0.2f, 0.2f, 0.2f, 1.0f };
                float[] position = new float[] { -1.0f, 0.2f, -0.4f, 1.0f };

                Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_AMBIENT, ambientLight);
                Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_DIFFUSE, diffuseLight);
                Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_SPECULAR, specularLight);
                Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_POSITION, position);

                foreach (RendererPass rendererpass in rendererpasses)
                {
                    rendererpass.Apply();

                    for (int x = 0; x + chunkwidth < minimapwidth; x += chunkwidth)
                    {
                        for (int y = 0; y + chunkheight < minimapheight; y += chunkheight)
                        {
                            Gl.glBegin(Gl.GL_QUADS);

                            //double ul = 0;
                            //double ur = mapwidth * Terrain.SquareSize;
                            //double vt = 0;
                            //double vb = mapheight * Terrain.SquareSize;
                            double ul = (double)x / minimapwidth * mapwidth;
                            double ur = (double)(x + chunkwidth) / minimapwidth * mapwidth;
                            double vt = (double)y / minimapheight * mapheight;
                            double vb = (double)(y + chunkheight) / minimapheight * mapheight;

                            double xl = minimapx + x;
                            double xr = minimapx + x + minimapwidth / (double)numchunks;
                            double yt = minimapy + y;
                            double yb = minimapy + y + minimapheight / (double)numchunks;

                            Gl.glTexCoord2d(ul, vt);
                            Gl.glMultiTexCoord2dARB(Gl.GL_TEXTURE1_ARB, ul, vt);
                            g.Normal(renderableheightmap.GetNormal(x * mapwidth / minimapwidth, y * mapheight / minimapheight));
                            //g.Normal(renderableheightmap.normalsperquad[, ]);
                            Gl.glVertex2d(xl, yt);

                            Gl.glTexCoord2d(ul, vb);
                            Gl.glMultiTexCoord2dARB(Gl.GL_TEXTURE1_ARB, ul, vb);
                            g.Normal(renderableheightmap.GetNormal(x * mapwidth / minimapwidth, (y + chunkheight) * mapheight / minimapheight));
                            //g.Normal( renderableheightmap.normalsperquad[x * mapwidth / minimapwidth, (y + 1) * mapheight / minimapheight ] );
                            Gl.glVertex2d(xl, yb);

                            Gl.glTexCoord2d(ur, vb);
                            Gl.glMultiTexCoord2dARB(Gl.GL_TEXTURE1_ARB, ur, vb);
                            g.Normal(renderableheightmap.GetNormal((x + chunkwidth) * mapwidth / minimapwidth, (y + chunkheight) * mapheight / minimapheight));
                            //g.Normal(renderableheightmap.normalsperquad[(x + 1) * mapwidth / minimapwidth, (y + 1) * mapheight / minimapheight]);
                            Gl.glVertex2d(xr, yb);

                            Gl.glTexCoord2d(ur, vt);
                            Gl.glMultiTexCoord2dARB(Gl.GL_TEXTURE1_ARB, ur, vt);
                            g.Normal(renderableheightmap.GetNormal((x + chunkwidth) * mapwidth / minimapwidth, y * mapheight / minimapheight));
                            //g.Normal(renderableheightmap.normalsperquad[(x + 1) * mapwidth / minimapwidth, y * mapheight / minimapheight]);
                            Gl.glVertex2d(xr, yt);

                            Gl.glEnd();
                        }
                    }
                }

                g.ActiveTexture(0);
                Gl.glBindTexture(Gl.GL_TEXTURE_2D, minimaptexture);
                Gl.glCopyTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGBA8, minimapx, 
                    RendererSdl.GetInstance().WindowHeight - minimapy - minimapsize, 
                    minimapsize, minimapsize, 0);
                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST);
                LastMinimapUpdate = DateTime.Now;

//                g.RemoveOrtho();

                g.ActiveTexture(1);
                g.DisableTexture2d();
                g.SetTextureScale(1);
                g.ActiveTexture(0);
                g.SetTextureScale(1);

                g.EnableModulate();

                Gl.glDisable(Gl.GL_BLEND);
            }
            else
            {
                GraphicsHelperGl g = new GraphicsHelperGl();

                //Gl.glMatrixMode(Gl.GL_PROJECTION);
                //Gl.glPushMatrix();
                //Gl.glLoadIdentity();
                //Gl.glOrtho(0, windowwidth, windowheight - RendererSdl.GetInstance().OuterWindowHeight, 0, -1, 1); // we'll just draw the minimap directly onto our display
                //Gl.glOrtho(0, windowwidth, windowheight, windowheight - RendererSdl.GetInstance().OuterWindowHeight, -1, 1); // we'll just draw the minimap directly onto our display

                //Gl.glMatrixMode(Gl.GL_MODELVIEW);
                //Gl.glPushMatrix();
                //Gl.glLoadIdentity();

                g.ActiveTexture(0);
                g.EnableTexture2d();
                Gl.glBindTexture(Gl.GL_TEXTURE_2D, minimaptexture);
                //Gl.glBindTexture(Gl.GL_TEXTURE_2D, (terrain.texturestages[0].texture as GlTexture).GlReference);
                Gl.glDisable(Gl.GL_LIGHTING);
                Gl.glBegin(Gl.GL_QUADS);

                Gl.glTexCoord2d(0, 1);
                Gl.glVertex2i(minimapx, minimapy);

                Gl.glTexCoord2d(0, 1 - minimapwidth / (double)minimapsize);
                Gl.glVertex2i(minimapx, minimapy + minimapheight);

                Gl.glTexCoord2d(minimapwidth / (double)minimapsize, 1 - minimapheight / (double)minimapsize);
                Gl.glVertex2i(minimapx + minimapwidth, minimapy + minimapheight);

                Gl.glTexCoord2d(minimapwidth / (double)minimapsize, 1);
                Gl.glVertex2i(minimapx + minimapwidth, minimapy);

                Gl.glEnd();

                Gl.glEnable(Gl.GL_LIGHTING);

                //Gl.glMatrixMode(Gl.GL_PROJECTION);
                //Gl.glPopMatrix();
                //Gl.glMatrixMode(Gl.GL_MODELVIEW);
                //Gl.glPopMatrix();
            }
        }
    }
}
