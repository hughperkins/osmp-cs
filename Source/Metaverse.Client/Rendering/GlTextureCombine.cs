// Copyright Hugh Perkins 2006
// hughperkins@gmail.com http://manageddreams.com
//
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the
// Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
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
using System.Collections.Generic;
using System.Text;
using Tao.OpenGl;
using Metaverse.Utility;

namespace OSMP
{
    // wraps Gl Texture combine stuff.  Yay!
    public class GlTextureCombine
    {
        // next line for testing only, comment out for ops
        //TestHarness Gl = new TestHarness();

        public GlTextureCombine()
        {
            Args = new GlCombineArg[3];
            Args[0] = new GlCombineArg();
            Args[1] = new GlCombineArg();
            Args[2] = new GlCombineArg();

            Operation = OperationType.Modulate;
            Args[0].SetRgbaSource( GlCombineArg.Source.Fragment );
            Args[1].SetRgbaSource( GlCombineArg.Source.Texture );
        }

        public enum OperationType
        {
            Replace,
            Add,
            Interpolate,
            Modulate,
            Subtract
        }

        public OperationType Operation = OperationType.Modulate;
        public GlCombineArg[] Args;

        // apply current settings
        public void Apply()
        {
            Gl.glTexEnvi( Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_COMBINE_ARB );
            switch (Operation)
            {
                case OperationType.Add:
                    {
                        Gl.glTexEnvi( Gl.GL_TEXTURE_ENV, Gl.GL_COMBINE_RGB_ARB, Gl.GL_ADD );
                        Gl.glTexEnvi( Gl.GL_TEXTURE_ENV, Gl.GL_COMBINE_ALPHA_ARB, Gl.GL_ADD );
                        Args[0].Apply( 0 );
                        Args[1].Apply( 1 );
                        break;
                    }
                case OperationType.Subtract:
                    {
                        Gl.glTexEnvi( Gl.GL_TEXTURE_ENV, Gl.GL_COMBINE_RGB_ARB, Gl.GL_SUBTRACT );
                        Gl.glTexEnvi( Gl.GL_TEXTURE_ENV, Gl.GL_COMBINE_ALPHA_ARB, Gl.GL_SUBTRACT );
                        Args[0].Apply( 0 );
                        Args[1].Apply( 1 );
                        break;
                    }
                case OperationType.Modulate:
                    {
                        Gl.glTexEnvi( Gl.GL_TEXTURE_ENV, Gl.GL_COMBINE_RGB_ARB, Gl.GL_MODULATE );
                        Gl.glTexEnvi( Gl.GL_TEXTURE_ENV, Gl.GL_COMBINE_ALPHA_ARB, Gl.GL_MODULATE );
                        Args[0].Apply( 0 );
                        Args[1].Apply( 1 );
                        break;
                    }
                case OperationType.Interpolate:
                    {
                        Gl.glTexEnvi( Gl.GL_TEXTURE_ENV, Gl.GL_COMBINE_RGB_ARB, Gl.GL_INTERPOLATE );
                        Gl.glTexEnvi( Gl.GL_TEXTURE_ENV, Gl.GL_COMBINE_ALPHA_ARB, Gl.GL_INTERPOLATE );
                        Args[0].Apply( 0 );
                        Args[1].Apply( 1 );
                        Args[2].Apply( 2 );
                        break;
                    }
                case OperationType.Replace:
                    {
                        Gl.glTexEnvi( Gl.GL_TEXTURE_ENV, Gl.GL_COMBINE_RGB_ARB, Gl.GL_REPLACE );
                        Gl.glTexEnvi( Gl.GL_TEXTURE_ENV, Gl.GL_COMBINE_ALPHA_ARB, Gl.GL_REPLACE );
                        Args[0].Apply( 0 );
                        break;
                    }
            }
        }

        public void SetOperation( OperationType operation )
        {
            this.Operation = operation;
        }

        public void SetArg( int argnum, GlCombineArg arg )
        {
            Args[argnum] = arg;
        }
    }

    // represents a single GlCombineArg, ie Arg0, Arg1, or Arg2
    public class GlCombineArg
    {
        // next line for testing only, comment out for ops
        //TestHarness Gl = new TestHarness();

        public enum Source
        {
            Texture,
            Fragment,
            Previous,
            Constant
        }
        public enum Operand
        {
            Rgb, // Rgb is arguably less ambiguous than Color
            OneMinusRgb,
            Alpha,
            OneMinusAlpha
        }
        public Source RgbSource = Source.Fragment;
        public Source AlphaSource = Source.Fragment;
        public Operand RgbOperand = Operand.Rgb;
        public Operand AlphaOperand = Operand.Alpha;
        public void SetRgbSource( Source source, Operand operand )
        {
            this.RgbSource = source;
            this.RgbOperand = operand;
        }
        public void SetAlphaSource( Source source, Operand operand )
        {
            this.AlphaSource = source;
            this.AlphaOperand = operand;
        }
        public void SetRgbaSource( Source source )
        {
            this.RgbSource = source;
            this.AlphaSource = source;
            this.RgbOperand = Operand.Rgb;
            this.AlphaOperand = Operand.Alpha;
        }
        public void Apply( int argnum )
        {
            int paramid_source_rgb = 0;
            int paramid_operando_rgb = 0;
            switch (argnum)
            {
                case 0:
                    paramid_source_rgb = Gl.GL_SOURCE0_RGB_ARB;
                    paramid_operando_rgb = Gl.GL_OPERAND0_RGB_ARB;
                    break;

                case 1:
                    paramid_source_rgb = Gl.GL_SOURCE1_RGB_ARB;
                    paramid_operando_rgb = Gl.GL_OPERAND1_RGB_ARB;
                    break;

                case 2:
                    paramid_source_rgb = Gl.GL_SOURCE2_RGB_ARB;
                    paramid_operando_rgb = Gl.GL_OPERAND2_RGB_ARB;
                    break;
            }
            int paramid_source_alpha = 0;
            int paramid_operand_alpha = 0;
            switch (argnum)
            {
                case 0:
                    paramid_source_alpha = Gl.GL_SOURCE0_ALPHA_ARB;
                    paramid_operand_alpha = Gl.GL_OPERAND0_ALPHA_ARB;
                    break;

                case 1:
                    paramid_source_alpha = Gl.GL_SOURCE1_ALPHA_ARB;
                    paramid_operand_alpha = Gl.GL_OPERAND1_ALPHA_ARB;
                    break;

                case 2:
                    paramid_source_alpha = Gl.GL_SOURCE2_ALPHA_ARB;
                    paramid_operand_alpha = Gl.GL_OPERAND2_ALPHA_ARB;
                    break;
            }

            switch (RgbSource)
            {
                case Source.Fragment:
                    Gl.glTexEnvi( Gl.GL_TEXTURE_ENV, paramid_source_rgb, Gl.GL_PRIMARY_COLOR_ARB );
                    break;
                case Source.Previous:
                    Gl.glTexEnvi( Gl.GL_TEXTURE_ENV, paramid_source_rgb, Gl.GL_PREVIOUS_ARB );
                    break;
                case Source.Texture:
                    Gl.glTexEnvi( Gl.GL_TEXTURE_ENV, paramid_source_rgb, Gl.GL_TEXTURE );
                    break;
                case Source.Constant:
                    Gl.glTexEnvi( Gl.GL_TEXTURE_ENV, paramid_source_rgb, Gl.GL_CONSTANT_ARB );
                    break;
            }
            switch (AlphaSource)
            {
                case Source.Fragment:
                    Gl.glTexEnvi( Gl.GL_TEXTURE_ENV, paramid_source_alpha, Gl.GL_PRIMARY_COLOR_ARB );
                    break;
                case Source.Previous:
                    Gl.glTexEnvi( Gl.GL_TEXTURE_ENV, paramid_source_alpha, Gl.GL_PREVIOUS_ARB );
                    break;
                case Source.Texture:
                    Gl.glTexEnvi( Gl.GL_TEXTURE_ENV, paramid_source_alpha, Gl.GL_TEXTURE );
                    break;
                case Source.Constant:
                    Gl.glTexEnvi( Gl.GL_TEXTURE_ENV, paramid_source_alpha, Gl.GL_CONSTANT_ARB );
                    break;
            }

            switch (RgbOperand)
            {
                case Operand.Rgb:
                    Gl.glTexEnvi( Gl.GL_TEXTURE_ENV, paramid_operando_rgb, Gl.GL_SRC_COLOR );
                    break;
                case Operand.OneMinusRgb:
                    Gl.glTexEnvi( Gl.GL_TEXTURE_ENV, paramid_operando_rgb, Gl.GL_ONE_MINUS_SRC_COLOR );
                    break;
                case Operand.Alpha:
                    Gl.glTexEnvi( Gl.GL_TEXTURE_ENV, paramid_operando_rgb, Gl.GL_SRC_ALPHA );
                    break;
                case Operand.OneMinusAlpha:
                    Gl.glTexEnvi( Gl.GL_TEXTURE_ENV, paramid_operando_rgb, Gl.GL_ONE_MINUS_SRC_ALPHA );
                    break;
            }
            switch (AlphaOperand)
            {
                case Operand.Alpha:
                    Gl.glTexEnvi( Gl.GL_TEXTURE_ENV, paramid_operand_alpha, Gl.GL_SRC_ALPHA );
                    break;
                case Operand.OneMinusAlpha:
                    Gl.glTexEnvi( Gl.GL_TEXTURE_ENV, paramid_operand_alpha, Gl.GL_ONE_MINUS_SRC_ALPHA );
                    break;
            }
        }
    }

    // TestHarness is for testing only
    // to activate this, uncomment the line TextHarness Gl = new TestHarness(); in each class to be tested
    class TestHarness
    {
        public int GL_SOURCE0_RGB_ARB = 0;
        public int GL_SOURCE1_RGB_ARB = 1;
        public int GL_SOURCE2_RGB_ARB = 2;
        public int GL_OPERAND0_RGB_ARB = 3;
        public int GL_OPERAND1_RGB_ARB = 4;
        public int GL_OPERAND2_RGB_ARB = 5;
        public int GL_TEXTURE_ENV = 6;
        public int GL_PRIMARY_COLOR_ARB = 7;
        public int GL_PREVIOUS_ARB = 8;
        public int GL_TEXTURE = 9;
        public int GL_CONSTANT_ARB = 10;
        public int GL_SRC_COLOR = 11;
        public int GL_ONE_MINUS_SRC_COLOR = 12;
        public int GL_SRC_ALPHA = 13;
        public int GL_ONE_MINUS_SRC_ALPHA = 14;
        public int GL_SOURCE0_ALPHA_ARB = 15;
        public int GL_SOURCE1_ALPHA_ARB = 16;
        public int GL_SOURCE2_ALPHA_ARB = 17;
        public int GL_OPERAND0_ALPHA_ARB = 18;
        public int GL_OPERAND1_ALPHA_ARB = 19;
        public int GL_OPERAND2_ALPHA_ARB = 20;
        public int GL_TEXTURE_ENV_MODE = 21;
        public int GL_COMBINE_ARB = 22;
        public int GL_ADD = 23;
        public int GL_INTERPOLATE = 24;
        public int GL_REPLACE = 25;
        public int GL_COMBINE_RGB_ARB = 26;
        public int GL_COMBINE_ALPHA_ARB = 27;
        enum glcodes
        {
            GL_SOURCE0_RGB_ARB,
            GL_SOURCE1_RGB_ARB,
            GL_SOURCE2_RGB_ARB,
            GL_OPERAND0_RGB_ARB,
            GL_OPERAND1_RGB_ARB,
            GL_OPERAND2_RGB_ARB,
            GL_TEXTURE_ENV,
            GL_PRIMARY_COLOR_ARB,
            GL_PREVIOUS_ARB,
            GL_TEXTURE,
            GL_CONSTANT_ARB,
            GL_SRC_COLOR,
            GL_ONE_MINUS_SRC_COLOR,
            GL_SRC_ALPHA,
            GL_ONE_MINUS_SRC_ALPHA,
            GL_SOURCE0_ALPHA_ARB,
            GL_SOURCE1_ALPHA_ARB,
            GL_SOURCE2_ALPHA_ARB,
            GL_OPERAND0_ALPHA_ARB,
            GL_OPERAND1_ALPHA_ARB,
            GL_OPERAND2_ALPHA_ARB,
            GL_TEXTURE_ENV_MODE,
            GL_COMBINE_ARB,
            GL_ADD,
            GL_INTERPOLATE,
            GL_REPLACE,
            GL_COMBINE_RGB_ARB,
            GL_COMBINE_ALPHA_ARB
        };
        public void glTexEnvi( int one, int two, int three )
        {
            LogFile.WriteLine( (glcodes)one + " " + (glcodes)two + " " + (glcodes)three );
        }
    }
}

