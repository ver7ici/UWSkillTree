using Blazor.Diagrams;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.PathGenerators;
using Blazor.Diagrams.Core.Routers;
using Blazor.Diagrams.Options;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using UWSkillTree.Shared;

namespace UWSkillTree
{
    public class CourseService
    {
        public CourseService()
        {
            Diagram.RegisterComponent<CourseNode, CourseWidget>();
        }

        #region course data
        private CourseInfo? centerCourse;
        public CourseInfo? CenterCourse
        {
            get => centerCourse;
            set
            {
                centerCourse = value;
                //NotifyStateChanged();
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
                //NotifyStateChanged();
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
            CourseInfo? c;
            if (index.TryGetValue(subject + " " + catalogNumber, out c) && c is not null)
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

        //public void SelectCourse(CourseNode node)
        //{
        //    foreach (var n in Diagram.Nodes)
        //    {
        //        if (n == node)
        //        {
        //            n.Selected = true;
        //        }
        //        else
        //        {
        //            n.Selected = false;
        //        }
        //    }
        //}
        #endregion

        #region diagram
        public BlazorDiagram Diagram { get; private set; } = new(options);
        private static readonly BlazorDiagramOptions options = new()
        {
            Links =
            {
                DefaultColor = "grey",
                DefaultSelectedColor = "grey",
                DefaultRouter = new OrthogonalRouter(),
                DefaultPathGenerator = new StraightPathGenerator(),
            },
            Zoom =
            {
                Inverse = true,
            },
        };

        private List<List<CourseNode>> nodeStackL = new(), nodeStackR = new();
        //private List<List<LinkModel>> linkStackL = new(), linkStackR = new();

        private Point centerPoint = new(0, 0);
        public CourseNode? CenterNode { get; set; }

        private const int NodeWidth = 150, NodeHeight = 50;
        public static string NodeWidthPx { get => NodeWidth + "px"; }
        public static string NodeHeightPx { get => NodeHeight + "px"; }

        private const int xInterval = 250, yInterval = 100;

        public void InitializeDiagram(double xCenter, double yCenter)
        {
            Diagram.Links.Clear();
            Diagram.Nodes.Clear();
            Diagram.SetZoom(1.0);
            Diagram.SetPan(0, 0);
            centerPoint = new(xCenter - NodeWidth / 2, yCenter - NodeHeight / 2);

            if (CenterCourse is null)
            {
                return;
            }
            CenterNode = Diagram.Nodes.Add(newNode(CenterCourse, centerPoint.X, centerPoint.Y, Direction.Center));
            nodeStackL.Add(new List<CourseNode>() { CenterNode });
            nodeStackR.Add(new List<CourseNode>() { CenterNode });

            ToggleBranch(CenterNode);
        }

        public void ToggleBranch(CourseNode node)
        {
            //SelectedCourse = node.Course;

            if (node.ExpandsRight)
            {
                bool closed = node.GetPort(PortAlignment.Right)!.Links.Count == 0;
                unbranch(node, Direction.Right);
                if (closed)
                {
                    branch(node, Direction.Right);
                }
            }
            if (node.ExpandsLeft)
            {
                bool closed = node.GetPort(PortAlignment.Left)!.Links.Count == 0;
                unbranch(node, Direction.Left);
                if (closed)
                {
                    branch(node, Direction.Left);
                }
            }
        }

        private void unbranch(CourseNode node, Direction d)
        {
            var nodeStack = d == Direction.Left ? nodeStackL : nodeStackR;
            //var linkStack = d == Direction.Left ? linkStackL : linkStackR;
            while (!nodeStack.Last().Contains(node))
            {
                Diagram.Nodes.Remove(nodeStack.Last());
                //Diagram.Links.Remove(linkStack.Last());
                nodeStack.RemoveAt(nodeStack.Count - 1);
                //linkStack.RemoveAt(linkStack.Count - 1);
            }
        }

        private void branch(CourseNode parentNode, Direction d)
        {
            var childNames = d == Direction.Left ? parentNode.Course.Prereq : parentNode.Course.Next;
            if (childNames.Count == 0)
            {
                return;
            }
            var y0 = parentNode.Position.Y - (childNames.Count - 1) * yInterval / 2;

            List<CourseNode> cNodes = new();
            List<LinkModel> cLinks = new();

            foreach (var (cName, i) in childNames.Select((cName, i) => (cName, i)))
            {
                var cCourse = GetCourse(cName);
                if (cCourse is null)
                {
                    continue;
                }
                var cNode = Diagram.Nodes.Add(newNode(cCourse, parentNode.Position.X + (int)d*xInterval, y0 + i * yInterval, d));

                var link = Diagram.Links.Add(newLink(parentNode, cNode));

                cNodes.Add(cNode); cLinks.Add(link);
            }

            if (d == Direction.Left)
            {
                nodeStackL.Add(cNodes); 
                //linkStackL.Add(cLinks);
            }
            else
            {
                nodeStackR.Add(cNodes); 
                //linkStackL.Add(cLinks);

            }
        }

        private static CourseNode newNode(CourseInfo c, double x, double y, Direction? d = null)
        {
            var node = new CourseNode(c, new Point(x, y));
            node.AddPort(PortAlignment.Left);
            node.AddPort(PortAlignment.Right);
            node.Locked = true;
            switch (d)
            {
                case Direction.Right:
                    node.ExpandsRight = true;
                    break;
                case Direction.Left:
                    node.ExpandsLeft = true;
                    break;
                case Direction.Center:
                    node.ExpandsLeft = true;
                    node.ExpandsRight = true;
                    break;
            }
            return node;
        }

        private static LinkModel newLink(NodeModel n1, NodeModel n2)
        {
            bool d = n1.Position.X < n2.Position.X;
            (n1, n2) = d ? (n1, n2) : (n2, n1); 
            return new LinkModel(n1.GetPort(PortAlignment.Right)!, n2.GetPort(PortAlignment.Left)!)
            {
                Locked = true,
                TargetMarker = LinkMarker.Arrow,
            };
        }

        private enum Direction
        {
            Left = -1,
            Center = 0,
            Right = 1,
        }

        #endregion

        #region event handler
        public event Action? OnChange;
        public event Action? OnCenterChange;
        public event Action? OnSelectionChange;
        public void NotifyStateChanged() => OnChange?.Invoke();
        #endregion
    }
}
