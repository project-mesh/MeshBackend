using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeshBackend.Models
{
    public class TaskBoard
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [MaxLength(100)]
        public string Description { get; set; }
        
        public int ProjectId { get; set; }
        
        [ForeignKey("ProjectId")]
        public Project Project { get; set; }

    }
}