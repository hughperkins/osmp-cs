// Copyright Hugh Perkins 2006
//
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the
// Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
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

namespace OSMP
{
    // base class for GlTexture
    public interface ITexture
    {
        int Width { get; }
        int Height { get; }
        string Filename { get; }
        byte[,] AlphaData { get; } // gives access to underlying texture Color data
        bool IsAlpha { get; }
        bool Modified { get; set; }

        void Apply();
        void LoadFromFile( string filename );
        void SaveAlphaToFile( string filename );
        void ReloadAlpha();
        void LoadNewImage( ImageWrapper image, bool isalpha );
    }
}
