using System;
using Microsoft.Practices.Prism.Events;

namespace Infrastructure
{
    public enum LockedState
    {
        Unlocked,
        Locked
    }

    public class LockedStateChangedEventArgs : EventArgs
    {
        public LockedState State { get; set; }
    }

    public class ShellLockedStateChangedEvent : CompositePresentationEvent<LockedStateChangedEventArgs>
    {
    }
}
