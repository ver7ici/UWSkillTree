using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.PathGenerators;
using Blazor.Diagrams.Core.Routers;
using Blazor.Diagrams.Options;
using Blazor.Diagrams;
using UWSkillTree.CustomDiagram;
using UWSkillTree.Shared;
using Blazor.Diagrams.Core.Geometry;

namespace UWSkillTree.Services
{
    public class DiagramService
    {
        public DiagramService(CourseService cSvc)
        {
            CSvc = cSvc;
            Diagram.RegisterComponent<CourseNode, CourseWidget>();
        }

        #region properties

        private readonly CourseService CSvc;

        public BlazorDiagram Diagram { get; } = new(options);
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

        private CourseNode? centerNode;

        private Point centerPoint = new(0, 0);

        private const int NodeWidth = 150, NodeHeight = 50;
        public static string NodeWidthPx { get => NodeWidth + "px"; }
        public static string NodeHeightPx { get => NodeHeight + "px"; }

        private const int xInterval = 300, yInterval = 100;

        private List<List<CourseNode>> nodeStackL = new(), nodeStackR = new();

        #endregion

        #region public methods

        public void InitializeDiagram(CourseInfo c, double xCenter, double yCenter)
        {
            Diagram.Links.Clear();
            Diagram.Nodes.Clear();
            Diagram.SetZoom(1.0);
            Diagram.SetPan(0, 0);
            centerPoint = new(xCenter - NodeWidth / 2, yCenter - NodeHeight / 2);

            centerNode = Diagram.Nodes.Add(newNode(c, centerPoint.X, centerPoint.Y, Direction.Center));
            nodeStackL.Add(new List<CourseNode>() { centerNode });
            nodeStackR.Add(new List<CourseNode>() { centerNode });
        }

        public void ToggleBranch(CourseNode node, PortModel port)
        {
            bool isLeftPort = port.Alignment == PortAlignment.Left;
            bool expands = isLeftPort ? node.ExpandsLeft : node.ExpandsRight;
            var d = isLeftPort ? Direction.Left : Direction.Right;
            var nodeStack = isLeftPort ? nodeStackR : nodeStackL;

            if (expands)
            {
                bool expanded = port.Links.Count > 0;
                unbranch(node, d);
                if (!expanded)
                {
                    branch(node, d);
                }
            }
            else if (node != centerNode)
            {
                var oppositePort = node.GetPort((PortAlignment)(((int)port.Alignment + 4) % 8));
                if (oppositePort is not null && oppositePort.Links.Count > 0)
                {
                    unbranch(node, (Direction)(-(int)d));
                }
                Diagram.Nodes.Remove(node);
                for (int i = nodeStack.Count - 1; i >= 0; i--)
                {
                    if (nodeStack[i].Remove(node)) break;
                }
            }
        }

        #endregion

        #region private methods

        private void unbranch(CourseNode node, Direction d)
        {
            var nodeStack = d == Direction.Left ? nodeStackL : nodeStackR;
            while (!nodeStack.Last().Contains(node))
            {
                Diagram.Nodes.Remove(nodeStack.Last());
                nodeStack.RemoveAt(nodeStack.Count - 1);
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

            int i = 0;
            foreach (var cName in childNames)
            {
                var cCourse = CSvc.GetCourse(cName);
                if (cCourse is null)
                {
                    continue;
                }
                var cNode = Diagram.Nodes.Add(newNode(cCourse, parentNode.Position.X + (int)d * xInterval, y0 + i * yInterval, d));

                var link = Diagram.Links.Add(newLink(parentNode, cNode));

                cNodes.Add(cNode); cLinks.Add(link);
                i++;
            }

            if (d == Direction.Left)
            {
                nodeStackL.Add(cNodes);
            }
            if (d == Direction.Right)
            {
                nodeStackR.Add(cNodes);
            }
        }

        private static CourseNode newNode(CourseInfo c, double x, double y, Direction? d = null)
        {
            var node = new CourseNode(c, new Point(x, y));
            node.Locked = true;

            if (c.Prereq.Count > 0)
            {
                node.AddPort(PortAlignment.Left);
            }
            if (c.Next.Count > 0)
            {
                node.AddPort(PortAlignment.Right);
            }

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

        #endregion

        #region enum

        private enum Direction
        {
            Left = -1,
            Center = 0,
            Right = 1,
        }

        #endregion
    }
}
