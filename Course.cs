

using System.Net.Http.Json;

namespace UWSkillTree
{
    public class Course
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
        public string Code 
        { 
            get { return Subject + " " + CatalogNumber; } 
        }

        public Course() { }

        public Course(string subject, string catalogNumber)
        {
            Subject = subject;
            CatalogNumber = catalogNumber;
        }

        public Course(string courseCode)
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

        public static async Task<Course?> GetCourseAsync(HttpClient Http, string subject, string catalogNumber) 
        {
            try {
                return await Http.GetFromJsonAsync<Course>(string.Format(
                    "data/{0}/{1}.json",
                    subject,
                    catalogNumber
                ));
            } catch (HttpRequestException)
            {
                return null;
            }
        }

        public static async Task<Course?> GetCourseAsync(HttpClient Http, string courseCode) 
        {
            Course c = new(courseCode);
            return await GetCourseAsync(Http, c.Subject, c.CatalogNumber);
        }
    }
}
