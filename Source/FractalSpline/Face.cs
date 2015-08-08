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

namespace FractalSpline
{
    public class Face
    {
        public int FaceNum;
        public Face( int FaceNum )
        {
            this.FaceNum = FaceNum;
        }
    }
    
    class OuterFace : Face
    {
        public int OuterFaceIndex;
        public OuterFace( int FaceNum, int OuterFaceIndex ) : base( FaceNum )
        {
            this.OuterFaceIndex = OuterFaceIndex;
        }
    }
    
    class InnerFace : Face
    {
        public int InnerFaceIndex;
        public InnerFace( int FaceNum, int InnerFaceIndex ) : base( FaceNum )
        {
            this.InnerFaceIndex = InnerFaceIndex;
        }
    }
    
    class CutFace : Face
    {
        public int CutFaceIndex;
        public CutFace( int FaceNum, int CutFaceIndex ) : base( FaceNum )
        {
            this.CutFaceIndex = CutFaceIndex;
        }
    }
    
    class EndCap : Face
    {
        public bool IsTop;
        public EndCap( int FaceNum, bool bTop ) : base( FaceNum )
        {
            this.IsTop = bTop;
        }
    }
    
    class EndCapNoCutNoHollow : EndCap
    {
        public EndCapNoCutNoHollow( int FaceNum, bool bTop ) : base( FaceNum, bTop ){}
    }

    class EndCapCutNoHollow : EndCap
    {
        public EndCapCutNoHollow( int FaceNum, bool bTop ) : base( FaceNum, bTop ){}
    }

    class EndCapHollow : EndCap
    {
        public EndCapHollow( int FaceNum, bool bTop ) : base( FaceNum, bTop ){}
    }
    
    class EndCapNoHollow : EndCap
    {
        public EndCapNoHollow( int FaceNum, bool bTop ) : base( FaceNum, bTop ){}
    }
}
