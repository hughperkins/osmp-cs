// Copyright Hugh Perkins 2006
// hughperkins@gmail.com http://manageddreams.com
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
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization; 
using System.IO;
using Gtk;
using System.Net;
using Metaverse.Utility;

namespace OSMP
{
    public class WorldPersistToXml : IWorldPersist
    {
        // what we will save/load
        public class World
        {
            [Replicate]
            public Entity[] Entities;
            [Replicate]
            public TerrainModel TerrainModel;
        }

        static WorldPersistToXml instance = new WorldPersistToXml();
        public static WorldPersistToXml GetInstance()
        {
            return instance;
        }
        
        public WorldPersistToXml()
        {
            //MenuController.GetInstance().RegisterMainMenu(new string[]{ "&File","&Save World..." }, new MainMenuCallback( SaveWorld ) );
            //MenuController.GetInstance().RegisterMainMenu(new string[]{ "&File","&Load World..." }, new MainMenuCallback( LoadWorld ) );
            
            ContextMenuController.GetInstance().RegisterPersistentContextMenu(new string[]{ "World","&Save to File..." }, new ContextMenuHandler( ContextMenuSave ) );
            ContextMenuController.GetInstance().RegisterPersistentContextMenu(new string[]{ "World","&Load from File..." }, new ContextMenuHandler( ContextMenuLoad ) );
            ContextMenuController.GetInstance().RegisterPersistentContextMenu( new string[] { "World", "&Load from Url..." }, new ContextMenuHandler( ContextMenuLoadFromUrl ) );
        }
        
        public void ContextMenuSave( object source, ContextMenuArgs e )
        {
            SaveWorld();
        }
        
        public void SaveWorld()
        {
            string filename = DialogHelpers.GetFilePath("Save world file", "world.OSMP");

            if( filename != "" )
            {
                Console.WriteLine ( filename );
                Store( filename );
                DialogHelpers.ShowInfoMessageModal(null, "World save completed");
            }
        }
        
        public void ContextMenuLoad( object source, ContextMenuArgs e )
        {
            string filename = DialogHelpers.GetFilePath( "Open world file", "world.OSMP" );

            if (filename != "")
            {
                Load( filename );
            }
        }

        public void ContextMenuLoadFromUrl( object source, ContextMenuArgs e )
        {
            new InputBox( "Please enter URL:", new InputBox.Callback( LoadFromUrl ) );
        }

        public void LoadFromUrl( string url )
        {
            if (url == "")
            {
                return;
            }

            Uri projecturi = new Uri( new Uri( url ), "." );

            LogFile.WriteLine( "loading [" + url + "] ..." );

            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create( url );
            HttpWebResponse httpwebresponse = (HttpWebResponse)myReq.GetResponse();
            Stream stream = httpwebresponse.GetResponseStream();
            
            Restore( stream, projecturi );
            stream.Close();
            httpwebresponse.Close();

            DialogHelpers.ShowInfoMessageModal( null, "World structure load completed; textures may continue to load in the background." );
        }

        /// <summary>
        /// filename or url (url starts with http://)
        /// </summary>
        /// <param name="filename"></param>
        public void Load( string filename )
        {
            LogFile.WriteLine( filename );
            if (filename.StartsWith( "http:" ))
            {
                LoadFromUrl( filename );
            }
            else
            {
                LoadFromFile( filename );
            }
        }

        public void LoadFromFile( string filename )
        {
            if (filename == "" || !File.Exists( filename ))
            {
                return;
            }
            Restore( filename );
            //DialogHelpers.ShowInfoMessageModal( null, "World load completed" );
            new MessageBox( MessageBox.MessageType.Info, "World load completed", "World load completed", new MessageBox.Callback( CallbackTest ) );
        }

        void CallbackTest()
        {
            LogFile.WriteLine( "messagebox closed" );
        }

        public void Store( string filename )
        {
            LogFile.WriteLine( "store " + filename );
            WorldModel worldmodel = MetaverseClient.GetInstance().worldstorage;
            
            ArrayList types = new ArrayList();
            foreach( Entity entity in worldmodel.entities )
            {
                if( !types.Contains( entity.GetType() ) )
                {
                    types.Add( entity.GetType() );
                }
            }
            
            //XmlSerializer serializer = new XmlSerializer( worldmodel.entities.GetType(), (Type[])types.ToArray( typeof( Type ) ) );
            //XmlSerializer serializer = new XmlSerializer( typeof( World),
                //(Type[])types.ToArray( typeof( Type ) ) );
            //StreamWriter streamwriter = new StreamWriter( filename );
            ProjectFileController.GetInstance().SetProjectPath( new Uri( Path.GetDirectoryName( filename ) + "/" ) );
            List<Entity> entitiestoserialize = new List<Entity>();
            foreach (Entity entity in worldmodel.entities)
            {
                // note to self: need to check root entity in fact
                // doesnt matter yet because no linking
                if (entity.GetType() != typeof(Avatar))
                {
                    entitiestoserialize.Add(entity);
                }
            }
            World world = new World();
            world.Entities = entitiestoserialize.ToArray();
            world.TerrainModel = MetaverseClient.GetInstance().worldstorage.terrainmodel;

            StringWriter stringwriter = new StringWriter();
            OsmpXmlSerializer.GetInstance().RegisterType( typeof( World ) );
            OsmpXmlSerializer.GetInstance().Serialize( stringwriter, world );
            stringwriter.Close();

            StreamWriter sw = new StreamWriter( filename, false );
            sw.WriteLine( stringwriter.ToString() );
            sw.Close();
            //serializer.Serialize( streamwriter, world );
            //streamwriter.Close();
        }

        public void Restore( string filename )
        {
            Uri projecturi = new Uri( Path.GetDirectoryName( filename ) + "/" );
            FileStream filestream = new FileStream( filename, FileMode.Open );
            Restore( filestream, projecturi );
            filestream.Close();
        }

        public void Restore( Stream stream, Uri projecturi )
        //public void Restore( StringReader stringreader, Uri projecturi )
        {
            WorldModel worldmodel = MetaverseClient.GetInstance().worldstorage;

            // note to self: should make these types a publisher/subscriber thing
            //XmlSerializer serializer = new XmlSerializer( typeof( World ), new Type[]{
              //  typeof( Avatar ),
//                typeof( FractalSplineCylinder ), 
  //              typeof( FractalSplineRing ), 
    //            typeof( FractalSplineBox ),
      //          typeof( FractalSplineTorus ),
        //        typeof( FractalSplinePrism ),
          //      typeof( FractalSplineTube )
            //    } );
            //DialogHelpers.ShowInfoMessage( null, serializer.Deserialize(filestream).GetType().ToString());
            ProjectFileController.GetInstance().SetProjectPath( projecturi );

            StreamReader sr = new StreamReader( stream );
            string contents = sr.ReadToEnd();
            sr.Close();
            StringReader stringreader = new StringReader( contents );
            OsmpXmlSerializer.GetInstance().RegisterType( typeof( World ) );
            World world = (World)OsmpXmlSerializer.GetInstance().Deserialize( stringreader );
            stringreader.Close();
            //World world = (World)serializer.Deserialize( stream );

            worldmodel.Clear();
            foreach (Entity entity in world.Entities)
            {
                if (entity.GetType() != typeof(Avatar))
                {
                    LogFile.WriteLine("WorldPersistToXml, restoring: " + entity);
                    worldmodel.AddEntity(entity);
                }
            }
            if( world.TerrainModel.HeightmapFilename != "" )
            {
                HeightMapPersistence.GetInstance().Load( world.TerrainModel.HeightmapFilename );
            }
            worldmodel.terrainmodel.texturestagesarray = world.TerrainModel.texturestagesarray;
            LogFile.WriteLine( worldmodel );
            worldmodel.terrainmodel.MinHeight = world.TerrainModel.MinHeight;
            worldmodel.terrainmodel.MaxHeight = world.TerrainModel.MaxHeight;
            worldmodel.terrainmodel.OnTerrainModified();
        }
    }
}
