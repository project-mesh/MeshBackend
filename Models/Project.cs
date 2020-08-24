using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeshBackend.Models
{
    public class Project
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        
        [DefaultValue(false)]
        public bool Publicity { get; set; }
        
        [MaxLength(2048)]
        public string Icon { get; set; }
        
        public int TeamId { get; set; }
        
        public int AdminId { get; set; }
        
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedTime { get; set; }
        
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedTime { get; set; }

        [ForeignKey("TeamId")]
        public Team Team;

        [ForeignKey("AdminId")]
        public User User;

        public ProjectMemoCollection ProjectMemoCollection { get; set; }
        
        public List<Develop>Develops { get; set; }

    }
}