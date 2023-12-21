using Microsoft.VisualStudio.Modeling.Diagrams;
using Microsoft.VisualStudio.Modeling.Diagrams.GraphObject;
using QuikGraph;
using QuikGraph.Graphviz;
using QuikGraph.Graphviz.Dot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   /// Engine that lays out a diagram using Graphviz
   /// </summary>
   public class GraphVizLayout
   {
      /// <summary>
      /// Layout the given shape elements in the specified entity framework model diagram.
      /// </summary>
      /// <param name="diagram">Owning diagram</param>
      public static void Execute(EFModelDiagram diagram)
      {
         BinaryLinkShape[] links = diagram.NestedChildShapes.OfType<BinaryLinkShape>().ToArray();

         NodeShape[] nodeShapes = diagram.NestedChildShapes.OfType<NodeShape>()
                                         .Union(links.Select(link => link.FromShape)
                                                     .Union(links.Select(link => link.ToShape))
                                                     .Where(node => node != null))
                                         .Distinct()
                                         .ToArray();

         List<DotNode> vertices = nodeShapes.Select(node => new DotNode { Shape = node }).ToList();

         List<DotEdge> edges = links.Select(link =>
                                               new DotEdge(vertices.Single(vertex => vertex.Shape.Id == link.FromShape.Id)
                                                         , vertices.Single(vertex => vertex.Shape.Id == link.ToShape.Id)) { Shape = link })
                                    .ToList();

         // set up to be a bidirectional graph with the edges we found
         BidirectionalGraph<DotNode, DotEdge> graph = edges.ToBidirectionalGraph<DotNode, DotEdge>();

         // add all the vertices that aren't connected by edges
         _ = graph.AddVertexRange(vertices.Except(edges.Select(e => e.Source).Union(edges.Select(e => e.Target))));

         // we'll process as Graphviz
         GraphvizAlgorithm<DotNode, DotEdge> graphviz = new GraphvizAlgorithm<DotNode, DotEdge>(graph);
         graphviz.GraphFormat.NodeSeparation = 1.0;
         graphviz.GraphFormat.Splines = GraphvizSplineType.Ortho;
         graphviz.CommonVertexFormat.Shape = GraphvizVertexShape.Record;

         // labels will be the Id of the underlying Shape
         graphviz.FormatVertex += (_, args) =>
                                  {
                                     args.VertexFormat.Label = args.Vertex.Shape.ModelElement is ModelClass modelClass
                                                                  ? modelClass.Name
                                                                  : args.Vertex.Shape.ModelElement is ModelEnum modelEnum
                                                                     ? modelEnum.Name
                                                                     : args.Vertex.Shape.ModelElement.Id.ToString();

                                     args.VertexFormat.FixedSize = true;

                                     args.VertexFormat.Size = new GraphvizSizeF((float)args.Vertex.Shape.Size.Width, (float)args.Vertex.Shape.Size.Height);

                                     args.VertexFormat.Label = args.Vertex.Shape.Id.ToString();
                                  };

         graphviz.FormatEdge += (_, args) =>
                                {
                                   args.EdgeFormat.Label.Value = args.Edge.Shape.Id.ToString();
                                };

         // generate the commands
         string dotFileName = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
         string dotCommands = graphviz.Generate(new DotEngine(), dotFileName);
         Debug.WriteLine(dotFileName);
         Debug.WriteLine(dotCommands);

         ProcessStartInfo dotStartInfo = new ProcessStartInfo(EFModelPackage.Options.DotExePath, "-T plain")
         {
            RedirectStandardInput = true
                                          ,
            RedirectStandardOutput = true
                                          ,
            UseShellExecute = false
                                          ,
            CreateNoWindow = true
         };

         string graphOutput;

         using (Process dotProcess = Process.Start(dotStartInfo))
         {
            // stdin is redirected to our stream, so pump the commands in through that
            dotProcess.StandardInput.WriteLine(dotCommands);

            // closing the stream starts the process
            dotProcess.StandardInput.Close();

            // stdout is redirected too, so capture the output
            graphOutput = dotProcess.StandardOutput.ReadToEnd();
            dotProcess.WaitForExit();
         }

         Debug.WriteLine(graphOutput);

         // break it up into lines of text for processing
         string[] outputLines = graphOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
         SizeD graphSize = SizeD.Empty;

         // ReSharper disable once LoopCanBePartlyConvertedToQuery
         foreach (string outputLine in outputLines)
         {
            // spaces aren't valid in any of the data, so we can treat them as delimiters
            string[] parts = outputLine.Split(' ');
            string id;

            double x, y;

            // graphviz coordinates have 0,0 at the bottom left, positive means moving up
            // our coordinates have 0,0 at the top left, positive means moving down
            // so we need to transform them
            switch (parts[0])
            {
               case "graph":
                  // 0     1 2      3
                  // graph 1 109.38 92.681
                  graphSize = new SizeD(double.Parse(parts[2]), double.Parse(parts[3]));

                  break;

               case "node":
                  // 0    1  2      3      4   5      6                                      7
                  // node 71 78.514 93.639 1.5 3.3056 "0f651fe7-da0f-453f-a08a-ec1d31ec0e71" solid record black lightgrey
                  id = parts[6].Trim('"');
                  DotNode dotNode = vertices.Single(v => v.Shape.Id.ToString() == id); // label

                  x = double.Parse(parts[2]);
                  y = graphSize.Height - double.Parse(parts[3]);

                  dotNode.Shape.Bounds = new RectangleD(x, y, dotNode.Shape.Size.Width, dotNode.Shape.Size.Height);

                  break;

               case "edge":
                  // 0    1 2  3 4      5      6      7      8      9     10     11    12
                  // edge 6 18 4 34.926 77.518 34.926 77.518 34.926 75.88 34.926 75.88 "567b5db7-7591-4aa7-845c-76635bf56f28" 36.083 77.16 solid black
                  id = parts[4 + int.Parse(parts[3]) * 2].Trim('"');
                  DotEdge edge = edges.Single(e => e.Shape.Id.ToString() == id);

                  // need to mark the connector as dirty. this is the easiest way to do this
                  BinaryLinkShape linkShape = edge.Shape;
                  linkShape.ManuallyRouted = !linkShape.ManuallyRouted;
                  linkShape.FixedFrom = VGFixedCode.NotFixed;
                  linkShape.FixedTo = VGFixedCode.NotFixed;

                  // make the labels follow the lines
                  foreach (LineLabelShape lineLabelShape in linkShape.RelativeChildShapes.OfType<LineLabelShape>())
                  {
                     lineLabelShape.ManuallySized = false;
                     lineLabelShape.ManuallyPlaced = false;
                  }

                  linkShape.EdgePoints.Clear();

                  int pointCount = int.Parse(parts[3]);

                  for (int index = 4; index < 4 + pointCount * 2; index += 2)
                  {
                     x = double.Parse(parts[index]);
                     y = graphSize.Height - double.Parse(parts[index + 1]);
                     _ = linkShape.EdgePoints.Add(new EdgePoint(x, y, VGPointType.Normal));
                  }

                  // since we're not changing the nodes this edge connects, this really doesn't do much.
                  // what it DOES do, however, is call ConnectEdgeToNodes, which is an internal method we'd otherwise
                  // be unable to access
                  linkShape.Connect(linkShape.FromShape, linkShape.ToShape);
                  linkShape.ManuallyRouted = false;

                  break;
            }
         }

         diagram.Invalidate();
      }
   }
}
