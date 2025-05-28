using Microsoft.SemanticKernel;
using MultiAgentResearch.Common;
using System.ComponentModel;

namespace MultiAgentResearch.Plugins
{
    public class EmailPlugin
    {
        [KernelFunction]
        [Description("Sends an Email using provided details using email provided by StaffLookupPlugin plugin")]
        public string SendEmail(EmailModel email)
        {
            string msg = $"** ACTION TAKEN ** Email sent to {email.To} from {email.From} subject {email.Subject} body {email.Body}";

            // Simulate sending an email
            Console.WriteLine(msg);
            return msg;
        }
    }
}
