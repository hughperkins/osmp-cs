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
    // handles primitives made by extruding a CrossSection along a line
    // Conventions for any descriptions in this class:
    // For anything in two dimesions involving cuts and so on, x points right, y points up
    // Quadrants rotate anticlockwise starting from the most-clockwise vertex of quadrant 0
    // the quadrants are defined to include exact complete sides, so for example for a cube quadrant zero starts from one corner
    public abstract class LinearPrimitive : Primitive
    {
        // properties        
        protected GLVector3d[] ReferenceVertices; //!< reference vertices of our prim, to build the crosssections etc
        
        protected CrossSection[] OuterFaces = new CrossSection[4]; //!< section for each extruded outer face
        protected CrossSection[] InnerFaces = new CrossSection[4]; //!< section for each extruded inner face (hollow)
        
        protected int iFirstOuterFace; //!< If we're cutting, this might not be 0
        protected int iLastOuterFace; //!< If we're cutting, this might not be iNumberFaces
        
        protected CrossSection[] CutFaces = new CrossSection[2]; //!< Two cut faces
        protected int iNumberFaces; //!< Number of faces on base primitive (set in derived primitive's constructor)
        
        protected bool bShowCut = false;
        protected bool bShowHollow = false;
        
        protected LinearExtrusionPath linearextrusionpath;

        // constructor(s)
        public LinearPrimitive()
        {
            linearextrusionpath = new LinearExtrusionPath();
            linearextrusionpath.LevelOfDetail =  iLevelOfDetail;
            
            for( int i = 0; i < 4; i++ )
            {
                OuterFaces[i] = new CrossSection();
                InnerFaces[i] = new CrossSection();
            }
            CutFaces[0] = new CrossSection();
            CutFaces[1] = new CrossSection();
        }

        // get/set accessors
        
        public override int LevelOfDetail{
            set{
                iLevelOfDetail = value;
                linearextrusionpath.LevelOfDetail = value;
            }
        }
        public override int Twist{
            set{
                linearextrusionpath.Twist = value;
            }
            get{ return linearextrusionpath.Twist; }
        }
        public override double Shear{
            set{
                linearextrusionpath.Shear = value;
            }
            get{ return linearextrusionpath.Shear; }
        }
        public override double TopSizeX{
            set{
                linearextrusionpath.TopSizeX = value;
            }
            get{ return linearextrusionpath.TopSizeX; }
        }
        public override double TopSizeY{
            set{
                linearextrusionpath.TopSizeY = value;
            }
            get{ return linearextrusionpath.TopSizeY; }
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
        public override int CutEnd{
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
        
                //cout << "SetCutEnd() " << iCutStart << " " << iCutEnd << endl;
        
                BuildFaces();
            }
            get{ return iCutEnd; }
        }
        
        // public methods

       // public abstract override void Render(); 
            
        public override void UpdateTransforms()
        {
            linearextrusionpath.UpdatePath();
            BuildFaces();
        }
        
        // protected abstract methods

        // protected override abstract void AssignFaces();
        
        // should return angle in radians for a given cut ratio (?)
        protected abstract double GetAngleWithXAxis( double fCutRatio );   
        
        // should return quadrant index, starting from 0, eg for cube this will be 0 for 0 <= cut < 50, 1 for 50 <= cut < 100 ,and so on. 
        protected abstract int GetCutQuadrant( int iCut );

        // protected methods

        protected override void RenderFace( Face thisface )
        {
            ApplyFaceColor( thisface.FaceNum );
            renderer.SetTextureId( iFaceTextures[ thisface.FaceNum ] );
            
            if( thisface is OuterFace )
            {
                OuterFace outerface = thisface as OuterFace;
                ( OuterFaces[ outerface.OuterFaceIndex ] ).Render( linearextrusionpath );
            }
            else if( thisface is InnerFace )
            {
                InnerFace innerface = thisface as InnerFace;
                ( InnerFaces[ innerface.InnerFaceIndex ] ).Render( linearextrusionpath );
            }
            else if( thisface is CutFace )
            {
                CutFace cutface = thisface as CutFace;
                ( CutFaces[cutface.CutFaceIndex] ).Render( linearextrusionpath );
            }
            else if ( thisface is EndCap )
            {
                EndCap endcap = thisface as EndCap;
                if( thisface is EndCapNoCutNoHollow )
                {
                    RenderEndCapNoCutNoHollow( endcap.IsTop );
                }
                else if( thisface is EndCapCutNoHollow )
                {
                    RenderEndCapCutNoHollow( endcap.IsTop );
                }
                else if( thisface is EndCapHollow )
                {
                    RenderEndCapHollow( endcap.IsTop );
                }
            }
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
	
        // This does the bulk of the hard work
        // What we are doing is taking the reference points defined in ReferencePoints, and working out where the cuts intersect the reference polygon
        // We also consider hollow
        // Altogether there are outside faces, inside faces (if hollowed), and cut faces (if cut) to consider...
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
    
            GLVector3d cutstartinnerface = null;
            GLVector3d cutendinnerface = null;
    
            //!< quadrant 0 is -45 to +45 degrees, quad 2 is +45 to +135, etc
            int iCutStartDiagQuadrant = GetCutQuadrant( iCutStart );
            int iCutEndDiagQuadrant = GetCutQuadrant( iCutEnd - 1 );

            GLVector3d cutstartouterface = GetCutIntersect( iCutStart, 0.5 );  // coordinates of where cut starts (for cut= 0 if no visible cut)
            GLVector3d cutendouterface = GetCutIntersect( iCutEnd, 0.5 );  // coordinates of where cut starts (for cut= 200 if no visible cut)
            
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
                    InnerFaces[0].AddPoint( cutendinnerface );
                    InnerFaces[0].AddPoint( cutstartinnerface );
                    InnerFaces[0].TextureMapping = texturemapping;
                }
            }
            else
            {
                iFirstOuterFace = iCutStartDiagQuadrant;
                double fTotalInnerLength = 0;
                double fStartSideInnerLength = 0;
                double fWholeSideLength = 0;
    
                PopulateSingleCutFacePositiveDirection( ref OuterFaces[iFirstOuterFace], cutstartouterface, iCutStartDiagQuadrant, 0.5, true );
                OuterFaces[iFirstOuterFace].TextureMapping = texturemapping;
                if( bShowHollow )
                {
                    fStartSideInnerLength = PopulateSingleCutFacePositiveDirection( ref InnerFaces[iFirstOuterFace], cutstartinnerface, iCutStartDiagQuadrant, fHollowRatio * 0.5, false );
                    InnerFaces[iFirstOuterFace].TextureMapping = texturemapping;
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
                        InnerFaces[iQuadrant].TextureMapping = texturemapping;
                        fTotalInnerLength += fWholeSideLength;
                    }
                    iQuadrant++;
                }
    
                PopulateSingleCutFaceNegativeDirection( ref OuterFaces[iQuadrant], cutendouterface, iCutEndDiagQuadrant, 0.5, true );
                OuterFaces[iQuadrant].TextureMapping = texturemapping;
                if( bShowHollow )
                {
                    double fEndSideInnerLength = PopulateSingleCutFaceNegativeDirection( ref InnerFaces[iQuadrant], cutendinnerface, iCutEndDiagQuadrant, fHollowRatio * 0.5, false );
                    InnerFaces[iQuadrant].TextureMapping = texturemapping;
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
        
        // note to self: can probably simplify this somewhat....
        protected virtual void RenderEndCapHollow( bool IsTop )
        {
            for( int i = iFirstOuterFace; i <= iLastOuterFace; i++ )
            {
                if( !IsTop )
                {
                    GLVector3d p1 = OuterFaces[i].GetTransformedVertex( linearextrusionpath, 0, 0 );
                    GLVector3d p2 = OuterFaces[i].GetTransformedVertex( linearextrusionpath, 0, 1 );
                    GLVector3d p3 = InnerFaces[i].GetTransformedVertex( linearextrusionpath, 0, 0 );
                    GLVector3d p4 = InnerFaces[i].GetTransformedVertex( linearextrusionpath, 0, 1 );

                    GLVector3d r1 = OuterFaces[i].GetRawVertex( 0 );
                    GLVector3d r2 = OuterFaces[i].GetRawVertex( 1 );
                    GLVector3d r3 = InnerFaces[i].GetRawVertex( 0 );
                    GLVector3d r4 = InnerFaces[i].GetRawVertex( 1 );

                    Vector2 t1 = texturemapping.GetTextureCoordinate( new Vector2( 1 - (r1.x + 0.5 ), r1.y + 0.5 ) );
                    Vector2 t2 = texturemapping.GetTextureCoordinate( new Vector2( 1 - (r2.x + 0.5 ), r2.y + 0.5 ) );
                    Vector2 t3 = texturemapping.GetTextureCoordinate( new Vector2( 1 - (r3.x + 0.5 ), r3.y + 0.5 ) );
                    Vector2 t4 = texturemapping.GetTextureCoordinate( new Vector2( 1 - (r4.x + 0.5 ), r4.y + 0.5 ) );
                    
                    //renderer.SetTextureId( iFaceTextures[ iBottomTextureID ] );
                    renderer.SetNormal( 0, 0, -1 );
        
                    renderer.StartTriangle();
        
                    renderer.SetTextureCoord( t4.x, t4.y );
                    renderer.AddVertex( p4.x, p4.y, p4.z );
        
                    renderer.SetTextureCoord( t3.x, t3.y );
                    renderer.AddVertex( p3.x, p3.y, p3.z );
        
                    renderer.SetTextureCoord( t2.x, t2.y );
                    renderer.AddVertex( p2.x, p2.y, p2.z );
        
                    renderer.EndTriangle();
        
                    renderer.StartTriangle();
        
                    renderer.SetTextureCoord( t4.x, t4.y );
                    renderer.AddVertex( p4.x, p4.y, p4.z );
        
                    renderer.SetTextureCoord( t2.x, t2.y );
                    renderer.AddVertex( p2.x, p2.y, p2.z );
        
                    renderer.SetTextureCoord( t1.x, t1.y );
                    renderer.AddVertex( p1.x, p1.y, p1.z );
                    
                    renderer.EndTriangle();
                }
                else
                {
                    GLVector3d p1 = OuterFaces[i].GetTransformedVertex( linearextrusionpath, linearextrusionpath.NumberOfTransforms - 1, 0 );
                    GLVector3d p2 = OuterFaces[i].GetTransformedVertex( linearextrusionpath, linearextrusionpath.NumberOfTransforms - 1, 1 );
                    GLVector3d p3 = InnerFaces[i].GetTransformedVertex( linearextrusionpath, linearextrusionpath.NumberOfTransforms - 1, 0 );
                    GLVector3d p4 = InnerFaces[i].GetTransformedVertex( linearextrusionpath, linearextrusionpath.NumberOfTransforms - 1, 1 );
        
                    GLVector3d r1 = OuterFaces[i].GetRawVertex( 0 );
                    GLVector3d r2 = OuterFaces[i].GetRawVertex( 1 );
                    GLVector3d r3 = InnerFaces[i].GetRawVertex( 0 );
                    GLVector3d r4 = InnerFaces[i].GetRawVertex( 1 );

                    Vector2 t1 = texturemapping.GetTextureCoordinate( new Vector2( r1.x + 0.5, r1.y + 0.5 ) );
                    Vector2 t2 = texturemapping.GetTextureCoordinate( new Vector2( r2.x + 0.5, r2.y + 0.5 ) );
                    Vector2 t3 = texturemapping.GetTextureCoordinate( new Vector2( r3.x + 0.5, r3.y + 0.5 ) );
                    Vector2 t4 = texturemapping.GetTextureCoordinate( new Vector2( r4.x + 0.5, r4.y + 0.5 ) );
                    
                    renderer.SetNormal( 0, 0, 1 );
        
                    renderer.StartTriangle();
        
                    renderer.SetTextureCoord( t1.x, t1.y );
                    renderer.AddVertex( p1.x, p1.y, p1.z );
        
                    renderer.SetTextureCoord( t2.x, t2.y );
                    renderer.AddVertex( p2.x, p2.y, p2.z );
        
                    renderer.SetTextureCoord( t3.x, t3.y );
                    renderer.AddVertex( p3.x, p3.y, p3.z );
        
                    renderer.EndTriangle();
        
                    renderer.StartTriangle();
        
                    renderer.SetTextureCoord( t1.x, t1.y );
                    renderer.AddVertex( p1.x, p1.y, p1.z );
        
                    renderer.SetTextureCoord( t3.x, t3.y );
                    renderer.AddVertex( p3.x, p3.y, p3.z );
        
                    renderer.SetTextureCoord( t4.x, t4.y );
                    renderer.AddVertex( p4.x, p4.y, p4.z );
                    renderer.EndTriangle();
                }
            }
        }
        
        // we're going to generate a fan of triangles from the centre, because otherwise cut fails horribly
        protected virtual void RenderEndCapCutNoHollow( bool IsTop )
        {
            if( !IsTop )
            {                
                GLVector3d p0 = linearextrusionpath.GetTransformedVertex( new GLVector3d( 0, 0, 0 ), 0 );
                Vector2 t0 = texturemapping.GetTextureCoordinate( new Vector2( 0.5, 0.5 ) );
                for( int i = iFirstOuterFace; i <= iLastOuterFace; i++ )
                {
                    GLVector3d p1 = OuterFaces[i].GetTransformedVertex( linearextrusionpath, 0, 0 );
                    GLVector3d p2 = OuterFaces[i].GetTransformedVertex( linearextrusionpath, 0, 1 );

                    GLVector3d r1 = OuterFaces[ i ].GetRawVertex( 0 );
                    GLVector3d r2 = OuterFaces[ i ].GetRawVertex( 1 );
                    
                    Vector2 t1 = texturemapping.GetTextureCoordinate( new Vector2( 1 - ( r1.x + 0.5 ), r1.y + 0.5 ) );
                    Vector2 t2 = texturemapping.GetTextureCoordinate( new Vector2( 1 - ( r2.x + 0.5 ), r2.y + 0.5 ) );
                    
                    renderer.SetNormal( 0, 0, -1 );
                    renderer.StartTriangle();
        
                    renderer.SetTextureCoord( t2.x, t2.y );
                    renderer.AddVertex( p2.x, p2.y, p2.z );
        
                    renderer.SetTextureCoord( t1.x, t1.y );
                    renderer.AddVertex( p1.x, p1.y, p1.z );
        
                    renderer.SetTextureCoord( t0.x, t0.y );
                    renderer.AddVertex( p0.x, p0.y, p0.z );
        
                    renderer.EndTriangle();
                }
            }
            else
            {
                GLVector3d p0 = linearextrusionpath.GetTransformedVertex( new GLVector3d( 0, 0, 0 ), linearextrusionpath.NumberOfTransforms - 1 );
                Vector2 t0 = texturemapping.GetTextureCoordinate( new Vector2( 0.5, 0.5 ) );
                for( int i = iFirstOuterFace; i <= iLastOuterFace; i++ )
                {
                    GLVector3d p1 = OuterFaces[i].GetTransformedVertex( linearextrusionpath, linearextrusionpath.NumberOfTransforms - 1, 0 );
                    GLVector3d p2 = OuterFaces[i].GetTransformedVertex( linearextrusionpath, linearextrusionpath.NumberOfTransforms - 1, 1 );

                    GLVector3d r1 = OuterFaces[ i ].GetRawVertex( 0 );
                    GLVector3d r2 = OuterFaces[ i ].GetRawVertex( 1 );

                    Vector2 t1 = texturemapping.GetTextureCoordinate( new Vector2( r1.x + 0.5, r1.y + 0.5 ) );
                    Vector2 t2 = texturemapping.GetTextureCoordinate( new Vector2( r2.x + 0.5, r2.y + 0.5 ) );

                    renderer.SetNormal( 0, 0, 1 );
                    renderer.StartTriangle();
        
                    renderer.SetTextureCoord( t1.x, t1.y );
                    renderer.AddVertex( p1.x, p1.y, p1.z );
        
                    renderer.SetTextureCoord( t2.x, t2.y );
                    renderer.AddVertex( p2.x, p2.y, p2.z );
        
                    renderer.SetTextureCoord( t0.x, t0.y );
                    renderer.AddVertex( p0.x, p0.y, p0.z );
        
                    renderer.EndTriangle();
                }
            }
        }
    
        protected virtual void RenderEndCapNoCutNoHollow( bool IsTop )
        {
            RenderEndCapCutNoHollow( IsTop );
        }
    
        // private methods
        
        int NormalizeQuadrant( int iQuadrant )
        {
            return ( ( iQuadrant % iNumberFaces ) + iNumberFaces ) % iNumberFaces;
        }
        
        //! BAsically we check the quadrant (0 to 3), then intersect the radius with the face corresponding to that quadrant
        // The cut starts from the clockwise-end vertex of quadrant 0, and rotates anticlockwise; assuming x points right and y points up
        GLVector3d GetCutIntersect( int iCut, double fCubeHalfWidth )
        {
            int iCutQuadrant = GetCutQuadrant( iCut );
            double fCutRatio = (double)iCut / (double)MaxCut;
    
            GLVector3d lineend = null;
            GLVector3d linestart = ReferenceVertices[ iCutQuadrant] * fCubeHalfWidth / 0.5;
            if( iCutQuadrant < iNumberFaces - 1 )
            {
                lineend = ReferenceVertices[ iCutQuadrant + 1 ]  * fCubeHalfWidth / 0.5;
            }
            else
            {
                lineend = ReferenceVertices[ 0 ]  * fCubeHalfWidth / 0.5;
            }
    
            double fAngle = GetAngleWithXAxis( fCutRatio );
            // CutVectorPerp is perpendicular to the radius vector, I think
            GLVector3d CutVectorPerp = new GLVector3d( - Math.Sin( fAngle ), Math.Cos( fAngle ), 0 );
    
            // Grabbed this from http://softsurfer.com/Archive/algorithm_0104/algorithm_0104B.htm
            GLVector3d IntersectPoint = linestart - ( lineend - linestart ) * CutVectorPerp.dot( linestart ) / CutVectorPerp.dot( lineend - linestart );
            //Console.WriteLine( "GetCutIntersect iCut " + iCut.ToString() + " cubehalfwidth " + fCubeHalfWidth.ToString() + " linestart " + linestart.ToString() +
            //    lineend.ToString() + " fAngle " + fAngle.ToString() + " CutVectorPerp " + CutVectorPerp.ToString() + " intersectpoint " + IntersectPoint.ToString() );
            return IntersectPoint;
        }
    
        // Handles the first face in the cut, starting from cutstart, and running anticlockwise to first reference vertex (assuming x points right, y points up)
        double PopulateSingleCutFacePositiveDirection( ref CrossSection face, GLVector3d CutPoint, int iQuadrant, double fHalfCubeWidth, bool bOuter )
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
    
        double PopulateSingleCutFaceNegativeDirection( ref CrossSection face, GLVector3d CutPoint, int iQuadrant, double fHalfCubeWidth, bool bOuter )
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
    
        double PopulateCompleteSide( ref CrossSection face, int iQuadrant, double fHalfCubeWidth, bool bOuter )
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
            InnerFaces[iFirstOuterFace].TextureMapping = new TextureMapping( texturemapping );
            InnerFaces[iFirstOuterFace].TextureMapping.PreTransformOffsetX = ( fTotalInnerLength - fStartSideInnerLength ) / fTotalInnerLength;
            InnerFaces[iFirstOuterFace].TextureMapping.PreTransformScaleX = fTotalInnerLength / fStartSideInnerLength;
            //Console.WriteLine( "fWholeSideLength: " + fWholeSideLength.ToString() + " fstartsideinnerlength: " + fStartSideInnerLength.ToString() + " fTotalInnerLength " + fTotalInnerLength.ToString() );

            double fCumulativeInnerLength = fStartSideInnerLength;
            for( int iQuadrant = iFirstOuterFace + 1; iQuadrant < iLastOuterFace; iQuadrant++ )
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
    }
}
