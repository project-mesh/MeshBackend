using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeshBackend.Models
{
    public class Feed
    {
        [Key]
        public int TaskId { get; set; }
        
        [ForeignKey("TaskId")]
        public Task Task { get; set; }
        
        public int UserId { get; set; }
        
        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}