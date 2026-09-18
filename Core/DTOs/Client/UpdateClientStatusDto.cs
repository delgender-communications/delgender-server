using Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Client
{
    public class UpdateClientStatusDto
    {
        [Required]
        public ClientStatus Status { get; set; }
        public bool RequestFeedback { get; set; } = false;
    }
}
