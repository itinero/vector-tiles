using Itinero.LocalGeo;
using Xunit;

namespace Itinero.VectorTiles.Test
{
    public class LocalGeoExtensionsTests
    {
        /// <summary>
        /// Tests the intersections of the line.
        /// </summary>
        [Fact]
        public void LineSegmentIntersectionTests()
        {
            // double segments.
            var segment1 = new Line(new Coordinate(0, 0), new Coordinate(5, 0));
            var segment2 = new Line(new Coordinate(0, 0), new Coordinate(0, 5));
            var segment3 = new Line(new Coordinate(0, 3), new Coordinate(3, 0));
            var segment4 = new Line(new Coordinate(1, 1), new Coordinate(2, 2));
            var segment5 = new Line(new Coordinate(3, 3), new Coordinate(4, 4));
            var segment6 = new Line(new Coordinate(3, 1), new Coordinate(3, -1));

            var primitive = segment1.Intersection(segment2, true);
            Assert.NotNull(primitive);
            Assert.Equal(0, primitive.Value.Latitude);
            Assert.Equal(0, primitive.Value.Longitude);

            primitive = segment1.Intersection(segment3, true);
            Assert.NotNull(primitive);
            //Assert.AreEqual(new PointF2D(3, 0), primitive as PointF2D);

            //primitive = segment2.Intersection(segment3);
            //Assert.IsNotNull(primitive);
            //Assert.IsInstanceOf<PointF2D>(primitive);
            //Assert.AreEqual(new PointF2D(0, 3), primitive as PointF2D);

            //primitive = segment3.Intersection(segment4);
            //Assert.IsNotNull(primitive);
            //Assert.IsInstanceOf<PointF2D>(primitive);
            //Assert.AreEqual(new PointF2D(1.5, 1.5), primitive as PointF2D);

            //primitive = segment5.Intersection(segment1);
            //Assert.IsNull(primitive);
            //primitive = segment5.Intersection(segment2);
            //Assert.IsNull(primitive);
            //primitive = segment5.Intersection(segment3);
            //Assert.IsNull(primitive);
            //primitive = segment5.Intersection(segment4);
            //Assert.IsNull(primitive);
            //primitive = segment5.Intersection(segment6);
            //Assert.IsNull(primitive);

            //primitive = segment6.Intersection(segment3);
            //Assert.IsNotNull(primitive);
            //Assert.IsInstanceOf<PointF2D>(primitive);
            //Assert.AreEqual(new PointF2D(3, 0), primitive as PointF2D);

            //primitive = segment6.Intersection(segment1);
            //Assert.IsNotNull(primitive);
            //Assert.IsInstanceOf<PointF2D>(primitive);
            //Assert.AreEqual(new PointF2D(3, 0), primitive as PointF2D);

            //primitive = segment6.Intersection(segment3);
            //Assert.IsNotNull(primitive);
            //Assert.IsInstanceOf<PointF2D>(primitive);
            //Assert.AreEqual(new PointF2D(3, 0), primitive as PointF2D);

            //// half segments.
            //LineF2D segment7 = new LineF2D(1.5, 2, 1.5, 0, true, false); // only closed upwards.
            //LineF2D segment9 = new LineF2D(1.5, 2, 1.5, 4, true, false); // only closed downwards.

            //LineF2D segment8 = new LineF2D(1.5, 1, 1.5, 0, true, false); // only closed upwards.
            //LineF2D segment10 = new LineF2D(1.5, 1, 1.5, 4, true, false); // only closed downwards.

            //primitive = segment7.Intersection(segment3);
            //Assert.IsNotNull(primitive);
            //Assert.IsInstanceOf<PointF2D>(primitive);
            //Assert.AreEqual(new PointF2D(1.5, 1.5), primitive as PointF2D);
            //primitive = segment3.Intersection(segment7);
            //Assert.IsNotNull(primitive);
            //Assert.IsInstanceOf<PointF2D>(primitive);
            //Assert.AreEqual(new PointF2D(1.5, 1.5), primitive as PointF2D);
            //primitive = segment9.Intersection(segment3);
            //Assert.IsNull(primitive);
            //primitive = segment3.Intersection(segment9);
            //Assert.IsNull(primitive);

            //primitive = segment10.Intersection(segment3);
            //Assert.IsNotNull(primitive);
            //Assert.IsInstanceOf<PointF2D>(primitive);
            //Assert.AreEqual(new PointF2D(1.5, 1.5), primitive as PointF2D);
            //primitive = segment3.Intersection(segment10);
            //Assert.IsNotNull(primitive);
            //Assert.IsInstanceOf<PointF2D>(primitive);
            //Assert.AreEqual(new PointF2D(1.5, 1.5), primitive as PointF2D);
            //primitive = segment8.Intersection(segment3);
            //Assert.IsNull(primitive);
            //primitive = segment3.Intersection(segment8);
            //Assert.IsNull(primitive);

            //LineF2D segment11 = new LineF2D(-1, 1, 0, 1, true, false);
            //LineF2D segment12 = new LineF2D(0, 3, 3, 0, true, true);
            //primitive = segment11.Intersection(segment12);
            //Assert.IsNotNull(primitive);
            //Assert.IsInstanceOf<PointF2D>(primitive);
            //Assert.AreEqual(new PointF2D(2, 1), primitive as PointF2D);
            //primitive = segment12.Intersection(segment11);
            //Assert.IsNotNull(primitive);
            //Assert.IsInstanceOf<PointF2D>(primitive);
            //Assert.AreEqual(new PointF2D(2, 1), primitive as PointF2D);
        }
    }
}
