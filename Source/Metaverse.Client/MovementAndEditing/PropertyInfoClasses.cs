// Copyright Hugh Perkins 2005,2006
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

namespace OSMP
{
    public delegate void SetIntPropertyHandler( int newvalue );
    public delegate void SetStringPropertyHandler( string newvalue );
    
    public interface IPropertyController
    {
        void RegisterIntProperty( string name, int currentvalue, int Min, int Max, SetIntPropertyHandler handler );
        void RegisterStringProperty( string name, string currentvalue, int Length, SetStringPropertyHandler handler );        
    }
    
    public abstract class EntityPropertyInfo
    {
        public string Name;
        public override string ToString(){ return this.GetType().ToString() + " " + Name; }
    }
    
    public class IntPropertyInfo : EntityPropertyInfo
    {
        public int Min;
        public int Max;
        public int InitialValue;
        public SetIntPropertyHandler Handler;
        public IntPropertyInfo( string name, int currentvalue, int min, int max, SetIntPropertyHandler handler )
        {
            this.Name = name; this.Min = min; this.Max = max; this.Handler = handler; InitialValue = currentvalue;
        }
    }
    
    public class StringPropertyInfo : EntityPropertyInfo
    {
        public int Length;
        public string InitialValue;
        public SetStringPropertyHandler Handler;
        public StringPropertyInfo( string name, string currentvalue, int length, SetStringPropertyHandler handler )
        {
            this.Name = name; this.Length = length; this.Handler = handler; InitialValue = currentvalue;
        }
    }
}
