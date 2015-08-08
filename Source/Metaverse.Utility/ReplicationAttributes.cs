// Copyright Hugh Perkins 2006
// hughperkins at gmail http://hughperkins.com
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
using System.Collections.Generic;
using System.Text;
using Metaverse.Utility;

namespace OSMP
{
    // attributes that affect which fields/properties are replicated

    public interface IReplicationAttribute
    {
    }

    public class Replicate : Attribute, IReplicationAttribute
    {
    }

    public class Movement : Attribute, IReplicationAttribute
    {
    }
    // Add more attributes here...


    public class ReplicateAttributeHelper
    {
        public ReplicateAttributeHelper()
        {
            RegisterAttributes();
        }
        
        // careful of changing this; OSMP clients with different lists here will be incompatible
        void RegisterAttributes()
        {
            Add( typeof( Replicate ) );
            Add( typeof( Movement ) );
        }

        Dictionary<Type, int> BitnumByTypeAttribute = new Dictionary<Type, int>();
        Dictionary<int, Type> TypeAttributeByBitnum = new Dictionary<int, Type>();
        
        int nextbitnum = 0;
        void Add(Type AttributeType)
        {
            BitnumByTypeAttribute.Add( AttributeType, nextbitnum );
            TypeAttributeByBitnum.Add( nextbitnum,AttributeType );
            nextbitnum++;
        }

        public List<Type> BitmapToAttributeTypeArray(int bitmap)
        {
            List<Type> typeattributes = new List<Type>();
            foreach( int bitnumber in TypeAttributeByBitnum.Keys )
            {
                if( ( ( 1 << bitnumber ) & bitmap ) > 0 )
                {
                    typeattributes.Add( TypeAttributeByBitnum[ bitnumber ] );
                }
            }
            return typeattributes;
        }

        public int TypeArrayToBitmap(Type[] typearray)
        {
            int bitmap = 0;
            foreach (Type attributetype in typearray)
            {
                bitmap |= ( 1 << BitnumByTypeAttribute[ attributetype ] );
            }
            return bitmap;
        }
    }
    
    public class TestReplicationAttributes
    {
        public void Go()
        {
            int bitmap = new ReplicateAttributeHelper().TypeArrayToBitmap(new Type[] { typeof(Replicate) });
            foreach (Type attributetype in new ReplicateAttributeHelper().BitmapToAttributeTypeArray(bitmap))
            {
                LogFile.WriteLine( attributetype );
            }
            LogFile.BlankLine();

            bitmap = new ReplicateAttributeHelper().TypeArrayToBitmap(new Type[] { typeof(Movement) });
            foreach (Type attributetype in new ReplicateAttributeHelper().BitmapToAttributeTypeArray(bitmap))
            {
                LogFile.WriteLine( attributetype );
            }
            LogFile.BlankLine();

            bitmap = new ReplicateAttributeHelper().TypeArrayToBitmap(new Type[] { typeof(Replicate), typeof(Movement) });
            foreach (Type attributetype in new ReplicateAttributeHelper().BitmapToAttributeTypeArray(bitmap))
            {
                LogFile.WriteLine( attributetype );
            }
            LogFile.BlankLine();
        }
    }
}
