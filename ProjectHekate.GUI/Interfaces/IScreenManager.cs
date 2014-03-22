using System;
using ProjectHekate.GUI.Screens;

namespace ProjectHekate.GUI.Interfaces
{
    interface IScreenManager
    {
        /// <summary>
        /// If true, the manager prints out a list of all the screens
        /// each time it is updated. This can be useful for making sure
        /// everything is being added and removed at the right times.
        /// </summary>
        bool TraceEnabled { get; set; }

        /// <summary>
        /// Initializes the screen manager component.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Load your graphics content.
        /// </summary>
        void LoadContent();

        /// <summary>
        /// Unload your graphics content.
        /// </summary>
        void UnloadContent();

        /// <summary>
        /// Allows each screen to run logic.
        /// </summary>
        void Update(TimeSpan gameTime);

        /// <summary>
        /// Tells each screen to draw itself.
        /// </summary>
        void Draw(TimeSpan gameTime);

        /// <summary>
        /// Adds a new screen to the screen manager.
        /// </summary>
        void AddScreen(GameScreen screen);

        /// <summary>
        /// Removes a screen from the screen manager. You should normally
        /// use GameScreen.ExitScreen instead of calling this directly, so
        /// the screen can gradually transition off rather than just being
        /// instantly removed.
        /// </summary>
        void RemoveScreen(GameScreen screen);

        /// <summary>
        /// Expose an array holding all the screens. We return a copy rather
        /// than the real master list, because screens should only ever be added
        /// or removed using the AddScreen and RemoveScreen methods.
        /// </summary>
        GameScreen[] GetScreens();

        /// <summary>
        /// Helper draws a translucent black fullscreen sprite, used for fading
        /// screens in and out, and for darkening the background behind popups.
        /// </summary>
        void FadeBackBufferToBlack(float alpha);
    }
}