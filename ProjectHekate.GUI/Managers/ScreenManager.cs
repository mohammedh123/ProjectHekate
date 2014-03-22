using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ProjectHekate.GUI.Interfaces;
using ProjectHekate.GUI.Screens;
using SFML.Graphics;
using SFML.Window;

namespace ProjectHekate.GUI.Managers
{
    /// <summary>
    /// An implementation of IScreenManager specifically for use with SFML.NET.
    /// </summary>
    class ScreenManager : IScreenManager
    {
        #region Fields

        readonly List<GameScreen> _screens = new List<GameScreen>();
        readonly List<GameScreen> _screensToUpdate = new List<GameScreen>();

        private RectangleShape _fullScreenQuad;

        bool _isInitialized;
        bool _traceEnabled;

        private readonly IInputManager<Mouse.Button, Vector2i, Window, Keyboard.Key> _input;
        private readonly MainGame _game;

        #endregion

        #region Properties

        /// <summary>
        /// If true, the manager prints out a list of all the screens
        /// each time it is updated. This can be useful for making sure
        /// everything is being added and removed at the right times.
        /// </summary>
        public bool TraceEnabled
        {
            get { return _traceEnabled; }
            set { _traceEnabled = value; }
        }
        

        #endregion

        #region Initialization


        /// <summary>
        /// Constructs a new screen manager component.
        /// </summary>
        public ScreenManager(IInputManager<Mouse.Button, Vector2i, Window, Keyboard.Key> input, MainGame game)
        {
            _input = input;
            _game = game;

            Initialize();
        }


        /// <summary>
        /// Initializes the screen manager component.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) {
                return;
            }

            _isInitialized = true;

            _fullScreenQuad = new RectangleShape {Size = new Vector2f(10000, 10000)};
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        public void LoadContent()
        {
            // Tell each of the screens to load their content.
            foreach (var screen in _screens)
            {
                screen.LoadContent();
            }
        }

        /// <summary>
        /// Unload your graphics content.
        /// </summary>
        public void UnloadContent()
        {
            // Tell each of the screens to unload their content.
            foreach (var screen in _screens)
            {
                screen.UnloadContent();
            }
        }

        #endregion

        #region Update and Draw


        /// <summary>
        /// Allows each screen to run logic.
        /// </summary>
        public void Update(TimeSpan gameTime)
        {
            // Read the keyboard and gamepad.
            _input.Update();

            // Make a copy of the master screen list, to avoid confusion if
            // the process of updating one screen adds or removes others.
            _screensToUpdate.Clear();

            foreach (var screen in _screens)
                _screensToUpdate.Add(screen);

            var otherScreenHasFocus = !_game.IsActive;
            var coveredByOtherScreen = false;

            bool prevOtherScreenHasFocus = otherScreenHasFocus, prevCoveredByOtherScreen = coveredByOtherScreen;

            // Loop as long as there are screens waiting to be updated.
            while (_screensToUpdate.Count > 0)
            {
                // Pop the topmost screen off the waiting list.
                var screen = _screensToUpdate[_screensToUpdate.Count - 1];

                _screensToUpdate.RemoveAt(_screensToUpdate.Count - 1);

                prevOtherScreenHasFocus = otherScreenHasFocus; 
                prevCoveredByOtherScreen = coveredByOtherScreen;

                if (screen.ScreenState == ScreenState.TransitionOn ||
                    screen.ScreenState == ScreenState.Active)
                {
                    // If this is the first active screen we came across,
                    // give it a chance to handle input.
                    if (!otherScreenHasFocus)
                    {
                        screen.HandleInput(_input, gameTime);

                        otherScreenHasFocus = true;
                    }

                    // If this is an active non-popup, inform any subsequent
                    // screens that they are covered by it.
                    if (!screen.IsPopup)
                        coveredByOtherScreen = true;
                }

                // Update the screen.
                screen.Update(gameTime, prevOtherScreenHasFocus, prevCoveredByOtherScreen);
            }

            // Print debug trace?
            if (_traceEnabled)
                TraceScreens();
        }


        /// <summary>
        /// Prints a list of all the screens, for debugging.
        /// </summary>
        void TraceScreens()
        {
            Debug.WriteLine(string.Join(", ", _screens.Select(screen => screen.GetType().Name).ToArray()));
        }


        /// <summary>
        /// Tells each screen to draw itself.
        /// </summary>
        public void Draw(TimeSpan gameTime)
        {
            foreach (var screen in _screens)
            {
                if (screen.ScreenState == ScreenState.Hidden)
                    continue;

                screen.Draw(gameTime);
            }
        }


        #endregion

        #region Public Methods


        /// <summary>
        /// Adds a new screen to the screen manager.
        /// </summary>
        public void AddScreen(GameScreen screen)
        {
            screen.Game = _game;
            screen.ScreenManager = this;
            screen.IsExiting = false;

            // If we have a graphics device, tell the screen to load content.
            if (_isInitialized)
            {
                screen.LoadContent();
            }

            _screens.Add(screen);
        }


        /// <summary>
        /// Removes a screen from the screen manager. You should normally
        /// use GameScreen.ExitScreen instead of calling this directly, so
        /// the screen can gradually transition off rather than just being
        /// instantly removed.
        /// </summary>
        public void RemoveScreen(GameScreen screen)
        {
            // If we have a graphics device, tell the screen to unload content.
            if (_isInitialized)
            {
                screen.UnloadContent();
            }

            _screens.Remove(screen);
            _screensToUpdate.Remove(screen);
        }


        /// <summary>
        /// Expose an array holding all the screens. We return a copy rather
        /// than the real master list, because screens should only ever be added
        /// or removed using the AddScreen and RemoveScreen methods.
        /// </summary>
        public GameScreen[] GetScreens()
        {
            return _screens.ToArray();
        }


        /// <summary>
        /// Helper draws a translucent black fullscreen sprite, used for fading
        /// screens in and out, and for darkening the background behind popups.
        /// </summary>
        public void FadeBackBufferToBlack(float alpha)
        {
            var size = _game.Window.Size;
            _fullScreenQuad.FillColor = new Color(0,0,0,(byte)(alpha*255));
            _fullScreenQuad.Position = new Vector2f(0,0);
            _fullScreenQuad.Size = new Vector2f(size.X, size.Y);

            _game.Window.Draw(_fullScreenQuad);
        }


        #endregion
    }
}
