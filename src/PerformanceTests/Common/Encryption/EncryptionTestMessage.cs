namespace Common.Encryption
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using NServiceBus;
    using Common.Messages;

    public class EncryptionTestMessage : MessageBase
    {
        public WireEncryptedString Secret { get; set; }
        public ClassForNesting CreditCard { get; set; }
        public WireEncryptedString SecretThatIsNull { get; set; }
        public DateTime DateTime { get; set; }
        public List<ClassForNesting> ListOfCreditCards { get; set; }
        public ArrayList ListOfSecrets { get; set; }
        public byte[] LargeByteArray { get; set; }
    }
}