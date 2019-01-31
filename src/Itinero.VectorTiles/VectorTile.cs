﻿using Itinero.VectorTiles.Layers;
using System.Collections.Generic;
using System.Linq;

namespace Itinero.VectorTiles
{
    /// <summary>
    /// A vector tile.
    /// </summary>
    public class VectorTile
    {
        /// <summary>
        /// Gets or sets the tile id.
        /// </summary>
        public ulong TileId { get; set; }

        /// <summary>
        /// Gets or sets the layers.
        /// </summary>
        public List<Layer> Layers { get; set; }

        /// <summary>
        /// Gets the is empty flag.
        /// </summary>
        public bool IsEmpty => this.Layers == null || this.Layers.All(x => x.IsEmpty);
    }
}