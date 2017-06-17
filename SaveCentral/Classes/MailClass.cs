using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaveCentral.Classes
{
    public class MailClass
    {
        // Auto-Impl Properties for trivial get and set
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }

        public MailClass()
        {

        }
    }
    public class MailClassContainer
    {
        public MailClass Maildata;
        public string message { get; set; }
        public string status { get; set; }
    }
}
