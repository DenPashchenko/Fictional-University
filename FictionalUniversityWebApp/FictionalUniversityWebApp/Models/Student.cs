using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FictionalUniversityWebApp.Models
{
    [Table("STUDENTS")]
    public partial class Student
    {
        [Key]
        [Column("STUDENT_ID")]
        public int StudentId { get; set; }

        [Column("GROUP_ID")]
        public int GroupId { get; set; }

        [Column("FIRST_NAME")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name length can't be more then 50 and less then 2.")]
        [RegularExpression(@"^[A-Z]+[a-zA-Z\s]*$", ErrorMessage = "Name must contain only letters and start from the capital one."), Required]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = null!;

        [Column("LAST_NAME")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name length can't be more then 50 and less then 2.")]
        [RegularExpression(@"^[A-Z]+[a-zA-Z\s]*$", ErrorMessage = "Name must contain only letters and start from the capital one."), Required]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = null!;

        [ForeignKey("GroupId")]
        [InverseProperty("Students")]
        public virtual Group? Group { get; set; } = null!;
    }
}
