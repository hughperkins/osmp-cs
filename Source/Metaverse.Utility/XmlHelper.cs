// Copyright Hugh Perkins 2005,2006
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

using System.Xml;
namespace Metaverse.Utility {
public class XmlHelper
{
	public static XmlDocument CreateDom()
	{
		XmlDocument newdoc = new XmlDocument();
		newdoc.PreserveWhitespace = true;
		return newdoc;
	}
	public static XmlDocument OpenDom( string sfilepath )
	{
		XmlDocument newdoc = CreateDom();
		newdoc.Load( sfilepath );
		return newdoc;
	}
    public static XmlElement AddChild( XmlElement parent, string nodename )
    {
        XmlElement childnode = parent.OwnerDocument.CreateElement( nodename );
        parent.AppendChild( childnode );
        return childnode;
    }
}
}
