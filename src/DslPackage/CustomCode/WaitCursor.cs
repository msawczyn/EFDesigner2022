using System;
using System.Windows.Input;

namespace Sawczyn.EFDesigner.EFModel
{
   public class WaitCursor : IDisposable
   {
      private readonly Cursor previousCursor;

      public WaitCursor()
      {
         previousCursor = Mouse.OverrideCursor;
         Mouse.OverrideCursor = Cursors.Wait;
      }

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