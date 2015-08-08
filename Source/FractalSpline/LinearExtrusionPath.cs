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

using System;
using MathGl;

namespace FractalSpline
{
    public class LinearExtrusionPath : ExtrusionPath
    {
        public override void UpdatePath()
        {
            iNumberOfTransforms = iLevelOfDetail;
            int i;
            double fRatio;
            for( i = 0; i < iNumberOfTransforms; i++ )
            {
                fRatio = (double)i / (double)( iNumberOfTransforms - 1 );
                transforms[i] = GLMatrix4D.Identity();
                transforms[i].ApplyTranslation( fRatio * fShear, 0, fRatio - 0.5 );
                transforms[i].ApplyScale( 1 + fRatio * (fTopSizeX - 1), 1 + fRatio * ( fTopSizeY - 1 ), 1 );
                transforms[i].applyRotate( fRatio * (double)iTwist, 0, 0, 1 );
            }
        }
    }
}
