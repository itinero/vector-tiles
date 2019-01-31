using System.Collections.Generic;
using Itinero.Attributes;
using Itinero.Data.Network.Edges;

namespace Itinero.VectorTiles.Layers
{
    /// <summary>
    /// Represents a layer of segments.
    /// </summary>
    public class SegmentLayer : Layer
    {
        /// <summary>
        /// Gets or sets the segments.
        /// </summary>
        public List<Segment> Segments { get; set; }

        /// <summary>
        /// Gets or sets the profiles.
        /// </summary>
        public AttributesIndex Profiles { get; set; }

        /// <summary>
        /// Gets or sets the meta index.
        /// </summary>
        public AttributesIndex Meta { get; set; }

        /// <summary>
        /// Gets or sets the edges meta data.
        /// </summary>
        public MetaCollectionDb EdgeMeta { get; set; }

        /// <summary>
        /// Gets the is empty flag.
        /// </summary>
        public override bool IsEmpty => this.Segments == null || this.Segments.Count == 0;
    }
}