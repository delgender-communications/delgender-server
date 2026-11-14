namespace Core.Entities
{
    public class OutreachNoteEditor
    {
        public int Id { get; set; }

        public int OutreachCompanyId { get; set; }
        public OutreachCompany OutreachCompany { get; set; } = null!;

        public int StaffId { get; set; }
        public Staff Staff { get; set; } = null!;
        public int EditCount { get; set; }

        public DateTime FirstEditedAt { get; set; }
        public DateTime LastEditedAt { get; set; }
    }
}
