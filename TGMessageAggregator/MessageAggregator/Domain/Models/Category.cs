using System.Collections.Generic;

namespace Domain.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Summary> Summaries { get; set; }
    }
}
