using Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Client
{
    internal class UpdateClientStatusDto
    {
        [Required]
        public ClientStatus Status { get; set; }
        public bool RequestFeedback { get; set; } = false;
    }
}
