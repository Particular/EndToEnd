using NServiceBus;
using NServiceBus.Features;

public class ShortcutFeature : Feature
{
    public ShortcutFeature()
    {
        EnableByDefault();
    }

    protected override void Setup(FeatureConfigurationContext context)
    {
        context.Container.ConfigureComponent<ShortcutBehavior>(DependencyLifecycle.SingleInstance);
        context.Pipeline.Register(nameof(ShortcutBehavior), typeof(ShortcutBehavior), "Shortcut processing to drain queue ASAP transport agnostic.");
    }
}
