// Copyright Hugh Perkins 2004,2005,2006
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

//! \file
//! \brief Contains quaternion and matrix maths

// see headerfile Math.h for documentation

using System;
using System.Collections;
using MathGl;
using Metaverse.Utility;

namespace OSMP
{
    public class mvMath
    {
        public const double PiOver180 = 0.0174532925;
        public const double Pi = 3.1415926535;
        public const double Pi2 = 2 * 3.1415926535;
            
        public static Vector3 XAxis = new Vector3( 1, 0, 0 );
        public static Vector3 YAxis = new Vector3( 0, 1, 0 );
        public static Vector3 ZAxis = new Vector3( 0, 0, 1 );
        
        public static Rot RotBetween( Vector3 v1, Vector3 v2 )
        {
            Rot rResult = new Rot();
            
            Vector3 VectorNorm1 = new Vector3( v1 ).Normalize();
            Vector3 VectorNorm2 = new Vector3( v2 ).Normalize();
        
            Vector3 RotationAxis = Vector3.CrossProduct( VectorNorm1, VectorNorm2 );
            //Test.Debug(  "math: " << RotationAxis ); // Test.Debug
        
            //Test.Debug(  Math.Abs( RotationAxis.x ) << " " << Math.Abs( RotationAxis.y ) << " " << Math.Abs( RotationAxis.z ) ); // Test.Debug
            if( Math.Abs( RotationAxis.x ) < 0.0005 && Math.Abs( RotationAxis.y ) < 0.0005 && Math.Abs( RotationAxis.z ) < 0.0005 )
            {
                Vector3 RandomVector = new Vector3( VectorNorm1 );
                RandomVector.x += 0.5;
                RandomVector.Normalize();
                RotationAxis = Vector3.CrossProduct( VectorNorm1, RandomVector );
        
                rResult = mvMath.AxisAngle2Rot( RotationAxis, 3.1415926535 );
            }
            else
            {
                double DotProduct = Vector3.DotProduct( VectorNorm1, VectorNorm2 );
                Test.Debug( "DotProduct: " + DotProduct.ToString() ); // Test.Debug
                double Vangle = Math.Acos( DotProduct );
                Test.Debug( "math: " + Vangle.ToString() ); // Test.Debug
                rResult = AxisAngle2Rot( RotationAxis, Vangle );
            }
            return rResult;
        }
        
        public static Rot AxisAngle2Rot( Vector3 V, double Theta )
        {
            Vector3 NormVector = new Vector3( V ).Normalize();
        
            double sin_a = Math.Sin( Theta / 2 );
            double cos_a = Math.Cos( Theta / 2 );
        
            return new Rot( NormVector.x * sin_a, NormVector.y * sin_a, NormVector.z * sin_a, cos_a );
            //Test.Debug(  "AXISANGLE2Rot in= " << V.x << " " << V.y << " " << V.z << " theta " << Theta << "out=" << Qr ); // Test.Debug
        }
        
        public static void Rot2AxisAngle( ref Vector3 Vr, ref double Thetar, Rot R )
        {
            //QuaternionNormalize( |X,Y,Z,W| );
            
            Vr = new Vector3();
        
            double cos_a = R.s;
            Thetar = Math.Acos( cos_a ) * 2;
            double sin_a = Math.Sqrt( 1.0 - cos_a * cos_a );
        
            if ( Math.Abs( sin_a )> 0.0005 )
            {
                Vr.x = R.x / sin_a;
                Vr.y = R.y / sin_a;
                Vr.z = R.z / sin_a;
            }
            else
            {
                Vr.x = 1;
                Vr.y = 0;
                Vr.z = 0;
            }
        }
        public static void ApplyRotToGLMatrix4d( ref GLMatrix4D matrix, Rot rot )
        {
            double fRotAngle = 0;
            Vector3 vAxis = new Vector3();
            mvMath.Rot2AxisAngle( ref vAxis, ref fRotAngle, rot );
            
            matrix.applyRotate( (float)( fRotAngle / mvMath.PiOver180 ), (float)vAxis.x, (float)vAxis.y, (float)vAxis.z );
        }
    }
}
