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
    public class CrossSection
    {
        int iNumPoints = 0;   //!< Number of points currently stored
        GLVector3d[] points;  //!< All points; currently space for 100; maybe should use an allocator or arraylist for this??

        IRenderer renderer;
        TextureMapping texturemapping = new TextureMapping();
        
        public CrossSection()
        {
            points = new GLVector3d[100];
        }

        public void RemoveAllPoints()
        {
            iNumPoints = 0;
        } //!< Removes all points

        public int GetNumPoints()
        {
            return iNumPoints;
        } //!< Returns number of points

        public TextureMapping TextureMapping
        {
            set{
                texturemapping = value;
            }
            get{
                return texturemapping;
            }
        }
    
        // replaced by GetRawVertex
      //  public GLVector3d GetPoint( int iPointNum )
        //{
         //   return points[ iPointNum ];
       // } //!< Return untransformed point

        public void SetRenderer( IRenderer renderer )
        {
            this.renderer = renderer;
        }
        
        GLVector3d CalculateNormal( GLVector3d p1,GLVector3d p2,GLVector3d p3,GLVector3d p4  )
        {
            GLVector3d vectorac = p3 - p1;
            GLVector3d vectorbd = p4 - p2;
            return vectorac.getCross( vectorbd ).unit();
        }
    
        public void AddPoint( GLVector3d point )
        {
            points[iNumPoints] = new GLVector3d( point );
            iNumPoints++;
        }
    
        public void AddPoint( double x, double y, double z )
        {
            points[iNumPoints] = new GLVector3d( x, y, z );
            iNumPoints++;
        }
    
        public GLVector3d GetTransformedVertex( ExtrusionPath extrusionpath, int iPathSliceIndex, int iPointIndex )
        {
            return extrusionpath.GetTransformedVertex( points[iPointIndex], iPathSliceIndex );
        }
        
        public GLVector3d GetRawVertex( int iPointIndex )
        {
            return points[iPointIndex];
        }
    
        public void Render( ExtrusionPath extrusionpath )
        {
            for( int i = 0; i < extrusionpath.NumberOfTransforms - 1; i++ )
            {
                for( int j = 0; j < iNumPoints - 1; j++ )
                {
                    // p1 - p4 are the quad we are rendering
                    GLVector3d p1 = extrusionpath.GetTransformedVertex( points[j], i );
                    GLVector3d p2 = extrusionpath.GetTransformedVertex( points[j + 1], i );
                    GLVector3d p3 = extrusionpath.GetTransformedVertex( points[j + 1], i + 1 );
                    GLVector3d p4 = extrusionpath.GetTransformedVertex( points[j], i + 1 );
                    
                    Vector2 t1 = texturemapping.GetTextureCoordinate( new Vector2( (double)j / (iNumPoints - 1 ), (double)i / ( extrusionpath.NumberOfTransforms - 1 ) ) );
                    Vector2 t2 = texturemapping.GetTextureCoordinate( new Vector2( (double)( j + 1 ) / (iNumPoints - 1 ), (double)i / ( extrusionpath.NumberOfTransforms - 1 ) ) );
                    Vector2 t3 = texturemapping.GetTextureCoordinate( new Vector2( (double)( j + 1 ) / (iNumPoints - 1 ), (double)( i + 1 ) / ( extrusionpath.NumberOfTransforms - 1 ) ) );
                    Vector2 t4 = texturemapping.GetTextureCoordinate( new Vector2( (double)j / (iNumPoints - 1 ), (double)( i + 1 ) / ( extrusionpath.NumberOfTransforms - 1 ) ) );
                    
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
        
        public override string ToString()
        {
            string result = "";
            for( int i = 0; i < iNumPoints; i++ )
            {
                result += points[i].ToString() + "; ";
            }
            return result;
        }
    }
}    

