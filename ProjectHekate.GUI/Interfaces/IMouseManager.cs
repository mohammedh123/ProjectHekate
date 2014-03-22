namespace ProjectHekate.GUI.Interfaces
{
    /// <summary>
    /// An interface for a class that manages mouse state.
    /// </summary>
    /// <typeparam name="TKeyType">The type to use to represent mouse keys.</typeparam>
    /// <typeparam name="TPositionType">The type to use for mouse position (should be some form of 2-dimensional vector using integer values).</typeparam>
    interface IMouseManager<in TKeyType, TPositionType, TWindowType> : IKeyStateManager<TKeyType>
    {
        TPositionType CurrentMousePosition { get; set; }
        TPositionType PreviousMousePosition { get; }
        TPositionType MousePositionDelta { get; }

        bool MouseMoved { get; }

        /// <summary>
        /// Called to update the position of the mouse relative to a window.
        /// </summary>
        /// <param name="window">The window of the application</param>
        void UpdateMousePosition(TWindowType window);
    }
}
