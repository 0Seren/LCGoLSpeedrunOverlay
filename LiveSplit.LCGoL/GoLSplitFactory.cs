using System;
using System.Reflection;
using LiveSplit.Model;
using LiveSplit.UI.Components;

namespace LiveSplit.LCGoL
{
	public class GoLSplitFactory : IComponentFactory
    {
        public string ComponentName => "Lara Croft: GoL";

        public string Description => "Game Time / Auto-splitting for Lara Croft and the Guardian of Light.";

        public ComponentCategory Category => ComponentCategory.Control;

        public string UpdateName => ComponentName;

        //TODO: Add an appropriate UpdateURL
        public string UpdateURL => "";

        public string XMLURL => UpdateURL + "Components/update.LiveSplit.GoLSplit.xml";

        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public IComponent Create(LiveSplitState state)
        {
            return new GoLSplitComponent(state);
        }
    }
}
