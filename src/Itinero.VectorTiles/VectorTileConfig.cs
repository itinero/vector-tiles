using System.Collections.Generic;
using Itinero.VectorTiles.Layers;

namespace Itinero.VectorTiles
{
    /// <summary>
    /// Represents configuration data for the layers generated from a router db.
    /// </summary>
    public class VectorTileConfig
    {
        /// <summary>
        /// Gets or sets the segment layer configurations.
        /// </summary>
        public SegmentLayerConfig SegmentLayerConfig { get; set; }
        
        /// <summary>
        /// Gets or sets the vertex layer configurations.
        /// </summary>
        public List<VertexLayerConfig> VertexLayerConfigs { get; set; }
    }
}