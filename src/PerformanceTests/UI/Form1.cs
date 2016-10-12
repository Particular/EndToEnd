using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace UI
{
    using System.Diagnostics;
    using System.Net;
    using Tests.Permutations;
    using Tests.Tools;
    using Variables;

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var permutation = new Permutation
            {
                AuditMode = (Audit)Enum.Parse(typeof(Audit), gpAudit.Controls.OfType<RadioButton>().First(x => x.Checked).Text),
                TransactionMode = TransactionMode.Receive,
                Persister = (Persistence)Enum.Parse(typeof(Persistence), gpPersistence.Controls.OfType<RadioButton>().First(x => x.Checked).Text),
                Transport = (Transport)Enum.Parse(typeof(Transport), gpTransports.Controls.OfType<RadioButton>().First(x => x.Checked).Text),
                ConcurrencyLevel = (ConcurrencyLevel)Enum.Parse(typeof(ConcurrencyLevel), gpConcurrency.Controls.OfType<RadioButton>().First(x => x.Checked).Text),
                Category = "Performance",
                OutboxMode = (Outbox)Enum.Parse(typeof(Outbox), gpOutbox.Controls.OfType<RadioButton>().First(x => x.Checked).Text),
                Fixture = "",
                Code = "",
                Tests = new[]
                {
                    gpTest.Controls.OfType<RadioButton>().First(x => x.Checked).Text
                },
                Description = "",
                Exe = "",
                GarbageCollector = (GarbageCollector)Enum.Parse(typeof(GarbageCollector), gpGC.Controls.OfType<RadioButton>().First(x => x.Checked).Text),
                MessageSize = MessageSize.Tiny,
                Platform = (Platform)Enum.Parse(typeof(Platform), gpPlatform.Controls.OfType<RadioButton>().First(x => x.Checked).Text),
                Serializer = (Serialization)Enum.Parse(typeof(Serialization), gpSerializer.Controls.OfType<RadioButton>().First(x => x.Checked).Text),
                Version = (NServiceBusVersion)Enum.Parse(typeof(NServiceBusVersion), gpVersion.Controls.OfType<RadioButton>().First(x => x.Checked).Text),
            };

            var sessionId = Dns.GetHostName() + "/" + DateTime.UtcNow.Ticks;
            var env = new TestEnvironment(sessionId);

            var descriptor = env.CreateTestEnvironments(permutation);


            Launch(descriptor, sessionId, TimeSpan.Parse(txtRun.Text), TimeSpan.Parse(txtWarmup.Text));
        }

        void Launch(TestDescriptor descriptor, string sessionId, TimeSpan run, TimeSpan warmup)
        {
            var permutation = descriptor.Permutation;

            var sessionIdArgument = $" --sessionId={sessionId}";

            var exe = new FileInfo(permutation.Exe);

            var arguments = string.Join(" "
                , PermutationParser.ToArgs(permutation)
                , sessionIdArgument
                , "--run:" + run, "--warmup:" + warmup
                );

            var pi = new ProcessStartInfo(exe.FullName, arguments)
            {
                UseShellExecute = false,
                WorkingDirectory = exe.DirectoryName,
            };

            using (var p = Process.Start(pi))
            {
                //if (!p.WaitForExit((int)MaxDuration.TotalMilliseconds))
                //{
                //    p.Kill();
                //    Assert.Fail($"Killed process because execution took more then {MaxDuration}.");
                //}
                //if (p.ExitCode == (int)ReturnCodes.NotSupported)
                //{
                //    Assert.Inconclusive("Not supported");
                //}
                //Assert.AreEqual((int)ReturnCodes.OK, p.ExitCode, "Execution failed.");
            }
        }
    }
}
