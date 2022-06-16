using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Commun.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Password { get; set; }
        [NotMapped]
        public int Version { get; set; } = 0;
    }
}
