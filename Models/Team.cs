using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeshBackend.Models
{
    public class Team
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        
        public int AdminId { get; set; }
        
        [ForeignKey("AdminId")]
        public User User { get; set; }
        
        public List<Cooperation>Cooperations { get; set; }
        public List<Project>Projects { get; set; }
    }
}