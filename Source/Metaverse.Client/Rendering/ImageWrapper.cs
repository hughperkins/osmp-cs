using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using Tao.DevIl;

namespace OSMP
{
    // read/write images to/from arrays from/to files
    // arrays should be 4 channel, one byte per channel, so four bytes per pixel
    // pixels arranged in rows, close-packed
    public class ImageWrapper
    {
        public byte[] data;
        public int Width;
        public int Height;

        public byte GetRed( int x, int y )
        {
            return data[y * Width * 4 + x * 4];
        }

        public byte GetGreen( int x, int y )
        {
            return data[y * Width * 4 + x * 4 + 1];
        }

        public byte GetBlue( int x, int y )
        {
            return data[y * Width * 4 + x * 4 + 2];
        }

        public byte GetAlpha( int x, int y )
        {
            return data[y * Width * 4 + x * 4 + 3];
        }

        public void SetPixel( int x, int y, byte red, byte green, byte blue, byte alpha )
        {
            data[y * Width * 4 + x * 4] = red;
            data[y * Width * 4 + x * 4 + 1] = green;
            data[y * Width * 4 + x * 4 + 2] = blue;
            data[y * Width * 4 + x * 4 + 3] = alpha;
        }

        public ImageWrapper( int width, int height )
        {
            data = new byte[width * height * 4];
            this.Width = width;
            this.Height = height;
        }

        public ImageWrapper( byte[] data, int width, int height )
        {
            this.data = data;
            this.Width = width;
            this.Height = height;
        }

        public ImageWrapper( string fullfilepath )
        {
            byte[] bytes = ReadFile( fullfilepath );

            int ilImage;
            Il.ilGenImages( 1, out ilImage );
            Il.ilBindImage( ilImage );
            if (!Il.ilLoadL( Il.IL_TYPE_UNKNOWN, bytes, bytes.Length ))
                throw new Exception( "Failed to load image." );
            if (!Il.ilConvertImage( Il.IL_RGBA, Il.IL_UNSIGNED_BYTE ))
                throw new Exception( "Failed to convert image." );

            int m_BytesPerPixel = Il.ilGetInteger( Il.IL_IMAGE_BPP );
            int m_Width = Il.ilGetInteger( Il.IL_IMAGE_WIDTH );
            int m_Height = Il.ilGetInteger( Il.IL_IMAGE_HEIGHT );
            int m_Format = Il.ilGetInteger( Il.IL_IMAGE_FORMAT );
            int m_Depth = Il.ilGetInteger( Il.IL_IMAGE_DEPTH );

            byte[] imagedata = new byte[m_BytesPerPixel * m_Width * m_Height];
            IntPtr dataptr = Il.ilGetData();
            Marshal.Copy( dataptr, imagedata, 0, m_BytesPerPixel * m_Width * m_Height );
            Il.ilDeleteImages( 1, ref ilImage );

            this.data = imagedata;
            this.Width = m_Width;
            this.Height = m_Height;
            //return imagedata;
        }

        byte[] ReadFile( string filepath )
        {
            FileStream fs = new FileStream( filepath, FileMode.Open );
            byte[] bytes = new byte[fs.Length];
            fs.Read( bytes, 0, (int)fs.Length );
            fs.Close();
            return bytes;
        }

        public void Save( string fullfilepath )
        {
            int ilImage;
            Il.ilGenImages( 1, out ilImage );
            Il.ilBindImage( ilImage );
            Il.ilTexImage( Width, Height, 1, 4, Il.IL_RGBA, Il.IL_UNSIGNED_BYTE, data );
            if (File.Exists( fullfilepath ))
            {
                File.Delete( fullfilepath );
            }
            Il.ilSaveImage( fullfilepath );
            Il.ilDeleteImages( 1, ref ilImage );
        }
    }
}
