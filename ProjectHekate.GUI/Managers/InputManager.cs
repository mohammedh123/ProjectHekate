using ProjectHekate.GUI.Interfaces;
using SFML.Window;

namespace ProjectHekate.GUI.Managers
{
    /// <summary>
    /// An implementation of the InputManager specifically for SFML.NET.
    /// </summary>
    class InputManager : IInputManager<Mouse.Button, Vector2i, Window, Keyboard.Key>
    {
        public IMouseManager<Mouse.Button, Vector2i, Window> Mouse { get; private set; }
        public IKeyStateManager<Keyboard.Key> Keyboard { get; private set; }

        private readonly Window _window;

        public InputManager(IMouseManager<Mouse.Button, Vector2i, Window> mouse, IKeyStateManager<Keyboard.Key> keyboard, Window window)
        {
            Mouse = mouse;
            Keyboard = keyboard;

            _window = window;
        }

        public void Update()
        {
            Mouse.UpdateMousePosition(_window);
        }

        public void PostUpdate()
        {
            Mouse.PostUpdate();
            Keyboard.PostUpdate();
        }
    }
}
