﻿using System;
using Microsoft.Practices.Prism.Events;

namespace Infrastructure
{
    public class DepartmentChangedEvent : CompositePresentationEvent<string>
    {
    }
}
