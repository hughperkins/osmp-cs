// Copyright Hugh Perkins 2006
// hughperkins at gmail http://hughperkins.com
//
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the
// Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURVector3E. See the GNU General Public License for
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

namespace OSMP
{
    public class SdlKeyCache
    {
        public bool[] _Keys = new bool[ 513 ];

        public event SdlDotNet.KeyboardEventHandler KeyUp;
        public event SdlDotNet.KeyboardEventHandler KeyDown;

        static SdlKeyCache instance = new SdlKeyCache();
        public static SdlKeyCache GetInstance()
        {
            return instance;
        }
        SdlKeyCache()
        {
            //Diag.Debug("Instantiating KeyFilterFormsKeyCache()");
            IRenderer renderer = RendererFactory.GetInstance();
            renderer.KeyDown += new SdlDotNet.KeyboardEventHandler(renderer_KeyDown);
            renderer.KeyUp += new SdlDotNet.KeyboardEventHandler(renderer_KeyUp);
        }

        void renderer_MouseDown(object sender, SdlDotNet.MouseButtonEventArgs e)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        void renderer_KeyDown(object sender, SdlDotNet.KeyboardEventArgs e)
        {
            Keys[ (int)e.Key ] = true;
            //Test.WriteOut("KeyFilterFormsKeyCache._KeyDown(" + e.KeyCode.ToString() + ")" );
            if( KeyDown != null )
            {
                //Test.WriteOut("KeyFilterFormsKeyCache Sending keydown(" + e.KeyCode.ToString() + ")" );
                KeyDown(sender, e);
            }
        }

        void renderer_KeyUp(object sender, SdlDotNet.KeyboardEventArgs e)
        {
            //Test.WriteOut("KeyFilterFormsKeyCache._KeyUp(" + e.KeyCode.ToString() + ")" );
            Keys[(int)e.Key] = false;
            if( KeyUp != null )
            {
                KeyUp(sender, e);
            }
        }
    
        public bool[]Keys
        {
            get
            {
                return _Keys;
            }
        }
    }
}
