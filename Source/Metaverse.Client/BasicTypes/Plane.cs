using System;
using System.Collections.Generic;
using System.Text;

namespace OSMP
{
    // represents a plane, defined by one normalized normal and one point
    public class Plane
    {
        [Replicate]
        public Vector3 normalizednormal;
        [Replicate]
        public Vector3 point;

        public Plane() { }
        public Plane(Vector3 normalizednormal, Vector3 point)
        {
            this.normalizednormal = normalizednormal;
            this.point = point;
        }
        public double GetDistance(Vector3 candidate)
        {
            return Vector3.DotProduct((candidate - point), normalizednormal);
        }
        public override string ToString()
        {
            return "Plane: point: " + point + " normal: " + normalizednormal;
        }
    }
}
