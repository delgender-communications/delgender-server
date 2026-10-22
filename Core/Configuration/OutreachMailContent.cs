namespace Core.Configuration
{
    /// <summary>
    /// Everything about the outreach email that a human writes lives in this file.
    /// Edit the text below; no other file needs to change.
    ///
    /// Placeholders you can use in <see cref="Heading"/>, <see cref="Subject"/> and <see cref="Body"/>:
    ///   {{CompanyName}}  the company's name
    ///   {{ContactName}}  the contact's name, or "there" when we don't have one
    ///   {{SenderName}}   the staff member who clicked Email in the portal
    ///
    /// Body is plain text. A blank line starts a new paragraph. The pitch deck PDF is
    /// attached automatically, and the email is wrapped in the same branded layout
    /// as the other emails the system sends.
    /// </summary>
    public static class OutreachEmailContent
    {
        // Any address on the verified delgendercommunications.co.za domain works for
        // sending. Use a mailbox that actually exists, or set ReplyTo below, because
        // clients will reply to this email.
        public const string FromEmail = "outreach@delgendercommunications.co.za";
        public const string FromName = "Delgender Communications";

        // Optional. Where client replies should land. Leave empty to reply to FromEmail.
        public const string ReplyTo = "";

        // TODO: write your subject line.
        public const string Subject = "";

        // TODO: the big title at the top of the email.
        public const string Heading = "";

        // Small grey text at the bottom. Leave empty for none.
        public const string FooterNote = "";

        // TODO: write your message between the triple quotes. For example:
        //
        //     Hi {{ContactName}},
        //
        //     We put together a short pitch for {{CompanyName}}. It's attached.
        //
        //     Kind regards,
        //     {{SenderName}}
        public const string Body = "";
    }
}
