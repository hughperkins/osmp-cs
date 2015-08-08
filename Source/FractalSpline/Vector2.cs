// Copyright Hugh Perkins 2005,2006
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

namespace FractalSpline
{
    public class Vector2{
        public double x;
        public double y;
            
        public Vector2()
        {
        }
        public Vector2( Vector2 orig )
        {
            x = orig.x;
            y = orig.y;
        }
        public Vector2( double x, double y )
        {
            this.x = x;
            this.y = y;
        }                
        public Vector2( double[]array )
        {
            this.x = array[0];
            this.y = array[1];
        }         
        public double[] ToArray()
        {
            return new double[]{ x,y};
        }
        public override String ToString()
        {
            return "<" + x.ToString() + "," + y.ToString() + ">";
        }
    }
}
