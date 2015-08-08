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
    //! Color contains a color, in r,g,b format; plus helper functions
    public class Color
    {
        [Replicate]
        public double r = 0;  //!< red value
        [Replicate]
        public double g = 0;  //!< green value
        [Replicate]
        public double b = 0;  //!< blue value
        [Replicate]
        public double a = 1; // alpha value
        
        public Color()
        {
        }
        public Color( double r, double g, double b )
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = 1;  // alpha of 1 means totally opaque (not transparent)
        }
        public Color( double r, double g, double b, double alpha )
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = alpha;
        }
        public Color( Color color )
        {
            this.r = color.r;
            this.g = color.g;
            this.b = color.b;
            this.a = color.a;
        }
        
        //! initializes from passed in string array, eg ["0.6", "0.2", "0.3"] in order r,g,b
        public Color( string[] array )
        {
            r = Convert.ToDouble( array[0] ); 
            g = Convert.ToDouble( array[1] ); 
            b = Convert.ToDouble( array[2] );
            if( array.GetUpperBound(0) == 3 )
            {
                a = Convert.ToDouble( array[3] );
            }
        }

        public double[] ToArray()
        {
            return new double[]{ r, g, b, a };
        }            
        
        //! Reads values from passed in XML element in format r="..." g="..." b="..."
        public Color( XmlElement xmlelement )
        {
            r = Convert.ToDouble( xmlelement.GetAttribute( "r" ) ); 
            g = Convert.ToDouble( xmlelement.GetAttribute( "g" ) ); 
            b = Convert.ToDouble( xmlelement.GetAttribute( "b" ) );
            if( xmlelement.GetAttribute( "a" ) != null )
            {
                a = Convert.ToDouble( xmlelement.GetAttribute( "a" ) );
            }
        }
        
        //! Writes values to passed-in xml element, <someelement r="..." g="..." b="..."/>
        public void WriteToXMLElement( XmlElement xmlelement )
        {
            xmlelement.SetAttribute( "r", r.ToString() );
            xmlelement.SetAttribute( "g", g.ToString() );
            xmlelement.SetAttribute( "b", b.ToString() );
            xmlelement.SetAttribute( "a", a.ToString() );
        }
        
        //! Writes values to xml string
        public string ToXMLAttributes()
        {
            return "r=\"" + r.ToString() + "\" g=\"" + g.ToString() + "\" b=\"" + b.ToString() + "\"" + "\" a=\"" + a.ToString() + "\"";
        }
        
        //! writes out to ostream
        public override string ToString()
        {
            return "<color r=\"" + r.ToString() + "\" g=\"" + g.ToString() + "\" b=\"" + b.ToString() + "\" a=\"" + a.ToString() + "\"/>";
        }
    }
}
