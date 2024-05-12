using System.ComponentModel.DataAnnotations;
// using MailKit.Net.Smtp;
// using MimeKit;

namespace FileFlows.Communication;

/// <summary>
/// Sends an email
/// </summary>
public class SendEmail:Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 2;
    /// <inheritdoc />
    public override string Icon => "fas fa-envelope";

    /// <inheritdoc />
    public override string HelpUrl => "http://localhost:3000/docs/plugins/email";

    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Communication;
    /// <inheritdoc />
    public override bool FailureNode => true;

    /// <summary>
    /// Gets or sets the recipients
    /// </summary>
    [StringArray(1)] public string[]? Recipients { get; set; }

    /// <summary>
    /// Gets or sets the subject
    /// </summary>
    [TextVariable(2)] public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message
    /// </summary>
    [Required]
    [Template(3, nameof(BodyTemplates))]
    public string Body { get; set; } = string.Empty;

    private static List<ListOption>? _BodyTemplates;
    /// <summary>
    /// Gets the body template options
    /// </summary>
    public static List<ListOption> BodyTemplates
    {
        get
        {
            if (_BodyTemplates == null)
            {
                _BodyTemplates = new List<ListOption>
                {
                    new () { Label = "Basic", Value = @"File: {{ file.Orig.FullName }}
Size: {{ file.Size }}" },
                    new () { Label = "File Size Changes", Value = @"
{{ difference = file.Size - file.Orig.Size }}
{{ percent = (difference / file.Orig.Size) * 100 | math.round 2 }}

Input File: {{ file.Orig.FullName }}
Output File: {{ file.FullName }}
Original Size: {{ file.Orig.Size | file_size }}
Final Size: {{ file.Size | file_size }}

{{- if difference > 0 }}
File grew in size: {{ difference | math.abs | file_size }}
{{ else }}
File shrunk in size by: {{ difference | file_size }} / {{ percent }}%
{{ end }}"}
                };
            }
            return _BodyTemplates;
        }
    }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        if (Recipients?.Any() != true)
        {
            args.FailureReason = "No recipients configured";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }
        
        string body = RenderBody(args);

        string subject = args.ReplaceVariables(this.Subject ?? String.Empty)?.EmptyAsNull() ??
                         "Email from FileFlows";

        if (args.SendEmail == null)
        {
            args.FailureReason = "SendEmail function is not set";
            args.Logger?.ELog(args.FailureReason);
            return -1;
        }
        var result = args.SendEmail(Recipients, subject, body);
        if (result.Failed(out string error))
        {
            args.Logger?.ELog("Failed to send email: " + error);
            return 2;
        }

        // SendMailKit(args, settings, sender, subject, body);

        //SendDotNet(args, settings, sender, subject, body);

        return 1;
    }

    /// <summary>
    /// Renders the body of the email
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the rendered body</returns>
    internal string RenderBody(NodeParameters args)
    {
        if (string.IsNullOrEmpty(this.Body))
            return string.Empty;

        return args.RenderTemplate!(this.Body);
    }

    // private void SendMailKit(NodeParameters args, PluginSettings settings, string sender, string subject, string body)
    // {
    //     var message = new MimeMessage();
    //     message.From.Add(new MailboxAddress(sender, sender));
    //     if (Recipients?.Any() != true)
    //     {
    //         args.Logger?.ELog("No recipients");
    //         return;
    //     }
    //     foreach (var recipient in Recipients)
    //         message.To.Add(new MailboxAddress(recipient, recipient));
    //     message.Subject = subject;
    //     message.Body = new TextPart("plain")
    //     {
    //         Text = body
    //     };
    //
    //     args.Logger?.ILog($"About to construct SmtpClient");
    //     using (var client = new SmtpClient())
    //     {
    //         args.Logger?.ILog($"Connecting to SMTP Server: {settings.SmtpServer}:{settings.SmtpPort}");
    //         client.Connect(settings.SmtpServer, settings.SmtpPort);
    //
    //         if (string.IsNullOrEmpty(settings.SmtpUsername) == false)
    //         {
    //             args.Logger?.ILog("Sending using credientials");
    //             client.AuthenticationMechanisms.Remove("XOAUTH2");
    //             client.Authenticate(settings.SmtpUsername, settings.SmtpPassword);
    //         }
    //         args.Logger?.ILog($"About to send message");
    //         client.Send(message);
    //         args.Logger?.ILog($"Message sent");
    //         client.Disconnect(true);
    //     }
    // }
}
