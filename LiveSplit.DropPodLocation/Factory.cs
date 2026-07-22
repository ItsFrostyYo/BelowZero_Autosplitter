using LiveSplit.Model;
using LiveSplit.UI.Components;
using System;
using UpdateManager;

namespace LiveSplit.DropPodLocation
{
    internal sealed class Factory : IComponentFactory, IUpdateable
    {
        public string ComponentName => "Below Zero Drop Pod Location";
        public string Description => "Displays the selected Below Zero Drop Pod location.";
        public ComponentCategory Category => ComponentCategory.Information;
        public string UpdateName => ComponentName;
        public string XMLURL => string.Empty;
        public string UpdateURL => string.Empty;
        public Version Version => new Version(1, 0, 0, 0);

        public IComponent Create(LiveSplitState state) => new Component(state);
    }
}
