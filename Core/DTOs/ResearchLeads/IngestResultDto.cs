namespace Core.DTOs.ResearchLeads
{
    public class SkippedLeadDto
    {
        public string CompanyName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public class IngestResultDto
    {
        public int Added { get; set; }
        public List<SkippedLeadDto> Skipped { get; set; } = new();
    }
}
