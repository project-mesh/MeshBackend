using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace MeshBackend.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Nickname { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Email { get; set; }
        
        [MaxLength(70)]
        public string PasswordDigest { get; set; }
        
        [MaxLength(70)]
        public string PasswordSalt { get; set; }
        
        [MaxLength(70)]
        public string RememberDigest { get; set; }
        
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedTime { get; set; }
        
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedTime { get; set; }
        
        public List<Cooperation>Cooperations { get; set; }
        
        public List<Task>Tasks { get; set; }
        public List<Assign>Assigns { get; set; }
        
        public TeamMemo TeamMemo { get; set; }
        
        public ProjectMemo ProjectMemo { get; set; }
    }
}