// Copyright Hugh Perkins 2006
//
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License version 2 as published by the
// Free Software Foundation;
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
using System.Collections.Generic;

namespace Metaverse.Utility
{
    public class Arguments
    {
        public Dictionary<string,string> Named;
        public List<string> Unnamed;
        
        public Arguments( string[] args )
        {
            Named = new Dictionary<string,string>();
            Unnamed = new List<string>();
            
            int i = 0; 
            while( i <= args.GetUpperBound(0) )
            {
                if( args[i][0] == '-' )
                {
                    if( args[i][1] == '-' )
                    {
                        Named[ args[i].Substring( 2 ) ] = args[i].Substring( 2 );
                        i += 0;
                    }
                    else
                    {
                        Named[ args[i].Substring( 1 ) ] = args[ i + 1 ];
                        i += 1;
                    }
                }
                else
                {
                    Unnamed.Add( args[i] );
                }
                i++;
            }
        }
    }
}
