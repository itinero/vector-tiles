namespace Itinero.VectorTiles.Layers
{
    /// <summary>
    /// Represents a vertex.
    /// </summary>
    public class Vertex
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public uint Id { get; set; }
        
        /// <summary>
        /// Gets or sets the latitude.
        /// </summary>
        public double Latitude { get; set; }
        
        /// <summary>
        /// Gets or sets the longitude.
        /// </summary>
        public double Longitude { get; set; }
    }
}