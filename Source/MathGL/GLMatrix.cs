//**************************************************************************
// *   Copyright (C) 2004,2006 by Jacques Gasselin, Hugh Perkins        *
 //*   jacquesgasselin@hotmail.com                                           *
 //*   hughperkins@gmail.com  http://manageddreams.com    *
 //*                                                                         *
 //*   This program is free software; you can redistribute it and/or modify  *
 //*   it under the terms of the GNU Lesser General Public License as       *
 //*   published by the Free Software Foundation; either version 2 of the    *
 //*   License, or (at your option) any later version.                       *
 //**************************************************************************
// *   Copyright (C) 2004,2006 by Jacques Gasselin, Hugh Perkins        *
 //*   jacquesgasselin@hotmail.com                                           *
 //*   hughperkins@gmail.com  http://manageddreams.com    *
 //*                                                                         *
 //*   This program is free software; you can redistribute it and/or modify  *
 //*   it under the terms of the GNU Lesser General Public License as       *
 //*   published by the Free Software Foundation; either version 2 of the    *
 //*   License, or (at your option) any later version.                       *
 //*                                                                         *
 //*   This program is distributed in the hope that it will be useful,       *
 //*   but WITHOUT ANY WARRANTY; without even the implied warranty of        *
 //*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
 //*   GNU General Public License for more details.                          *
 //*                                                                         *
 //*   You should have received a copy of the GNU Lesser General Public     *
 //*   License along with this program; if not, write to the                 *
 //*   Free Software Foundation, Inc.,                                       *
 //*   59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.             * 
 //**************************************************************************

// History:
// - First created by Jacques Gasselin, 2004
// - applyRotateX, applyRotateY, applyRotateZ, applyRotateXYZ methods added by Sebastien Bloc
// - Ported to C#.Net by Hugh Perkins, 2006

using System;

namespace MathGl
{
   
	/// <summary>
	/// A 4D matrix for math calculations
	/// </summary>
    public class GLMatrix4D
    {
    	/// <summary>
    	/// Indices of our matrix
    	/// </summary>
        double[]m = new double[16];
        
         /// <summary>
        /// Construct a matrix from string
        /// </summary>
        /// <param name="inputstring">String representation of a matrix</param>
        /// <remarks>Row major order is expected to conform with standard output</remarks>
        public GLMatrix4D( string inputstring )
        {
            string[]splitinputstring = inputstring.Split(new char[]{'\n'});
            for(int j = 0; j < 4; ++j)
            {
                string[]splitsingleline = splitinputstring[j].Split( new char[]{' '} );
                for(int i = 0; i < 4; ++i)
                {
                    m[i*4+j] = Convert.ToDouble( splitsingleline[i] );
                }
            }
        }
        
		/// <summary>
		/// Create an uninitialized matrix
		/// </summary>
        public GLMatrix4D() { }
    
		/// <summary>
		/// Create an initialised matrix
		/// </summary>
		/// <param name="val">A value to initialize all fields of the matrix</param>
        public GLMatrix4D(double val)
        { 
        	m[0]=m[1]=m[2]=m[3]=m[4]=m[5]=m[6]=m[7]=m[8]=m[9]=m[10]=m[11]=m[12]=m[13]=m[14]=m[15]=val; 
        	
        	return;
        }
    
		/// <summary>
		/// Create a matrix from an array
		/// </summary>
		/// <param name="val">Array of values to initialize the matrix with</param>
        public GLMatrix4D( double[] val)
        {
            Buffer.BlockCopy( val, 0, m, 0, Buffer.ByteLength( val ) );
            
            return;
        }
    
		/// <summary>
		/// Copy a matrix
		/// </summary>
		/// <param name="mat">Source matrix to copy from</param>
        public GLMatrix4D(GLMatrix4D mat)
        { 
            Buffer.BlockCopy( mat.m, 0, m, 0, Buffer.ByteLength( mat.m ) );
            
            return;
        }
        
        /// <summary>
        /// Convert to matrix to an array of double values
        /// </summary>
        /// <returns>Array of the matrice's indicies</returns>
        public double[] ToArray()
        {
            double[]arraycopy = new double[16];
            Buffer.BlockCopy( m, 0, arraycopy, 0, Buffer.ByteLength( m ) );
           
            return arraycopy;
        }
    
        /// <summary>
        /// Get the matrix determinant
        /// </summary>
        /// <returns>Determinant</returns>
        public double Det() 
        { 
        	return m[0]*CofactorM0() - m[1]*CofactorM1() + m[2]*CofactorM2() - m[3]*CofactorM3(); 
        }
    
        /// <summary>
        /// Get the adjoint matrix
        /// </summary>
        /// <returns>A new matrix that is adjoint to this one</returns>
        public GLMatrix4D adjoint()
        {
            GLMatrix4D ret = new GLMatrix4D();
    
            ret.m[0] = CofactorM0();
            ret.m[1] = -CofactorM4();
            ret.m[2] = CofactorM8();
            ret.m[3] = -CofactorM12();
    
            ret.m[4] = -CofactorM1();
            ret.m[5] = CofactorM5();
            ret.m[6] = -CofactorM9();
            ret.m[7] = CofactorM13();
    
            ret.m[8] = CofactorM2();
            ret.m[9] = -CofactorM6();
            ret.m[10] = CofactorM10();
            ret.m[11] = -CofactorM14();
    
            ret.m[12] = -CofactorM3();
            ret.m[13] = CofactorM7();
            ret.m[14] = -CofactorM11();
            ret.m[15] = CofactorM15();
    
            return ret;
        }
    
        /// <summary>
        /// Adjoint method inverse, constant time inversion implementation
        /// </summary>
        /// <returns>A new matrix that is inverse to this one</returns>
        public GLMatrix4D inverse() 
        {
            GLMatrix4D ret = new GLMatrix4D(adjoint());
    
            double determinant = m[0]*ret[0] + m[1]*ret[4] + m[2]*ret[8] + m[3]*ret[12];
           
            //assert(determinant!=0 && "Singular matrix has no inverse");
    
            ret /= determinant;
            
            return ret;
        }
    
    
        /// <summary>
        /// Equality check. 
        /// </summary>
        /// <param name="one">First Matrix</param>
        /// <param name="two">Second Matrix</param>
        /// <returns>True if they are equal, false otherwise</returns>
        public static bool operator== (GLMatrix4D one, GLMatrix4D two)
        {
            for( int i = 0; i < 16; i++ )
            {
                if( one.m[i] != two.m[i] )
                {
                    return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// dot net requires we also define !=
        /// </summary>
     	/// <param name="one">First Matrix</param>
        /// <param name="two">Second Matrix</param>
        /// <returns>True if they are not equal, false otherwise</returns>
        public static bool operator!= (GLMatrix4D one, GLMatrix4D two)
        {
            for( int i = 0; i < 16; i++ )
            {
                if( one.m[i] != two.m[i] )
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Equality function override
        /// </summary>
        /// <param name="twoobject">Matrix object</param>
        /// <returns>True if they are equal, false otherwise</returns>
        public override bool Equals( object twoobject )
        {
            GLMatrix4D two = twoobject as GLMatrix4D;
           
            for( int i = 0; i < 16; i++ )
            {
                if( m[i] != two.m[i] )
                {
                    return false;
                }
            }
            
            return true;
        }
    
        /// <summary>
        /// Direct access to the matrix elements
        /// </summary>
        /// <remarks>just remember they are in column major format</remarks>
        public virtual double this[ int ind ]{
            get{
                return m[ ind ];
            }
        }
    
        //!Implicit cast vector access as suggested by maquinn
        //operator const double*(void) const { return m; }
    
        //!Implicit cast vector access as suggested by maquinn
        //operator double*(void) { return m; }

        /// <summary>
        /// Multiply this matrix by a scalar
        /// </summary>
        /// <param name="mat">Matrix</param>
        /// <param name="val">Scalar</param>
        /// <returns>New resulting matrix</returns>
        public static GLMatrix4D operator*( GLMatrix4D mat, double val)
        {
            GLMatrix4D result = new GLMatrix4D();
           
            for( byte i = 0; i < 16; ++i )
            {
                result.m[i] = mat.m[i] * val;
            }
            
            return result;
        }
        
        /// <summary>
        /// Multiply this matrix by a scalar
        /// </summary>
        /// <param name="val">Scalar</param>
        /// <param name="mat">Matrix</param>
        /// <returns>New resulting matrix</returns>
        public static GLMatrix4D operator*( double val, GLMatrix4D mat )
        {
            GLMatrix4D result = new GLMatrix4D();
           
            for( byte i = 0; i < 16; ++i )
            {
                result.m[i] = mat.m[i] * val;
            }
            
            return result;
        }
    
        /// <summary>
        /// Divide this matrix by a scalar
        /// </summary>
        /// <param name="mat">Matrix</param>
        /// <param name="val">Scalar</param>
        /// <returns>New resulting matrix</returns>
        public static GLMatrix4D operator/( GLMatrix4D mat, double val)
        {
            GLMatrix4D result = new GLMatrix4D();
           
            for( byte i = 0; i < 16; ++i )
            {
                result.m[i] = mat.m[i] / val;
            }
            
            return result;
        }
    
        /// <summary>
        /// Add two matrices together
        /// </summary>
        /// <param name="one">Left Matrix</param>
        /// <param name="two">Right Matrix</param>
        /// <returns>New resulting matrix</returns>
        public static GLMatrix4D operator+( GLMatrix4D one, GLMatrix4D two)
        {
            GLMatrix4D result = new GLMatrix4D();
           
            for( byte i = 0; i < 16; ++i )
            {
                result.m[i] = one.m[i] + two.m[i];
            }
            
            return result;
        }
    
       /// <summary>
        /// Subtract one matrix from another
        /// </summary>
        /// <param name="one">Left Matrix</param>
        /// <param name="two">Right Matrix</param>
        /// <returns>New resulting matrix</returns>
        public static GLMatrix4D operator-( GLMatrix4D one, GLMatrix4D two)
        {
            GLMatrix4D result = new GLMatrix4D();
            
            for( byte i = 0; i < 16; ++i )
            {
                result.m[i] = one.m[i] - two.m[i];
            }
            
            return result;
        }
    
        /// <summary>
        /// Get the matrix dot product, the most commonly used form of matrix multiplication
        /// </summary>
        /// <param name="one">Left Matrix</param>
        /// <param name="two">Right Matrix</param>
        /// <returns>New resulting matrix</returns>
        public static GLMatrix4D operator* ( GLMatrix4D one, GLMatrix4D two )
        {
            GLMatrix4D ret = new GLMatrix4D();
           
            for( byte j = 0; j < 4; ++j )
            {
                ret.m[j] = one.m[j]*two.m[0]
                         + one.m[j+4]*two.m[1]
                         + one.m[j+8]*two.m[2]
                         + one.m[j+12]*two.m[3];
    
                ret.m[j+4] = one.m[j]*two.m[4]
                           + one.m[j+4]*two.m[5]
                           + one.m[j+8]*two.m[6]
                           + one.m[j+12]*two.m[7];
    
                ret.m[j+8] = one.m[j]*two.m[8]
                           + one.m[j+4]*two.m[9]
                           + one.m[j+8]*two.m[10]
                           + one.m[j+12]*two.m[11];
    
                ret.m[j+12] = one[j]*two.m[12]
                            + one[j+4]*two.m[13]
                            + one[j+8]*two.m[14]
                            + one[j+12]*two.m[15];
            }
            
            return ret;
        }
    
        /// <summary>
        /// Apply the matrix dot product to this matrix
        /// </summary>
        /// <param name="mat">Right Side Matrix</param>
        /// <returns>This Matrix</returns>
        /// <remarks>unrolling by sebastien bloc</remarks>
        public GLMatrix4D Multiply3By3( GLMatrix4D mat )
        {
            GLMatrix4D temp = new GLMatrix4D( this );
            
            m[0] = temp.m[0]*mat.m[0]+temp.m[4]*mat.m[1]+temp.m[8]*mat.m[2];
            m[4] = temp.m[0]*mat.m[4]+temp.m[4]*mat.m[5]+temp.m[8]*mat.m[6];
            m[8] = temp.m[0]*mat.m[8]+temp.m[4]*mat.m[9]+temp.m[8]*mat.m[10];
    
            m[1] = temp.m[1]*mat.m[0]+temp.m[5]*mat.m[1]+temp.m[9]*mat.m[2];
            m[5] = temp.m[1]*mat.m[4]+temp.m[5]*mat.m[5]+temp.m[9]*mat.m[6];
            m[9] = temp.m[1]*mat.m[8]+temp.m[5]*mat.m[9]+temp.m[9]*mat.m[10];
    
            m[2] = temp.m[2]*mat.m[0]+temp.m[6]*mat.m[1]+temp.m[10]*mat.m[2];
            m[6] = temp.m[2]*mat.m[4]+temp.m[6]*mat.m[5]+temp.m[10]*mat.m[6];
            m[10] = temp.m[2]*mat.m[8]+temp.m[6]*mat.m[9]+temp.m[10]*mat.m[10];
    
            m[3] = temp.m[3]*mat.m[0]+temp.m[7]*mat.m[1]+temp.m[11]*mat.m[2];
            m[7] = temp.m[3]*mat.m[4]+temp.m[7]*mat.m[5]+temp.m[11]*mat.m[6];
            m[11] = temp.m[3]*mat.m[8]+temp.m[7]*mat.m[9]+temp.m[11]*mat.m[10];
           
            return this;
        }
    
        /// <summary>
        /// Get the matrix vector dot product, used to transform vertices
        /// </summary>
        /// <param name="mat">Left Side Matrix</param>
        /// <param name="vec">Right Side Vector</param>
        /// <returns>Resulting Vector</returns>
        public static GLVector4d operator* ( GLMatrix4D mat, GLVector4d vec)
        {
            GLVector4d ret = new GLVector4d();
           
            for( byte j = 0; j < 4; ++j )
            {
                ret.val[j] = vec.x*mat[j]
                           + vec.y*mat[j+4]
                           + vec.z*mat[j+8]
                           + vec.w*mat[j+12];
            }
            
            return ret;
        }
    
        /// <summary>
        /// Get the matrix vector dot product with w = 1, use for transforming non 4D vectors
        /// </summary>
        /// <param name="mat">Left Side Matrix</param>
        /// <param name="vec">Right Side Vector</param>
        /// <returns>Resulting  vector</returns>
        public static GLVector3d operator* ( GLMatrix4D mat, GLVector3d vec)
        {
            GLVector3d ret = new GLVector3d();
            for( byte j = 0; j < 3; ++j )
            {
            	for( byte i = 0; i < 3; ++i )
            	{
                    ret.val[j] += vec.val[i]*mat.m[j+i*4]; //scale and rotate disregarding w scaling
            	}
            }
    
            for( byte i = 0; i < 3; ++i )
            {
                ret.val[i] += mat.m[i+3*4]; //translate
            }
    
            //do w scaling
            double w = mat.m[15];
            for( byte i = 0; i < 3; ++i )
            {
                w += vec.val[i]*mat.m[3+i*4];
            }
    
            double resip = 1/w;
    
            for( byte i = 0; i < 3; ++i )
            {
                ret.val[i] *= resip;
            }
    
            return ret;
        }
    
    
        //!Transpose the matrix
        /// <summary>
        /// Transpose this matrix
        /// </summary>
        /// <returns>This matrix in transposed form</returns>
        public GLMatrix4D Transpose()
        {
            double temp;
            
            for( int i = 0; i < 4; ++i ) 
            {
                for( int j = 0; j < 4; ++j )
                {
                    temp = m[j+i*4];
                    m[j+i*4] = m[i+j*4];
                    m[i+j*4] = temp;
                }
            }
            
            return this;
        }
    
        //!Return the transpose
        
        /// <summary>
        /// Get a transposed matrix
        /// </summary>
        /// <returns>A new matrix that is the transposed form of this</returns>
        public GLMatrix4D GetTranspose()
        {
            GLMatrix4D temp = new GLMatrix4D();
           
            for( int i = 0; i < 4; ++i)
            {
                for( int j = 0; j < 4; ++j)
                {
                    temp.m[j+i*4] = m[i+j*4];
                }
            }
            
            return temp;
        }
    
        /// <summary>
        /// Identity matrix
        /// </summary>
        /// <returns>A new identity matrix</returns>
        public static GLMatrix4D Identity()
        {
            GLMatrix4D ret = new GLMatrix4D();
    
            ret.m[0] = 1;   ret.m[4] = 0;   ret.m[8]  = 0;  ret.m[12] = 0;
            ret.m[1] = 0;   ret.m[5] = 1;   ret.m[9]  = 0;  ret.m[13] = 0;
            ret.m[2] = 0;   ret.m[6] = 0;   ret.m[10] = 1;  ret.m[14] = 0;
            ret.m[3] = 0;   ret.m[7] = 0;   ret.m[11] = 0;  ret.m[15] = 1;
    
            return ret;
        }
    
        /// <summary>
        /// Make this an identity matrix
        /// </summary>
        /// <returns>This matrix</returns>
        public GLMatrix4D LoadIdentity()
        {
            m[0] = 1;   m[4] = 0;   m[8]  = 0;  m[12] = 0;
            m[1] = 0;   m[5] = 1;   m[9]  = 0;  m[13] = 0;
            m[2] = 0;   m[6] = 0;   m[10] = 1;  m[14] = 0;
            m[3] = 0;   m[7] = 0;   m[11] = 0;  m[15] = 1;
    
            return this;
        }
    
        //!
        /// <summary>
        /// Make this an OpenGL rotation matrix
        /// </summary>
        /// <param name="angle">Angle of the rotation</param>
        /// <param name="x">X value of the rotation vector</param>
        /// <param name="y">Y value of the rotation vector</param>
        /// <param name="z">Z value of the rotation vector</param>
        /// <returns>This matrix</returns>
        public GLMatrix4D LoadRotate( double angle, double x, double y, double z )
        {
            double magSqr = x*x + y*y + z*z;
           
            if( magSqr != 1.0 )
            {
                double mag = Math.Sqrt( magSqr );
                x/=mag;
                y/=mag;
                z/=mag;
            }
            
            double c = Math.Cos( angle*Math.PI/180 );
            double s = Math.Sin( angle*Math.PI/180 );
            
            m[0] = x*x*(1-c)+c;
            m[1] = y*x*(1-c)+z*s;
            m[2] = z*x*(1-c)-y*s;
            m[3] = 0;
    
            m[4] = x*y*(1-c)-z*s;
            m[5] = y*y*(1-c)+c;
            m[6] = z*y*(1-c)+x*s;
            m[7] = 0;
    
            m[8] = x*z*(1-c)+y*s;
            m[9] = y*z*(1-c)-x*s;
            m[10] = z*z*(1-c)+c;
            m[11] = 0;
    
            m[12] = 0;
            m[13] = 0;
            m[14] = 0;
            m[15] = 1;
    
            return this;
        }
    
        /// <summary>
        /// Make this an X rotation matrix
        /// </summary>
        /// <param name="angle">Angle of rotation</param>
        /// <returns>This matrix</returns>
        /// <remarks>Load rotate[X,Y,Z,XYZ] specialisations by sebastien bloc. Same as loadRotate(angle,1,0,0)</remarks>
        public GLMatrix4D LoadRotateX( double angle )
        {
            double c = Math.Cos( angle*Math.PI/180 );
            double s = Math.Sin( angle*Math.PI/180 );
            
            m[0] = 1;
            m[1] = 0;
            m[2] = 0;
            m[3] = 0;
    
            m[4] = 0;
            m[5] = c;
            m[6] = s;
            m[7] = 0;
    
            m[8] = 0;
            m[9] = -s;
            m[10] = c;
            m[11] = 0;
    
            m[12] = 0;
            m[13] = 0;
            m[14] = 0;
            m[15] = 1;
    
            return this;
        }
    
         /// <summary>
        /// Make this an Y rotation matrix
        /// </summary>
        /// <param name="angle">Angle of rotation</param>
        /// <returns>This matrix</returns>
        /// <remarks>Load rotate[X,Y,Z,XYZ] specialisations by sebastien bloc. Same as loadRotate(angle,0,1,0)</remarks>
        public GLMatrix4D loadRotateY( double angle )
        {
            double c = Math.Cos( angle*Math.PI/180 );
            double s = Math.Sin( angle*Math.PI/180 );
            
            m[0] = c;
            m[1] = 0;
            m[2] = -s;
            m[3] = 0;
    
            m[4] = 0;
            m[5] = 1;
            m[6] = 0;
            m[7] = 0;
    
            m[8] = s;
            m[9] = 0;
            m[10] = c;
            m[11] = 0;
    
            m[12] = 0;
            m[13] = 0;
            m[14] = 0;
            m[15] = 1;
    
            return this;
        }
    
         /// <summary>
        /// Make this an Z rotation matrix
        /// </summary>
        /// <param name="angle">Angle of rotation</param>
        /// <returns>This matrix</returns>
        /// <remarks>Load rotate[X,Y,Z,XYZ] specialisations by sebastien bloc. Same as loadRotate(angle,0,0,1)</remarks>
        public GLMatrix4D loadRotateZ( double angle )
        {
            double c = Math.Cos( angle*Math.PI/180 );
            double s = Math.Sin( angle*Math.PI/180 );
            
            m[0] = c;
            m[1] = s;
            m[2] = 0;
            m[3] = 0;
    
            m[4] = -s;
            m[5] = c;
            m[6] = 0;
            m[7] = 0;
    
            m[8] = 0;
            m[9] = 0;
            m[10] = 1;
            m[11] = 0;
    
            m[12] = 0;
            m[13] = 0;
            m[14] = 0;
            m[15] = 1;
    
            return this;
        }
    
        /// <summary>
        /// Apply a rotation to this matrix
        /// </summary>
        /// <param name="angle">Angle of rotation</param>
        /// <param name="x">X value of the rotation vector</param>
        /// <param name="y">Y value of the rotation vector</param>
        /// <param name="z">Z value of the rotation vector</param>
        /// <returns>This matrix</returns>
        public GLMatrix4D applyRotate( double angle, double x, double y, double z )
        {
            GLMatrix4D temp = new GLMatrix4D();
            temp.LoadRotate( angle,x,y,z );
            
            return Multiply3By3( temp );
        }
    
        /// <summary>
        /// Apply a X rotation to this matrix
        /// </summary>
        /// <param name="angle">Angle of rotation</param>
        /// <returns>This matrix</returns>
        /// <remarks>Apply rotate[X,Y,Z,XYZ] specialisations by sebastien bloc</remarks>
        public GLMatrix4D ApplyRotateX( double angle )
        {
            GLMatrix4D temp = new GLMatrix4D();
            temp.LoadRotateX( angle );
            
            return Multiply3By3( temp );
        }
    
        /// <summary>
        /// Apply a Y rotation to this matrix
        /// </summary>
        /// <param name="angle">Angle of rotation</param>
        /// <returns>This matrix</returns>
        /// <remarks>Apply rotate[X,Y,Z,XYZ] specialisations by sebastien bloc</remarks>
        public GLMatrix4D ApplyRotateY( double angle )
        {
            GLMatrix4D temp = new GLMatrix4D();
            temp.loadRotateY( angle );
            
            return Multiply3By3( temp );
        }
    
        /// <summary>
        /// Apply a Z rotation to this matrix
        /// </summary>
        /// <param name="angle">Angle of rotation</param>
        /// <returns>This matrix</returns>
        /// <remarks>Apply rotate[X,Y,Z,XYZ] specialisations by sebastien bloc</remarks>
        public GLMatrix4D ApplyRotateZ( double angle )
        {
            GLMatrix4D temp = new GLMatrix4D();
            temp.loadRotateZ( angle );
            
            return Multiply3By3( temp );
        }
    
        /// <summary>
        /// Apply a XYZ rotation to this matrix
        /// </summary>
        /// <param name="angle">Angle of rotation around X axis</param>
        /// <param name="angle">Angle of rotation around Y axis</param>
        /// <param name="angle">Angle of rotation around Z axis</param>
        /// <returns>This matrix</returns>
        /// <remarks>Apply rotate[X,Y,Z,XYZ] specialisations by sebastien bloc</remarks>
        public GLMatrix4D ApplyRotateXYZ( double x,double y,double z )
        {
            GLMatrix4D temp = new GLMatrix4D();
            temp.LoadRotateX( x );
            Multiply3By3( temp );
            temp.loadRotateY( y );
            Multiply3By3( temp );
            temp.loadRotateZ( z );
            
            return Multiply3By3( temp );
        }
    
        /// <summary>
        /// Make this an OpenGL scale matrix
        /// </summary>
        /// <param name="x">X scale value</param>
        /// <param name="y">Y scale value</param>
        /// <param name="z">Z scale value</param>
        /// <returns>This Matrix</returns>
        public GLMatrix4D LoadScale( double x, double y, double z )
        {
            m[0] = x;   m[4] = 0;   m[8]  = 0;  m[12] = 0;
            m[1] = 0;   m[5] = y;   m[9]  = 0;  m[13] = 0;
            m[2] = 0;   m[6] = 0;   m[10] = z;  m[14] = 0;
            m[3] = 0;   m[7] = 0;   m[11] = 0;  m[15] = 1;
    
            return this;
        }
    
        /// <summary>
        /// Apply an XY scale to this matrix
        /// </summary>
        /// <param name="x">X scale value</param>
        /// <param name="y">Y scale value</param>
        /// <returns>This Matrix</returns>
        public GLMatrix4D ApplyScale( double x, double y )
        {
            //improved version
            //assuming z = 1
            m[0]*=x;    m[1]*=x;    m[2]*=x;    m[3]*=x;
            m[4]*=y;    m[5]*=y;    m[6]*=y;    m[7]*=y;
            
            return this;
        }
    
        /// <summary>
        /// Apply an XYZ scale to this matrix
        /// </summary>
        /// <param name="x">X scale value</param>
        /// <param name="y">Y scale value</param>
        /// <param name="z">Z scale value</param>
        /// <returns>This Matrix</returns>
        public GLMatrix4D ApplyScale( double x, double y, double z )
        {
            //improved version
            m[0]*=x;    m[1]*=x;    m[2]*=x;    m[3]*=x;
            m[4]*=y;    m[5]*=y;    m[6]*=y;    m[7]*=y;
            m[8]*=z;    m[9]*=z;    m[10]*=z;   m[11]*=z;
            
            return this;
        }
    
         /// <summary>
        /// Apply an XY scale vector to this matrix
        /// </summary>
        /// <param name="scale">2D scale vector</param>
        /// <returns>This Matrix</returns>
        public GLMatrix4D ApplyScale( GLVector2d scale)
        {
            //improved version
            //Assuming z = 1
           m[0]*=scale.x;    m[1]*=scale.x;    m[2]*=scale.x;    m[3]*=scale.x;
           m[4]*=scale.y;    m[5]*=scale.y;    m[6]*=scale.y;    m[7]*=scale.y;
           
           return this;
        }
    
        /// <summary>
        /// Apply an XYZ scale vector to this matrix
        /// </summary>
        /// <param name="scale">3D scale vector</param>
        /// <returns>This Matrix</returns>
        public GLMatrix4D applyScale( GLVector3d scale)
        {
            //improved version
            m[0]*=scale.x;    m[1]*=scale.x;    m[2]*=scale.x;    m[3]*=scale.x;
            m[4]*=scale.y;    m[5]*=scale.y;    m[6]*=scale.y;    m[7]*=scale.y;
            m[8]*=scale.z;    m[9]*=scale.z;    m[10]*=scale.z;   m[11]*=scale.z;
            
            return this;
        }
    
        //!Make this an OpenGL translate matrix
        /// <summary>
        /// Make this a translation matrix
        /// </summary>
        /// <param name="x">X translation</param>
        /// <param name="y">Y translation</param>
        /// <param name="z">Z translation</param>
        /// <returns>This matrix</returns>
        public GLMatrix4D LoadTranslation( double x, double y, double z )
        {
           
        	m[0] = 1;   m[4] = 0;   m[8]  = 0;  m[12] = x;
            m[1] = 0;   m[5] = 1;   m[9]  = 0;  m[13] = y;
            m[2] = 0;   m[6] = 0;   m[10] = 1;  m[14] = z;
            m[3] = 0;   m[7] = 0;   m[11] = 0;  m[15] = 1;
    
            return this;
        }
    
        /// <summary>
        /// Apply a XY translation to this matrix
        /// </summary>
        /// <param name="x">X translation</param>
        /// <param name="y">Y translation</param>
        /// <returns>This matrix</returns>
        public GLMatrix4D ApplyTranslation( double x, double y )
        {
            //improved version
            //assuming z = 0
            m[12] += m[0]*x + m[4]*y;
            m[13] += m[1]*x + m[5]*y;
            m[14] += m[2]*x + m[6]*y;
            
            return this;
        }
    
        /// <summary>
        /// Apply a XY translation to this matrix
        /// </summary>
        /// <param name="x">X translation</param>
        /// <param name="y">Y translation</param>
        /// <param name="z">Z translation</param>
        /// <returns>This matrix</returns>
        public GLMatrix4D ApplyTranslation( double x, double y, double z )
        {
            //improved version
            m[12] += m[0]*x + m[4]*y + m[8]*z;
            m[13] += m[1]*x + m[5]*y + m[9]*z;
            m[14] += m[2]*x + m[6]*y + m[10]*z;
            
            return this;
        }
    
        /// <summary>
        /// Apply a XY translation to this matrix
        /// </summary>
        /// <param name="trans">2D vector</param>
        /// <returns>This matrix</returns>
        public GLMatrix4D ApplyTranslation( GLVector2d trans)
        {
            ////improved version
            ////assuming z = 0
            m[12] += m[0]*trans.x + m[4]*trans.y;
            m[13] += m[1]*trans.x + m[5]*trans.y;
            m[14] += m[2]*trans.x + m[6]*trans.y;
            
            return this;
        }
    
        /// <summary>
        /// Apply a XYZ translation to this matrix
        /// </summary>
        /// <param name="trans">3D vector</param>
        /// <returns>This matrix</returns>
        public GLMatrix4D applyTranslate( GLVector3d trans)
        {
            //improved version
            m[12] += m[0]*trans.x + m[4]*trans.y + m[8]*trans.z;
            m[13] += m[1]*trans.x + m[5]*trans.y + m[9]*trans.z;
            m[14] += m[2]*trans.x + m[6]*trans.y + m[10]*trans.z;
            
            return this;
        }
    
        
        /// <summary>
        /// Create a frustrum matrix
        /// </summary>
        /// <param name="left">Left value</param>
        /// <param name="right">Right value</param>
        /// <param name="bottom">Bottom value</param>
        /// <param name="top">Top value</param>
        /// <param name="zNear">Near z plane distance</param>
        /// <param name="zFar">Far z plane distance</param>
        /// <returns>Frustrum matrix</returns>
        public static GLMatrix4D glFrustrum( double left, double right, double bottom, double top, double zNear, double zFar )
        {
            GLMatrix4D ret = new GLMatrix4D();
            ret.m[0] = 2*zNear/(right-left);
            ret.m[1] = 0;
            ret.m[2] = 0;
            ret.m[3] = 0;
    
            ret.m[4] = 0;
            ret.m[5] = 2*zNear/(top-bottom);
            ret.m[6] = 0;
            ret.m[7] = 0;
    
            ret.m[8] = (right+left)/(right-left);
            ret.m[9] = (top+bottom)/(top-bottom);
            ret.m[10] = -(zFar+zNear)/(zFar-zNear);
            ret.m[11] = -1;
    
            ret.m[12] = 0;
            ret.m[13] = 0;
            ret.m[14] = -2*zFar*zNear/(zFar-zNear);
            ret.m[15] = 0;
    
            return ret;
        }
    
        
        /// <summary>
        /// Make this a frustrum matrix
        /// </summary>
        /// <param name="left">Left value</param>
        /// <param name="right">Right value</param>
        /// <param name="bottom">Bottom value</param>
        /// <param name="top">Top value</param>
        /// <param name="zNear">Near z plane distance</param>
        /// <param name="zFar">Far z plane distance</param>
        /// <returns>This matrix</returns>
        public GLMatrix4D LoadFrustum( double left, double right, double bottom, double top, double zNear, double zFar )
        {
            m[0] = 2*zNear/(right-left);
            m[1] = 0;
            m[2] = 0;
            m[3] = 0;
    
            m[4] = 0;
            m[5] = 2*zNear/(top-bottom);
            m[6] = 0;
            m[7] = 0;
    
            m[8] = (right+left)/(right-left);
            m[9] = (top+bottom)/(top-bottom);
            m[10] = -(zFar+zNear)/(zFar-zNear);
            m[11] = -1;
    
            m[12] = 0;
            m[13] = 0;
            m[14] = -2*zFar*zNear/(zFar-zNear);
            m[15] = 0;
    
            return this;
        }
    
        /// <summary>
        /// Make an orthogonal matrix
        /// </summary>
        /// <param name="left">Left value</param>
        /// <param name="right">Right value</param>
        /// <param name="bottom">Bottom value</param>
        /// <param name="top">Top value</param>
        /// <param name="zNear">Near z plane distance</param>
        /// <param name="zFar">Far z plane distance</param>
        /// <returns>Frustrum matrix</returns>
        public static GLMatrix4D Orthogonal( double left, double right, double bottom, double top, double zNear, double zFar )
        {
            GLMatrix4D ret = new GLMatrix4D();
    
            ret.m[0] = 2/(right-left);
            ret.m[1] = 0;
            ret.m[2] = 0;
            ret.m[3] = 0;
    
            ret.m[4] = 0;
            ret.m[5] = 2/(top-bottom);
            ret.m[6] = 0;
            ret.m[7] = 0;
    
            ret.m[8] = 0;
            ret.m[9] = 0;
            ret.m[10] = -2/(zFar-zNear);
            ret.m[11] = 0;
    
            ret.m[12] = -(right+left)/(right-left);
            ret.m[13] = -(top+bottom)/(top-bottom);
            ret.m[14] = -(zFar+zNear)/(zFar-zNear);
            ret.m[15] = 1;
    
            return ret;
        }
    
        /// <summary>
        /// Make this an orthogonal matrix
        /// </summary>
        /// <param name="left">Left value</param>
        /// <param name="right">Right value</param>
        /// <param name="bottom">Bottom value</param>
        /// <param name="top">Top value</param>
        /// <param name="zNear">Near z plane distance</param>
        /// <param name="zFar">Far z plane distance</param>
        /// <returns>This matrix</returns>
        public GLMatrix4D LoadOrthogonal(double left, double right, double bottom, double top, double zNear, double zFar )
        {
            m[0] = 2/(right-left);
            m[1] = 0;
            m[2] = 0;
            m[3] = 0;
    
            m[4] = 0;
            m[5] = 2/(top-bottom);
            m[6] = 0;
            m[7] = 0;
    
            m[8] = 0;
            m[9] = 0;
            m[10] = -2/(zFar-zNear);
            m[11] = 0;
    
            m[12] = -(right+left)/(right-left);
            m[13] = -(top+bottom)/(top-bottom);
            m[14] = -(zFar+zNear)/(zFar-zNear);
            m[15] = 1;
    
            return this;
        }
    
        /// <summary>
        /// Make this a view matrix
        /// </summary>
        /// <param name="front">Front vector</param>
        /// <param name="up">Up vector</param>
        /// <param name="side">Side vector</param>
        /// <returns>This matrix</returns>
        public GLMatrix4D LoadView( GLVector3d front, GLVector3d up, GLVector3d side )
        {
            m[0] = side.x;
            m[1] = up.x;
            m[2] = -front.x;
            m[3] = 0;
    
            m[4] = side.y;
            m[5] = up.y;
            m[6] = -front.y;
            m[7] = 0;
    
            m[8] = side.z;
            m[9] = up.z;
            m[10] = -front.z;
            m[11] = 0;
    
            m[12] = 0;
            m[13] = 0;
            m[14] = 0;
            m[15] = 1;
    
            return this;
        }
    
        
       
    
        /// <summary>
        /// Get a string version of this matrix
        /// </summary>
        /// <returns>String representation</returns>
        /// <remarks>Row major order is expected to conform with standard input</remarks>
        public override string ToString()
        {
            string outstring = "";
            for( int j = 0; j < 4; ++j )
            {
                for( int i = 0; i < 4; ++i )
                {
                    outstring += m[i*4+j] + " ";
                }
                outstring += "\n";
            }
          
            return outstring;
        }
    
        /// <summary>
        /// Helper function for creating cofactors
        /// </summary>
        /// <param name="f1">Factor 1</param>
        /// <param name="mj1">Major 1</param>
        /// <param name="mi1">Minor 1</param>
        /// <param name="f2">Factor 2</param>
        /// <param name="mj2">Major 2</param>
        /// <param name="mi2">Minor 2</param>
        /// <param name="f3">Factor 3</param>
        /// <param name="mj3">Major 3</param>
        /// <param name="mi3">Minor 3</param>
        /// <returns>Cofactor</returns>
        /// <remarks>
        ///  Cofactor maker after the looping determinant theory I'm sure we all learnt in high-school
        ///  *  factor1 |   ^          |
        ///  *  ------------|----------v----
        ///  *          | major   |
        ///  *          |         |  minor
        ///  *              ^          |
        ///  *  factor2 |   |          |
        ///  *  ------------|----------v----
        ///  *          | major   |
        ///  *          |         |  minor
        ///  *              ^          |
        ///  *  factor3 |   |          |
        ///  *  ------------|----------v----
        ///  *          | major   |
        ///  *          |         |  minor
        ///  *              ^          |
        ///  *              |          v
        /// </remarks>
        private double CofactorHelper( double f1,double mj1,double mi1, double f2,double mj2,double mi2, double f3,double mj3,double mi3 )
        {
            return f1*(mj1*mi1-mj2*mi3) + f2*(mj2*mi2-mj3*mi1) + f3*(mj3*mi3-mj1*mi2);
        }
    
        //T cofactorm0()const { return m[5]*(m[10]*m[15]-m[11]*m[14])+m[6]*(m[11]*m[13]-m[8]*m[15])+m[7]*(m[8]*m[14]*m[10]*m[13]);  }
        
        
        /// <summary>
        /// Cofactor of m[0]
        /// </summary>
        /// <returns>Cofactor</returns>
        private double CofactorM0() { return CofactorHelper(m[5],m[10],m[15], m[6],m[11],m[13], m[7],m[9],m[14]); }
        
        /// <summary>
        /// Cofactor of m[1]
        /// </summary>
        /// <returns>Cofactor</returns>
        private double CofactorM1() { return CofactorHelper(m[6],m[11],m[12], m[7],m[8],m[14], m[4],m[10],m[15]); }
        
        /// <summary>
        /// Cofactor of m[2]
        /// </summary>
        /// <returns>Cofactor</returns>
        private double CofactorM2() { return CofactorHelper(m[7],m[8],m[13], m[4],m[9],m[15], m[5],m[11],m[12]); }
        
        /// <summary>
        /// Cofactor of m[3]
        /// </summary>
        /// <returns>Cofactor</returns>
        private double CofactorM3() { return CofactorHelper(m[4],m[9],m[14], m[5],m[10],m[12], m[6],m[8],m[13]); }
    
        /// <summary>
        /// Cofactor of m[4]
        /// </summary>
        /// <returns>Cofactor</returns>
        private double CofactorM4() { return CofactorHelper(m[9],m[14],m[3], m[10],m[15],m[1], m[11],m[13],m[2]); }
        
        /// <summary>
        /// Cofactor of m[5]
        /// </summary>
        /// <returns>Cofactor</returns>
        private double CofactorM5() { return CofactorHelper(m[10],m[15],m[0], m[11],m[12],m[2], m[8],m[14],m[3]); }
        
        /// <summary>
        /// Cofactor of m[6]
        /// </summary>
        /// <returns>Cofactor</returns>
        private double CofactorM6() { return CofactorHelper(m[11],m[12],m[1], m[8],m[13],m[3], m[9],m[15],m[0]); }
        
        /// <summary>
        /// Cofactor of m[7]
        /// </summary>
        /// <returns>Cofactor</returns>
        private double CofactorM7() { return CofactorHelper(m[8],m[13],m[2], m[9],m[14],m[0], m[10],m[12],m[1]); }
    
        /// <summary>
        /// Cofactor of m[8]
        /// </summary>
        /// <returns></returns>
        private double CofactorM8() { return CofactorHelper(m[13],m[2],m[7], m[14],m[3],m[5], m[15],m[1],m[6]); }
        
        /// <summary>
        /// Cofactor of m[9]
        /// </summary>
        /// <returns>Cofactor</returns>
        private double CofactorM9() { return CofactorHelper(m[14],m[13],m[4], m[15],m[0],m[6], m[12],m[2],m[7]); }
        
        /// <summary>
        /// Cofactor of m[10]
        /// </summary>
        /// <returns>Cofactor</returns>
        private double CofactorM10() { return CofactorHelper(m[15],m[0],m[5], m[12],m[1],m[7], m[13],m[3],m[4]); }
        
        /// <summary>
        /// Cofactor of m[11]
        /// </summary>
        /// <returns>Cofactor</returns>
        private double CofactorM11() { return CofactorHelper(m[12],m[1],m[6], m[13],m[2],m[4], m[14],m[0],m[5]); }
            
        /// <summary>
        /// Cofactor of m[12]
        /// </summary>
        /// <returns>Cofactor</returns>
        private double CofactorM12() { return CofactorHelper(m[1],m[6],m[11], m[2],m[7],m[9], m[3],m[5],m[10]); }
        
        /// <summary>
        /// Cofactor of m[13]
        /// </summary>
        /// <returns>Cofactor</returns>
        private double CofactorM13() { return CofactorHelper(m[2],m[7],m[8], m[3],m[4],m[10], m[10],m[6],m[11]); }
        
        /// <summary>
        /// Cofactor of m[14]
        /// </summary>
        /// <returns>Cofactor</returns>
        private double CofactorM14() { return CofactorHelper(m[3],m[4],m[9], m[0],m[5],m[11], m[1],m[7],m[8]); }

        /// <summary>
        /// Cofactor of m[15]
        /// </summary>
        /// <returns>Cofactor</returns>
        private double CofactorM15() { return CofactorHelper(m[0],m[5],m[10], m[1],m[6],m[8], m[2],m[4],m[9]); }
        
        /// <summary>
        /// Get the hashcode of this matrix
        /// </summary>
        /// <returns>Integer hashcode</returns>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		} 
    }
}
