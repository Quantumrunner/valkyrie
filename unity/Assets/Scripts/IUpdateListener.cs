namespace Assets.Scripts
{
    public interface IUpdateListener
    {
        /// <summary>
        /// This method is called on click
        /// </summary>
        void Click();

        /// <summary>
        /// This method is called on Unity Update.  Must return false to allow garbage collection.
        /// </summary>
        /// <returns>True to keep this in the update list, false to remove.</returns>
        bool Update();
    }
}
