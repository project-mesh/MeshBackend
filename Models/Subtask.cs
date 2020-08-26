using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeshBackend.Models
{
    public class Subtask
    {
        [Required]
        public string Title { get; set; }
        
        [MaxLength(100)]
        public string Description { get; set; }
        
        public bool Finished { get; set; }
        
        public int TaskId { get; set; }
        
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedTime { get; set; }
        
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedTime { get; set; }
        
        public Task Task { get; set; }
        
        public List<Assign>Assigns { get; set; }
    }
}