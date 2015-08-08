// Copyright Hugh Perkins 2004,2005,2006
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
// You can contact me at hughperkins@gmail.com for more information.

using System;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using Metaverse.Utility;
using FractalSpline;

namespace OSMP
{
    public class FractalSplinePrim : Prim
    {
        const int iMaxFaces = 12;

        Uri[] texturefullpaths = new Uri[ iMaxFaces ];
        
        protected FractalSpline.Primitive primitive;
            
        public override int NumFaces{
            get{ return primitive.NumFaces; }
        }

        [Replicate]
        public Color[] FaceColors
        {
            get{ return facecolors; }
            set{
                facecolors = value;
                for( int i = 0; i < facecolors.GetLength(0); i++ )
                {
                    _SetColor( i, facecolors[i] );
                }
            }
        }
        
        Color[] facecolors = new Color[ iMaxFaces ];

        //[Replicate]
        //[XmlIgnore]
        // for client/server replication, or perhaps not
        public string[] TextureFullPaths
        {
            get{
                LogFile.WriteLine( "get texturefullpaths" );
                string[] texturefullpathstrings = new string[texturefullpaths.GetLength( 0 )];
                for (int i = 0; i < texturefullpaths.GetLength( 0 ); i++)
                {
                    if (texturefullpaths[i] != null)
                    {
                        texturefullpathstrings[i] = texturefullpaths[i].ToString();
                    }
                    else
                    {
                        texturefullpathstrings[i] = "";
                    }
                }
                return texturefullpathstrings;
            }
            set{
                LogFile.WriteLine( "set texturefullpaths" );
                // Note to self: this should be moved somewhere else really
                texturefullpaths = new Uri[ value.GetLength(0) ];
                for (int i = 0; i < value.GetLength( 0 ); i++)
                {
                    if (value[i] != null && value[i] != "" )
                    {
                        texturefullpaths[i] = new Uri( value[i] );
                        Test.Debug( "loading texture " + texturefullpaths[i] + "..." );
                        int textureid = (int)TextureController.GetInstance().LoadUri( texturefullpaths[i] );
                        _SetTexture( i, textureid );
                    }
                }
            }
        }

        [Replicate]
        public string[] TextureRelativePaths
        {
            get
            {
                LogFile.WriteLine( "get texturerelativepaths" );
                string[] relativepaths = new string[texturefullpaths.GetLength( 0 )];
                for (int i = 0; i < relativepaths.GetLength( 0 ); i++)
                {
                    //LogFile.WriteLine( "i: " + i );
                    if (texturefullpaths[i] != null)
                    {
                        relativepaths[i] = ProjectFileController.GetInstance().GetRelativePath( texturefullpaths[i] );
                        LogFile.WriteLine( "i " + texturefullpaths[i] + " -> " + relativepaths[i] );
                    }
                    else
                    {
                        relativepaths[i] = "";
                    }
                }
                return relativepaths;
            }
            set
            {
                LogFile.WriteLine( "set texturerelativepaths" );
                for (int i = 0; i < value.GetLength( 0 ); i++)
                {
                    if (value[i] != null && value[i] != "" )
                    {
                        texturefullpaths[i] = ProjectFileController.GetInstance().GetFullPath( value[i] );
                        int textureid = (int)TextureController.GetInstance().LoadUri( texturefullpaths[i] );
                        Test.Debug( "loading texture " + value[i] + " -> " + texturefullpaths[i] );
                        _SetTexture( i, textureid );
                    }
                }
            }
        }

        [Replicate]
        public int Hollow
        {
            get{ return primitive.Hollow; }
            set{ primitive.Hollow = value;primitive.UpdateTransforms(); }
        }
        [Replicate]
        public int Twist
        {
            get{ return primitive.Twist; }
            set{ primitive.Twist = value;primitive.UpdateTransforms(); }
        }
        [Replicate]
        public double Shear
        {
            get{ return primitive.Shear; }
            set{ primitive.Shear = value;primitive.UpdateTransforms(); }
        }
        [Replicate]
        public int CutStart
        {
            get{ return primitive.CutStart; }
            set{ primitive.CutStart = value; primitive.UpdateTransforms();}
        }
        [Replicate]
        public int CutEnd
        {
            get{ return primitive.CutEnd; }
            set{ primitive.CutEnd = value;primitive.UpdateTransforms(); }
        }
        [Replicate]
        public int AdvancedCutStart
        {
            get{ return primitive.AdvancedCutStart; }
            set{ primitive.AdvancedCutStart = value; primitive.UpdateTransforms();}
        }
        [Replicate]
        public int AdvancedCutEnd
        {
            get{ return primitive.AdvancedCutEnd; }
            set{ primitive.AdvancedCutEnd = value; primitive.UpdateTransforms();}
        }
        [Replicate]
        public double TopSizeX
        {
            get{ return primitive.TopSizeX; }
            set{ primitive.TopSizeX = value;primitive.UpdateTransforms();}
        }
        [Replicate]
        public double TopSizeY
        {
            get{ return primitive.TopSizeY; }
            set{ primitive.TopSizeY = value; primitive.UpdateTransforms();}
        }
        [Replicate]
        public double TextureOffsetX
        {
            get{ return primitive.TextureOffset[0]; }
            set{
                double[] offset = primitive.TextureOffset;
                offset[0] = value;
                primitive.TextureOffset = offset;
            }
        }
        [Replicate]
        public double TextureOffsetY
        {
            get{ return primitive.TextureOffset[1];}
            set{
                double[] offset = primitive.TextureOffset;
                offset[1] = value;
                primitive.TextureOffset = offset;
            }
        }
        [Replicate]
        public double TextureScaleX
        {
            get{ return primitive.TextureScale[0]; }
            set{
                double[] scale = primitive.TextureScale;
                scale[0] = value;
                primitive.TextureScale = scale;
            }
        }
        [Replicate]
        public double TextureScaleY
        {
            get{ return primitive.TextureScale[1]; }
            set{
                double[] scale = primitive.TextureScale;
                scale[1] = value;
                primitive.TextureScale = scale;
            }
        }
        [Replicate]
        public double TextureRotate
        {
            get{ return primitive.TextureRotate; }
            set{ primitive.TextureRotate = value; }
        }
        
        public void UpdateTransforms()
        {
            primitive.UpdateTransforms();
        }
            
        public void SetHollow( int value ){ 
            primitive.Hollow = value;
            primitive.UpdateTransforms();            
        }
        public void SetTwist( int value ){ 
            primitive.Twist = value;
            primitive.UpdateTransforms();            
        }
        public void SetShear( int value ){ 
            primitive.Shear = (double)value / 100d;
            primitive.UpdateTransforms();            
        }
        public void SetTopSizeX( int value ){ 
            primitive.TopSizeX = (double)value / 200d;
            primitive.UpdateTransforms();            
        }
        public void SetTopSizeY( int value ){ 
            primitive.TopSizeY = (double)value / 200d;
            primitive.UpdateTransforms();            
        }
        public void SetCutStart( int value ){ 
            primitive.CutStart = value;
            primitive.UpdateTransforms();            
        }
        public void SetCutEnd( int value ){ 
            primitive.CutEnd = value;
            primitive.UpdateTransforms();            
        }
        public void SetAdvancedCutStart( int value ){ 
            primitive.AdvancedCutStart = value;
            primitive.UpdateTransforms();            
        }
        public void SetAdvancedCutEnd( int value ){ 
            primitive.AdvancedCutEnd = value;
            primitive.UpdateTransforms();            
        }
        public void SetTextureOffsetX( int value ){ 
            TextureOffsetX = value / 200d;
        }
        public void SetTextureOffsetY( int value ){ 
            TextureOffsetY = value / 200d;
        }
        public void SetTextureScaleX( int value ){ 
            TextureScaleX = value / 20d;
        }
        public void SetTextureScaleY( int value ){ 
            TextureScaleY = value / 20d;
        }
        public void SetTextureRotate( int value ){ 
            TextureRotate = value * 2;
        }
        
        public override void RegisterProperties( IPropertyController propertycontroller )
        {
            base.RegisterProperties( propertycontroller );

            propertycontroller.RegisterIntProperty( "Hollow", primitive.Hollow, 0, 200, new SetIntPropertyHandler( SetHollow ) );
            propertycontroller.RegisterIntProperty( "Twist", primitive.Twist, -360, 360, new SetIntPropertyHandler( SetTwist ) );
            propertycontroller.RegisterIntProperty( "Shear", (int)( primitive.Shear * 100 ), -100, 100, new SetIntPropertyHandler( SetShear ) );
            propertycontroller.RegisterIntProperty( "TopSize X", (int)(primitive.TopSizeX * 200 ), 0, 200, new SetIntPropertyHandler( SetTopSizeX ) );
            propertycontroller.RegisterIntProperty( "TopSize Y", (int)(primitive.TopSizeY * 200 ), 0, 200, new SetIntPropertyHandler( SetTopSizeY ) );
            propertycontroller.RegisterIntProperty( "Cut Begin", primitive.CutStart, 0, 200, new SetIntPropertyHandler( SetCutStart ) );
            propertycontroller.RegisterIntProperty( "Cut End", primitive.CutEnd, 0, 200, new SetIntPropertyHandler( SetCutEnd ) );
            propertycontroller.RegisterIntProperty( "Advanced Cut Start", primitive.AdvancedCutStart, 0, 200, new SetIntPropertyHandler( SetAdvancedCutStart ) );
            propertycontroller.RegisterIntProperty( "Advanced Cut End", primitive.AdvancedCutEnd, 0, 200, new SetIntPropertyHandler( SetAdvancedCutEnd ) );
            
            propertycontroller.RegisterIntProperty( "Texture Offset X", (int)( TextureOffsetX * 200 ), 0, 200, new SetIntPropertyHandler( SetTextureOffsetX ) );
            propertycontroller.RegisterIntProperty( "Texture Offset Y", (int)( TextureOffsetY * 200 ), 0, 200, new SetIntPropertyHandler( SetTextureOffsetY ) );
            propertycontroller.RegisterIntProperty( "Texture Scale X", (int)( TextureScaleX * 20 ), 0, 200, new SetIntPropertyHandler( SetTextureScaleX ) );
            propertycontroller.RegisterIntProperty( "Texture Scale Y", (int)( TextureScaleY * 20 ), 0, 200, new SetIntPropertyHandler( SetTextureScaleY ) );
            propertycontroller.RegisterIntProperty( "Texture Rotate", (int)( TextureRotate / 2 ), 0, 180, new SetIntPropertyHandler( SetTextureRotate ) );
        }

        protected void LoadDefaults()
        {
            for( int i = 0; i < iMaxFaces; i++ )
            {
                facecolors[i] = new Color( 0.8,0.8,1.0 );
                _SetColor( i, facecolors[i] );
            }
        }
        
        public FractalSplinePrim()
        {
            Test.Debug( "FractalSplinePrim::FractalSplinePrim" );
            for (int i = 0; i < facecolors.GetLength(0); i++)
            {
                facecolors[i] = new Color();
            }
            for (int i = 0; i < texturefullpaths.GetLength(0); i++)
            {
                texturefullpaths[i] = null;
            }
        }
        public override void Draw()
        {
            IGraphicsHelper graphics = GraphicsHelperFactory.GetInstance();
            
            graphics.PushMatrix();            
            
            graphics.SetMaterialColor( facecolors[0] );
            graphics.Translate( pos );            
            graphics.Rotate( rot );            
            graphics.Scale( scale );
            
            primitive.Render();
            
            graphics.PopMatrix();
        }
        public override void DrawSelected()
        {
            IGraphicsHelper graphics = GraphicsHelperFactory.GetInstance();
            
            graphics.SetMaterialColor( SelectionView.GetInstance().SelectionGridColor );
            graphics.PushMatrix();            
            graphics.Translate( pos );
            graphics.Rotate( rot );
            graphics.Scale( scale );
            
            graphics.DrawWireframeBox( SelectionView.GetInstance().SelectionGridNumLines );
            
            graphics.PopMatrix();
        }
        void _SetColor( int face, Color newcolor )
        {
            primitive.SetFaceColor( face, new FractalSpline.Color( newcolor.ToArray() ) );
        }
        public override void SetColor( int face, Color newcolor )
        {
            LogFile.WriteLine( "setting color to " + newcolor.ToString() );
            if( face == FractalSpline.Primitive.AllFaces )
            {
                for( int i = 0; i < facecolors.GetUpperBound(0) + 1; i++ )
                {
                    facecolors[i] = new Color( newcolor );
                    _SetColor( face, newcolor );
                }
            }
            else
            {
                facecolors[ face ] = new Color( newcolor );
                _SetColor( face, newcolor );
            }
        }
        public void _SetTexture( int face, int opengltextureid )
        {
            LogFile.WriteLine( "FractalSplinePrim, setting texture to " + opengltextureid );
            primitive.SetTexture( face, opengltextureid );
        }
        public void SetTexture( int face, Uri uri )
        {
            if( face == FractalSpline.Primitive.AllFaces )
            {
                for (int i = 0; i < texturefullpaths.GetUpperBound( 0 ) + 1; i++)
                {
                    texturefullpaths[i] = uri; // Note to self: fix this dependency, wrong level of abstraction
                }
            }
            else
            {
                texturefullpaths[face] = uri;
            }
            
            int textureid = (int)TextureController.GetInstance().LoadUri( uri );
            _SetTexture( face, textureid );
        }
        public override Color GetColor( int face )
        {
            return facecolors[ face ];
        }
        public override string ToString()
        {
            return primitive.GetType().ToString() + " " + iReference + " " + pos.ToString() + " " + rot.ToString() + " " + scale.ToString();
        }

        public override void RenderSingleFace( int ifacenum )
        {
            IGraphicsHelper graphics = GraphicsHelperFactory.GetInstance();
            
            graphics.PushMatrix();            
            
            graphics.SetMaterialColor( facecolors[0] );
            graphics.Translate( pos );            
            graphics.Rotate( rot );            
            graphics.Scale( scale );
            
            primitive.RenderSingleFace( ifacenum );
            
            graphics.PopMatrix();
        }
    }
}
