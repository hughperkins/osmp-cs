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

// dependencies: Vector3

using System;

namespace OSMP
{
    public class Axis
    {
        [Replicate]
        bool bPositiveAxis = true;
        [Replicate]
        int axisindex = 0;
        
        static string[] axisnames = new string[]{"x","y","z"};
    
        public const int XAxisIndex = 0;
        public const int YAxisIndex = 1;
        public const int ZAxisIndex = 2;
        
        // axisindex = 0,1,2 meaning x,y,z
        public Axis()
        {
        }
        public Axis( Axis orig )
        {
            bPositiveAxis = orig.bPositiveAxis;
            axisindex = orig.axisindex;
        }
        public Axis( bool bPositiveAxis, int axisindex )
        {
            this.axisindex = axisindex;
            this.bPositiveAxis = bPositiveAxis;
        }
        
        public static Axis PosX = new Axis( true, 0 );
        public static Axis PosY = new Axis( true, 1 );
        public static Axis PosZ = new Axis( true, 2 );
        public static Axis NegX = new Axis( false, 0 );
        public static Axis NegY = new Axis( false, 1 );
        public static Axis NegZ = new Axis( false, 2 );
        
        public Vector3 ToVector()
        {
            Vector3 result = null;
            if( axisindex == 0 )
            {
                result = new Vector3( 1,0,0 );
            }
            else if( axisindex == 1 )
            {
                result = new Vector3( 0,1,0 );
            }
            else if( axisindex == 2 )
            {
                result = new Vector3( 0,0,1 );
            }
            if( !bPositiveAxis )
            {
                result = - result;
            }
            return result;
        }
        
        // This obviously gives an ambiguous result, but its ok for things like editing handles that 
        // are symmetric when rotated by 90,180, or 270 degrees around the target axis
        public Rot ToRot()
        {
            Rot result = null;
            if( axisindex == 0 && bPositiveAxis )
            {
                result = new Rot();
            }
            else if( axisindex == 1 && bPositiveAxis )
            {
                result = mvMath.AxisAngle2Rot( Axis.PosZ.ToVector(), Math.PI / 2 );
            }
            else if( axisindex == 2 && bPositiveAxis )
            {
                result = mvMath.AxisAngle2Rot(Axis.PosY.ToVector(), - Math.PI / 2 );
            }
            else if( axisindex == 0  )
            {
                result = mvMath.AxisAngle2Rot(Axis.PosZ.ToVector(), Math.PI );
            }
            else if( axisindex == 1  )
            {
                result = mvMath.AxisAngle2Rot(Axis.PosZ.ToVector(), - Math.PI / 2 );
            }
            else if( axisindex == 2  )
            {
                result = mvMath.AxisAngle2Rot(Axis.PosY.ToVector(), Math.PI / 2 );
            }
            return result;
        }
        
        public double GetAxisComponentIgnoreAxisDirection( Vector3 vector )
        {
            if( axisindex == 0 )
            {
                return vector.x;
            }
            else if( axisindex == 1 )
            {
                return vector.y;
            }
            else
            {
                return vector.z;
            }
        }
        public double GetAxisComponent( Vector3 vector )
        {
            double value = GetAxisComponentIgnoreAxisDirection( vector );
            if( !bPositiveAxis )
            {
                return -value;
            }
            else
            {
                return value;
            }
        }
        
        public bool IsPositiveAxis
        {
            get{
                return bPositiveAxis;
            }
        }
        public bool IsXAxis
        {
            get{
                return axisindex == 0;
            }
        }
        public bool IsYAxis
        {
            get{
                return axisindex == 1;
            }
        }
        public bool IsZAxis
        {
            get{
                return axisindex == 2;
            }
        }
        public int AxisIndex{
            get{
                return axisindex;
            }
        }
        public string AxisName
        {
            get{
                return axisnames[ axisindex ];
            }
        }
        public override string ToString()
        {
            return "<axis positive=\"" + bPositiveAxis.ToString() + "\"" + AxisName + "\"/>";
        }
    }
}
