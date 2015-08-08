using System;
using System.Collections.Generic;
using System.Text;

namespace OSMP
{
    /*
    public enum BrushType
    {
        RaiseLower,
        Flatten,
        EditTexture,
        AddFeature
    };
     */

    // can add change events to this if we want
    public class CurrentEditBrush
    {
        static CurrentEditBrush instance = new CurrentEditBrush();
        public static CurrentEditBrush GetInstance() { return instance; }

        //public BrushType BrushType = BrushType.RaiseLower;
        public IBrushEffect BrushEffect = null;
        public IBrushShape BrushShape = null;
        public int BrushSize = 20;
    }
}
