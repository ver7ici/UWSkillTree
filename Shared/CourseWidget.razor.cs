using Blazor.Diagrams.Core.Models;
using Microsoft.AspNetCore.Components;
using UWSkillTree.CustomDiagram;
using UWSkillTree.Services;

namespace UWSkillTree.Shared
{
    public class CourseWidgetBase : ComponentBase
    {
        [Inject] protected DiagramService DSvc { get; set; } = null!;
        [Inject] protected CourseService CSvc { get; set; } = null!;

        [Parameter] public CourseNode Node { get; set; } = null!;

        protected void HandlePortClick(PortModel port)
        {
            DSvc.ToggleBranch(Node, port);
        }

        protected void HandleNodeClick()
        {
            CSvc.SelectedCourse = Node.Course;
        }
    }
}
