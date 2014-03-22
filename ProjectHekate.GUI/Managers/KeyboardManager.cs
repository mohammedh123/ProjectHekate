using System;
using ProjectHekate.GUI.Interfaces;
using SFML.Window;

namespace ProjectHekate.GUI.Managers
{
    /// <summary>
    /// An implementation of the IKeyStateManager interface, specifically for use with SFML.NET.
    /// </summary>
    class KeyboardManager : IKeyStateManager<Keyboard.Key>
    {
        // true = key is down, false otherwise
        private readonly bool[] _currentKeyState = new bool[(int)Keyboard.Key.KeyCount];
        private readonly bool[] _previousKeyState = new bool[(int)Keyboard.Key.KeyCount];
        
        public bool IsKeyDown(Keyboard.Key key)
        {
            return key >= 0 && _currentKeyState[(int) key];
        }

        public bool IsKeyUp(Keyboard.Key key)
        {
            return key >= 0 && !_currentKeyState[(int)key];
        }

        public bool IsKeyPressed(Keyboard.Key key)
        {
            return key >= 0 && !_previousKeyState[(int) key] && _currentKeyState[(int) key];
        }

        public void UpdateKey(Keyboard.Key key, bool pressed)
        {
            if (key >= 0) {
                _currentKeyState[(int) key] = pressed;
            }
        }

        public void PostUpdate()
        {
            Array.Copy(_currentKeyState, _previousKeyState, _previousKeyState.Length);
        }
    }
}
