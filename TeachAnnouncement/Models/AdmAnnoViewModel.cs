using System.ComponentModel.DataAnnotations;

namespace TeachAnnouncement.Models
{
    public class AdmAnnoViewModel
    {
        public class QueryIn
        {
            public string AnnoSubject { get; set; }
            public string AnnoStatus { get; set; }

            public PaginationModel pagination { get; set; }
        }

        public class QueryOut
        {
            public List<AnnoModel> Grid { get; set; }
            public PaginationModel pagination { get; set; }
        }

        public class AnnoModel
        {
            public string Pkey { get; set; }
            public string AnnoDate { get; set; }
            public string AnnoSubject { get; set; }
            public string AnnoContent { get; set; }
            public string AnnoStatus { get; set; }
            public string AnnoStatusName { get; set; }
        }

        /// <summary>
        // 分頁 Model
        /// </summary>
        public class PaginationModel
        {
            public List<string> pages { get; set; }
            public int pageNo { get; set; }
            public int pageSize { get; set; }
            public int totalPage { get; set; }
            public int totalCount { get; set; }
        }

        public class AddSaveIn
        {
            [Required]
            public string AnnoDate { get; set; }
            [Required]
            public string AnnoSubject { get; set; }
            [Required]
            public string AnnoContent { get; set; }
            [Required]
            public string AnnoStatus { get; set; }
        }

        public class AddSaveOut
        {
            public string ErrMsg { get; set; }
            public string ResultMsg { get; set; }
        }

        public class EditSaveIn
        {
            [Required]
            public string Pkey { get; set; }
            [Required]
            public string AnnoDate { get; set; }
            [Required]
            public string AnnoSubject { get; set; }
            [Required]
            public string AnnoContent { get; set; }
            [Required]
            public string AnnoStatus { get; set; }
        }

        public class EditSaveOut
        {
            public string ErrMsg { get; set; }
            public string ResultMsg { get; set; }
        }

        public class DelCheckIn
        {
            public List<AnnoModel> checks { get; set; }
        }

        public class DelCheckOut
        {
            public string ErrMsg { get; set; }
            public string ResultMsg { get; set; }
        }
    }
}
