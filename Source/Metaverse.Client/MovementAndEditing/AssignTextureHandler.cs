// Copyright Hugh Perkins 2006
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

using System;
using System.Collections;
using System.IO;
using Metaverse.Utility;

namespace OSMP
{
    // This class hooks into the gui, providing an Assign Texture... option to the context menu.  Yay!
    public class AssignTextureHandler
    {
        static AssignTextureHandler instance = new AssignTextureHandler(); // instantiate handler
        public static AssignTextureHandler GetInstance()
        {
            return instance;
        }
        
        public AssignTextureHandler()
        {
            LogFile.WriteLine("instantiating AssignTextureHandler" );
            ContextMenuController.GetInstance().ContextMenuPopup += new ContextMenuHandler( ContextMenuPopup );
        }
        
        Entity entity;
        int iMouseX;
        int iMouseY;
        
        public void ContextMenuPopup( object source, ContextMenuArgs e )
        {
            iMouseX = e.MouseX;
            iMouseY = e.MouseY;
            entity = e.Entity;
            if( entity != null )
            {
                LogFile.WriteLine("AssignTextureHandler registering in contextmenu");
                ContextMenuController.GetInstance().RegisterContextMenu(new string[]{ "Assign &Texture", "&All Faces" }, new ContextMenuHandler( AssignTextureAllFacesClick ) );
                ContextMenuController.GetInstance().RegisterContextMenu( new string[] { "Assign &Texture", "&Single Face" }, new ContextMenuHandler( AssignTextureSingleFaceClick ) );
            }
        }
        
        public void AssignTexture( int FaceNumber, Uri uri )
        {
            if( entity is FractalSplinePrim )
            {
                ((FractalSplinePrim)entity).SetTexture( FaceNumber, uri );
                MetaverseClient.GetInstance().worldstorage.OnModifyEntity(entity);
            }
        }

        public void AssignTextureAllFacesClick( object source, ContextMenuArgs e )
        {
            if (!(entity is Prim))
            {
                return;
            }

            int FaceNumber = FractalSpline.Primitive.AllFaces;

            string filename = DialogHelpers.GetFilePath( "Select image file (*.bmp,*.jpg,*.gif,*.tga):", "*.JPG" );
            if (filename != "")
            {
                Console.WriteLine( filename );
                if (File.Exists( filename ))
                {
                    AssignTexture( FaceNumber, new Uri( filename ) );
                }
            }
        }

        public void AssignTextureSingleFaceClick( object source, ContextMenuArgs e )
        {
            if( ! ( entity is Prim ) )
            {
                return;
            }
            
            int FaceNumber = Picker3dController.GetInstance().GetClickedFace( entity as Prim, iMouseX, iMouseY );

            string filename = DialogHelpers.GetFilePath("Select image file (*.bmp,*.jpg,*.gif,*.tga):","*.JPG");
            if( filename != "" )
            {
                Console.WriteLine ( filename );
                if( File.Exists( filename ) )
                {
                    AssignTexture( FaceNumber, new Uri( filename ) );
                }
            }
        }
    }
}
