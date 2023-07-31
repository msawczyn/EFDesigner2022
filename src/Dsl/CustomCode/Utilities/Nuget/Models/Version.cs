using Newtonsoft.Json;

namespace Sawczyn.EFDesigner.EFModel.Nuget
{
   /// <summary>
   /// Represents a NuGet pakcage version number in the format major.minor.build.revision.
   /// </summary>
   public class Version
   {
      /// <summary>
      /// Represents a string version.
      /// </summary>
      [JsonProperty("version")]
      public string VersionVersion { get; set; }

      /// <summary>
      /// Gets or sets the number of downloads.
      /// </summary>
      [JsonProperty("downloads")]
      public long Downloads { get; set; }

      /// <summary>
      /// Gets or sets the ID value.
      /// </summary>
      [JsonProperty("@id")]
      public string Id { get; set; }
   }
}