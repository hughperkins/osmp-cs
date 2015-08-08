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
    public abstract class ExtrusionPath
    {
        public const int MaxSteps = 32;
            
        protected GLMatrix4D[] transforms = new GLMatrix4D[MaxSteps];  //!< All transforms
        protected int iNumberOfTransforms = 0;  //!< Number of Transforms
        
        protected int iLevelOfDetail = 16; //!< Current LOD
        
        protected int iTwist = 0;  //!< Current Twist
        protected double fShear = 0; //!< Current Sheer
        protected double fTopSizeX = 1;
        protected double fTopSizeY = 1;

        public ExtrusionPath()
        {
            for( int i = 0; i < MaxSteps; i++ )
            {
                transforms[i] = GLMatrix4D.Identity();
            }
        }

        public GLVector3d GetTransformedVertex( GLVector3d PointToTransform, int iTransformIndex )
        {
            return transforms[ iTransformIndex ] * PointToTransform;
        }
        
        public abstract void UpdatePath(); //!< Update transforms matrices

        public int NumberOfTransforms
        {
            get{ return iNumberOfTransforms; }
        }
        public int LevelOfDetail{ 
            set{ iLevelOfDetail = value; }
        }
        public int Twist{ 
            set{ iTwist = value; }
            get{ return iTwist; }
        }
        public double Shear{ 
            set{ fShear = value; }
            get{ return fShear; }
        }
        public double TopSizeX{ 
            set{ fTopSizeX = value; }
            get{ return fTopSizeX; }
        }
        public double TopSizeY{ 
            set{ fTopSizeY = value; }
            get{ return fTopSizeY; }
        }
        
        public override string ToString()
        {
            string result = iNumberOfTransforms.ToString();
            //for( int i = 0; i < iNumberOfTransforms; i++ )
            //{
            //}
            return result;
        }
    }
}
