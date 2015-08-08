// Copyright Hugh Perkins 2004, 2005, 2006
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
// ======================================================================================
//

using System;
using System.Collections;
using System.Xml;

namespace OSMP
{
    //! Rot contains a rotation, in x,y,z,s quaternion format; plus helper functions
    public class Rot
    {
        [Replicate]
        public double x; //!< quaternion x
        [Replicate]
        public double y; //!< quaternion y
        [Replicate]
        public double z; //!< quaternion z
        [Replicate]
        public double s; //!< quaternion s ("scalar")
        //! initializes from passed in string array, eg ["0","0","0","1"]
        public Rot( string[] array )
        { 
            x = Convert.ToDouble( array[0] ); 
            y = Convert.ToDouble( array[1] ); 
            z = Convert.ToDouble( array[2] ); 
            s = Convert.ToDouble( array[3] ); 
        }
        
        //! default constructor, initialies to [0,0,0,1]
        public Rot() { x = y = z = 0; s = 1; }
        
        //! constructs from passed-in x,y,z,s
        public Rot( double x, double y, double z, double s )
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.s = s;
        }
        //! initializes from passed-in xml element, in format <someelement x="..." y="..." z="..." s="..."/>
        public Rot( XmlElement xmlelement )
        {
            x = Convert.ToDouble( xmlelement.GetAttribute( "x" ) ); 
            y = Convert.ToDouble( xmlelement.GetAttribute( "y" ) ); 
            z = Convert.ToDouble( xmlelement.GetAttribute( "z" ) ); 
            s = Convert.ToDouble( xmlelement.GetAttribute( "s" ) ); 
        }

        public static Rot operator*( Rot R1, Rot R2 )
        {
            Rot result = new Rot();
            result.s = R1.s * R2.s - R1.x * R2.x - R1.y * R2.y - R1.z * R2.z;
        
            result.x = R1.s * R2.x + R1.x * R2.s + R1.y * R2.z -R1.z * R2.y;
            result.y = R1.s * R2.y + R1.y * R2.s + R1.z * R2.x - R1.x * R2.z;
            result.z = R1.s * R2.z + R1.z * R2.s + R1.x * R2.y - R1.y * R2.x;
        
            //Test.Debug(  "RotMULITPLY in=" << Q1 << " " << Q2 << " out=" << Qr <<endl;
            return result;
        }

        public Rot Inverse()
        {
            return new Rot( - x, - y, - z, s );
        }
                
        //! writes out to xml string in format x="..." y="..." z="..." s="..."
        public string ToXMLAttributes()
        {
            return "x=\"" + x.ToString() + "\" y=\"" + y.ToString() + "\" z=\"" + z.ToString() + "\" s=\"" + s.ToString() + "\"";
        }
        
        //! writes out to passed-in xml element in format <someelement x="..." y="..." z="..." s="..."/>
        public void WriteToXMLElement( XmlElement xmlelement )
        {
            xmlelement.SetAttribute( "x", x.ToString() );
            xmlelement.SetAttribute( "y", y.ToString() );
            xmlelement.SetAttribute( "z", z.ToString() );
            xmlelement.SetAttribute( "s", s.ToString() );
        }
        
        public override string ToString()
        {
            return "<rot x=\"" + x.ToString() + "\" y=\"" + y.ToString() + "\" z=\"" + z.ToString() + "\" s=\"" + s.ToString() + "\"/>";
        }
    }
}    
