using System.IO;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   /// Provides extension methods for the ModelClass class.
   /// </summary>
   public static class ModelClassExtensions
   {
      /// <summary>
      /// Gets the relative file name, from the project directory, for the code defining this ModelClass instance.
      /// </summary>
      /// <param name="modelClass">The ModelClass instance.</param>
      /// <returns>The relative file name.</returns>
      public static string GetRelativeFileName(this ModelClass modelClass)
      {
         string outputDirectory = modelClass.OutputDirectory ?? modelClass.ModelRoot.EntityOutputDirectory;

         return Path.Combine(outputDirectory, $"{modelClass.Name}.{modelClass.ModelRoot.FileNameMarker}.cs");
      }
   }
}