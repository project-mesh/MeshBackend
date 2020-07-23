using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        
        [MaxLength(100)]
        public string Description { get; set; }
        
        [Required]
        public DateTime StartTime { get; set; }
        
        [Required]
        public DateTime EndTime { get; set; }
        
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedTime { get; set; }
        
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedTime { get; set; }

        [DefaultValue(false)]
        public bool Finished { get; set; }
        
        public int BoardId { get; set; }

        public int LeaderId { get; set; }
        
        
        [ForeignKey("BoardId")]
        public TaskBoard TaskBoard { get; set; }

        [ForeignKey("LeaderId")]
        public User User { get; set; }

        public List<TaskTag>TaskTags { get; set; }
        
        public List<Subtask>Subtasks { get; set; }
        
        public List<Assign>Assigns { get; set; }
    }
}