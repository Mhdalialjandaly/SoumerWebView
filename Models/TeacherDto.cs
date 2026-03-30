using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class TeacherDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public string Bio { get; set; }
        public string Image { get; set; }
        public List<string> Courses { get; set; }
    }
}
