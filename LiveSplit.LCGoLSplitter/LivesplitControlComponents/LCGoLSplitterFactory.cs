using LiveSplit.Model;
using LiveSplit.UI.Components;
using System;
using System.Reflection;

namespace LiveSplit.LCGoLSplitter.LiveSplitControlComponents
{
    class LCGoLSplitterFactory : IComponentFactory
    {
        public string ComponentName => "Lara Croft: Guardian of Light - AutoSplitter Injector";

        public string Description => "Game Time / Auto-splitting for Lara Croft and the Guardian of Light.";

        public ComponentCategory Category => ComponentCategory.Control;

        public string UpdateName => ComponentName;

        //TODO: Add an appropriate UpdateURL
        public string UpdateURL => "";

        public string XMLURL => UpdateURL + "";

        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public IComponent Create(LiveSplitState state)
        {
            return new LCGoLSplitterComponent(state);
        }
    }
}
