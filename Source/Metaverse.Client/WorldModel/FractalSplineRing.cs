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
// You can contact me at hughperkins at gmail for more information.

using System;
using System.Collections;
using Metaverse.Utility;
using FractalSpline;

namespace OSMP
{
    public class FractalSplineRing : FractalSplinePrim
    {
        // register our contextmenu
        public static void Register()
        {
            ContextMenuController.GetInstance().RegisterPersistentContextMenu(new string[]{ "&Create...","&Ring" }, new ContextMenuHandler( Create ) );
        }
        
        public static void Create( object source, ContextMenuArgs e )
        {
            EntityCreationProperties buildproperties = new EntityCreationProperties( e.MouseX, e.MouseY );
            
            FractalSplineRing newentity = new FractalSplineRing();
            buildproperties.WriteToEntity( newentity, "Ring" );
            
            MetaverseClient.GetInstance().worldstorage.AddEntity( newentity );
        }
        
        public FractalSplineRing()
        {
            Test.Debug( "Ring" );
            primitive = new FractalSpline.Ring( FractalSpline.RendererOpenGl.GetInstance() );
            LoadDefaults();
        }
    }
}
