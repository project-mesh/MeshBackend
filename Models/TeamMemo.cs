using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeshBackend.Models
{
    public class TeamMemo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [MaxLength(50)]
        [Required]
        public string Title { get; set; }
        
        [MaxLength(100)]
        public string Text { get; set; }
        
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedTime { get; set; }

        public int UserId { get; set; }
        
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedTime { get; set; }
        
        public int CollectionId { get; set; }
        
        [ForeignKey("CollectionId")]
        public TeamMemoCollection TeamMemoCollection { get; set; }
        
        public User User { get; set; }

    }
}