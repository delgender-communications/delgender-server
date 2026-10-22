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
        public const string FromEmail = "team@delgendercommunications.co.za";
        public const string FromName = "Delgender Communications";

        // Optional. Where client replies should land. Leave empty to reply to FromEmail.
        public const string ReplyTo = "delgendercommunications@gmail.com";

        public const string Subject = "Ideas to strengthen {{CompanyName}}'s brand";

        public const string Heading = "A proposal for {{CompanyName}}";

        public const string FooterNote =
            "You're receiving this because we prepared a proposal for your company. " +
            "If you'd rather not hear from us, just reply and let us know.";

        public const string Body = """
            Hi {{ContactName}},
            
            We are Delgender Communications, a strategic communications and brand consultancy. We spent some time looking at how {{CompanyName}} shows up online, across your website, social media and public presence, and put our findings into a short proposal. It's attached.
            
            Inside you'll find:
            - Where {{CompanyName}} stands today and what customers see
            - The key issues we'd fix first, in plain language
            - A simple 90-day roadmap and how we'd measure the results

            We think there's a real opportunity here and we'd love to walk you through it. Would you be open to a 20-minute call this week or next? Just reply with a time that suits you and we'll make it work.

            If now isn't the right time, no problem at all. The deck is yours to keep either way.

            Kind regards,
            Delgender Communications Team
            https://delgendercommunications.co.za

            Contact us:
            delgendercommunications@gmail.com
            060 619 6707
            """;
    }
}
