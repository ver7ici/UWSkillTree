using System.Net.Http.Json;

namespace UWSkillTree.Services
{
    public class CourseService
    {
        private CourseInfo? centerCourse;
        public CourseInfo? CenterCourse
        {
            get => centerCourse;
            set
            {
                centerCourse = value;
                OnCenterChange?.Invoke();
            }
        }

        private CourseInfo? selectedCourse;
        public CourseInfo? SelectedCourse
        {
            get => selectedCourse;
            set
            {
                selectedCourse = value;
                OnSelectionChange?.Invoke();
            }
        }

        private Dictionary<string, CourseInfo> index = new();
        public async Task InitializeIndex(HttpClient http)
        {
            var idx = await http.GetFromJsonAsync<Dictionary<string, CourseInfo>>("data/courses.json");
            if (idx is not null)
            {
                index = idx;
            }
        }

        public CourseInfo? GetCourse(string subject, string catalogNumber)
        {
            if (index.TryGetValue(subject + " " + catalogNumber, out CourseInfo? c) && c is not null)
            {
                return c;
            }
            return null;
        }

        public CourseInfo? GetCourse(string courseCode)
        {
            if (string.IsNullOrEmpty(courseCode))
            {
                return null;
            }
            var c = new CourseInfo(courseCode);
            return GetCourse(c.Subject, c.CatalogNumber);
        }

        public List<CourseInfo>? GetSearchResults(string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return null;
            }
            return index
                .Select(kvp => kvp.Value)
                .Where
                (c =>
                    c.Code.Replace(" ", "").StartsWith(filter.Replace(" ", "").ToUpper())
                    || c.Title.ToLower().Contains(filter.ToLower())
                )
                .OrderBy(c => c.Code + c.Title)
                .ToList();
        }

        #region event handler
        public event Action? OnCenterChange;
        public event Action? OnSelectionChange;
        #endregion
    }
}
