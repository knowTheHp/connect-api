using System;

namespace ConnectApi.Models
{
    public class Skill
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Experience { get; set; }
        public DateTime DateAdded { get; set; }
        public User User { get; set; }
        public int UserId { get; set; }
    }
}