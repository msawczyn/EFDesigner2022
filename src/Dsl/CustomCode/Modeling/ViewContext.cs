using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using Microsoft.VisualStudio.Modeling;

namespace Mexedge.VisualStudio.Modeling
{
   /// <summary>
   /// Represents the context in which a view is rendered.
   /// </summary>
   public sealed class ViewContext : IXmlSerializable
   {
      /// <summary>
      /// Initializes a new instance of the ViewContext class with specified diagram name.
      /// </summary>
      /// <param name="diagramName">The name of the diagram associated with the view context.</param>
      public ViewContext(string diagramName)
      {
         if (string.IsNullOrEmpty(diagramName))
            throw new ArgumentNullException(nameof(diagramName));

         DiagramName = diagramName;
      }

      /// <summary>
      /// Initializes a new instance of the ViewContext class with the specified diagram name and type.
      /// </summary>
      /// <param name="diagramName">The name of the diagram.</param>
      /// <param name="diagramType">The type of the diagram.</param>
      public ViewContext(string diagramName, Type diagramType)
               : this(diagramName)
      {
         DiagramType = diagramType ?? throw new ArgumentNullException(nameof(diagramType));
      }

      /// <summary>
      /// Initializes a new instance of the ViewContext class.
      /// </summary>
      /// <param name="diagramName">The name of the diagram.</param>
      /// <param name="diagramType">The type of the diagram.</param>
      /// <param name="rootElement">The root element of the diagram.</param>
      public ViewContext(string diagramName, Type diagramType, ModelElement rootElement)
               : this(diagramName)
      {
         DiagramType = diagramType;
         RootElement = rootElement;
      }

      private ViewContext() { }

      /// <summary>
      /// Gets or sets the name of the diagram.
      /// </summary>
      public string DiagramName { get; private set; }

      /// <summary>
      /// Gets or sets the type of the diagram.
      /// </summary>
      public Type DiagramType { get; private set; }

      /// <summary>
      /// Gets the root element of the model
      /// </summary>
      public ModelElement RootElement { get; }

      /// <summary>
      /// Gets the XML schema that is associated with the model file.
      /// </summary>
      /// <returns>The XML schema for the model file.</returns>
      public XmlSchema GetSchema()
      {
         throw new NotImplementedException();
      }

      /// <summary>
      /// Reads XML content from the specified XmlReader.
      /// </summary>
      /// <param name="reader">The XmlReader that reads the XML content.</param>
      public void ReadXml(XmlReader reader)
      {
         reader.MoveToContent();

         string diagramNameValue = reader.GetAttribute("diagramName");
         DiagramName = diagramNameValue;

         string diagramTypeValue = reader.GetAttribute("diagramType");
         DiagramType = Type.GetType(diagramTypeValue);
      }

      /// <summary>
      /// Writes XML data to the specified XmlWriter.
      /// </summary>
      /// <param name="writer">The XmlWriter to which to write the XML data.</param>
      public void WriteXml(XmlWriter writer)
      {
         writer.WriteStartElement("ViewContext");
         writer.WriteAttributeString("diagramName", DiagramName);

         if (DiagramType != null)
            writer.WriteAttributeString("diagramType", DiagramType.AssemblyQualifiedName);

         writer.WriteEndElement();
      }

      /// <summary>Returns a string that represents the current object.</summary>
      /// <returns>A string that represents the current object.</returns>
      public override string ToString()
      {
         using (StringWriter stringWriter = new StringWriter())
         {
            using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter))
            {
               WriteXml(xmlWriter);
            }

            return stringWriter.ToString();
         }
      }

      /// <summary>
      /// Tries to parse the specified physical view.
      /// </summary>
      /// <param name="physicalView">The physical view.</param>
      /// <param name="viewContext">The view context.</param>
      /// <returns>A boolean value indicating whether the operation was successful.</returns>
      public static bool TryParse(string physicalView, out ViewContext viewContext)
      {
         if (string.IsNullOrEmpty(physicalView))
         {
            viewContext = null;

            return false;
         }

         viewContext = new ViewContext();

         try
         {
            using (StringReader reader = new StringReader(physicalView))
            {
               using (XmlReader xmlReader = XmlReader.Create(reader))
               {
                  viewContext.ReadXml(xmlReader);
               }
            }
         }
         catch (Exception exception)
         {
            Debug.WriteLine(exception);
            viewContext = null;

            return false;
         }

         return true;
      }
   }
}