using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeshBackend.Models
{
    public class Assign
    {
        public int TaskId { get; set; }

        public string Title { get; set; }
        
        public Task Task { get; set; }
        
        public int UserId { get; set; }
        
        public User User { get; set; }

    }
}