using System.Collections.Generic;

using Microsoft.VisualStudio.Modeling;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   /// Interface for processing files containing Entity Framework model entities.
   /// </summary>
   public interface IFileProcessor

   {
      /// <summary>
      /// Process a given input file and returns a list of new model elements.
      /// </summary>
      /// <param name="inputFile">Input file path.</param>
      /// <param name="newElements">List of new model elements created during processing.</param>
      /// <returns>True if processing was successful, otherwise false.</returns>
      bool Process(string inputFile, out List<ModelElement> newElements);
   }
}