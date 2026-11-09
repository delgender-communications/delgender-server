namespace Core.Configuration
{
    public static class OutreachEmailContent
    {
        public const string FromEmail = "team@delgendercommunications.co.za";
        public const string FromName = "Delgender Communications";
        public const string ReplyTo = "delgendercommunications@gmail.com";
        public const string Subject = "A Proposal for {{CompanyName}}";
        public const string Heading = "Ideas to Enhance {{CompanyName}}";

        public const string FooterNote =
            "You're receiving this because we prepared a proposal for your company. " +
            "If you'd rather not hear from us, just reply and let us know.";

        public const string Body = """
            Hi {{ContactName}},
            
            We are Delgender Communications, a strategic communications and brand consultancy. We spent some time looking at how {{CompanyName}} shows up online, across your website, social media and public presence, and put our findings into a short proposal. It's attached.
            
            Inside you'll find:
            - Where {{CompanyName}} stands today and what customers see
            - The key issues we'd fix first, in plain language
            - A simple roadmap and how we'd measure the results

            We think there's a real opportunity here and we'd love to walk you through it. Would you be open to a few minutes' call any time soon? Just reply with a time that suits you and we'll make it work, or visit our website to make an appointment for the time that suits you at https://delgendercommunications.co.za/.

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
