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
using System.Collections;
using MathGl;

namespace FractalSpline
{
    public class Ring : RotationalPrimitive
    {
        public Ring( IRenderer renderer )
        {
            ReferenceVertices = new GLVector3d[3];
            
            ReferenceVertices[0] = new GLVector3d( 0.5, -0.5, 0 );
            ReferenceVertices[1] = new GLVector3d( 0, 0.5, 0 );
            ReferenceVertices[2] = new GLVector3d( -0.5, -0.5, 0 );
            
            iFirstOuterFace = 0;
            iLastOuterFace = 2;
            
            iNumberFaces = 3;
            
            bShowCut = false;
            bShowHollow = false;
            
            iCutStart = 0;
            iCutEnd = MaxCut;
            
            this.renderer = renderer;
            SendRendererCallbacksToCrossSections();
            
            rotationalextrusionpath.UpdatePath();
            BuildFaces();
        }
        
        protected override int GetCutQuadrant( int iCut )
        {
            return ( ( iCut / 67 ) % 3 + 3 ) % 3;
        }
        
        protected override double GetAngleWithXAxis( double fCutRatio )
        {
            return ( fCutRatio - 0.125 ) * 2 * Math.PI;
        }
        
        protected override void AssignFaces()
        {
            ArrayList FacesAL = new ArrayList();
   
            for( int i = iFirstOuterFace; i <= iLastOuterFace; i++ )
            {
                int iFaceNum = i + 1;                
                FacesAL.Add( new OuterFace( iFaceNum, i ) );
            }
                
            if( bShowHollow )
            {
                for( int i = iFirstOuterFace; i <= iLastOuterFace; i++ )
                {
                    FacesAL.Add( new InnerFace( 4, i ) );
                }
            }
            
            if( bShowCut )
            {
                int iCutStartFace = 5;
                int iCutEndFace = 6;
                if( bShowHollow )
                {
                    iCutStartFace = 6;
                    iCutEndFace = 7;
                }
                
                FacesAL.Add( new CutFace( iCutStartFace, 0 ) );
                FacesAL.Add( new CutFace( iCutEndFace, 1 ) );
            }
            
            if( !bShowHollow )
            {
                FacesAL.Add( new EndCapNoHollow( 4, false ) );
                FacesAL.Add( new EndCapNoHollow( 0, true ) );
            }
            else
            {
                FacesAL.Add( new EndCapHollow( 5, false ) );
                FacesAL.Add( new EndCapHollow( 0, true ) );
            }
            
            Faces = (Face[])FacesAL.ToArray( typeof( Face ) );
        }
    }
}
