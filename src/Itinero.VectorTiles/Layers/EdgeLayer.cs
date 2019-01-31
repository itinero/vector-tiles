using System.Collections.Generic;
using Itinero.Attributes;
using Itinero.Data.Network.Edges;

namespace Itinero.VectorTiles.Layers
{
    /// <summary>
    /// Represents a layer of segments.
    /// </summary>
    public class EdgeLayer : Layer
    {
        /// <summary>
        /// Gets or sets the segments.
        /// </summary>
        public List<Edge> Edges { get; set; }
        
        /// <summary>
        /// Gets or sets the config.
        /// </summary>
        public EdgeLayerConfig Config { get; set; }

        /// <summary>
        /// Gets the is empty flag.
        /// </summary>
        public override bool IsEmpty => this.Edges == null || this.Edges.Count == 0;
    }
}