using System.IO;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   /// Provides extension methods for the ModelEnum class.
   /// </summary>
   public static class ModelEnumExtensions
   {
      /// <summary>
      /// Gets the relative file name, from the project directory, for the code defining this ModelEnum instance.
      /// </summary>
      /// <param name="modelEnum">The ModelEnum instance.</param>
      /// <returns>The relative file name.</returns>
      public static string GetRelativeFileName(this ModelEnum modelEnum)
      {
         string outputDirectory = modelEnum.OutputDirectory ?? modelEnum.ModelRoot.EnumOutputDirectory;

         return Path.Combine(outputDirectory, $"{modelEnum.Name}.{modelEnum.ModelRoot.FileNameMarker}.cs");
      }
   }
}