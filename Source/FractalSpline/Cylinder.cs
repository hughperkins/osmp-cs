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
using System.Collections;
using MathGl;

namespace FractalSpline
{
    // Cylinder
    //
    // Note to self: There's tons of stuff in this class thats also in LinearPrimitive, eg BuildFaces, various EndCap functions.
    // I cant help thinking a lot of that stuff shouldnt actually be here at all?
    public class Cylinder : LinearPrimitive
    {
        public Cylinder( IRenderer renderer )
        {
            iFirstOuterFace = 0;
            iLastOuterFace = 0;
    
            iNumberFaces = 1;
    
            bShowCut = false;
            bShowHollow = false;
    
            iCutStart = 0;
            iCutEnd = MaxCut;
    
            this.renderer = renderer;
            SendRendererCallbacksToCrossSections();
    
            linearextrusionpath.UpdatePath();
            BuildFaces();
        }
    
        protected GLVector3d GetCutIntersect( int iCut, double fPrimHalfWidth )
        {
            double fAngle = (double)iCut / (double)MaxCut * 2 * Math.PI;
            return new GLVector3d( fPrimHalfWidth * Math.Cos( fAngle ), fPrimHalfWidth * Math.Sin( fAngle ), 0 );
        }
        
        protected override double GetAngleWithXAxis( double fCutRatio )
        {
            return fCutRatio * 2 * Math.PI;
        }
   
        // I guess since we are overriding tons of stuff we dont need this; also, quadrant is meaningless for a circle
        protected override int GetCutQuadrant( int iCut )
        {
            return 0;
        }
    
        protected override void BuildFaces()
        {
            //  cout << endl << endl << "PrimitiveCylinderImpl, BuildFaces() >>>" << endl;
            if( iCutStart == 0 && iCutEnd == MaxCut )
            {
                bShowCut = false;
            }
            else
            {
                bShowCut = true;
            }
    
            if( iHollow == 0 )
            {
                bShowHollow = false;
            }
            else
            {
                bShowHollow = true;
            }
    
            double fHollowRatio = (double)iHollow / 100;
    
            GLVector3d cutstartinnerface = null;
            GLVector3d cutendinnerface = null;
    
            GLVector3d cutstartouterface = GetCutIntersect( iCutStart, 0.5 );
            GLVector3d cutendouterface = GetCutIntersect( iCutEnd, 0.5 );
    
            if( bShowHollow )
            {
                cutstartinnerface = GetCutIntersect( iCutStart, fHollowRatio * 0.5 );
                cutendinnerface = GetCutIntersect( iCutEnd, fHollowRatio * 0.5 );
            }
    
            if( bShowCut )
            {
                BuildCutFaces( cutstartouterface, cutstartinnerface, cutendouterface, cutendinnerface );
            }
    
            OuterFaces[0].RemoveAllPoints();
            InnerFaces[0].RemoveAllPoints();
    
            double fAngle = 0;
            double fStartAngle = (double)iCutStart / (double)MaxCut * 2 * Math.PI;
            double fEndAngle = (double)iCutEnd / (double)MaxCut * 2 * Math.PI;
    
            GLVector3d NextOuterPoint = new GLVector3d();
            GLVector3d NextInnerPoint = new GLVector3d();
    
            int iFacePoint = 0;
            for( iFacePoint = 0; iFacePoint <= iLevelOfDetail; iFacePoint++ )
            {
                fAngle = fStartAngle + (double)iFacePoint / (double)iLevelOfDetail * ( fEndAngle - fStartAngle );
    
                NextOuterPoint.x = 0.5 * Math.Cos( fAngle );
                NextOuterPoint.y = 0.5 * Math.Sin( fAngle );
                OuterFaces[0].AddPoint( NextOuterPoint.x, NextOuterPoint.y, NextOuterPoint.z );
            }
    
            if( bShowHollow )
            {
                for( iFacePoint = iLevelOfDetail; iFacePoint >= 0; iFacePoint-- )
                {
                    fAngle = fStartAngle + (double)iFacePoint / (double)iLevelOfDetail * ( fEndAngle - fStartAngle );
    
                    NextInnerPoint.x = 0.5 * Math.Cos( fAngle ) * (double)iHollow / 100.0;
                    NextInnerPoint.y = 0.5 * Math.Sin( fAngle ) * (double)iHollow / 100.0;
                    InnerFaces[0].AddPoint( NextInnerPoint.x, NextInnerPoint.y, NextInnerPoint.z );
                }
            }
    
            OuterFaces[0].TextureMapping = texturemapping;
            InnerFaces[0].TextureMapping = texturemapping;
            
            AssignFaces();
        }
    
        protected override void RenderEndCapNoCutNoHollow( bool IsTop )
        {
            RenderEndCapCutNoHollow( IsTop );
        }

        // we're going to do a fan to avoid calculating the centre.        
        protected override void RenderEndCapCutNoHollow( bool IsTop )
        {
            Vector2 t0 = texturemapping.GetTextureCoordinate( new Vector2( 0.5, 0.5 ) );
            
            if( !IsTop )
            {
                renderer.SetNormal( 0, 0, -1 );    

                GLVector3d p0 = linearextrusionpath.GetTransformedVertex( new GLVector3d( 0, 0, 0 ), 0 );
                
                for( int iFacePoint = 0; iFacePoint < iLevelOfDetail; iFacePoint++ )
                {
                    GLVector3d p1 = OuterFaces[ 0 ].GetTransformedVertex( linearextrusionpath, 0, iFacePoint );
                    GLVector3d p2 = OuterFaces[ 0 ].GetTransformedVertex( linearextrusionpath, 0, iFacePoint + 1 );
        
                    GLVector3d r1 = OuterFaces[ 0 ].GetRawVertex( iFacePoint );
                    GLVector3d r2 = OuterFaces[ 0 ].GetRawVertex( iFacePoint + 1 );

                    Vector2 t1 = texturemapping.GetTextureCoordinate( new Vector2( 1 - ( r1.x + 0.5 ), r1.y + 0.5 ) );
                    Vector2 t2 = texturemapping.GetTextureCoordinate( new Vector2( 1 - ( r2.x + 0.5 ), r2.y + 0.5 ) );

                    renderer.StartTriangle();
        
                    renderer.SetTextureCoord( t0.x, t0.y );
                    renderer.AddVertex( p0.x, p0.y, p0.z );
                    
                    renderer.SetTextureCoord( t2.x, t2.y );
                    renderer.AddVertex( p2.x, p2.y, p2.z );
        
                    renderer.SetTextureCoord( t1.x, t1.y );
                    renderer.AddVertex( p1.x, p1.y, p1.z );
        
                    renderer.EndTriangle();
                }
            }
            else
            {
                renderer.SetNormal( 0, 0, 1 );
                
                GLVector3d p0 = linearextrusionpath.GetTransformedVertex( new GLVector3d( 0, 0, 0 ), linearextrusionpath.NumberOfTransforms - 1 );
                
                for( int iFacePoint = 0; iFacePoint < iLevelOfDetail; iFacePoint++ )
                {
                    GLVector3d p1 = OuterFaces[ 0 ].GetTransformedVertex( linearextrusionpath, linearextrusionpath.NumberOfTransforms - 1, iFacePoint );
                    GLVector3d p2 = OuterFaces[ 0 ].GetTransformedVertex( linearextrusionpath, linearextrusionpath.NumberOfTransforms - 1, iFacePoint + 1 );
        
                    GLVector3d r1 = OuterFaces[ 0 ].GetRawVertex( iFacePoint );
                    GLVector3d r2 = OuterFaces[ 0 ].GetRawVertex( iFacePoint + 1 );

                    Vector2 t1 = texturemapping.GetTextureCoordinate( new Vector2( r1.x + 0.5, r1.y + 0.5 ) );
                    Vector2 t2 = texturemapping.GetTextureCoordinate( new Vector2( r2.x + 0.5, r2.y + 0.5 ) );

                    renderer.StartTriangle();
        
                    renderer.SetTextureCoord( t0.x, t0.y );
                    renderer.AddVertex( p0.x, p0.y, p0.z );
        
                    renderer.SetTextureCoord( t1.x, t1.y );
                    renderer.AddVertex( p1.x, p1.y, p1.z );
        
                    renderer.SetTextureCoord( t2.x, t2.y );
                    renderer.AddVertex( p2.x, p2.y, p2.z );
        
                    renderer.EndTriangle();
                }
            }
        }
    
        protected override void RenderEndCapHollow( bool IsTop )
        {
            if( !IsTop )
            {
                renderer.SetNormal( 0, 0, -1 );
                for( int iFacePoint = 0; iFacePoint < iLevelOfDetail; iFacePoint++ )
                {
                    GLVector3d o1 = OuterFaces[ 0 ].GetTransformedVertex( linearextrusionpath, 0, iFacePoint );
                    GLVector3d o2 = OuterFaces[ 0 ].GetTransformedVertex( linearextrusionpath, 0, iFacePoint + 1 );                    
                    GLVector3d i1 = InnerFaces[ 0 ].GetTransformedVertex( linearextrusionpath, 0, iLevelOfDetail - iFacePoint - 1 );
                    GLVector3d i2 = InnerFaces[ 0 ].GetTransformedVertex( linearextrusionpath, 0, iLevelOfDetail - iFacePoint );
        
                    GLVector3d ro1 = OuterFaces[ 0 ].GetRawVertex( iFacePoint );
                    GLVector3d ro2 = OuterFaces[ 0 ].GetRawVertex( iFacePoint + 1 );
                    GLVector3d ri1 = InnerFaces[ 0 ].GetRawVertex( iLevelOfDetail - iFacePoint - 1 );
                    GLVector3d ri2 = InnerFaces[ 0 ].GetRawVertex( iLevelOfDetail - iFacePoint );
                    
                    Vector2 to1 = texturemapping.GetTextureCoordinate( new Vector2( 1 - ( ro1.x + 0.5 ), ro1.y + 0.5 ) );
                    Vector2 to2 = texturemapping.GetTextureCoordinate( new Vector2( 1 - ( ro2.x + 0.5 ), ro2.y + 0.5 ) );
                    Vector2 ti1 = texturemapping.GetTextureCoordinate( new Vector2( 1 - ( ri1.x + 0.5 ), ri1.y + 0.5 ) );
                    Vector2 ti2 = texturemapping.GetTextureCoordinate( new Vector2( 1 - ( ri2.x + 0.5 ), ri2.y + 0.5 ) );
                    
                    renderer.StartTriangle();
        
                    renderer.SetTextureCoord( to1.x, to1.y );
                    renderer.AddVertex( o1.x, o1.y, o1.z );
        
                    renderer.SetTextureCoord( ti1.x, ti1.y );
                    renderer.AddVertex( i1.x, i1.y, i1.z );
        
                    renderer.SetTextureCoord( to2.x, to2.y );
                    renderer.AddVertex( o2.x, o2.y, o2.z );
        
                    renderer.EndTriangle();
        
        
                    renderer.StartTriangle();
        
                    renderer.SetTextureCoord( to1.x, to1.y );
                    renderer.AddVertex( o1.x, o1.y, o1.z );
        
                    renderer.SetTextureCoord( ti2.x, ti2.y );
                    renderer.AddVertex( i2.x, i2.y, i2.z );
        
                    renderer.SetTextureCoord( ti1.x, ti1.y );
                    renderer.AddVertex( i1.x, i1.y, i1.z );
        
                    renderer.EndTriangle();
                }
            }
            else
            {
                renderer.SetNormal( 0, 0, 1 );
                for( int iFacePoint = 0; iFacePoint < iLevelOfDetail; iFacePoint++ )
                {
                    GLVector3d o1 = OuterFaces[ 0 ].GetTransformedVertex( linearextrusionpath, linearextrusionpath.NumberOfTransforms - 1, iFacePoint );
                    GLVector3d o2 = OuterFaces[ 0 ].GetTransformedVertex( linearextrusionpath, linearextrusionpath.NumberOfTransforms - 1, iFacePoint + 1 );
                    GLVector3d i1 = InnerFaces[ 0 ].GetTransformedVertex( linearextrusionpath, linearextrusionpath.NumberOfTransforms - 1, iLevelOfDetail - iFacePoint - 1 );
                    GLVector3d i2 = InnerFaces[ 0 ].GetTransformedVertex( linearextrusionpath, linearextrusionpath.NumberOfTransforms - 1, iLevelOfDetail - iFacePoint );
        
                    GLVector3d ro1 = OuterFaces[ 0 ].GetRawVertex( iFacePoint );
                    GLVector3d ro2 = OuterFaces[ 0 ].GetRawVertex( iFacePoint + 1 );
                    GLVector3d ri1 = InnerFaces[ 0 ].GetRawVertex( iLevelOfDetail - iFacePoint - 1 );
                    GLVector3d ri2 = InnerFaces[ 0 ].GetRawVertex( iLevelOfDetail - iFacePoint );
                    
                    Vector2 to1 = texturemapping.GetTextureCoordinate( new Vector2( ro1.x + 0.5, ro1.y + 0.5 ) );
                    Vector2 to2 = texturemapping.GetTextureCoordinate( new Vector2( ro2.x + 0.5, ro2.y + 0.5 ) );
                    Vector2 ti1 = texturemapping.GetTextureCoordinate( new Vector2( ri1.x + 0.5, ri1.y + 0.5 ) );
                    Vector2 ti2 = texturemapping.GetTextureCoordinate( new Vector2( ri2.x + 0.5, ri2.y + 0.5 ) );
                    
                    renderer.StartTriangle();
        
                    renderer.SetTextureCoord( to1.x, to1.y );
                    renderer.AddVertex( o1.x, o1.y, o1.z );
        
                    renderer.SetTextureCoord( to2.x, to2.y );
                    renderer.AddVertex( o2.x, o2.y, o2.z );
        
                    renderer.SetTextureCoord( ti1.x, ti1.y );
                    renderer.AddVertex( i1.x, i1.y, i1.z );
        
                    renderer.EndTriangle();
        
        
                    renderer.StartTriangle();
        
                    renderer.SetTextureCoord( to1.x, to1.y );
                    renderer.AddVertex( o1.x, o1.y, o1.z );
        
                    renderer.SetTextureCoord( ti1.x, ti1.y );
                    renderer.AddVertex( i1.x, i1.y, i1.z );
        
                    renderer.SetTextureCoord( ti2.x, ti2.y );
                    renderer.AddVertex( i2.x, i2.y, i2.z );
        
                    renderer.EndTriangle();
                }
            }
        }
    
        protected override void AssignFaces()
        {
            ArrayList FacesAL = new ArrayList();
            
            FacesAL.Add( new OuterFace( 1, 0 ) );
    
            if( bShowHollow )
            {
                FacesAL.Add( new InnerFace( 2, 0 ) );
            }
    
            if( bShowCut )
            {
                int iCutStartFace = 3;
                int iCutEndFace = 4;
                if( bShowHollow )
                {
                    iCutStartFace = 4;
                    iCutEndFace = 5;
                }

                FacesAL.Add( new CutFace( iCutStartFace, 0 ) );
                FacesAL.Add( new CutFace( iCutEndFace, 1 ) );
            }
    
            if( !bShowHollow )
            {
                if( !bShowCut )
                {
                    FacesAL.Add( new EndCapNoCutNoHollow( 2, false ) );
                    FacesAL.Add( new EndCapNoCutNoHollow( 0, true ) );
                }
                else
                {
                    FacesAL.Add( new EndCapCutNoHollow( 2, false ) );
                    FacesAL.Add( new EndCapCutNoHollow( 0, true ) );
                }
            }
            else
            {
                FacesAL.Add( new EndCapHollow( 3, false ) );
                FacesAL.Add( new EndCapHollow( 0, true ) );
            }    
    
            Faces = (Face[])FacesAL.ToArray( typeof( Face ) );
        }
    }
}

