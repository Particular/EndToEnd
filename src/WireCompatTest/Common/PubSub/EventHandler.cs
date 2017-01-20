using NServiceBus;

public class EventHandler :
    IHandleMessages<Version_3_3.Messages.MyEvent>,
    IHandleMessages<Version_4_0.Messages.MyEvent>,
    IHandleMessages<Version_4_1.Messages.MyEvent>,
    IHandleMessages<Version_4_2.Messages.MyEvent>,
    IHandleMessages<Version_4_3.Messages.MyEvent>,
    IHandleMessages<Version_4_4.Messages.MyEvent>,
    IHandleMessages<Version_4_5.Messages.MyEvent>,
    IHandleMessages<Version_4_6.Messages.MyEvent>,
    IHandleMessages<Version_4_7.Messages.MyEvent>,
    IHandleMessages<Version_5_0.Messages.MyEvent>,
    IHandleMessages<Version_5_1.Messages.MyEvent>,
    IHandleMessages<Version_5_2.Messages.MyEvent>,
    IHandleMessages<Version_6_0.Messages.MyEvent>,
    IHandleMessages<Version_6_1.Messages.MyEvent>
{
    public void Handle(Version_3_3.Messages.MyEvent message)
    {
        PubSubVerifier.EventReceivedFrom.Add(message.Sender);
    }

    public void Handle(Version_4_0.Messages.MyEvent message)
    {
        PubSubVerifier.EventReceivedFrom.Add(message.Sender);
    }

    public void Handle(Version_4_1.Messages.MyEvent message)
    {
        PubSubVerifier.EventReceivedFrom.Add(message.Sender);
    }

    public void Handle(Version_4_2.Messages.MyEvent message)
    {
        PubSubVerifier.EventReceivedFrom.Add(message.Sender);
    }

    public void Handle(Version_4_3.Messages.MyEvent message)
    {
        PubSubVerifier.EventReceivedFrom.Add(message.Sender);
    }

    public void Handle(Version_4_4.Messages.MyEvent message)
    {
        PubSubVerifier.EventReceivedFrom.Add(message.Sender);
    }

    public void Handle(Version_4_5.Messages.MyEvent message)
    {
        PubSubVerifier.EventReceivedFrom.Add(message.Sender);
    }

    public void Handle(Version_4_6.Messages.MyEvent message)
    {
        PubSubVerifier.EventReceivedFrom.Add(message.Sender);
    }

    public void Handle(Version_4_7.Messages.MyEvent message)
    {
        PubSubVerifier.EventReceivedFrom.Add(message.Sender);
    }

    public void Handle(Version_5_0.Messages.MyEvent message)
    {
        PubSubVerifier.EventReceivedFrom.Add(message.Sender);
    }

    public void Handle(Version_5_1.Messages.MyEvent message)
    {
        PubSubVerifier.EventReceivedFrom.Add(message.Sender);
    }

    public void Handle(Version_5_2.Messages.MyEvent message)
    {
        PubSubVerifier.EventReceivedFrom.Add(message.Sender);
    }

    public void Handle(Version_6_0.Messages.MyEvent message)
    {
        PubSubVerifier.EventReceivedFrom.Add(message.Sender);
    }

    public void Handle(Version_6_1.Messages.MyEvent message)
    {
        PubSubVerifier.EventReceivedFrom.Add(message.Sender);
    }
}