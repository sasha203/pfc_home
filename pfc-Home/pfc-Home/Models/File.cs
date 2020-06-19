using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace pfc_Home.Models
{
    public class File
    {
        public int Id { get; set; }
        public string FileOwner { get; set; }
        public string FileTitle { get; set; }
        public string Link { get; set; }
    }
}