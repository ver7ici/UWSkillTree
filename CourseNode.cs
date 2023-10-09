using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;

namespace UWSkillTree
{
    public class CourseNode : NodeModel
    {
        public CourseInfo Course { get; set; } = new();

        public bool ExpandsLeft { get; set; } = false;
        public bool ExpandsRight { get; set; } = false;

        public CourseNode(Point? position = null) : base(position: position) { }

        public CourseNode(CourseInfo course, Point? position = null)
            : base(position: position)
        {
            Course = course;
            Title = course.Code;
        }
    }
}
