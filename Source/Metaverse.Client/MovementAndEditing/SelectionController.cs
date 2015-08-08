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

using System;
using System.Collections;

namespace OSMP
{
    // provides key/mouse interface to SelectionModel
    // so if you think this sucks, just write a different one ;-)
    // Be sure to register the new one as a plugin, in PluginsLoader.cs , or equivalent
    public class SelectionController
    {
        SelectionModel selectionmodel;
        
        //bool bSelectIndividualOn;
        //bool bSelectObjectOn;
        
        static SelectionController instance = new SelectionController();
        public static SelectionController GetInstance()
        {
            return instance;
        }
        
        public SelectionController()
        {
            selectionmodel = SelectionModel.GetInstance();
            
            //KeyFilterComboKeys keyfiltercombokeys = KeyFilterComboKeys.GetInstance();
            CommandCombos.GetInstance().RegisterAtLeastCommand("selectobject",
                new KeyCommandHandler(SelectObjectKeyDown));
            CommandCombos.GetInstance().RegisterAtLeastCommand("selectindividual",
                new KeyCommandHandler(SelectIndividualKeyDown));

            //keyfiltercombokeys.RegisterCombo( new string[]{"selectobject"},null, new KeyComboHandler( SelectObjectKeyDown ) );
            //keyfiltercombokeys.RegisterCombo( new string[]{"selectindividual"},null, new KeyComboHandler( SelectIndividualKeyDown ) );
        }

        public void SelectObjectKeyDown( string command, bool down )
        {
            if (down)
            {
                selectionmodel.ToggleClickedInSelection(true, MouseCache.GetInstance().MouseX, MouseCache.GetInstance().MouseY);
            }

          //  bSelectObjectOn = down;
            //if( bSelectObjectOn )
            //{
              //  Test.Debug("adding capture");
                //keyandmousehandler.MouseDown += new MouseCallback( MouseDown );
//                MouseFilterMouseCacheFactory.GetInstance().MouseDown += new MouseEventHandler( MouseDown );
  //          }
    //        else
      //      {
        //        Test.Debug("removing capture");
                //keyandmousehandler.MouseDown -= new MouseCallback( MouseDown );
          //      MouseFilterMouseCacheFactory.GetInstance().MouseDown -= new MouseEventHandler( MouseDown );
            //}
        }

        public void SelectIndividualKeyDown( string command, bool down )
        {
            if (down)
            {
                selectionmodel.ToggleClickedInSelection(false, MouseCache.GetInstance().MouseX, MouseCache.GetInstance().MouseY);
            }
            //bSelectIndividualOn = down;
            //if( bSelectIndividualOn )
            //{
              //  Test.Debug("adding capture");
                //keyandmousehandler.MouseDown += new MouseHandler( MouseDown );
                //MouseFilterMouseCacheFactory.GetInstance().MouseDown += new MouseEventHandler( MouseDown );
            //}
            //else
            //{
              //  Test.Debug("removing capture");
                //keyandmousehandler.MouseDown -= new MouseHandler( MouseDown );
//                MouseFilterMouseCacheFactory.GetInstance().MouseDown -= new MouseEventHandler( MouseDown );
  //          }
        }
        
        //public void MouseDown( object source, MouseEventArgs e )
        //{
            //Test.Debug("mousedown");
            //if( bSelectObjectOn && e.Button == MouseButtons.Left )
            //{
              //  selectionmodel.ToggleClickedInSelection( true,  e.X, e.Y );
            //}
            //else if( bSelectIndividualOn && e.Button == MouseButtons.Left )
            //{
            //    selectionmodel.ToggleClickedInSelection( false, e.X, e.Y );
          //  }
        //}
    }
}
