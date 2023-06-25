using Newtonsoft.Json;

namespace Sawczyn.EFDesigner.EFModel.Nuget
{
   /// <summary>
   /// Represents a context object that contains information about the current Nuget request being processed.
   /// </summary>
   public class Context

   {
      /// <summary>
      /// Gets or sets the vocabulary.
      /// </summary>
      public string Vocab { get; set; }

      /// <summary>
      /// Gets or sets the base.
      /// </summary>
      [JsonProperty("@base")]
      public string Base { get; set; }
   }
}