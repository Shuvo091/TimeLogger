using System;
using System.ComponentModel.DataAnnotations;

namespace TimeLogger.Models.ViewModels
{
    public class CreateEditTopicModel
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = null!;

        [Required]
        [Range(1, 100)]
        public int Percentage { get; set; }
    }
}