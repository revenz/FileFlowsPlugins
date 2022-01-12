
using MailKit.Net.Smtp;
using MimeKit;

namespace FileFlows.Communication
{
    public class SendEmail:Node
    {
        public override int Inputs => 1;
        public override int Outputs => 1;
        public override string Icon => "fas fa-envelope";

        public override FlowElementType Type => FlowElementType.Process;

        [StringArray(1)]
        public string[] Recipients { get; set; }

        [TextVariable(2)]
        public string Subject { get; set; }

        [TextArea(3)]
        public string Body { get; set; }

        public override int Execute(NodeParameters args)
        {
            var settings = args.GetPluginSettings<PluginSettings>();

            if (string.IsNullOrEmpty(settings?.SmtpServer))
            {
                args.Logger?.ELog("No SMTP Server configured, configure this under the 'Plugins > Email Nodes > Edit' page.");
                return -1;
            }

            args.Logger?.ILog($"Got SMTP Server: {settings.SmtpServer} [{settings.SmtpPort}]");

            string body = this.Body ?? string.Empty;
            string sender = settings.Sender ?? "fileflows@" + Environment.MachineName;
            string subject = args.ReplaceVariables(this.Subject ?? String.Empty)?.EmptyAsNull() ?? "Email from FileFlows"; ;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(sender, sender));
            foreach (var recipient in Recipients)
                message.To.Add(new MailboxAddress(recipient, recipient));
            message.Subject = subject;
            message.Body = new TextPart("plain")
            {
                Text = body
            };

            args.Logger?.ILog($"About to construct SmtpClient");
            using (var client = new SmtpClient())
            {
                args.Logger?.ILog($"Connecting to SMTP Server: {settings.SmtpServer}:{settings.SmtpPort}");
                client.Connect(settings.SmtpServer, settings.SmtpPort);

                if (string.IsNullOrEmpty(settings.SmtpUsername) == false)
                {
                    args.Logger?.ILog("Sending using credientials");
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Authenticate(settings.SmtpUsername, settings.SmtpPassword);
                }
                args.Logger?.ILog($"About to send message");
                client.Send(message);
                args.Logger?.ILog($"Message sent");
                client.Disconnect(true);
            }





            //MailMessage message = new MailMessage();
            //message.From = new MailAddress(sender);
            //foreach (var recipient in Recipients)
            //    message.To.Add(recipient);
            //message.Subject = subject;
            //message.Body = args.ReplaceVariables(body);



            //SmtpClient smtp = new SmtpClient();
            //smtp.Port = settings.SmtpPort;
            //smtp.Host = settings.SmtpServer;
            //if (string.IsNullOrEmpty(settings.SmtpUsername) == false)
            //{
            //    args.Logger?.ILog("Sending using credientials");
            //    smtp.EnableSsl = true;
            //    smtp.UseDefaultCredentials = false;
            //    smtp.Credentials = new NetworkCredential(settings.SmtpUsername, settings.SmtpPassword);
            //    //smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            //}
            //smtp.Send(message);

            return 1;
        }
    }
}
