using System;
using ProjectHekate.GUI.Interfaces;
using SFML.Window;

namespace ProjectHekate.GUI.Managers
{
    /// <summary>
    /// An implementation of the IMouseManager interface, specifically for use with SFML.NET.
    /// </summary>
    class MouseManager : IMouseManager<Mouse.Button, Vector2i, Window>
    {
        // true = key is down, false otherwise
        private readonly bool[] _currentKeyState = new bool[(int)Keyboard.Key.KeyCount];
        private readonly bool[] _previousKeyState = new bool[(int)Keyboard.Key.KeyCount];
        private Vector2i _currentMousePosition;
        private Window _window;

        public bool IsKeyDown(Mouse.Button key)
        {
            return key >= 0 && _currentKeyState[(int)key];
        }

        public bool IsKeyUp(Mouse.Button key)
        {
            return key >= 0 && !_currentKeyState[(int)key];
        }

        public bool IsKeyPressed(Mouse.Button key)
        {
            return key >= 0 && !_previousKeyState[(int)key] && _currentKeyState[(int)key];
        }

        public void UpdateKey(Mouse.Button key, bool pressed)
        {
            if (key >= 0)
            {
                _currentKeyState[(int)key] = pressed;
            }
        }

        public void PostUpdate()
        {
            Array.Copy(_currentKeyState, _previousKeyState, _previousKeyState.Length);

            PreviousMousePosition = CurrentMousePosition;
        }

        /// <summary>
        /// Gets/sets the current mouse position in local (window) coordinates.
        /// Note: UpdateMousePosition(window) must be called before attempting to set the mouse position using this property.
        /// </summary>
        public Vector2i CurrentMousePosition
        {
            get { return _currentMousePosition; }
            set
            {
                _currentMousePosition = value;

                if (_window != null) {
                    Mouse.SetPosition(_currentMousePosition, _window);
                }
            }
        }

        public Vector2i PreviousMousePosition { get; set; }
        public Vector2i MousePositionDelta => CurrentMousePosition - PreviousMousePosition;

        public bool MouseMoved => MousePositionDelta.X == 0 && MousePositionDelta.Y == 0;

        public void UpdateMousePosition(Window window)
        {
            _currentMousePosition = Mouse.GetPosition(window);

            _window = window;
        }
    }
}
