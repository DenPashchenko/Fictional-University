using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FictionalUniversityWebApp.Models
{
    [Table("GROUPS")]
    public partial class Group
    {
        public Group()
        {
            Students = new HashSet<Student>();
        }

        [Key]
        [Column("GROUP_ID")]
        public int GroupId { get; set; }

        [Column("COURSE_ID")]
        public int CourseId { get; set; }

        [Column("NAME")]
        [StringLength(10, MinimumLength = 3, ErrorMessage = "Name length can't be more then 10 and less then 3.")]
        [Required]
        [Display(Name = "Group name")]
        public string Name { get; set; } = null!;


        [ForeignKey("CourseId")]
        [InverseProperty("Groups")]
        public virtual Course? Course { get; set; } = null!;

        [InverseProperty("Group")]
        public virtual ICollection<Student> Students { get; set; }
    }
}
