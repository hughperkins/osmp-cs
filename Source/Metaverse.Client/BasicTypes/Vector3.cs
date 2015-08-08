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
using System.Xml;

namespace OSMP
{
    public class Vector3{
        [Replicate]
        public double x;
        [Replicate]
        public double y;
        [Replicate]
        public double z;
            
        public Vector3()
        {
        }
        public Vector3( Vector3 orig )
        {
            x = orig.x;
            y = orig.y;
            z = orig.z;
        }
        public Vector3( double x, double y, double z )
        {
            this.x = x;
            this.y = y;
            this.z=z;
        }
        public Vector3 ( XmlElement xmlelement ) //!< Initializes Vector3 from passed in XML element in format <something x="..." y="..." z="..."/>
        {
            x = Convert.ToDouble( xmlelement.GetAttribute( "x" ) ); 
            y = Convert.ToDouble( xmlelement.GetAttribute( "y" ) ); 
            z = Convert.ToDouble( xmlelement.GetAttribute( "z" ) ); 
        }

        public double[] ToArray()
        {
            return new double[]{ x, y, z };
        }
        
        static public Vector3 operator+( Vector3 first, Vector3 second )
        {
            return new Vector3( first.x + second.x, first.y + second.y, first.z + second.z );
        }
        static public Vector3 operator-( Vector3 first, Vector3 second )
        {
            return new Vector3( first.x - second.x, first.y - second.y, first.z - second.z );
        }
        static public Vector3 operator-( Vector3 target )
        {
            return new Vector3( - target.x, -target.y, -target.z );
        }
        static public Vector3 operator*( double fMultiplier, Vector3 V )
        {
            return new Vector3( V.x * fMultiplier, V.y * fMultiplier, V.z * fMultiplier );
        }
        static public Vector3 operator*( Vector3 V, double fMultiplier )
        {
            return new Vector3( V.x * fMultiplier, V.y * fMultiplier, V.z * fMultiplier );
        }
        static public Vector3 operator/( Vector3 V, double fMultiplier )
        {
            return new Vector3( V.x / fMultiplier, V.y / fMultiplier, V.z / fMultiplier );
        }
        static public Vector3 operator*( Vector3 V, Rot rot )
        {
            Rot InverseInRot = rot.Inverse();
            Rot VectorRot = new Rot( V.x, V.y, V.z, 0 );
            Rot IntRot = VectorRot * rot;
            Rot ResultRot = InverseInRot * IntRot;            
            return new Vector3( ResultRot.x, ResultRot.y, ResultRot.z );  
        }
        
        public static Vector3 CrossProduct( Vector3 v1, Vector3 v2 )
        {
            Vector3 vR = new Vector3();
            vR.x = v1.y * v2.z - v1.z * v2.y;
            vR.y = v1.z * v2.x - v1.x * v2.z;
            vR.z = v1.x * v2.y - v1.y * v2.x;
            return vR;
        }

        public static Vector3 operator *(Vector3 v1, Vector3 v2)
        {
            return CrossProduct(v1, v2);
        }

        public static double DotProduct(Vector3 v1, Vector3 v2)
        {
            return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
        }
        
        static public double DistanceSquared( Vector3 one, Vector3 two )
        {
            return ( one.x - two.x ) * ( one.x - two.x ) + ( one.y - two.y ) * ( one.y - two.y ) + ( one.z - two.z ) * ( one.z - two.z );
        }
        
        public double DetSquared()
        {
            return x * x + y * y + z * z;
        }        
        public double Det()
        {
            return Math.Sqrt( x * x + y * y + z * z );
        }
        
        public Vector3 Unit()
        {
            Vector3 unitvector = new Vector3( x, y, z );
            unitvector.Normalize();
            return unitvector;
        }
        
        public Vector3 Normalize()
        {
            //Test.Debug(  "VectorNormalize in= " << V.x << " " << V.y << " " << V.z ); // Test.Debug
            double fMag = Det();
        
            if( fMag > 0.00000001 )
            {
                x = x / fMag;
                y = y / fMag;
                z = z / fMag;
            }
            else
            {
                x = 1.0;
                y = 0.0;
                z = 0.0;
            }
            //Test.Debug(  "VectorNormalize out= " << V.x << " " << V.y << " " << V.z ); // Test.Debug
            return this;
        }        
        
        //public override bool Equals( object two ){return ((Vector3)two).x == this.x && ((Vector3)two).y == this.y && ((Vector3)two).z == this.z;}
        //static public bool operator==( Vector3 first, Vector3 second )	{return first.x == second.x && first.y == second.y && first.z == second.z;	}
        //static public bool operator!=( Vector3 first, Vector3 second ){return first.x != second.x || first.y != second.y || first.z != second.z;}
        
        //public override int GetHashCode()
        //{
          //  return x.GetHashCode() / 3 + y.GetHashCode() / 3 + z.GetHashCode() / 3;
        //}

        public void WriteToXMLElement( XmlElement xmlelement )
        {
            xmlelement.SetAttribute( "x", x.ToString() );
            xmlelement.SetAttribute( "y", y.ToString() );
            xmlelement.SetAttribute( "z", z.ToString() );
        }
                
        public override String ToString()
        {
            return "<" + x.ToString() + "," + y.ToString() + "," + z.ToString() + ">";
        }
    }
}
