using System;

namespace ConnectApi.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int StartMonth { get; set; }
        public int StartYear { get; set; }
        public bool IsOnGoing { get; set; }
        public int EndMonth { get; set; }
        public int EndYear { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public DateTime DateAdded { get; set; }
        public User User { get; set; }
        public int UserId { get; set; }
    }
}