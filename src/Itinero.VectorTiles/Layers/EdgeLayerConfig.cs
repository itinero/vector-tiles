using System;
using System.Collections.Generic;
using Attribute = Itinero.Attributes.Attribute;

namespace Itinero.VectorTiles.Layers
{
    /// <summary>
    /// Represents configuration data for the segment layer.
    /// </summary>
    public class EdgeLayerConfig : LayerConfig
    {
        /// <summary>
        /// Gets or sets the function to get the attributes to include.
        ///
        /// If this returns null the edge is not to be included.
        /// </summary>
        public Func<uint, int, IEnumerable<Attribute>> GetAttributesFunc { get; set; }

        /// <summary>
        /// Creates a new layer based on this configuration.
        /// </summary>
        /// <returns></returns>
        public EdgeLayer NewLayer(RouterDb routerDb)
        {
            return new EdgeLayer()
            {
                Name = this.Name,
                Edges = new List<Edge>(),
                Config = this
            };
        }
    }
}