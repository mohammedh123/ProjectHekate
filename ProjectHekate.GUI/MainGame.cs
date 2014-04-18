using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ProjectHekate.Core;
using ProjectHekate.GUI.Interfaces;
using ProjectHekate.GUI.Managers;
using ProjectHekate.GUI.Screens;
using SFML.Graphics;
using SFML.Window;

namespace ProjectHekate.GUI
{
    class MainGame : IDisposable
    {
        public ITextureManager<Texture> TextureManager { get; private set; }

        public IScreenManager ScreenManager { get; private set; }

        public IMouseManager<Mouse.Button, Vector2i, Window> MouseManager { get; private set; }

        public IKeyStateManager<Keyboard.Key> KeyboardManager { get; private set; }

        public IInputManager<Mouse.Button, Vector2i, Window, Keyboard.Key> InputManager { get; private set; }

        public bool IsActive { get; private set; }

        public TimeSpan LastFrameTime { get; private set; }

        /// <summary>
        /// The last update call's execution time, in milliseconds.
        /// </summary>
        public double AverageUpdateTime { get; private set; }

        /// <summary>
        /// The last frame's render time, in milliseconds.
        /// </summary>
        public double LastRenderTime { get; private set; }

        /// <summary>
        /// The average render time for the last second's worth of frames, in milliseconds.
        /// </summary>
        public double AverageRenderTimeForLastSecond { get; private set; }
        private List<double> _lastRenderTimes = new List<double>();
        private double _averageRenderTimeTimer = 0;

        public RenderWindow Window { get; private set; }

        public void Run(int updatesPerSecond)
        {
            IsActive = true;
            var ctxSettings = new ContextSettings();
            ctxSettings.AntialiasingLevel = 8;

            Window = new RenderWindow(new VideoMode(1024,768), "ProjectHekate", Styles.Default, ctxSettings);
            Window.SetFramerateLimit(60);

            Window.GainedFocus += OnGainedFocus;
            Window.LostFocus += OnLostFocus;
            Window.Closed += OnClosed;
            Window.KeyPressed += OnKeyPressed;
            Window.KeyReleased += OnKeyReleased;
            Window.MouseButtonPressed += OnMouseButtonPressed;
            Window.MouseButtonReleased += OnMouseButtonReleased;
                
            Window.SetActive(IsActive);
            
            TextureManager = new TextureManager();

            MouseManager = new MouseManager();
            KeyboardManager = new KeyboardManager();
            InputManager = new InputManager(MouseManager, KeyboardManager, Window);

            ScreenManager = new ScreenManager(InputManager, this);
            ScreenManager.Initialize();
            // add screens
            ScreenManager.AddScreen(new TestScreen());
            ScreenManager.LoadContent();

            const double currentFrameTimeFactor = 0.80; // weighted average of frame time, with this percent being the current frame time
            const double previousFrameTimeFactor = 1.0 - currentFrameTimeFactor;
            Stopwatch clock = new Stopwatch(), updateClock = new Stopwatch();
            clock.Start();
            // run the program as long as the window is open
            while (Window.IsOpen()) {
                LastFrameTime = clock.Elapsed;
                clock.Restart();

                Window.DispatchEvents();

                updateClock.Restart();
                Update();
                AverageUpdateTime = previousFrameTimeFactor*AverageUpdateTime + currentFrameTimeFactor*updateClock.Elapsed.TotalMilliseconds;

                RenderFrame(Window);
            }
        }
        
        #region Event Handlers (simple) 

        private void OnGainedFocus(object sender, EventArgs eventArgs)
        {
            IsActive = true;
        }

        private void OnLostFocus(object sender, EventArgs eventArgs)
        {
            IsActive = false;
        }
        
        private void OnClosed(object sender, EventArgs e)
        {
            Window.Close();
        }

        private void OnKeyPressed(object sender, KeyEventArgs e)
        {
            KeyboardManager.UpdateKey(e.Code, true);
        }
        private void OnKeyReleased(object sender, KeyEventArgs e)
        {
            KeyboardManager.UpdateKey(e.Code, false);
        }

        private void OnMouseButtonPressed(object sender, MouseButtonEventArgs e)
        {
            MouseManager.UpdateKey(e.Button, true);
        }
        private void OnMouseButtonReleased(object sender, MouseButtonEventArgs e)
        {
            MouseManager.UpdateKey(e.Button, false);
        }

        #endregion

        private void Update()
        {
            InputManager.Update();

            ScreenManager.Update(LastFrameTime);
            _averageRenderTimeTimer += LastFrameTime.TotalSeconds;

            InputManager.PostUpdate();
        }

        private void RenderFrame(RenderWindow window)
        {
            var renderClock = new Stopwatch();
            renderClock.Start();
            ScreenManager.Draw(LastFrameTime);
            LastRenderTime = renderClock.Elapsed.TotalMilliseconds;
            renderClock.Reset();

            UpdateAverageFrameTime();

            window.Display();
        }

        private void UpdateAverageFrameTime()
        {
            _lastRenderTimes.Add(LastRenderTime);

            if (_averageRenderTimeTimer >= 1.0f) {
                AverageRenderTimeForLastSecond = _lastRenderTimes.Average();
                _lastRenderTimes.Clear();

                _averageRenderTimeTimer = 0.0f;
            }
        }

        public void Dispose()
        {
        }
    }
}
