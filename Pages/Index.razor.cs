using Microsoft.AspNetCore.Components;
using UWSkillTree.Services;

namespace UWSkillTree.Pages
{
    public class IndexBase : ComponentBase
    {
        [Inject] private CourseService CSvc { get; set; } = null!;
        [Inject] private HttpClient Http { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await CSvc.InitializeIndex(Http);
        }
    }
}
