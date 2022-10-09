// 

using System;
using System.IO;

namespace ParsingModels
{
   public class Logger : IDisposable
   {
      private TextWriter _writer;

      public static Logger GetLogger(string outputFile)
      {
         return new Logger(outputFile);
      }

      private Logger(string outputFile)
      {
         try
         {
            _writer = new StreamWriter(outputFile);
         }
         catch
         {
            _writer = null;
         }
      }

      public void Debug(string msg)
      {
         Write($"[DEBUG] {msg}");
      }

      public void Info(string msg)
      {
         Write($"[INFO]  {msg}");
      }

      public void Error(string msg)
      {
         Write($"[ERROR] {msg}");
      }

      public void Warn(string msg)
      {
         Write($"[WARN]  {msg}");
      }

      public void Fatal(string msg)
      {
         Write($"[FATAL] {msg}");
      }

      private void Write(string msg)
      {
         string message = $"{DateTime.Now:O} {msg} ";
         _writer?.WriteLine(message);
         System.Diagnostics.Debug.WriteLine($"Assembly Parser: {message}");
      }

      /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
      public void Dispose()
      {
         _writer?.Flush();
         _writer?.Close();
         _writer?.Dispose();
         _writer = null;
      }
   }
}