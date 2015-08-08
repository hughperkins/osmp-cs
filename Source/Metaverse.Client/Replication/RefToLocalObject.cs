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
    // this generates 

    // note to self: might want to add Tick to remove dead objects (ie weakreference.Target is null)
    public class RefToLocalObject
    {
        static Hashtable nextreferencebytype = new Hashtable();
        
        static Hashtable objectbyreference = new Hashtable();
        static Hashtable referencebyobject = new Hashtable();
        
        bool islocal;
        HashableWeakReference targetobjectweakreference;
        int reference;
        
        public RefToLocalObject( object targetobject )
        {
            islocal = true;
            this.targetobject = targetobject;
            Type targettype = targetobject.GetType();
            reference = GetReference( targettype, targetobject );
        }
        
        static int GetNextReference( Type targettype, object targetobject )
        {
            HashableWeakReference targetobjectweakreference = new HashableWeakReference( targetobject );
            if( referencebyobject.Contains( targetobjectweakreference ) )
            {
                return (int)referencebyobject[ targetobjectweakreference ];
            }
            
            if( !nextreferencebytype.Contains( targettype ) )
            {
                nextreferencebytype.Add( targettype, 1 );
            }
            int nextreference = (int)nextreferencebytype[ targettype ];
            nextreferencebytype[ targettype ] = nextreference + 1;
            
            objectbyreference.Add( nextreference, targetobjectweakreference );
            referencebyobject.Add( targetobjectweakreference, nextreference );
            
            return nextreference;
        }
        
        public int Reference{
            get{
                return reference;
            }
        }
        
        public bool IsLocal{
            get{
                return islocal;
            }
        }
        
        public Type TargetType{
            get{
                object targetobject = targetobjectweakreference.Target;
                if( targetobject == null )
                {
                    return null;
                }
                return targetobject.GetType();
            }
        }
        
        public object Target{
            get{ return targetobjectweakreference.Target;
            }
        }
    }
}
