// Copyright Hugh Perkins 2004,2005,2006
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

namespace OSMP
{
    // Renders selection highlighting
    public class SelectionView : IRenderable
    {
        public Color SelectionGridColor = new Color( 0.8,0.8,1.0, 1.0 );
        public int SelectionGridNumLines = 5;
        
        static SelectionView instance = new SelectionView();
        public static SelectionView GetInstance()
        {
            return instance;
        }
            
        public void Render()
        {
            SelectionModel selectionmodel = SelectionModel.GetInstance();
            IGraphicsHelper graphics = GraphicsHelperFactory.GetInstance();
            for( int i = 0; i < selectionmodel.SelectedObjects.Count; i++ )
            {
                Entity entity = selectionmodel.SelectedObjects[i];
                
                graphics.PushMatrix();
                
                if( entity.Parent != null )
                {
                    ( entity as EntityGroup ).ApplyTransforms();
                }
                    
                 entity.DrawSelected();
                 graphics.PopMatrix();
            }
        }
    }
}
