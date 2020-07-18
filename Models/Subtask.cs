using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeshBackend.Models
{
    public class Subtask
    {
        [Required]
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        public bool Finished { get; set; }
        
        public int TaskId { get; set; }
        
        public Task Task { get; set; }
        
    }
}