using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeshBackend.Models
{
    public class ProjectMemo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [MaxLength(50)]
        [Required]
        public string Title { get; set; }
        
        [MaxLength(100)]
        public string Text { get; set; }
        
        public int CollectionId { get; set; }
        
        public int UserId { get; set; }
        
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedTime { get; set; }
        
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedTime { get; set; }
        
        [ForeignKey("CollectionId")]
        public ProjectMemoCollection ProjectMemoCollection { get; set; }

        [ForeignKey("UserId")]
        public List<User> User { get; set; }
        
    }
}