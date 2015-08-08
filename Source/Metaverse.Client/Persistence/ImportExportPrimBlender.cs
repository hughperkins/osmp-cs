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
using System.Xml;
using System.Xml.Serialization; 
using System.IO;
using Metaverse.Utility;

namespace OSMP
{
    public class ImportExportPrimBlender : IWorldPersist
    {
        static ImportExportPrimBlender instance = new ImportExportPrimBlender();
        public static ImportExportPrimBlender GetInstance()
        {
            return instance;
        }
        
        public ImportExportPrimBlender()
        {
            //MenuController.GetInstance().RegisterMainMenu(new string[]{ "&File","&Save World..." }, new MainMenuCallback( SaveWorld ) );
            MenuController.GetInstance().RegisterMainMenu(new string[]{ "&File","&Load Prim Blender file..." }, new MainMenuCallback( LoadWorld ) );
            
            //ContextMenuController.GetInstance().RegisterPersistentContextMenu(new string[]{ "World","&Save to File..." }, new ContextMenuHandler( ContextMenuSave ) );
            ContextMenuController.GetInstance().RegisterPersistentContextMenu(new string[]{ "World","&Load Prim Blender file..." }, new ContextMenuHandler( ContextMenuLoad ) );
        }
        
        public void ContextMenuSave( object source, ContextMenuArgs e )
        {
            SaveWorld();
        }
        
        public void SaveWorld()
        {
            string filename = DialogHelpers.GetFilePath("Save Prim Blender file file", "world.PRIMS");

            if (filename != "")
            {
                LogFile.WriteLine(filename);
                Store(filename);
                DialogHelpers.ShowInfoMessageModal(null, "World export to prim blender completed");
            }
        }
        
        public void ContextMenuLoad( object source, ContextMenuArgs e )
        {
            LoadWorld();
        }
        
        public void LoadWorld()
        {
            string filename = DialogHelpers.GetFilePath("Open Prim Blender file file", ".PRIMS");

            if (filename != "")
            {
                LogFile.WriteLine(filename);
                Restore(filename);
                DialogHelpers.ShowInfoMessageModal(null, "World load from prim blender completed");
            }
        }
        
        public void Store( string filename )
        {
            WorldModel worldmodel = MetaverseClient.GetInstance().worldstorage;
            
            ArrayList types = new ArrayList();
            foreach( Entity entity in worldmodel.entities )
            {
                if( !types.Contains( entity.GetType() ) )
                {
                    types.Add( entity.GetType() );
                }
            }
            
            XmlSerializer serializer = new XmlSerializer( worldmodel.entities.GetType(), (Type[])types.ToArray( typeof( Type ) ) );
            StreamWriter streamwriter = new StreamWriter( filename );
            serializer.Serialize( streamwriter, worldmodel.entities );
        }
        
        // need to add a publisher/subscriber to this ;-)
        public void Restore( string filename )
        {
            WorldModel worldmodel = MetaverseClient.GetInstance().worldstorage;
            
            //// note to self: should make these types a publisher/subscriber thing
            //XmlSerializer serializer = new XmlSerializer( worldmodel.entities.GetType(), new Type[]{
             //   typeof( Avatar ),
              //  typeof( FractalSplineCylinder ), 
              //  //typeof( FractalSplineRing ), 
              //  typeof( FractalSplineBox )
               // } );            
            //FileStream filestream = new FileStream( filename, FileMode.Open );
            //worldmodel.entities = (EntityArrayList)serializer.Deserialize( filestream );                
/*                
            <type val="0" />
<position x="5.25000047684" y="7.75" z="8.75" />
<rotation x="0.707107126713" y="-0.707106411457" z="-1.1520197063e-007" s="6.65917468723e-007" />
<size x="4.75" y="10.0" z="0.10000000149" />
<cut x="0.0" y="1.0" />
<dimple x="0.0" y="1.0" />
<advancedcut x="0.0" y="1.0" />
<hollow val="0" />
<twist x="0" y="0" />
<topsize x="1.0" y="1.0" />
<holesize x="1.0" y="0.5" />
<topshear x="0.0" y="0.0" />
<taper x="0.0" y="0.0" />
<revolutions val="1.0" />
<radiusoffset val="0.0" />
<skew val="0.0" />
<material val="3" />
<hollowshape val="0" />
*/

            worldmodel.entities.Clear();
            XmlDocument primsdom = XmlHelper.OpenDom( filename );
            foreach( XmlElement primitiveelement in primsdom.DocumentElement.SelectNodes( "primitive" ) )
            {
                FractalSplineBox newbox = new FractalSplineBox();
                XmlElement propertieselement = primitiveelement.SelectSingleNode("properties" ) as XmlElement;
                newbox.pos = new Vector3( primitiveelement.SelectSingleNode("properties/position" ) as XmlElement );
                newbox.rot = new Rot( primitiveelement.SelectSingleNode("properties/rotation" ) as XmlElement );
                newbox.scale = new Vector3( primitiveelement.SelectSingleNode("properties/size" ) as XmlElement );
                newbox.Hollow = Convert.ToInt32( ( propertieselement.SelectSingleNode( "hollow" ) as XmlElement ).GetAttribute("val" ).ToString() );
                // newbox.Twist = Convert.ToInt32( propertieselement.SelectSingleNode( "twist" ) as XmlElement ).GetAttribute("x" ).ToString() );                
                newbox.TopSizeX = Convert.ToDouble( ( propertieselement.SelectSingleNode( "topsize" ) as XmlElement ).GetAttribute("x" ).ToString() );
                newbox.TopSizeY = Convert.ToDouble( ( propertieselement.SelectSingleNode( "topsize" ) as XmlElement ).GetAttribute("y" ).ToString() );                
                newbox.CutStart = (int)( Convert.ToDouble( ( propertieselement.SelectSingleNode( "cut" ) as XmlElement ).GetAttribute("x" ).ToString() ) * 200 ); 
                newbox.CutEnd = (int)( Convert.ToDouble( ( propertieselement.SelectSingleNode( "cut" ) as XmlElement ).GetAttribute("y" ).ToString() ) * 200 );    
                worldmodel.entities.Add( newbox );
            }
        }
    }
}
