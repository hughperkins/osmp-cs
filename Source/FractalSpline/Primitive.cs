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

using MathGl;
using System;

namespace FractalSpline
{
    public abstract class Primitive
    {
        // public properties
        public const int AllFaces = 99;       
        public const int MaxCut = 200;  //!< Maximum value for cut
            
        public int NumFaces{
            get{ return Faces.GetUpperBound(0) + 1; }
        }
            
        // protected/private properties
        
        protected Face[] Faces;
                
        protected IRenderer renderer = null;
        
        protected const int iMaxFaces = 9;      
            
        protected TextureMapping texturemapping = new TextureMapping();
        
        protected int iTwist = 0;  //!< Current Twist
        protected double fShear = 0; //!< Current Sheer
        protected int iLevelOfDetail = 16; //!< Current LOD
        protected double fTopSizeX = 0; //!< Topsize X
        protected double fTopSizeY = 0; //!< Topsize Y
        protected int iCutStart = 0;  //!< Cut Start (0 to 200)
        protected int iCutEnd = MaxCut;  //!< Cut end ( 0 to 200 )
        protected int iHollow = 0; //!< Hollow % (0 to 95)
      
        protected int[] iFaceTextures = new int[ iMaxFaces ]; //!< OpenGL Texture IDs
        protected Color[] vFaceColors = new Color[ iMaxFaces ];
        
        // constructors
        public Primitive()
        {
            SetFaceColor( AllFaces, new Color( 0.4, 0.8, 0.2 ) );
        }
        
        // accessors (get/set)
        public abstract int LevelOfDetail{ set; }
        public abstract int Twist{ set; get; }
        public abstract double Shear{ set; get; }
        public abstract double TopSizeX{ set; get;}
        public abstract double TopSizeY{ set; get; }
        public abstract int CutStart{ set; get; }
        public abstract int CutEnd{ set; get; }
        public virtual int AdvancedCutStart{ set{} get{ return 0; } }
        public virtual int AdvancedCutEnd{ set{} get{ return 0; } }
        public abstract int Hollow{ set; get; }
        
        public double[] TextureOffset{
            set{
                texturemapping.Offset = new Vector2( value[0], value[1] );
                BuildFaces();
            }
            get{
                return new double[]{ texturemapping.Offset.x, texturemapping.Offset.y };
            }
        }
        public double[] TextureScale{
            set{
                texturemapping.Scale = new Vector2( value[0], value[1] );
                BuildFaces();
            }
            get{
                return new double[]{ texturemapping.Scale.x, texturemapping.Scale.y };
            }
        }
        // degrees (full circle is 360 degrees)
        public double TextureRotate{
            set{
                texturemapping.Rotate = value;
                BuildFaces();
            }
            get{
                return texturemapping.Rotate;
            }
        }
        
        public void SetTexture( int iFaceNumber, int iTextureId )
        {
            if( iFaceNumber == AllFaces )
            {
                for( int i = 0; i < iMaxFaces; i++ )
                {
                    iFaceTextures[i] = iTextureId;
                }
            }
            else if( iFaceNumber < iMaxFaces )
            {
                iFaceTextures[ iFaceNumber ] = iTextureId;
            }
        }
    
        public void SetFaceColor( int iFaceNumber, Color color )
        {
            if( iFaceNumber >= 0 && iFaceNumber < iMaxFaces )
            {
                vFaceColors[ iFaceNumber ] = new Color( color );
            }
            else if( iFaceNumber == AllFaces )  // all faces
            {
                for( int i = 0; i < iMaxFaces; i++ )
                {
                    vFaceColors[ i ] = new Color( color );
                }
            }
        }
        
        // public methods
        public void Render()  //!< Renders primitive to OpenGL at origin
        {
            for( int i = 0; i < Faces.GetUpperBound(0) + 1; i++ )
            {
                RenderFace( Faces[i] );
            }
        }
        public void RenderSingleFace( int iFaceNum )
        {
            for( int i = 0; i < Faces.GetUpperBound(0) + 1; i++ )
            {
                Face thisface = Faces[i];
                if( thisface.FaceNum == iFaceNum )
                {
                    RenderFace( thisface );
                }
            }
        }        
        public abstract void UpdateTransforms(); //!< Update transforms etc internally
                    
        // protected / private methods
        
        protected abstract void BuildFaces();

        protected abstract void AssignFaces();
            
        protected abstract void RenderFace( Face face );
            
        protected void ApplyFaceColor( int iFaceNum )
        {
            renderer.SetColor( vFaceColors[ iFaceNum ].r, vFaceColors[ iFaceNum ].g, vFaceColors[ iFaceNum ].b );
        }

        // useful for triangles
        protected GLVector3d CalculateNormal( GLVector3d p1,GLVector3d p2,GLVector3d p3 )
        {
            GLVector3d vectorab = p2 - p1;
            GLVector3d vectorac = p3 - p1;
            return vectorab.getCross( vectorac ).unit();
        }
        
        // for quads
        protected GLVector3d CalculateNormal( GLVector3d p1,GLVector3d p2,GLVector3d p3,GLVector3d p4  )
        {
            GLVector3d vectorac = p3 - p1;
            GLVector3d vectorbd = p4 - p2;
            return vectorac.getCross( vectorbd ).unit();
        }
    }
}
