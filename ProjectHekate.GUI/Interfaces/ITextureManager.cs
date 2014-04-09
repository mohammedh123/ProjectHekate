using SFML;

namespace ProjectHekate.GUI.Interfaces
{
    /// <summary>
    /// An interface for a class that manages textures.
    /// </summary>
    /// <typeparam name="TTextureType"></typeparam>
    internal interface ITextureManager<out TTextureType>
    {
        /// <summary>
        /// Loads a texture given a filename, adds it to the internal collection of Textures and returns said texture if found.
        /// </summary>
        /// <param name="key">The key to associate the TTextureType with</param>
        /// <param name="filename">The name of the file that the texture will use.</param>
        /// <param name="smooth">Should the texture be smoothed</param>
        /// <param name="repeated">Should the texture be repeated (as opposed to stretched)</param>
        /// <returns>A texture if the file exists and is a texture, <b>null</b> otherwise.</returns>
        /// <exception cref="LoadingFailedException"></exception>
        TTextureType LoadTexture(string key, string filename, bool smooth = true, bool repeated = false);
        
        /// <summary>
        /// Loads a texture given a filename, adds it to the internal collection of Textures and returns said texture if found.
        /// </summary>
        /// <param name="key">The key to associate the TTextureType with</param>
        /// <param name="filename">The name of the file that the texture will use.</param>
        /// <param name="x">The top-left x-coordinate of the texture rect.</param>
        /// <param name="y">The top-left y-coordinate of the texture rect.</param>
        /// <param name="width">The width of the texture rect.</param>
        /// <param name="height">The height of the texture rect.</param>
        /// <param name="smooth">Should the texture be smoothed</param>
        /// <param name="repeated">Should the texture be repeated (as opposed to stretched)</param>
        /// <returns>A texture if the file exists and is a texture, <b>null</b> otherwise.</returns>
        /// <exception cref="LoadingFailedException"></exception>
        TTextureType LoadSubTexture(string key, string filename, int x, int y, int width, int height, bool smooth = true, bool repeated = false);

        /// <summary>
        /// Returns a texture associated with a certain key.
        /// </summary>
        /// <param name="key">A key</param>
        /// <returns>A texture if an associated texture exists, <b>null</b> otherwise.</returns>
        TTextureType GetTexture(string key);
    }
}