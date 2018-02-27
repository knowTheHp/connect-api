using System;

namespace connect_api.Dtos
{
    public class SkillDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Experience { get; set; }
        public DateTime DateAdded { get; set; }
    }
}