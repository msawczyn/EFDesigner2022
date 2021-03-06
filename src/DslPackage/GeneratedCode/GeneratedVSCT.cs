using System;

namespace Sawczyn.EFDesigner.EFModel.DslPackage.GeneratedCode
{
   /// <summary>
   ///    Helper class that exposes all GUIDs used across VS Package.
   /// </summary>
   internal sealed class PackageGuids
   {
      public const string guidPkgString = "56bbe1ba-aaee-4883-848f-e3c8656f8db2";

      public const string guidCmdSetString = "49f3aada-0211-457c-84e5-9925654ab8a1";

      public const string guidEditorString = "4e135186-c9c4-4b55-8959-217a3e025622";
      public static Guid guidPkg = new Guid(guidPkgString);
      public static Guid guidCmdSet = new Guid(guidCmdSetString);
      public static Guid guidEditor = new Guid(guidEditorString);
   }

   /// <summary>
   ///    Helper class that encapsulates all CommandIDs uses across VS Package.
   /// </summary>
   internal sealed class PackageIds
   {
      public const int menuidContext = 0x10000;
      public const int grpidContextMain = 0x20000;
      public const int grpidExplorerCopyPaste = 0x30000;
      public const int menuidExplorer = 0x10001;
      public const int cmdidViewExplorer = 0x0001;
   }
}