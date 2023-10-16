using System.Net.Http.Json;

namespace UWSkillTree.Services
{
    public class CourseInfo
    {
        public string Id { get; set; } = "";
        public string Subject { get; set; } = "";
        public string CatalogNumber { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public List<string> Term { get; set; } = new List<string>();
        public List<string> Prereq { get; set; } = new List<string>();
        public List<string> Coreq { get; set; } = new List<string>();
        public List<string> Antireq { get; set; } = new List<string>();
        public List<string> Next { get; set; } = new List<string>();
        public string Code
        {
            get { return Subject + " " + CatalogNumber; }
        }

        public CourseInfo() { }

        public CourseInfo(string subject, string catalogNumber)
        {
            Subject = subject;
            CatalogNumber = catalogNumber;
        }

        public CourseInfo(string courseCode)
        {
            string s = courseCode.Replace(" ", "").ToUpper();
            int i = 0;
            while (i < s.Length)
            {
                if ("1234567890".Contains(s[i])) break;
                i++;
            }
            Subject = s.Substring(0, i);
            CatalogNumber = s.Substring(i);
        }
    }
}
