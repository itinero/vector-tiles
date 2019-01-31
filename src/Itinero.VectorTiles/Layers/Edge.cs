using Itinero.LocalGeo;

namespace Itinero.VectorTiles.Layers
{
    /// <summary>
    /// Represents a segment, a part of an edge.
    /// </summary>
    public class Edge
    {
        /// <summary>
        /// The shape of the segment.
        /// </summary>
        public Coordinate[] Shape { get; set; }

        /// <summary>
        /// Gets or sets the edge id.
        /// </summary>
        public uint EdgeId { get; set; }
    }
}