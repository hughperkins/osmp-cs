// Copyright Hugh Perkins 2006
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
using System.Collections;

namespace OSMP
{
    // runs on server
    // manages reference indexes for objects involved in rpc or replication
    public class NetObjectReferenceController
    {
        Hashtable objectarraylistsbytype = new Hashtable();
        
        Hashtable objecttablesbytype = new Hashtable();
        Hashtable nextobjectreference = new Hashtable();
        
        Hashtable referencebyobject = new Hashtable();
        
        static NetObjectReferenceController instance = new NetObjectReferenceController();
        public static NetObjectReferenceController GetInstance(){ return instance; }
        
        public NetObjectReferenceController()
        {
        }
        
        public ArrayList GetObjectsByType( Type type )
        {
            if( objecttablesbytype[type ] == null )
            {
                return new ArrayList();
            }
            return (ArrayList)objectarraylistsbytype[ type ];
        }
        
        public object GetObjectForReference( Type objecttype, int reference )
        {
            if( objecttablesbytype[ objecttype ] != null )
            {
                return ((Hashtable)objecttablesbytype[ objecttype ])[ reference ];
            }
            return null;
        }
        
        public int GetReferenceForObject( object targetobject )
        {
            return (int)referencebyobject[ targetobject ];
        }
        
        public void Register( object registrant )
        {
            Type objecttype = registrant.GetType();
            
            if( !objecttablesbytype.Contains( objecttype ) )
            {
                objecttablesbytype.Add( objecttype, new Hashtable() );
                objectarraylistsbytype.Add( objecttype, new ArrayList() );
                if( !( registrant is IHasReference ) )
                {
                    nextobjectreference.Add( objecttype, 1 );
                }
            }
            Hashtable objecttable = (Hashtable)objecttablesbytype[ objecttype ];
            
            int objectreference = 0;
            if( registrant is IHasReference )
            {
                objectreference = ((IHasReference)registrant).Reference;
            }
            else
            {
                objectreference = (int)nextobjectreference[ objecttype ];
                nextobjectreference[ objecttype ] = (int)nextobjectreference[ objecttype ] + 1;
            }
            
            if( !objecttable.Contains( objectreference ) )
            {
                objecttable.Add( objectreference, registrant );
                referencebyobject.Add( registrant, objectreference );
            }
            
            ((ArrayList)objectarraylistsbytype[ objecttype ]).Add( registrant );
        }
    }
}
