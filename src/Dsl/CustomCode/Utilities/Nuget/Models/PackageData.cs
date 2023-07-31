using System.Collections.Generic;

using Newtonsoft.Json;

namespace Sawczyn.EFDesigner.EFModel.Nuget
{
   /// <summary>
   /// Represents a NuGet package data object.
   /// </summary>
   public class PackageData
   {
      /// <summary>
      /// Gets or sets the Id value.
      /// </summary>
      [JsonProperty("@id")]
      public string Id { get; set; }

      /// <summary>
      /// Gets or sets the type of the object.
      /// </summary>
      [JsonProperty("@type")]
      public TypeEnum Type { get; set; }

      /// <summary>
      /// Gets or sets the registration string.
      /// </summary>
      [JsonProperty("registration")]
      public string Registration { get; set; }

      /// <summary>
      /// Gets or sets the DatumId.
      /// </summary>
      [JsonProperty("id")]
      public string DatumId { get; set; }

      /// <summary>
      /// Gets or sets the version.
      /// </summary>
      [JsonProperty("version")]
      public string Version { get; set; }

      /// <summary>
      /// Gets or sets the description.
      /// </summary>
      [JsonProperty("description")]
      public string Description { get; set; }

      /// <summary>
      /// Gets or sets the summary of the object
      /// </summary>
      [JsonProperty("summary")]
      public string Summary { get; set; }

      /// <summary>
      /// Gets or sets the title of an object.
      /// </summary>
      [JsonProperty("title")]
      public string Title { get; set; }

      /// <summary>
      /// Gets or sets the URL of the icon.
      /// </summary>
      [JsonProperty("iconUrl", NullValueHandling = NullValueHandling.Ignore)]
      public string IconUrl { get; set; }

      /// <summary>
      /// Gets or sets the URL where the license for the software can be found.
      /// </summary>
      [JsonProperty("licenseUrl", NullValueHandling = NullValueHandling.Ignore)]
      public string LicenseUrl { get; set; }

      /// <summary>
      /// Gets or sets the project URL.
      /// </summary>
      [JsonProperty("projectUrl", NullValueHandling = NullValueHandling.Ignore)]
      public string ProjectUrl { get; set; }

      /// <summary>
      /// Gets or sets a list of strings for the object tags.
      /// </summary>
      [JsonProperty("tags")]
      public List<string> Tags { get; set; }

      /// <summary>
      /// List of authors for a book.
      /// </summary>
      [JsonProperty("authors")]
      public List<string> Authors { get; set; }

      /// <summary>
      /// Gets or sets the total number of downloads.
      /// </summary>
      [JsonProperty("totalDownloads")]
      public long TotalDownloads { get; set; }

      /// <summary>
      /// Gets or sets a value indicating whether the object has been verified.
      /// </summary>
      [JsonProperty("verified")]
      public bool Verified { get; set; }

      /// <summary>
      /// A list of versions.
      /// </summary>
      [JsonProperty("versions")]
      public List<Version> Versions { get; set; }
   }
}