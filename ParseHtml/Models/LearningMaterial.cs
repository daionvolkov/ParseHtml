using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models
{
    public class LearningMaterial
    {
        public int UserId { get; set;}
        public string? Title { get; set; }
        public int TargetLanguageId { get; set; }
        public int WrittenLanguageId { get; set; }
        public int CategoryId { get; set; }
        public string? PublishDate { get; set; }
        public string? Content { get; set; }
        public int MaterialType { get; set; }
        public string? Tags { get; set; }
    }
}