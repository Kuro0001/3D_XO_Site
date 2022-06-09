using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace _3D_XO_Site.Models
{
    public class NewsBlock
    {
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [Display(Name = "Title")]
        public string Title { get; set; }

        [Display(Name = "Content")]
        [UIHint("MultilineText")]
        public string Text { get; set; }

        [Display(Name = "Date")]
        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; }
    }
}
