 //**************************************************************************
 //*   Copyright (C) 2004,2006 by Jacques Gasselin, Hugh Perkins                     *
 //*   jacquesgasselin@hotmail.com                                           *
 //*   hughperkins@gmail.com  http://manageddreams.com     *
 //*                                                                *
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
// - Ported to C#.Net by Hugh Perkins, 2006

// Notes on C# port:
// - C# doesnt do templates in 1.1 so doing everything as a double
// - Everything that depends on OpenGL directly stripped, to reduce dependencies

using System;

namespace MathGl
{
    public class GLVector2d
    {
        public const int D = 2;
        public double[] val = new double[D];        

    //!Create an uninitialised vector
        public GLVector2d()
        { }
    
        //!Create an initialised vector
        public GLVector2d(double v)
        {   val[0] = val[1] = v; }
    
        //!Create an initialised vector from values
        public GLVector2d( double v1,  double v2)
        {   val[0] = v1; val[1] = v2; }
    
        //!Copy a vector
        public GLVector2d( GLVector2d gv)
        {
            Buffer.BlockCopy( gv.val, 0, val, 0, Buffer.ByteLength( gv.val ) );
        }
    
        //!Create a vector from an array
        public GLVector2d( double[] f)
        {
            Buffer.BlockCopy( f, 0, val, 0, Buffer.ByteLength( f ) );            
        }
    
        //! element by element initialiser
        public void set( double v1,  double v2)
        {   val[0] = v1; val[1] = v2; }
    
        //! element by element accessor
        public virtual double this[ int ind ]
        {
            get
            {
                return val[ind];
            }
            //set
            //{
             //   val[ind] = value;
            //}
        }

        //! implicit casting to a pointer
       // public operator double* (void)
       // {   return val; }
    
        //! implicit casting to a  pointer
        //public operator  double* (void)
        //{   return val; }
    
        //!copy values to a array
        public double[] ToArray()
        {
            double[] newarray = new double[D];
            Buffer.BlockCopy( val, 0, newarray, 0, Buffer.ByteLength( val ) );            
            return newarray;
        }
    
        //!Get the sum of this and a vector
        public  static GLVector2d operator+ ( GLVector2d one, GLVector2d two)
        {
            return new GLVector2d(one.x+two.x,one.y+two.y);
        }
    
        //!Get the difference of this and a vector
        public  static GLVector2d operator- ( GLVector2d one, GLVector2d two)
        {
            return new GLVector2d(one.x-two.x,one.y-two.y);
        }
    
        //!Get the element-by-element product of this and a vector
        public  static GLVector2d operator* ( GLVector2d one, GLVector2d two)
        {
            return new GLVector2d(one.x*two.x,one.y*two.y);
        }
        
        //!Get the element-by-element quota of this and a vector
        public  static GLVector2d operator/ ( GLVector2d one, GLVector2d two)
        {
            return new GLVector2d(one.x/two.x,one.y/two.y);
        }
    
        //!Get the element-by-element product of this and a scalar
        public  static GLVector2d operator* ( GLVector2d one, double v )
        {
            return new GLVector2d(one.x*v,one.y*v);
        }
    
        //!Get the element-by-element quota of this and a scalar
        public  static GLVector2d operator/ ( GLVector2d one, double v )
        {
            return new GLVector2d(one.x/v,one.y/v);
        }
        
        //!negate this
         public static GLVector2d operator-(GLVector2d one)
        {
            return new GLVector2d(-one.x,-one.y);
        }
    
        //!Get the dot product of this and a vector
        public double dot( GLVector2d gv)
        {
            return x*gv.x + y*gv.y;
        }
    
        //!Get the length of this
        public double length()
        {   return Math.Sqrt(lengthSqr()); }
    
        //!Get the length squared, less computation than length()
        public double lengthSqr()
        {   return x * x + y * y; }
    
        //!Get the the unit vector of this
        public GLVector2d unit()
        {
            //assert(lengthSqr() != 0 && "Divide by zero");
            return new GLVector2d(this)/length();
        }
    
        //!Normalize this, makes this a unit vector, US spelling
        public GLVector2d normalize()
        {
            //assert(lengthSqr() != 0 && "Divide by zero");
            double thislength = length();
            for( int i = 0; i < D; i++ )
            {
                val[i] /= thislength;
            }
            return this;
        }
    
        //!Get the projection of this and a vector
         public double projection( GLVector2d vin)
        {
            return dot(vin);
        }
    
        //!Get the orthogonal projection of this and a vector
         public GLVector2d orthogonalProjection( GLVector2d vin)
        {   return vin - vectorProjection(vin); }
    
        //!Get the vector projection of this and a vector
        public GLVector2d vectorProjection( GLVector2d vin)
        {   return this * dot(vin); }
    

        // here, we create a new constructor to handle strings
        public GLVector2d( string inputstring )
        {
            string[]splitinputstring = inputstring.Split(new char[]{' '});
            val[0] = Int32.Parse( splitinputstring[0] );
            val[1] = Int32.Parse( splitinputstring[1] );
        }
        
        public override string ToString()
        {
            return val[0].ToString() + " " + val[1].ToString();
        }        
        
        // this hopefully gets optimized away by compiler to a certain extent:
        public double x
        {
            get{ return val[0];}
            set{ val[0] = value;}
        }
        public double y
        {
            get{ return val[1];}
            set{ val[1] = value;}
        }
    }
        
    //!a 3D vector class for OpenGL
    public class GLVector3d
    {
        public const int D = 3;
        public double[] val = new double[D];        
        
    //!Create an uninitialised vector
        public GLVector3d()
        { }
    
        //!Create an initialised vector
        public GLVector3d(double v)
        {   val[0] = val[1] = val[2] = v; }
    
        //!Create an initialised vector from values
        public GLVector3d( double v1,  double v2,double v3)
        {   val[0] = v1; val[1] = v2; val[2] = v3; }
    
        //!Copy a vector
        public GLVector3d( GLVector3d gv)
        {
            Buffer.BlockCopy( gv.val, 0, val, 0, Buffer.ByteLength( gv.val ) );
        }
    
        //!Create a vector from an array
        public GLVector3d( double[] f)
        {
            Buffer.BlockCopy( f, 0, val, 0, Buffer.ByteLength( f ) );            
        }
    
        //! element by element initialiser
        public void set( double v1,  double v2, double v3)
        {   val[0] = v1; val[1] = v2; val[2] = v3; }
    
        //! element by element accessor
        public virtual double this[ int ind ]
        {
            get
            {
                return val[ind];
            }
            //set
            //{
             //   val[ind] = value;
            //}
        }

        //! implicit casting to a pointer
       // public operator double* (void)
       // {   return val; }
    
        //! implicit casting to a  pointer
        //public operator  double* (void)
        //{   return val; }
    
        //!copy values to a array
        public double[] ToArray()
        {
            double[] newarray = new double[D];
            Buffer.BlockCopy( val, 0, newarray, 0, Buffer.ByteLength( val ) );            
            return newarray;
        }
    
        //!Get the sum of this and a vector
        public  static GLVector3d operator+ ( GLVector3d one, GLVector3d two)
        {
            return new GLVector3d(one.val[0]+two.val[0],one.val[1]+two.val[1],one.val[2] + two.val[2]);
        }
    
        //!Get the difference of this and a vector
        public  static GLVector3d operator- ( GLVector3d one, GLVector3d two)
        {
            return new GLVector3d(one.x-two.x,one.y-two.y,one.z - two.z);
        }
    
        //!Get the element-by-element product of this and a vector
        public  static GLVector3d operator* ( GLVector3d one, GLVector3d two)
        {
            return new GLVector3d(one.x*two.x,one.y*two.y,one.z*two.z);
        }
        
        //!Get the element-by-element quota of this and a vector
        public  static GLVector3d operator/ ( GLVector3d one, GLVector3d two)
        {
            return new GLVector3d(one.x/two.x,one.y/two.y,one.z/two.z);
        }
    
        //!Get the element-by-element product of this and a scalar
        public  static GLVector3d operator* ( GLVector3d one, double v )
        {
            return new GLVector3d(one.x*v,one.y*v,one.z*v);
        }
    
        //!Get the element-by-element quota of this and a scalar
        public  static GLVector3d operator/ ( GLVector3d one, double v )
        {
            return new GLVector3d(one.x/v,one.y/v,one.z/v);
        }
        
        //!negate this
         public static GLVector3d operator-(GLVector3d one)
        {
            return new GLVector3d(-one.x,-one.y,one.z);
        }
    
        //!Get the dot product of this and a vector
        public double dot( GLVector3d gv)
        {
            return x*gv.x + y*gv.y + z * gv.z;
        }
    
        //!Get the length of this
        public double length()
        {   return Math.Sqrt(lengthSqr()); }
    
        //!Get the length squared, less computation than length()
        public double lengthSqr()
        {   return x * x + y * y + z * z; }
    
        //!Get the the unit vector of this
        public GLVector3d unit()
        {
            //assert(lengthSqr() != 0 && "Divide by zero");
            return new GLVector3d(this)/length();
        }
    
        //!Normalize this, makes this a unit vector, US spelling
        public GLVector3d normalize()
        {
            //assert(lengthSqr() != 0 && "Divide by zero");
            double thislength = length();
            for( int i = 0; i < D; i++ )
            {
                val[i] /= thislength;
            }
            return this;
        }
    
        //!Get the projection of this and a vector
         public double projection( GLVector3d vin)
        {
            return dot(vin);
        }
    
        //!Get the orthogonal projection of this and a vector
         public GLVector3d orthogonalProjection( GLVector3d vin)
        {   return vin - vectorProjection(vin); }
    
        //!Get the vector projection of this and a vector
        public GLVector3d vectorProjection( GLVector3d vin)
        {   return this * dot(vin); }
    

        // here, we create a new constructor to handle strings
        public GLVector3d( string inputstring )
        {
            string[]splitinputstring = inputstring.Split(new char[]{' '});
            val[0] = Int32.Parse( splitinputstring[0] );
            val[1] = Int32.Parse( splitinputstring[1] );
            val[2] = Int32.Parse( splitinputstring[2] );
        }
        
        public override string ToString()
        {
            return val[0].ToString() + " " + val[1].ToString() + " " + val[2].ToString();
        }        
    
        //!Get the cross-product of this and a vector
        public GLVector3d getCross( GLVector3d gv)
        {
            return new GLVector3d(y*gv.z-z*gv.y,z*gv.x-x*gv.z,x*gv.y-y*gv.x);
        }
    
        //!Apply the cross-product of this and a vector
        public GLVector3d cross( GLVector3d gv)
        {
            double[] temp = new double[]{ x, y, z };
    
            x = temp[1] * gv.z - temp[2] * gv.y;
            y = temp[2] * gv.x - temp[0] * gv.z;
            z = temp[0] * gv.y - temp[1] * gv.x;
    
            return this;
        }
    
        // this hopefully gets optimized away by compiler to a certain extent:
        public double x
        {
            get{ return val[0];}
            set{ val[0] = value;}
        }
        public double y
        {
            get{ return val[1];}
            set{ val[1] = value;}
        }
        public double z
        {
            get{ return val[2];}
            set{ val[2] = value;}
        }
    }
    
    //!a 3D rational vector class for OpenGL
    public class GLVector4d
    {
        public const int D = 4;
        public double[] val = new double[D];

    //!Create an uninitialised vector
        public GLVector4d()
        { }
    
        //!Create an initialised vector
        public GLVector4d(double v)
        {   val[0] = val[1] = val[2] = val[3] = v; }
    
        //!Create an initialised vector from values
        public GLVector4d( double v1,  double v2, double v3, double v4)
        {   val[0] = v1; val[1] = v2; val[2] = v3; val[3] = v4; }
    
        //!Copy a vector
        public GLVector4d( GLVector4d gv)
        {
            Buffer.BlockCopy( gv.val, 0, val, 0, Buffer.ByteLength( gv.val ) );
        }
    
        //!Create a vector from an array
        public GLVector4d( double[] f)
        {
            Buffer.BlockCopy( f, 0, val, 0, Buffer.ByteLength( f ) );            
        }
    
        //! element by element initialiser
        public void set( double v1,  double v2, double v3, double v4)
        {   val[0] = v1; val[1] = v2; val[2] = v3; val[3] = v4; }
    
        //! element by element accessor
        public virtual double this[ int ind ]
        {
            get
            {
                return val[ind];
            }
            //set
            //{
             //   val[ind] = value;
            //}
        }

        //! implicit casting to a pointer
       // public operator double* (void)
       // {   return val; }
    
        //! implicit casting to a  pointer
        //public operator  double* (void)
        //{   return val; }
    
        //!copy values to a array
        public double[] ToArray()
        {
            double[] newarray = new double[D];
            Buffer.BlockCopy( val, 0, newarray, 0, Buffer.ByteLength( val ) );            
            return newarray;
        }
    
        //!Get the sum of this and a vector
        public  static GLVector4d operator+ ( GLVector4d one, GLVector4d two)
        {
            return new GLVector4d(one.val[0]+two.val[0],one.val[1]+two.val[1],one.val[2] + two.val[2],one.val[3] + two.val[3]);
        }
    
        //!Get the difference of this and a vector
        public  static GLVector4d operator- ( GLVector4d one, GLVector4d two)
        {
            return new GLVector4d(one.x-two.x,one.y-two.y,one.z - two.z,one.w - two.w);
        }
    
        //!Get the element-by-element product of this and a vector
        public  static GLVector4d operator* ( GLVector4d one, GLVector4d two)
        {
            return new GLVector4d(one.x*two.x,one.y*two.y,one.z*two.z,one.w * two.w);
        }
        
        //!Get the element-by-element quota of this and a vector
        public  static GLVector4d operator/ ( GLVector4d one, GLVector4d two)
        {
            return new GLVector4d(one.x/two.x,one.y/two.y,one.z/two.z,one.w/two.w);
        }
    
        //!Get the element-by-element product of this and a scalar
        public  static GLVector4d operator* ( GLVector4d one, double v )
        {
            return new GLVector4d(one.x*v,one.y*v,one.z*v,one.w*v);
        }
    
        //!Get the element-by-element quota of this and a scalar
        public  static GLVector4d operator/ ( GLVector4d one, double v )
        {
            return new GLVector4d(one.x/v,one.y/v,one.z/v,one.w/v);
        }
        
        //!negate this
         public static GLVector4d operator-(GLVector4d one)
        {
            return new GLVector4d(-one.x,-one.y,one.z,-one.w);
        }
    
        //!Get the dot product of this and a vector
        public double dot( GLVector4d gv)
        {
            return x*gv.x + y*gv.y + z * gv.z + w * gv.w;
        }
    
        //!Get the length of this
        public double length()
        {   return Math.Sqrt(lengthSqr()); }
    
        //!Get the length squared, less computation than length()
        public double lengthSqr()
        {   return x * x + y * y + z * z + w * w; }
    
        //!Get the the unit vector of this
        public GLVector4d unit()
        {
            //assert(lengthSqr() != 0 && "Divide by zero");
            return new GLVector4d(this)/length();
        }
    
        //!Normalize this, makes this a unit vector, US spelling
        public GLVector4d normalize()
        {
            //assert(lengthSqr() != 0 && "Divide by zero");
            double thislength = length();
            for( int i = 0; i < D; i++ )
            {
                val[i] /= thislength;
            }
            return this;
        }
    
        //!Get the projection of this and a vector
         public double projection( GLVector4d vin)
        {
            return dot(vin);
        }
    
        //!Get the orthogonal projection of this and a vector
         public GLVector4d orthogonalProjection( GLVector4d vin)
        {   return vin - vectorProjection(vin); }
    
        //!Get the vector projection of this and a vector
        public GLVector4d vectorProjection( GLVector4d vin)
        {   return this * dot(vin); }
    

        // here, we create a new constructor to handle strings
        public GLVector4d( string inputstring )
        {
            string[]splitinputstring = inputstring.Split(new char[]{' '});
            val[0] = Int32.Parse( splitinputstring[0] );
            val[1] = Int32.Parse( splitinputstring[1] );
            val[2] = Int32.Parse( splitinputstring[2] );
            val[3] = Int32.Parse( splitinputstring[3] );
        }
        
        public override string ToString()
        {
            return val[0].ToString() + " " + val[1].ToString() + " " + val[2].ToString() + " " + val[3].ToString();
        }        
    
        //!Get the cross-product of this and a vector
        public GLVector4d getCross( GLVector4d gv)
        {
            return new GLVector4d(y*gv.z-z*gv.y,z*gv.w-w*gv.z,w*gv.x-x*gv.w,x*gv.y-y*gv.x);
        }
    
        //!Apply the cross-product of this and a vector
        public GLVector4d cross( GLVector4d gv)
        {
            double[] temp = new double[]{ x, y, z, w };
    
            x = temp[1] * gv.z - temp[2] * gv.y;
            y = temp[2] * gv.w - temp[3] * gv.z;
            z = temp[3] * gv.x - temp[0] * gv.w;
            w = temp[0] * gv.y - temp[1] * gv.x;
    
            return this;
        }
    
        // this hopefully gets optimized away by compiler to a certain extent:
        public double x
        {
            get{ return val[0];}
            set{ val[0] = value;}
        }
        public double y
        {
            get{ return val[1];}
            set{ val[1] = value;}
        }
        public double z
        {
            get{ return val[2];}
            set{ val[2] = value;}
        }
        public double w
        {
            get{ return val[3];}
            set{ val[3] = value;}
        }
    }
}
