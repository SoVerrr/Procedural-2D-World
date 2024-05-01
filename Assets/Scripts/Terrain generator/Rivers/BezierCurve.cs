using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Procedural
{
    public static class BezierCurve
    {
        public static Vector3 GetPointOnBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) //Function returns a point on a quadratic bezier curve
        {
            //Optimized calculations of the bezier curve point, credit: https://denisrizov.com/2016/06/02/bezier-curves-unity-package-included/
            float u = 1f - t;
            float t2 = t * t;
            float u2 = u * u;
            float u3 = u2 * u;
            float t3 = t2 * t;

            Vector3 result = u3 * p0 + (3f * u2 * t) * p1 + (3f * u * t2) * p2 + t3 * p3;
            return result;
        }

    }
}
