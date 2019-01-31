using System;
using System.Collections.Generic;
using Itinero.Attributes;
using Itinero.LocalGeo;
using Attribute = Itinero.Attributes.Attribute;

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
        /// </summary>
        public Func<uint, IEnumerable<Attribute>> GetAttributesFunc { get; set; }

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