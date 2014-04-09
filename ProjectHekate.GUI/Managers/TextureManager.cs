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

        public Texture LoadTexture(string key, string filename, bool smooth = true, bool repeated = false)
        {
            var tex = GetTexture(key);

            if (tex == null) {
                tex = new Texture(filename) {Smooth = smooth, Repeated = repeated};

                _textureDictionary.Add(key, tex);
            }

            return tex;
        }

        public Texture LoadSubTexture(string key, string filename, int x, int y, int width, int height, bool smooth = true, bool repeated = false)
        {
            var tex = GetTexture(key);

            if (tex == null)
            {
                tex = new Texture(filename, new IntRect(x, y, width, height)) { Smooth = smooth, Repeated = repeated };

                _textureDictionary.Add(key, tex);
            }

            return tex;
        }

        public Texture GetTexture(string key)
        {
            Texture tex;
            _textureDictionary.TryGetValue(key, out tex);

            return tex;
        }
    }
}
