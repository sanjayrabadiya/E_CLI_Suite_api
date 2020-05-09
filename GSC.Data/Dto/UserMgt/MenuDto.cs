using System.Collections.Generic;

namespace GSC.Data.Dto.UserMgt
{
    public class MenuDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Icon { get; set; }
        public string Url { get; set; }
        public bool ExactMatch { get; set; }
        public List<MenuDto> Children { get; set; }
        public bool IsFavorited { get; set; }
    }
}