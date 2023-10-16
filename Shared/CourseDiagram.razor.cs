using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using UWSkillTree.Services;

namespace UWSkillTree.Shared
{
    public class CourseDiagramBase : ComponentBase, IDisposable
    {
        [Inject] protected DiagramService DSvc { get; set; } = null!;
        [Inject] private CourseService CSvc { get; set; } = null!;
        [Inject] private IJSRuntime JsRuntime { get; set; } = null!;

        private IJSObjectReference? jsModule { get; set; }
        private int windowHeight, windowWidth;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            CSvc.OnCenterChange += StateHasChanged;
        }

        public void Dispose()
        {
            CSvc.OnCenterChange -= StateHasChanged;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                jsModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./scripts/getWindowSize.js");
            }
            if (jsModule is null)
            {
                return;
            }
            var dimensions = await jsModule.InvokeAsync<WindowDimensions>("getWindowSize");
            windowWidth = dimensions.Width;
            windowHeight = dimensions.Height;

            if (windowWidth > 0 && windowHeight > 0)
            {
                var centerCourse = CSvc.CenterCourse;
                if (centerCourse is null)
                {
                    return;
                }
                DSvc.InitializeDiagram(centerCourse, windowWidth / 2, windowHeight / 2);
            }
        }

        private class WindowDimensions
        {
            public int Width { get; set; }
            public int Height { get; set; }
        }
    }
}
