using System.Collections.Generic;
using Itinero.Attributes;

namespace Itinero.VectorTiles.Layers
{
    /// <summary>
    /// Represents configuration data for the vertex layer.
    /// </summary>
    public class VertexLayerConfig : LayerConfig
    {
        /// <summary>
        /// Gets or sets the function to get the attributes to include.
        ///
        /// If this returns null the vertex is not included.
        ///
        /// </summary>
        public System.Func<uint, int, IEnumerable<Attribute>> GetAttributesFunc { get; set; }

        /// <summary>
        /// Optional post processing step. Might turn a collection set into multiple collection set,
        /// in which case the vertex will be added multiple times to the vector tiles - once for each returned collection
        /// </summary>
        public System.Func<IEnumerable<Attribute>, IEnumerable<IEnumerable<Attribute>>> PostProcess { get; set; }

        /// <summary>
        /// Creates a new layer based on this configuration.
        /// </summary>
        /// <returns></returns>
        public VertexLayer NewLayer()
        {
            return new VertexLayer()
            {
                Name = this.Name,
                Vertices = new List<Vertex>(),
                Config = this
            };
        }
    }
}