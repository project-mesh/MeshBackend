using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeshBackend.Models
{
    public class Assign
    {
        public int TaskId { get; set; }

        public string Title { get; set; }
        
        public Subtask Subtask { get; set; }
        
        public int UserId { get; set; }
        
        public User User { get; set; }
        
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedTime { get; set; }
        
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedTime { get; set; }
        
    }
}