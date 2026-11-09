namespace Core.Configuration
{
    public static class DocumentEmailContent
    {
        public const string FromEmail = "team@delgendercommunications.co.za";
        public const string FromName = "Delgender Communications";
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
