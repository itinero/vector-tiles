﻿// The MIT License (MIT)

// Copyright (c) 2017 Ben Abelshausen

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using Itinero.LocalGeo;

namespace Itinero.VectorTiles
{
    /// <summary>
    /// Contains extension methods of the Itinero local geo objects.
    /// </summary>
    public static class LocalGeoExtensions
    {
        /// <summary>
        /// Calculates the intersection between the box and the given line.
        /// </summary>
        public static Coordinate? Intersection(this Box box, Line line)
        {
            var intersection = line.Intersection(new Line(new Coordinate(box.MaxLat, box.MaxLon),
                new Coordinate(box.MaxLat, box.MinLon)), true);
            if (intersection != null)
            {
                return intersection.Value;
            }
            intersection = line.Intersection(new Line(new Coordinate(box.MinLat, box.MaxLon),
                new Coordinate(box.MinLat, box.MinLon)), true);
            if (intersection != null)
            {
                return intersection.Value;
            }
            intersection = line.Intersection(new Line(new Coordinate(box.MinLat, box.MaxLon),
                new Coordinate(box.MaxLat, box.MaxLon)), true);
            if (intersection != null)
            {
                return intersection.Value;
            }
            intersection = line.Intersection(new Line(new Coordinate(box.MinLat, box.MinLon),
                new Coordinate(box.MaxLat, box.MinLon)), true);
            if (intersection != null)
            {
                return intersection.Value;
            }
            return null;
        }

        /// <summary>
        /// Calculates the intersection between the two lines.
        /// </summary>
        public static Coordinate? Intersection(this Line line1, Line line2, bool assumeSegments)
        {
            double det = (line2.A * line1.B - line1.A * line2.B);
            if (det == 0) // TODO: implement an accuracy threshold epsilon.
            { // lines are parallel; no intersections.
                return null;
            }

            var x = (float)((line1.B * line2.C - line2.B * line1.C) / det);
            var y = (float)((line2.A * line1.C - line1.A * line2.C) / det);

            if (assumeSegments)
            {
                // check line1.
                var length = UnSquaredDistance(line1.Coordinate1.Latitude, line1.Coordinate1.Longitude,
                    line1.Coordinate2.Latitude, line1.Coordinate2.Longitude);
                var dist = UnSquaredDistance(line1.Coordinate1.Latitude, line1.Coordinate1.Longitude, y, x);
                if (dist > length)
                {
                    return null;
                }
                dist = UnSquaredDistance(line1.Coordinate2.Latitude, line1.Coordinate2.Longitude, y, x);
                if (dist > length)
                {
                    return null;
                }

                // check line1.
                length = UnSquaredDistance(line2.Coordinate1.Latitude, line2.Coordinate1.Longitude,
                    line2.Coordinate2.Latitude, line2.Coordinate2.Longitude);
                dist = UnSquaredDistance(line2.Coordinate1.Latitude, line2.Coordinate1.Longitude, y, x);
                if (dist > length)
                {
                    return null;
                }
                dist = UnSquaredDistance(line2.Coordinate2.Latitude, line2.Coordinate2.Longitude, y, x);
                if (dist > length)
                {
                    return null;
                }
            }
            return new Coordinate(y, x);
        }

        private static float UnSquaredDistance(float x1, float y1, float x2, float y2)
        {
            var xDiff = x1 - x2;
            var yDiff = y1 - y2;
            return xDiff * xDiff + yDiff * yDiff;
        }
    }
}
