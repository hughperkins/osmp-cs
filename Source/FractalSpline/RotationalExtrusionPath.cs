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

using System;
using MathGl;

namespace FractalSpline
{
    public class RotationalExtrusionPath : ExtrusionPath
    {
        double fCutStartAngle = 0;
        double fCutEndAngle = 2 * Math.PI;
        double fExtrusionRadius = 1.0;
        double fRadialSectionScale = 1.0;

        public RotationalExtrusionPath()
        {
            UpdatePath();
        }
   
        public override void UpdatePath()
        {
            iNumberOfTransforms = iLevelOfDetail;
            int i;
            double fRatio;
            
            for( i = 0; i < iNumberOfTransforms; i++ )
            {
                fRatio = (double)i / (double)( iNumberOfTransforms - 1 );
                
                transforms[i].LoadIdentity();
                
                double fSliceAngle = ( fCutStartAngle + fRatio * (fCutEndAngle - fCutStartAngle ) );
                
                transforms[i].applyRotate( 180 / Math.PI * fSliceAngle, 1, 0, 0 );
                transforms[i].ApplyTranslation( 0, fExtrusionRadius, 0 );
                transforms[i].ApplyScale( 1, fRadialSectionScale, 1 );
            }
        }
        // start angle for extrusion
        public double CutStartAngle{
            set{ fCutStartAngle = value; }
        }
        // end angle for extrusion
        public double CutEndAngle{
            set{ fCutEndAngle = value; }
        }
        // radius used for rotation section
        public double Radius{
            set{ fExtrusionRadius = value; }
        }
        // thickness of section in radial direction
        public double RadialSectionScale{
            set{ fRadialSectionScale = value; }
        }
    }
}
