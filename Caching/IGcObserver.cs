namespace JointCode.Common.Caching
{
    /// <summary>
    /// Interface that must be implemented by objects that want to
    /// register to the GcCollected notification.
    /// </summary>
    public interface IGcObserver
    {
        /// <summary>
        /// Method invoked when a collection occurs.
        /// </summary>
        void OnGarbageCollected();
    }
}