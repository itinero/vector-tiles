using System.Collections.Generic;
using Itinero.Attributes;
using Itinero.Data.Network.Edges;

namespace Itinero.VectorTiles.Layers
{
    /// <summary>
    /// Represents a layer containing vertices.
    /// </summary>
    public class VertexLayer : Layer
    {
        /// <summary>
        /// Gets or sets the vertices.
        /// </summary>
        public List<Vertex> Vertices { get; set; }
        
        /// <summary>
        /// Gets or sets the config.
        /// </summary>
        public VertexLayerConfig Config { get; set; }

        /// <summary>
        /// Gets the is empty flag.
        /// </summary>
        public override bool IsEmpty => this.Vertices == null || this.Vertices.Count == 0;
    }
}