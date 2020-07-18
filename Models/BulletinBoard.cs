using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeshBackend.Models
{
    public class BulletinBoard
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [MaxLength(100)]
        public string description { get; set; }
        
        public int ProjectId { get; set; }
        
        [ForeignKey("ProjectId")]
        public Project Project { get; set; }
        
    }
}