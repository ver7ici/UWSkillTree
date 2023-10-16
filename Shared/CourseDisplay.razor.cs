using Microsoft.AspNetCore.Components;
using UWSkillTree.Services;

namespace UWSkillTree.Shared
{
    public class CourseDisplayBase : ComponentBase, IDisposable
    {
        [Inject] protected CourseService CSvc { get; set; } = null!;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            CSvc.OnSelectionChange += StateHasChanged;
        }

        public void Dispose()
        {
            CSvc.OnSelectionChange -= StateHasChanged;
        }
    }
}