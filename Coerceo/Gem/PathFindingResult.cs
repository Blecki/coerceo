using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Gem;

namespace Gem
{
    public class PathFindingResult
    {
        public bool PathFound = false;
        public List<Vector3> PathPoints = new List<Vector3>();
        public List<Vector3> DebugTracePoints = new List<Vector3>();

		public float PathLength
		{
			get
			{
				var v = 0.0f;
				for (int i = 1; i < PathPoints.Count; ++i)
					v += (PathPoints[i] - PathPoints[i - 1]).Length();
				return v;
			}
		}

		public Vector3 PointAt(float D)
		{
			var p = 1;

			while (p < PathPoints.Count && D > (PathPoints[p] - PathPoints[p - 1]).Length())
			{
				D -= (PathPoints[p] - PathPoints[p - 1]).Length();
				++p;
			}

			var v = PathPoints[p] - PathPoints[p - 1];
			v.Normalize();
			return PathPoints[p - 1] + (v * D);
		}

        public List<Object> QuantizePath(double stepSize)
        {
            var r = new List<Object>();
            if (!PathFound) return r;
            if (PathPoints.Count == 0) return r;
            if (PathPoints.Count == 1) { r.Add(PathPoints[0]); return r; }

            r.Add(PathPoints[0]);
            var nextIndex = 1;
            var currentStep = PathPoints[0];

            while (nextIndex < PathPoints.Count)
            {
                var nextGoal = PathPoints[nextIndex];
                var delta = nextGoal - currentStep;
                if (delta.LengthSquared() <= (stepSize * stepSize))
                {
                    r.Add(nextGoal);
                    currentStep = nextGoal;
                    nextIndex += 1;
                }
                else
                {
                    delta.Normalize();
                    delta *= (float)stepSize;
                    currentStep += delta;
                    r.Add(currentStep);
                }
            }

            return r;
        }
    }
}
