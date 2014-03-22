using System.Collections.Generic;
using ProjectHekate.GUI.Interfaces;
using SFML;
using SFML.Graphics;

namespace ProjectHekate.GUI.Managers
{
    class TextureManager : ITextureManager<Texture>
    {
        private readonly Dictionary<string, Texture> _textureDictionary;

        public TextureManager()
        {
            _textureDictionary = new Dictionary<string, Texture>();
        }

        /// <summary>
        /// Loads a texture given a filename, adds it to the internal collection of Textures and returns said texture if found.
        /// </summary>
        /// <param name="key">The key to associate the Texture with</param>
        /// <param name="filename">The name of the file that the texture will use.</param>
        /// <param name="smooth">Should the texture be smoothed</param>
        /// <param name="repeated">Should the texture be repeated (as opposed to stretched)</param>
        /// <returns>A texture if the file exists and is a texture, <b>null</b> otherwise.</returns>
        /// <exception cref="LoadingFailedException"></exception>
        public Texture LoadTexture(string key, string filename, bool smooth = true, bool repeated = false)
        {
            var tex = GetTexture(key);

            if (tex == null) {
                tex = new Texture(filename) {Smooth = smooth, Repeated = repeated};

                _textureDictionary.Add(key, tex);
            }

            return tex;
        }

        /// <summary>
        /// Returns a texture associated with a certain key.
        /// </summary>
        /// <param name="key">A key</param>
        /// <returns>A texture if an associated texture exists, <b>null</b> otherwise.</returns>
        public Texture GetTexture(string key)
        {
            Texture tex;
            _textureDictionary.TryGetValue(key, out tex);

            return tex;
        }
    }
}
