// Created by Hugh Perkins 2006
// hughperkins at gmail http://hughperkins.com
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
    public interface IBrushEffect
    {
        /// <summary>
        /// applies brush
        /// </summary>
        /// <param name="map"></param>
        /// <param name="brushshape"></param>
        /// <param name="brushsize"></param>
        /// <param name="brushcentrex">x position of brush centre, opengl coords</param>
        /// <param name="brushcentrey">y position of brush centre, opengl coords</param>
        /// <param name="israising">true = raising, false = lowering (or equivalent for texture painting etc)</param>
        /// <param name="timespanmilliseconds">time frame over which to calculate effect, roughly 1/framerate</param>
        void ApplyBrush( IBrushShape brushshape, int brushsize, double brushcentrex, double brushcentrey, bool israising, double timespanmilliseconds );
        /// <summary>
        /// whether to apply this as long as mouse button is depressed, or just once per click
        /// </summary>
        bool Repeat { get; }
        string Name { get; }
        string Description { get; }
        /// <summary>
        /// This is called whenever this brush is selected, so can populate 
        /// control box in UI, using passed in vboxes, if it wants. *optional*.
        /// See FixedHeight for an example.
        /// </summary>
        void ShowControlBox( Gtk.VBox labels, Gtk.VBox widgets );
    }
}
