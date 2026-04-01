using Core;
using Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities
{
    public class CourseVideo : IEntity
    {
        public CourseVideo()
        {
            CreatedAt = DateTime.Now;
        }

        public int Id { get; set; }

        // العلاقة مع الكورس
        public int CourseId { get; set; }
        public  Course Course { get; set; }

        // معلومات الفيديو
        public string Title { get; set; }         
        public string Description { get; set; }     
        public string VideoUrl { get; set; }         
        public VideoPlatform Platform { get; set; }  

        // معلومات إضافية 
        public string VideoId { get; set; }        
        public int Duration { get; set; }            
        public int Order { get; set; }              
        public bool IsFree { get; set; }             

        // حالة الفيديو
        public bool IsPublished { get; set; }        
        public DateTime? PublishDate { get; set; }   

        // التتبع
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public string DeletedBy { get; set; }
    }
}
