using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.InformConcent
{
    public class Payload
    {
        public string to { get; set; }
        public string priority { get; set; }
        public string content_available { get; set; }
        public Notification notification { get; set; }
    }

    public class Notification
    {
        public string body { get; set; }
        public string title { get; set; }
        public string icon { get; set; }
    }
}
