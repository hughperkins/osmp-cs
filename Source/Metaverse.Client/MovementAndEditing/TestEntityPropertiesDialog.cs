// Copyright Hugh Perkins 2006
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
using System.Windows.Forms;
using System.Drawing;

namespace OSMP
{
    public interface IRenderer
    {
        object RegisterContextMenu( string[] strings, ContextMenuCallback e );
        object UnregisterContextMenu( object o );
    }
    
    public class RendererFactory
    {
        static IRenderer instance = new Renderer();
        public static IRenderer GetInstance()
        {
            return instance;
        }
    }
    
    public delegate void ContextMenuCallback( int X, int Y );
    
    public class Renderer : IRenderer
    {
        public object RegisterContextMenu( string[] strings, ContextMenuCallback e ){ return null; }
        public object UnregisterContextMenu( object o ){ return null; }
    }
    
    public class SelectionModel
    {
        public class ChangedEventArgs : EventArgs
        {
            public int NumSelected;
            public ChangedEventArgs( int iNumSelected )
            {
                this.NumSelected = iNumSelected;
            }
        }
        
        static SelectionModel instance = new SelectionModel();
        public static SelectionModel GetInstance()
        {
            return instance;
        }
        
        public delegate void ChangedHandler( object source, ChangedEventArgs e );
        public event ChangedHandler ChangedEvent;
            
        public Entity GetFirstSelectedEntity(){ return new Entity(); }
    }
    
    public class Entity
    {
        public string name = "testname";
        public int hollow = 30;
            
        public void SetHollow( int hollow ){ 
            this.hollow = hollow;
            Console.WriteLine("New hollow: " + hollow.ToString() );
        }
        public void SetName( string name ){
            this.name = name;
            Console.WriteLine("New name: " + name );
        }
        
        public void RegisterProperties( IPropertyController propertycontroller )
        {
            propertycontroller.RegisterStringProperty( "Name", name, 64, new SetStringPropertyHandler( SetName ) );
            propertycontroller.RegisterIntProperty( "Hollow", hollow, 0, 200, new SetIntPropertyHandler( SetHollow ) );
        }        
    }
    
    public class TestEntityPropertiesDialog : Form
    {
        public TestEntityPropertiesDialog()
        {
            EntityPropertiesDialog entitypropertiesdialog = new EntityPropertiesDialog();
            entitypropertiesdialog.ContextMenuProperties( 20, 20 );
        }
    }

    public class EntryPoint
    {
        public static void Main()
        {
            try{
                Application.Run( new TestEntityPropertiesDialog() );
            }catch( Exception e )
            {
                Console.WriteLine( e );
            }
        }
    }
}

