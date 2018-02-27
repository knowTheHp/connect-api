using System;

namespace connect_api.Dtos
{
    public class EducationDetailDto
    {
        public int Id { get; set; }
        public string School { get; set; }
        public string Degree { get; set; }
        public string Field { get; set; }
        public string Grade { get; set; }
        public int StartYear { get; set; }
        public int EndYear { get; set; }
        public DateTime DateAdded { get; set; }
    }
}