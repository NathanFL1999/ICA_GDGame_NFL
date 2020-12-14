using GDLibrary.Events;

namespace GDLibrary.Interfaces
{
    /// <summary>
    /// Parent interface for all event handlers attached to an Actor
    /// </summary>
    public interface IEventHandler
    {
        void HandleEvent(EventData eventData);
    }
}
