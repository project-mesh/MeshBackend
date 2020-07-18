using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeshBackend.Models
{
    public class Cooperation
    {
        public int UserId { get; set; }
        public User User { get; set; }
        
        public int TeamId { get; set; }
        public Team Team { get; set; }
        
    }
}