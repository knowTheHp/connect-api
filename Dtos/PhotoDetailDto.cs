using System;

namespace connect_api.Dtos
{
    public class PhotoDetailDto
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public DateTime DateAdded { get; set; }
        public bool IsMain { get; set; }
    }
}