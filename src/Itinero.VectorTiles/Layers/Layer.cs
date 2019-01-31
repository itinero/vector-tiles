namespace Itinero.VectorTiles.Layers
{
    /// <summary>
    /// Abstract representation of a layer.
    /// </summary>
    public abstract class Layer
    {
        /// <summary>
        /// Gets or sets the name of the layer.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the is empty flag.
        /// </summary>
        public virtual bool IsEmpty { get; } = true;
    }
}