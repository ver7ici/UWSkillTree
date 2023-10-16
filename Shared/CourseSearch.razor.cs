using Microsoft.AspNetCore.Components;
using UWSkillTree.Services;

namespace UWSkillTree.Shared
{
    public class CourseSearchBase : ComponentBase, IDisposable
    {
        [Inject] protected CourseService CSvc { get; set; } = null!;

        protected string? SelectedCourseCode;
        protected List<CourseInfo>? CourseList;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            CSvc.OnCenterChange += StateHasChanged;
        }

        public void Dispose()
        {
            CSvc.OnCenterChange -= StateHasChanged;
        }

        protected void HandleInput(ChangeEventArgs e)
        {
            var filter = e.Value?.ToString();

            if (filter?.Replace(" ", null).Length > 2)
            {
                CourseList = CSvc.GetSearchResults(filter);
            }
            else
            {
                CourseList = null;
            }
        }

        protected void SelectCourse(CourseInfo c)
        {
            CSvc.CenterCourse = c;
            CSvc.SelectedCourse = c;
            SelectedCourseCode = c.Code;
            CourseList = null;
        }

        protected void HandleSubmit()
        {
            if (string.IsNullOrEmpty(SelectedCourseCode))
            {
                return;
            }
            var c = CSvc.GetCourse(SelectedCourseCode);
            if (c is not null)
            {
                SelectCourse(c);
            }
            else if (CourseList is not null && CourseList.Count > 0)
            {
                SelectCourse(CourseList.First());
            }
        }
    }
}
