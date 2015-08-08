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

using System;

namespace OSMP
{
    public class Vector2{
        [Replicate]
        public double x;
        [Replicate]
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
        static public Vector2 operator+( Vector2 first, Vector2 second )
        {
            return new Vector2( first.x + second.x, first.y + second.y );
        }
        static public Vector2 operator-( Vector2 first, Vector2 second )
        {
            return new Vector2( first.x - second.x, first.y - second.y );
        }
        static public double DistanceSquared( Vector2 one, Vector2 two )
        {
            return ( one.x - two.x ) * ( one.x - two.x ) + ( one.y - two.y ) * ( one.y - two.y );
        }
        public double Det()
        {
            return Math.Sqrt( x * x + y * y );
        }
        public override String ToString()
        {
            return "<" + x.ToString() + "," + y.ToString() + ">";
        }
        public bool InRange( int minx, int miny, int maxx, int maxy )
        {
            return( x >= minx && x <= maxx && y >= miny && y <= maxy );
        }
        public override bool Equals( object two )
        {
            return ((Vector2)two).x == this.x && ((Vector2)two).y == this.y;
        }
        static public bool operator==( Vector2 first, Vector2 second )
        {
            return first.x == second.x && first.y == second.y;
        }
        static public bool operator!=( Vector2 first, Vector2 second )
        {
            return first.x != second.x || first.y != second.y;
        }
        public override int GetHashCode()
        {
            return x.GetHashCode() / 3 + y.GetHashCode() / 3;
        }
    }
}
