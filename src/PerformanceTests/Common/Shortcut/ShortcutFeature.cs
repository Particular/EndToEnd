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
#if Version5
        context.Pipeline.Register<ShortcutBehavior.Step>();
#endif
    }
}
