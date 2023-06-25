using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

using Microsoft.VisualStudio.Modeling;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   /// Provides helper methods for working with files dropped onto a UI element.
   /// </summary>
   public static class FileDropHelper
   {
      /// <summary>
      /// Represents a constant integer that is used when there is a bad argument count.
      /// </summary>
      public const int BAD_ARGUMENT_COUNT = 1;
      /// <summary>
      /// Constant representing the error code when an assembly cannot be loaded.
      /// </summary>
      public const int CANNOT_LOAD_ASSEMBLY = 2;
      /// <summary>
      /// Constant that represents the error code used when the program cannot write to the output file.
      /// </summary>
      public const int CANNOT_WRITE_OUTPUTFILE = 3;
      /// <summary>
      /// Constant integer value used to indicate failure in creating a database context.
      /// </summary>
      public const int CANNOT_CREATE_DBCONTEXT = 4;
      /// <summary>
      /// Represents the error code for when an appropriate constructor cannot be found.
      /// </summary>
      public const int CANNOT_FIND_APPROPRIATE_CONSTRUCTOR = 5;
      /// <summary>
      /// Represents a constant value indicating an ambiguous request.
      /// </summary>
      public const int AMBIGUOUS_REQUEST = 6;

      /// <summary>
      /// Handles multiple file drops in the Store and returns an IEnumerable of ModelElements.
      /// </summary>
      /// <param name="store">The Store object that represents the model to which the files are added.</param>
      /// <param name="filenames">The IEnumerable of file names to be added to the Store.</param>
      /// <returns>An IEnumerable of ModelElements representing the files added to the Store.</returns>
      public static IEnumerable<ModelElement> HandleMultiDrop(Store store, IEnumerable<string> filenames)
      {
         List<ModelElement> newElements = new List<ModelElement>();
         List<string> filenameList = filenames?.ToList();

         if ((store == null) || (filenameList == null))
            return newElements;

         try
         {
            StatusDisplay.Show($"Processing {filenameList.Count} files");

            AssemblyProcessor assemblyProcessor = new AssemblyProcessor(store);
            TextFileProcessor textFileProcessor = new TextFileProcessor(store);

            try
            {
               // may not work. Might not be a text file
               textFileProcessor.LoadCache(filenameList);
            }
            catch
            {
               // if not, no big deal. Either it's not a text file, or we'll just process suboptimally
            }

            foreach (string filename in filenameList)
               newElements.AddRange(Process(store, filename, assemblyProcessor, textFileProcessor));
         }
         catch (Exception e)
         {
            ErrorDisplay.Show(store, e.Message);
         }
         finally
         {
            StatusDisplay.Show("Ready");
         }

         return newElements;
      }

      private static bool IsAssembly(string filename)
      {
         try
         {
            AssemblyName.GetAssemblyName(filename);
         }
         catch (BadImageFormatException)
         {
            return false;
         }

         return true;
      }

      private static IEnumerable<ModelElement> Process(Store store, string filename, AssemblyProcessor assemblyProcessor, TextFileProcessor textFileProcessor)
      {
         List<ModelElement> newElements;
         Cursor prev = Cursor.Current;

         try
         {
            Cursor.Current = Cursors.WaitCursor;
            ModelRoot.BatchUpdating = true;

            using (Transaction tx = store.TransactionManager.BeginTransaction("Process drop"))
            {
               bool processingResult = IsAssembly(filename)
                                          ? assemblyProcessor.Process(filename, out newElements)
                                          : textFileProcessor.Process(filename, out newElements);

               if (processingResult)
               {
                  StatusDisplay.Show("Creating diagram elements. This might take a while...");
                  tx.Commit();
               }
               else
                  newElements = new List<ModelElement>();
            }
         }
         finally
         {
            Cursor.Current = prev;
            ModelRoot.BatchUpdating = false;

            StatusDisplay.Show("Ready");
         }

         return newElements;
      }
   }
}