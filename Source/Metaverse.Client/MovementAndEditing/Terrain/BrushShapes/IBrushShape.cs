// Created by Hugh Perkins 2006
// hughperkins@gmail.com http://manageddreams.com
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
    public interface IBrushShape
    {
        /// <summary>
        /// number from 0 to 1 describing strength of brush.
        /// x and y are normalized from -1 to 1, where 1 is normalized radius of brush.
        /// outside of brush is negative.
        /// </summary>
        /// <returns></returns>
        double GetStrength(double x, double y);

        /// <summary>
        /// This is called whenever this brush is selected, so can populate 
        /// control box in UI, using passed in vboxes, if it wants. *optional*.
        /// See FixedHeight for an example.
        /// </summary>
        void ShowControlBox( Gtk.VBox labels, Gtk.VBox widgets );

        /// <summary>
        /// optional.  You can just copy RoundBrush's or SquareBrush's
        /// </summary>
        void Render( Vector3 intersectpos );

        /// <summary>
        /// Name of brush
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Description of brush
        /// </summary>
        string Description { get; }
    }
}
