namespace SolidStack.Coroutines
{
    /// <summary>
    /// Coroutine Service Runner - invokes Update and LateUpdate events,
    /// notifying Coroutine Service of the beginning and end of new frame.
    /// </summary>
    public interface ICoroutineServiceRunner
    {
    }

    public delegate void UpdateHandler();
}