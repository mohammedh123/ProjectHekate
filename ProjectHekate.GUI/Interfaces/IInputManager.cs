namespace ProjectHekate.GUI.Interfaces
{
    /// <summary>
    /// An interface for a class that handles all forms of input (mouse, keyboard).
    /// </summary>
    interface IInputManager<in TMouseKeyType, TPositionType, TWindowType, in TKeyboardKeyType>
    {
        IMouseManager<TMouseKeyType, TPositionType, TWindowType> Mouse { get; }

        IKeyStateManager<TKeyboardKeyType> Keyboard { get; }

        /// <summary>
        /// Tells all input managers to update.
        /// </summary>
        void Update();

        /// <summary>
        /// Performs any post-update work that might need to be done.
        /// </summary>
        void PostUpdate();
    }
}
