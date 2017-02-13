using Itinero.LocalGeo;

namespace Itinero.VectorTiles
{
    /// <summary>
    /// Represents a segment, a part of an edge.
    /// </summary>
    public class Segment
    {
        /// <summary>
        /// The shape of the segment.
        /// </summary>
        public Coordinate[] Shape { get; set; }

        /// <summary>
        /// Gets or sets the profile id.
        /// </summary>
        public ushort Profile { get; set; }

        /// <summary>
        /// Gets or sets the meta id.
        /// </summary>
        public uint Meta { get; set; }
    }
}