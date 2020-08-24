using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeshBackend.Models
{
    public class Develop
    {
        public int UserId { get; set; }
        
        public User User { get; set; }
        
        public int ProjectId { get; set; }
        
        public Project Project { get; set; }
        
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedTime { get; set; }
   
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedTime { get; set; }

    }
}