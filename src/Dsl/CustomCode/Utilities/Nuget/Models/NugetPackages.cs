using System;
using System.Collections.Generic;

using Newtonsoft.Json;

// ReSharper disable UnusedMember.Global

namespace Sawczyn.EFDesigner.EFModel.Nuget
{
   // ReSharper disable once PartialTypeWithSinglePart
   /// <summary>
   /// This class represents a collection of NuGet packages.
   /// </summary>
   public partial class NuGetPackages
   {
      ///<summary>
      ///Gets or sets the context object.
      ///</summary>
      [JsonProperty("@context")]
      public Context Context { get; set; }

      /// <summary>
      /// The total number of hits received for the pakage.
      /// </summary>
      [JsonProperty("totalHits")]
      public long TotalHits { get; set; }

      /// <summary>
      /// Gets or sets the date and time of the last reopen.
      /// </summary>
      [JsonProperty("lastReopen")]
      public DateTimeOffset LastReopen { get; set; }

      /// <summary>
      /// Gets or sets the index value as a string.
      /// </summary>
      [JsonProperty("index")]
      public string Index { get; set; }

      /// <summary>
      /// Gets or sets the data list of PackageData objects.
      /// </summary>
      [JsonProperty("data")]
      public List<PackageData> Data { get; set; }

      /// <summary>
      /// Parses the given JSON string and returns a NuGetPackages object.
      /// </summary>
      /// <param name="json">The JSON string to parse.</param>
      /// <returns>A NuGetPackages object representing the parsed JSON.</returns>
      public static NuGetPackages FromJson(string json)
      {
         return JsonConvert.DeserializeObject<NuGetPackages>(json, Converter.Settings);
      }
   }
}