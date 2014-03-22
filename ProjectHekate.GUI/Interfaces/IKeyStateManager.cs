namespace ProjectHekate.GUI.Interfaces
{
    /// <summary>
    /// An interface for a class that manages key state from some form of input.
    /// </summary>
    /// <typeparam name="TKeyType">The type to use to represent keys.</typeparam>
    interface IKeyStateManager<in TKeyType>
    {
        bool IsKeyDown(TKeyType key);
        bool IsKeyUp(TKeyType key);
        bool IsKeyPressed(TKeyType key);

        /// <summary>
        /// Called when the key is pressed/released, so that it can updated in the KeyStateManager.
        /// </summary>
        /// <param name="key">The key pressed/released.</param>
        /// <param name="pressed">If the key was pressed or not</param>
        void UpdateKey(TKeyType key, bool pressed);

        /// <summary>
        /// Called after all input is processed.
        /// </summary>
        void PostUpdate();
    }
}
