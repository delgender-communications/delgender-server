namespace Core.Configuration
{
    /// <summary>
    /// The pre-written text staff start from when they email documents to a client.
    /// Staff can edit the subject and body in the portal before sending, so this
    /// is only the starting point. Edit the text below; no other file needs to change.
    ///
    /// Placeholders (filled in when the email window opens):
    ///   {{ContactName}}  the client's first name
    ///   {{CompanyName}}  the client's company
    ///   {{SenderName}}   the staff member who is sending
    ///
    /// Body is plain text. A blank line starts a new paragraph. The ticked documents
    /// are attached automatically, and the email is wrapped in the same branded
    /// layout as the other emails the system sends.
    /// </summary>
    public static class DocumentEmailContent
    {
        public const string FromEmail = "team@delgendercommunications.co.za";
        public const string FromName = "Delgender Communications";

        // Where client replies should land. Leave empty to reply to FromEmail.
        public const string ReplyTo = "delgendercommunications@gmail.com";

        public const string FooterNote =
            "You're receiving this because you're working with Delgender Communications. " +
            "If something doesn't look right, just reply and let us know.";

        public const string Subject = "Your documents from Delgender Communications";

        public const string Body = """
            Hi {{ContactName}},

            Please find the documents for {{CompanyName}} attached to this email. Have a look through them and let us know if anything needs changing.

            If you have any questions, just reply to this email and we'll be happy to help.

            Kind regards,
            {{SenderName}}
            Delgender Communications
            https://delgendercommunications.co.za

            Contact us:
            delgendercommunications@gmail.com
            060 619 6707
            """;
    }
}
