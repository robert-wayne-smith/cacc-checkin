using System;
using Microsoft.Practices.Prism.Events;

namespace Infrastructure
{
    public class StatusBarUpdateEventArgs : EventArgs
    {
        public string StatusMessage { get; set; }
    }

    public class ShellStatusBarUpdateEvent : CompositePresentationEvent<StatusBarUpdateEventArgs>
    {
    }
}
