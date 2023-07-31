using System;
using System.Windows.Input;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   /// Represents a wait cursor that can be used in a using statement to automatically dispose and restore the original cursor.
   /// </summary>
   public class WaitCursor : IDisposable
   {
      private readonly Cursor previousCursor;

      /// <summary>
      /// Initializes a new instance of the WaitCursor class.
      /// </summary>
      public WaitCursor()
      {
         previousCursor = Mouse.OverrideCursor;
         Mouse.OverrideCursor = Cursors.Wait;
      }

      /// <summary>
      /// Disposes the resources used by this object.
      /// </summary>
      public void Dispose()
      {
         Clear();
         GC.SuppressFinalize(this);
      }

      /// <summary>Restore the original cursor</summary>
      public void Clear()
      {
         Mouse.OverrideCursor = previousCursor;
      }

      /// <summary>
      /// Creates a new instance of the WaitCursor class.
      /// </summary>
      public static WaitCursor Create()
      {
         return new WaitCursor();
      }

      /// <summary>
      ///    Ensure that the cursor is cleared when the object is disposed.
      /// </summary>
      ~WaitCursor()
      {
         Dispose();
      }
   }
}