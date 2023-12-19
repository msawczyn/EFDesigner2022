using QuikGraph.Graphviz;
using QuikGraph.Graphviz.Dot;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   /// Implementation of the IDotEngine interface that uses the Graphviz dot.exe
   /// </summary>
   public class DotEngine : IDotEngine
   {
      /// <summary>
      /// Runs Graphviz with the specified image type, dot source, and output file name and returns the output as a string.
      /// </summary>
      /// <param name="imageType">The image type to create.</param>
      /// <param name="dot">The Graphviz dot source to create an image from.</param>
      /// <param name="outputFileName">The name of the output file to create.</param>
      /// <returns>The output of Graphviz as a string.</returns>
      public string Run(GraphvizImageType imageType, string dot, string outputFileName)
      {
         return dot;
      }
   }
}