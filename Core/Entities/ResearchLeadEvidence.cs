namespace Core.Entities
{
    public class ResearchLeadEvidence
    {
        public int Id { get; set; }

        public int ResearchLeadId { get; set; }
        public ResearchLead ResearchLead { get; set; } = null!;

        public string Finding { get; set; } = null!;
        public string? SourceUrl { get; set; }
    }
}
