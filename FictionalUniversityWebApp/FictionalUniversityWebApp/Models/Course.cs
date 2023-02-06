using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FictionalUniversityWebApp.Models
{
	[Table("COURSES")]
	public partial class Course
	{
		public Course()
		{
			Groups = new HashSet<Group>();
		}

		[Key]
		[Column("COURSE_ID")]
		public int CourseId { get; set; }

		[Column("NAME")]
		[StringLength(50, MinimumLength = 3, ErrorMessage = "Name length can't be more then 50 and less then 3.")]
		[Required]
		[Display(Name = "Course name")]
		public string Name { get; set; } = null!;

		[Column("DESCRIPTION")]
		[StringLength(500, MinimumLength = 3, ErrorMessage = "Name length can't be more then 500 and less then 3.")]
		[Required]
		public string Description { get; set; } = null!;

		[InverseProperty("Course")]
		public virtual ICollection<Group> Groups { get; set; }
	}
}
