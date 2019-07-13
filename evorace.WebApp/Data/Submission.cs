using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace evorace.WebApp.Data
{
    public partial class Submission
    {
        [Key]
        [Required]
        [StringLength(36)]
        public string Id { get; set; }

        [Required]
        public virtual ApplicationUser User { get; set; }

        #region File attributes

        [Required]
        [StringLength(255)]
        [DisplayName("Dll neve")]
        public string OriginalFileName { get; set; }

        [StringLength(255)]
        public string StoredFileName { get; set; }

        [DisplayName("Fájl méret")]
        public int FileSize { get; set; }

        [DisplayName("Feltöltés ideje")]
        public DateTime UploadDate { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletionDate { get; set; }

        #endregion

        public ValidationStateEnum ValidationState { get; set; }

        public bool IsValid { get; set; }

        [StringLength(2048)]
        public string Error { get; set; }

        public Submission()
        {
            Id = Guid.NewGuid().ToString();
        }

        public enum ValidationStateEnum
        {
            None = 0,
            File = 1,
            Static = 2,
            UnitTest = 3,
            Completed = 100
        }
    }
}
