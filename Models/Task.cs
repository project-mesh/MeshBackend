using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeshBackend.Models
{
    public class Task
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        
        [Required]
        public int Priority { get; set; }
        
        [Required]
        public DateTime StartTime { get; set; }
        
        public DateTime EndTime { get; set; }
        
        public int BoardId { get; set; }

        [ForeignKey("BoardId")]
        public TaskBoard TaskBoard { get; set; }
        
        public List<TaskTag>TaskTags { get; set; }
        
        public List<Subtask>Subtasks { get; set; }
        
        public List<Assign>Assigns { get; set; }
    }
}