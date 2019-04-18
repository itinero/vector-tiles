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
        /// If this returns null (or an empty collection) the edge is not to be included.
        ///
        /// </summary>
        public Func<uint, int, IEnumerable<Attribute>> GetAttributesFunc { get; set; }

        /// <summary>
        /// Optional post processing step. Might turn a collection set into multiple collection set,
        /// in which case the vertex will be added multiple times to the vector tiles - once for each returned collection
        /// </summary>
        public Func<IEnumerable<Attribute>, IEnumerable<IEnumerable<Attribute>>> PostProcess { get; set; }

        /// <summary>
        /// Creates a new layer based on this configuration.
        /// </summary>
        /// <returns></returns>
        public EdgeLayer NewLayer()
        {
            return new EdgeLayer()
            {
                Name = this.Name,
                Edges = new List<Edge>(),
                Config = this,
            };
        }
    }
}