using LiveSplit.BelowZero;
using LiveSplit.Model;
using LiveSplit.UI.Components;
using System;
using System.Reflection;

namespace LiveSplit.BelowZero {
    public class Factory : IComponentFactory {
        public string ComponentName => "Subnautica: Below Zero Autosplitter";

        public string Description => "Autosplitter for Subnautica: Below Zero";

        public ComponentCategory Category => ComponentCategory.Control;

        public string UpdateName => ComponentName;

        public string XMLURL => UpdateURL + "Components/BelowZero.Updates.xml";

        public string UpdateURL => "https://raw.githubusercontent.com/Sprinter31/Subnautica_Autosplitter/LiveSplit.BelowZero-voxif/";

        public Version Version => ExAssembly.GetName().Version;

        public IComponent Create(LiveSplitState state) => new BelowZeroComponent(state);

        public static Assembly ExAssembly = Assembly.GetExecutingAssembly();
    }
}
