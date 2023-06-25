namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   /// This class represents a NuGet package display information, including its ID, version, and authors.
   /// </summary>
   public class NuGetDisplay
   {
      /// <summary>
      /// Initializes a new instance of the NuGetDisplay class with the specified parameters.
      /// </summary>
      /// <param name="efVersion">The EF version that the package is compatible with.</param>
      /// <param name="packageId">The ID of the package to be displayed.</param>
      /// <param name="packageVersion">The version of the package to be displayed.</param>
      /// <param name="display">The display text for the package.</param>
      /// <param name="majorMinorVersion">The major and minor version of the package.</param>
      public NuGetDisplay(EFVersion efVersion, string packageId, string packageVersion, string display, string majorMinorVersion)
      {
         EFVersion = efVersion;
         PackageId = packageId;
         ActualPackageVersion = packageVersion;
         DisplayVersion = display;
         MajorMinorVersion = majorMinorVersion;
      }

      /// <summary>
      /// Gets the EFVersion property.
      /// </summary>
      public EFVersion EFVersion { get; }
      /// <summary>
      /// Gets the identifier of the package.
      /// </summary>
      public string PackageId { get; }
      /// <summary>
      /// Gets the actual version of the package.
      /// </summary>
      public string ActualPackageVersion { get; }
      /// <summary>
      /// Gets the version of the item to be displayed.
      /// </summary>
      public string DisplayVersion { get; }
      /// <summary>
      /// Gets the major and minor version of the assembly.
      /// </summary>
      public string MajorMinorVersion { get; }

      /// <summary>
      /// Gets the major.minor version number.
      /// </summary>
      public double MajorMinorVersionNum
      {
         get
         {
            return double.TryParse(MajorMinorVersion, out double result)
                      ? result
                      : 0;
         }
      }
   }
}