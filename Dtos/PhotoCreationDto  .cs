using System;
using Microsoft.AspNetCore.Http;

namespace connect_api.Dtos
{
    public class PhotoCreationDto
    {
        public string Url { get; set; }
        public IFormFile File { get; set; }
        public DateTime DateAdded { get; set; }
        public string PublicId { get; set; }

        public PhotoCreationDto()
        {
            DateAdded = DateTime.Now;
        }
    }
}