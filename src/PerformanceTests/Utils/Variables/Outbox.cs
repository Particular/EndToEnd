using System.ComponentModel;

namespace Variables
{
    public enum Outbox
    {
        [Description("Outbox On")]
        On,
        [Description("Outbox Off")]
        Off
    }
}