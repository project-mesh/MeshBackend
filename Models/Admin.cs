using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeshBackend.Models
{
    public class Admin
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
        
        [MaxLength(100)]
        public string Description { get; set; }
        
        [DefaultValue(0)]
        public int Status { get; set; }
        
        [MaxLength(100)]
        public string Address { get; set; }
        
        public DateTime Birthday { get; set; }

        [DefaultValue(0)]
        public int Gender { get; set; }
        
    }
}