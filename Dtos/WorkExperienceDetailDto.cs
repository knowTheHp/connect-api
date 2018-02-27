using System;

namespace connect_api.Dtos
{
    public class WorkExperienceDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Company { get; set; }
        public string Location { get; set; }
        public int StartMonth { get; set; }
        public int StartYear { get; set; }
        public bool IsCurrentlyWorking { get; set; }
        public int EndMonth { get; set; }
        public int EndYear { get; set; }
        public string Description { get; set; }
        public DateTime DateAdded { get; set; }
    }
}