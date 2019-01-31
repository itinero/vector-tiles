using System.Collections.Generic;
using Itinero.Attributes;

namespace Itinero.VectorTiles.Layers
{
    public static class SegmentLayerExtensions
    {
        /// <summary>
        /// Gets all data from the edge meta collections for the given segment.
        /// </summary>
        /// <param name="segmentLayer">The segment layer.</param>
        /// <param name="segment">The segment.</param>
        /// <returns></returns>
        public static IEnumerable<Attribute> GetEdgeMetaFor(this SegmentLayer segmentLayer, Segment segment)
        {
            var edgeMeta = segmentLayer.EdgeMeta;
            if (edgeMeta == null) yield break;
            
            foreach (var metaName in edgeMeta.Names)
            {
                var edgeMetaCollection = edgeMeta.Get(metaName);
                var edgeMetaData = edgeMetaCollection.GetRaw(segment.EdgeId);
                var key = metaName;
                var value = string.Empty;
                if (edgeMetaData != null)
                {
                    value = edgeMetaData.ToInvariantString();
                }
                yield return new Attribute(key, value);
            }
        }
    }
}