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
    public abstract class RotationalPrimitive : Primitive
    {
        // properties
        protected GLVector3d[] ReferenceVertices; //!< reference vertices of our prim, to build the crosssections etc
        
        protected CrossSection[] OuterFaces = new CrossSection[4]; //!< section for each extruded outer face
        protected CrossSection[] InnerFaces = new CrossSection[4]; //!< section for each extruded inner face (hollow)
        
        protected int iFirstOuterFace; //!< If we're cutting, this might not be 0
        protected int iLastOuterFace; //!< If we're cutting, this might not be iNumberFaces
        
        protected CrossSection[] CutFaces = new CrossSection[2]; //!< Two cut faces
        protected int iNumberFaces; //!< Number of faces on base primitive (set in derived primitive's constructor)
        
        protected bool bShowCut;
        protected bool bShowHollow;
        protected bool bShowEndCaps;
        
        protected int iExtrusionStart = 0; //!< 0 to 200
        protected int iExtrusionEnd = 200; //!< 0 to 200
        
        protected int iHoleSize = 50; // 0 to 199
        
        protected RotationalExtrusionPath rotationalextrusionpath;
        
        // constructors
        
        public RotationalPrimitive()
        {
            for( int i = 0; i < 4; i++ )
            {
                OuterFaces[i] = new CrossSection();
                InnerFaces[i] = new CrossSection();
            }
            CutFaces[0] = new CrossSection();
            CutFaces[1] = new CrossSection();
            
            rotationalextrusionpath = new RotationalExtrusionPath();
            rotationalextrusionpath.LevelOfDetail = iLevelOfDetail;
            
            UpdateExtrusionScaling();
        }
        
        // get / set accessors

        public int HoleSize{
            set{
                iHoleSize = value;
                UpdateExtrusionScaling();
            }
        }
    
        public override int LevelOfDetail{
            set{
                iLevelOfDetail = value;
                rotationalextrusionpath.LevelOfDetail = value;
            }
        }
        public override int Twist{
            set{
                rotationalextrusionpath.Twist = value;
            }
            get{ return rotationalextrusionpath.Twist; }
        }
        public override double Shear{
            set{
                rotationalextrusionpath.Shear = value;
            }
            get{ return rotationalextrusionpath.Shear; }
        }
        public override double TopSizeX{
            set{
                rotationalextrusionpath.TopSizeX = value;
            }
            get{ return rotationalextrusionpath.TopSizeX; }
        }
        public override double TopSizeY{
            set{
                rotationalextrusionpath.TopSizeY = value;
            }
            get{ return rotationalextrusionpath.TopSizeY; }
        }
        public override int Hollow{
            set{
                iHollow = value;
        
                if( iHollow > 95 )
                {
                    iHollow = 95;
                }
                else if( iHollow < 0 )
                {
                    iHollow = 0;
                }    
                
                if( iHollow == 0 )
                {
                    bShowHollow = false;
                }
            }
            get{ return iHollow; }
        }
        public override int CutStart{
            set{
                iExtrusionStart = value;
                rotationalextrusionpath.CutStartAngle = (double)value * 2 * Math.PI / 200.0;
            }
            get{ return iExtrusionStart; }
        }
        public override int CutEnd{
            set{
                iExtrusionEnd = value;
                rotationalextrusionpath.CutEndAngle = (double)value * 2 * Math.PI / 200.0;
            }
            get{ return iExtrusionEnd; }
        }    
        public override int AdvancedCutStart{
            set{
                iCutStart = value;
                if( iCutStart < 0 )
                {
                    iCutStart = 0;
                }
                else if( iCutEnd >= MaxCut )
                {
                    iCutEnd = MaxCut - 1;
                }
        
                if( iCutStart >= iCutEnd )
                {
                    iCutEnd = iCutStart + 1;
                }
        
                BuildFaces();
            }
            get{ return iCutStart; }
        }
        public override int AdvancedCutEnd{
            set{
                iCutEnd = value;
                if( iCutEnd < 1 )
                {
                    iCutEnd = 1;
                }
                else if( iCutEnd > MaxCut )
                {
                    iCutEnd = MaxCut;
                }
        
                if( iCutEnd <= iCutStart )
                {
                    iCutStart = iCutEnd - 1;
                }
        
                BuildFaces();
            }
            get{ return iCutEnd; }
        }
                
        // public methods
   
        public override void UpdateTransforms()
        {
            rotationalextrusionpath.UpdatePath();
            BuildFaces();
        }
        
        // abstract protected methods
        protected abstract int GetCutQuadrant( int iCut );
        protected abstract double GetAngleWithXAxis( double fCutRatio );
        
        // protected / private methods

        protected void UpdateExtrusionScaling()
        {
            double fHoleSize = (double)iHoleSize / 200.0;
            rotationalextrusionpath.RadialSectionScale = 0.5 - fHoleSize / 2.0;
            rotationalextrusionpath.Radius = 0.25 + fHoleSize / 4.0;
        }

        protected int NormalizeQuadrant( int iQuadrant )
        {
            return ( ( iQuadrant % iNumberFaces ) + iNumberFaces ) % iNumberFaces;
        }
    
        protected void SendRendererCallbacksToCrossSections()
        {
            for( int i = 0; i < 4; i++ )
            {
                OuterFaces[i].SetRenderer( renderer );
                InnerFaces[i].SetRenderer( renderer );
            }
            CutFaces[0].SetRenderer( renderer );
            CutFaces[1].SetRenderer( renderer );
        }
   
        //! BAsically we check the quadrant, then intersect it, then we just intersect the radius with the appropriate face
        protected virtual GLVector3d GetCutIntersect( int iCut, double fCubeHalfWidth )
        {
            int iCutQuadrant = GetCutQuadrant( iCut );
            double fCutRatio = (double)iCut / (double)MaxCut;
    
            GLVector3d linestart = ReferenceVertices[ iCutQuadrant] * fCubeHalfWidth / 0.5;
            GLVector3d lineend = null;
            if( iCutQuadrant < iNumberFaces - 1 )
            {
                lineend = ReferenceVertices[ iCutQuadrant + 1 ]  * fCubeHalfWidth / 0.5;
            }
            else
            {
                lineend = ReferenceVertices[ 0 ]  * fCubeHalfWidth / 0.5;
            }
    
            double fAngle = GetAngleWithXAxis( fCutRatio );
    
            GLVector3d CutVectorPerp = new GLVector3d( - Math.Sin( fAngle ), Math.Cos( fAngle ), 0 );
    
            // Grabbed this from http://softsurfer.com/Archive/algorithm_0104/algorithm_0104B.htm
            return linestart - ( lineend - linestart ) * CutVectorPerp.dot( linestart ) / CutVectorPerp.dot( lineend - linestart );
        }
    
        protected double PopulateSingleCutFacePositiveDirection( ref CrossSection face, GLVector3d CutPoint, int iQuadrant, double fHalfCubeWidth, bool bOuter )
        {
            iQuadrant = NormalizeQuadrant( iQuadrant );
    
            face.RemoveAllPoints();
        
            GLVector3d StartPoint = new GLVector3d( CutPoint );
            GLVector3d EndPoint = null;
            if( iQuadrant < iNumberFaces - 1 )
            {
                EndPoint = ReferenceVertices[ iQuadrant + 1 ] * fHalfCubeWidth / 0.5;
            }
            else
            {
                EndPoint = ReferenceVertices[ 0 ] * fHalfCubeWidth / 0.5;
            }
    
            if( bOuter )
            {
                face.AddPoint( StartPoint );
                face.AddPoint( EndPoint );
            }
            else
            {
                face.AddPoint( EndPoint );
                face.AddPoint( StartPoint );
            }
            return ( EndPoint - StartPoint ).length();
        }
    
        protected double PopulateSingleCutFaceNegativeDirection( ref CrossSection face, GLVector3d CutPoint, int iQuadrant, double fHalfCubeWidth, bool bOuter )
        {
            iQuadrant = NormalizeQuadrant( iQuadrant );
    
            face.RemoveAllPoints();
            
            GLVector3d StartPoint = ReferenceVertices[ iQuadrant ] * fHalfCubeWidth / 0.5;
            GLVector3d EndPoint = new GLVector3d( CutPoint );
            
            if( bOuter )
            {
                face.AddPoint( StartPoint );
                face.AddPoint( EndPoint );
            }
            else
            {
                face.AddPoint( EndPoint );
                face.AddPoint( StartPoint );
            }
            return ( EndPoint - StartPoint ).length();
        }
    
        protected double PopulateCompleteSide( ref CrossSection face, int iQuadrant, double fHalfCubeWidth, bool bOuter )
        {
            iQuadrant = NormalizeQuadrant( iQuadrant );
    
            face.RemoveAllPoints();
    
            GLVector3d StartPoint = ReferenceVertices[ iQuadrant ];
            GLVector3d EndPoint = null;
            if( iQuadrant < iNumberFaces - 1 )
            {
                EndPoint = ReferenceVertices[ iQuadrant + 1 ];
            }
            else
            {
                EndPoint = ReferenceVertices[ 0 ];
            }
    
            StartPoint = StartPoint * fHalfCubeWidth / 0.5;
            EndPoint = EndPoint * fHalfCubeWidth / 0.5;
    
            if( bOuter )
            {
                face.AddPoint( StartPoint.x, StartPoint.y, StartPoint.z );
                face.AddPoint( EndPoint.x, EndPoint.y, EndPoint.z );
            }
            else
            {
                face.AddPoint( EndPoint.x, EndPoint.y, EndPoint.z );
                face.AddPoint( StartPoint.x, StartPoint.y, StartPoint.z );
            }
    
            return 2 * fHalfCubeWidth;
        }
    
        protected virtual void RenderEndCapNoHollow( bool IsTop )
        {    
            GLVector3d rawcentre = new GLVector3d();
            
            if( !IsTop )
            {
                GLVector3d p0 = rotationalextrusionpath.GetTransformedVertex( new GLVector3d( 0, 0, 0 ), 0 );
                Vector2 t0 = texturemapping.GetTextureCoordinate( new Vector2( 0.5, 0.5 ) );
                
                for( int i = iFirstOuterFace; i <= iLastOuterFace; i++ )
                {
                    GLVector3d p1 = OuterFaces[i].GetTransformedVertex( rotationalextrusionpath, 0, 0 );
                    GLVector3d p2 = OuterFaces[i].GetTransformedVertex( rotationalextrusionpath, 0, 1 );
        
                    GLVector3d r1 = OuterFaces[i].GetRawVertex( 0 );
                    GLVector3d r2 = OuterFaces[i].GetRawVertex( 1 );

                    Vector2 t1 = texturemapping.GetTextureCoordinate( new Vector2( 1 - ( r1.x + 0.5 ), r1.y + 0.5 ) );
                    Vector2 t2 = texturemapping.GetTextureCoordinate( new Vector2( 1 - ( r2.x + 0.5 ), r2.y + 0.5 ) );
                    
                    GLVector3d normal = CalculateNormal( p2,p1,p0 );
                    renderer.SetNormal( normal.x, normal.y, normal.z );
        
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
                GLVector3d p0 = rotationalextrusionpath.GetTransformedVertex( new GLVector3d( 0, 0, 0 ), rotationalextrusionpath.NumberOfTransforms - 1 );
                Vector2 t0 = texturemapping.GetTextureCoordinate( new Vector2( 0.5, 0.5 ) );
                
                for( int i = iFirstOuterFace + 1; i < iLastOuterFace; i++ )
                {
                    GLVector3d p1 = OuterFaces[i].GetTransformedVertex( rotationalextrusionpath, rotationalextrusionpath.NumberOfTransforms - 1, 0 );
                    GLVector3d p2 = OuterFaces[i].GetTransformedVertex( rotationalextrusionpath, rotationalextrusionpath.NumberOfTransforms - 1, 1 );
        
                    GLVector3d r1 = OuterFaces[i].GetRawVertex( 0 );
                    GLVector3d r2 = OuterFaces[i].GetRawVertex( 1 );

                    Vector2 t1 = texturemapping.GetTextureCoordinate( new Vector2( r1.x + 0.5, r1.y + 0.5 ) );
                    Vector2 t2 = texturemapping.GetTextureCoordinate( new Vector2( r2.x + 0.5, r2.y + 0.5 ) );
                    
                    GLVector3d normal = CalculateNormal( p2,p1,p0 );
                    renderer.SetNormal( normal.x, normal.y, normal.z );
        
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
    
        protected virtual void RenderEndCapHollow( bool IsTop )
        {
            if( !IsTop )
            {
                for( int i = iFirstOuterFace; i <= iLastOuterFace; i++ )
                {
                    GLVector3d p1 = OuterFaces[i].GetTransformedVertex( rotationalextrusionpath, 0, 0 );
                    GLVector3d p2 = OuterFaces[i].GetTransformedVertex( rotationalextrusionpath, 0, 1 );
                    GLVector3d p3 = InnerFaces[i].GetTransformedVertex( rotationalextrusionpath, 0, 0 );
                    GLVector3d p4 = InnerFaces[i].GetTransformedVertex( rotationalextrusionpath, 0, 1 );
        
                    GLVector3d r1 = OuterFaces[i].GetRawVertex( 0 );
                    GLVector3d r2 = OuterFaces[i].GetRawVertex( 1 );
                    GLVector3d r3 = InnerFaces[i].GetRawVertex( 0 );
                    GLVector3d r4 = InnerFaces[i].GetRawVertex( 1 );
    
                    Vector2 t1 = texturemapping.GetTextureCoordinate( new Vector2( 1 - ( r1.x + 0.5 ), r1.y + 0.5 ) );
                    Vector2 t2 = texturemapping.GetTextureCoordinate( new Vector2( 1 - ( r2.x + 0.5 ), r2.y + 0.5 ) );
                    Vector2 t3 = texturemapping.GetTextureCoordinate( new Vector2( 1 - ( r3.x + 0.5 ), r3.y + 0.5 ) );
                    Vector2 t4 = texturemapping.GetTextureCoordinate( new Vector2( 1 - ( r4.x + 0.5 ), r4.y + 0.5 ) );
                
                    GLVector3d normal = CalculateNormal( p4,p3,p2,p1 );
                    renderer.SetNormal( normal.x, normal.y, normal.z );
        
                    renderer.StartTriangle();
        
                    renderer.SetTextureCoord( t4.x, t4.y );
                    renderer.AddVertex( p4.x, p4.y, p4.z );
        
                    renderer.SetTextureCoord( t3.x, t3.y );
                    renderer.AddVertex( p3.x, p3.y, p3.z );
        
                    renderer.SetTextureCoord( t2.x, t2.y );
                    renderer.AddVertex( p2.x, p2.y, p2.z );
        
                    renderer.EndTriangle();
        
                    renderer.StartTriangle();
        
                    renderer.SetTextureCoord( t2.x, t2.y );
                    renderer.AddVertex( p2.x, p2.y, p2.z );
        
                    renderer.SetTextureCoord( t1.x, t1.y );
                    renderer.AddVertex( p1.x, p1.y, p1.z );
        
                    renderer.SetTextureCoord( t4.x, t4.y );
                    renderer.AddVertex( p4.x, p4.y, p4.z );
        
                    renderer.EndTriangle();
                }
            }
            else
            {
                for( int i = iFirstOuterFace; i <= iLastOuterFace; i++ )
                {
                    GLVector3d p1 = OuterFaces[i].GetTransformedVertex( rotationalextrusionpath, rotationalextrusionpath.NumberOfTransforms - 1, 0 );
                    GLVector3d p2 = OuterFaces[i].GetTransformedVertex( rotationalextrusionpath, rotationalextrusionpath.NumberOfTransforms - 1, 1 );
                    GLVector3d p3 = InnerFaces[i].GetTransformedVertex( rotationalextrusionpath, rotationalextrusionpath.NumberOfTransforms - 1, 0 );
                    GLVector3d p4 = InnerFaces[i].GetTransformedVertex( rotationalextrusionpath, rotationalextrusionpath.NumberOfTransforms - 1, 1 );
        
                    GLVector3d r1 = OuterFaces[i].GetRawVertex( 0 );
                    GLVector3d r2 = OuterFaces[i].GetRawVertex( 1 );
                    GLVector3d r3 = InnerFaces[i].GetRawVertex( 0 );
                    GLVector3d r4 = InnerFaces[i].GetRawVertex( 1 );
    
                    Vector2 t1 = texturemapping.GetTextureCoordinate( new Vector2( r1.x + 0.5, r1.y + 0.5 ) );
                    Vector2 t2 = texturemapping.GetTextureCoordinate( new Vector2( r2.x + 0.5, r2.y + 0.5 ) );
                    Vector2 t3 = texturemapping.GetTextureCoordinate( new Vector2( r3.x + 0.5, r3.y + 0.5 ) );
                    Vector2 t4 = texturemapping.GetTextureCoordinate( new Vector2( r4.x + 0.5, r4.y + 0.5 ) );
                
                    GLVector3d normal = CalculateNormal( p1,p2,p3, p4 );
                    renderer.SetNormal( normal.x, normal.y, normal.z );
        
                    renderer.StartTriangle();
        
                    renderer.SetTextureCoord( t1.x, t1.y );
                    renderer.AddVertex( p1.x, p1.y, p1.z );
        
                    renderer.SetTextureCoord( t2.x, t2.y );
                    renderer.AddVertex( p2.x, p2.y, p2.z );
        
                    renderer.SetTextureCoord( t3.x, t3.y );
                    renderer.AddVertex( p3.x, p3.y, p3.z );
        
                    renderer.EndTriangle();
        
                    renderer.StartTriangle();
        
                    renderer.SetTextureCoord( t3.x, t3.y );
                    renderer.AddVertex( p3.x, p3.y, p3.z );
        
                    renderer.SetTextureCoord( t4.x, t4.y );
                    renderer.AddVertex( p4.x, p4.y, p4.z );
        
                    renderer.SetTextureCoord( t1.x, t1.y );
                    renderer.AddVertex( p1.x, p1.y, p1.z );
        
                    renderer.EndTriangle();
                }
            }
        }
    
        protected void BuildCutFaces( GLVector3d cutstartouterface, GLVector3d cutstartinnerface, GLVector3d cutendouterface, GLVector3d cutendinnerface )
        {
            CutFaces[0].RemoveAllPoints();
            if( bShowHollow )
            {
                CutFaces[0].AddPoint(cutstartinnerface );
            }
            else
            {
                CutFaces[0].AddPoint( 0, 0, 0 );
            }
            CutFaces[0].AddPoint(cutstartouterface );
            CutFaces[0].TextureMapping = texturemapping;
    
            CutFaces[1].RemoveAllPoints();
            CutFaces[1].AddPoint(cutendouterface );
            if( bShowHollow )
            {
                CutFaces[1].AddPoint(cutendinnerface );
            }
            else
            {
                CutFaces[1].AddPoint( 0, 0, 0 );
            }    
            CutFaces[1].TextureMapping = texturemapping;
        }
    
        void SetupInnerFaceTextureOffsets( double fStartSideInnerLength, double fWholeSideLength, double fTotalInnerLength )
        {
            int iQuadrant;
    
            InnerFaces[iFirstOuterFace].TextureMapping = new TextureMapping( texturemapping );
            InnerFaces[iFirstOuterFace].TextureMapping.PreTransformOffsetX = ( fTotalInnerLength - fStartSideInnerLength ) / fTotalInnerLength;
            InnerFaces[iFirstOuterFace].TextureMapping.PreTransformScaleX = fTotalInnerLength / fStartSideInnerLength;
    
            double fCumulativeInnerLength = fStartSideInnerLength;
            for( iQuadrant = iFirstOuterFace + 1; iQuadrant < iLastOuterFace; iQuadrant++ )
            {
                fCumulativeInnerLength += fWholeSideLength;
                InnerFaces[iQuadrant].TextureMapping = new TextureMapping( texturemapping );
                InnerFaces[iQuadrant].TextureMapping.PreTransformOffsetX = ( fTotalInnerLength - fCumulativeInnerLength ) / fTotalInnerLength;
                InnerFaces[iQuadrant].TextureMapping.PreTransformScaleX = fTotalInnerLength / fWholeSideLength;
            }
    
            InnerFaces[iLastOuterFace].TextureMapping = new TextureMapping( texturemapping );
            InnerFaces[iLastOuterFace].TextureMapping.PreTransformOffsetX = 0;
            InnerFaces[iLastOuterFace].TextureMapping.PreTransformScaleX = 1 / ( 1 - fCumulativeInnerLength / fTotalInnerLength );
        }
    
        protected override void BuildFaces()
        {
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
    
            int iCutStartDiagQuadrant, iCutEndDiagQuadrant; //!< quadrant 0 is -45 to +45 degrees, quad 2 is +45 to +135, etc
    
            iCutStartDiagQuadrant = GetCutQuadrant( iCutStart );
            iCutEndDiagQuadrant = GetCutQuadrant( iCutEnd - 1 );
    
            GLVector3d cutstartouterface = GetCutIntersect( iCutStart, 0.5 );
            GLVector3d cutendouterface = GetCutIntersect( iCutEnd, 0.5 );
    
            GLVector3d cutstartinnerface = null;
            GLVector3d cutendinnerface = null;
    
            if( bShowHollow )
            {
                cutstartinnerface = GetCutIntersect( iCutStart, fHollowRatio * 0.5 );
                cutendinnerface = GetCutIntersect( iCutEnd, fHollowRatio * 0.5 );
            }
        
            if( bShowCut )
            {
                BuildCutFaces( cutstartouterface, cutstartinnerface, cutendouterface, cutendinnerface );
            }
    
            if( iCutStartDiagQuadrant == iCutEndDiagQuadrant )
            {
                iFirstOuterFace = iLastOuterFace = iCutStartDiagQuadrant;
                OuterFaces[0].RemoveAllPoints();
                OuterFaces[0].AddPoint( cutstartouterface );
                OuterFaces[0].AddPoint( cutendouterface );
                OuterFaces[0].TextureMapping = texturemapping;
    
                if( bShowHollow )
                {
                    InnerFaces[0].RemoveAllPoints();
                    InnerFaces[0].AddPoint( cutstartinnerface );
                    InnerFaces[0].AddPoint( cutendinnerface );
                    InnerFaces[0].TextureMapping = texturemapping;
                }
            }
            else
            {
                iFirstOuterFace = iCutStartDiagQuadrant;
    
                double fTotalInnerLength = 0;
    
                double fStartSideInnerLength = 0;
                double fWholeSideLength = 0;
                double fEndSideInnerLength = 0;
    
                PopulateSingleCutFacePositiveDirection( ref OuterFaces[iFirstOuterFace], cutstartouterface, iCutStartDiagQuadrant, 0.5, true );
                OuterFaces[iFirstOuterFace].TextureMapping = texturemapping;
                if( bShowHollow )
                {
                    fStartSideInnerLength = PopulateSingleCutFacePositiveDirection( ref InnerFaces[iFirstOuterFace], cutstartinnerface, iCutStartDiagQuadrant, fHollowRatio * 0.5, false );
                    fTotalInnerLength += fStartSideInnerLength;
                }
    
                int iQuadrant = iCutStartDiagQuadrant + 1;
                while( iQuadrant < iCutEndDiagQuadrant )
                {
                    PopulateCompleteSide( ref OuterFaces[ iQuadrant ], iQuadrant, 0.5, true );
                    OuterFaces[iQuadrant].TextureMapping = texturemapping;
                    if( bShowHollow )
                    {
                        fWholeSideLength = PopulateCompleteSide( ref InnerFaces[ iQuadrant ], iQuadrant, fHollowRatio * 0.5, false );
                        fTotalInnerLength += fWholeSideLength;
                    }
                    iQuadrant++;
                }
    
                PopulateSingleCutFaceNegativeDirection( ref OuterFaces[iQuadrant], cutendouterface, iCutEndDiagQuadrant, 0.5, true );
                OuterFaces[iQuadrant].TextureMapping = texturemapping;
                if( bShowHollow )
                {
                    fEndSideInnerLength = PopulateSingleCutFaceNegativeDirection( ref InnerFaces[iQuadrant], cutendinnerface, iCutEndDiagQuadrant, fHollowRatio * 0.5, false );
                    fTotalInnerLength += fEndSideInnerLength;
                }
    
                iLastOuterFace = iQuadrant;
    
                if( bShowHollow )
                {
                    SetupInnerFaceTextureOffsets( fStartSideInnerLength, fWholeSideLength, fTotalInnerLength );
                }
            }
            AssignFaces();
        }

        protected override void RenderFace( Face thisface )
        {
            ApplyFaceColor( thisface.FaceNum );
            renderer.SetTextureId( iFaceTextures[ thisface.FaceNum ] );
            
            if( thisface is OuterFace )
            {
                OuterFace outerface = thisface as OuterFace;
                ( OuterFaces[ outerface.OuterFaceIndex ] ).Render( rotationalextrusionpath );
            }
            else if( thisface is InnerFace )
            {
                InnerFace innerface = thisface as InnerFace;
                ( InnerFaces[ innerface.InnerFaceIndex ] ).Render( rotationalextrusionpath );
            }
            else if( thisface is CutFace )
            {
                CutFace cutface = thisface as CutFace;
                ( CutFaces[cutface.CutFaceIndex] ).Render( rotationalextrusionpath );
            }
            else if ( thisface is EndCap )
            {
                EndCap endcap = thisface as EndCap;
                if( thisface is EndCapNoHollow )
                {
                    RenderEndCapNoHollow( endcap.IsTop );
                }
                else if( thisface is EndCapHollow )
                {
                    RenderEndCapHollow( endcap.IsTop );
                }
            }
        }        
    }
}
