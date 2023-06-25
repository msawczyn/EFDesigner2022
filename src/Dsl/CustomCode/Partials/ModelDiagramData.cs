using System;

namespace Sawczyn.EFDesigner.EFModel
{
   partial class ModelDiagramData
   {
      private EFModelDiagram diagram;

      /// <summary>
      /// Gets or sets a static action to open a model diagram.
      /// </summary>
      public static Action<ModelDiagramData> OpenDiagram { get; set; }
      /// <summary>
      /// Gets or sets the action to close a given EFModelDiagram.
      /// </summary>
      public static Action<EFModelDiagram> CloseDiagram { get; set; }
      /// <summary>
      /// Gets or sets the action to rename the EFModelDiagram window.
      /// </summary>
      public static Action<EFModelDiagram> RenameWindow { get; set; }

      /// <summary>
      /// Gets the EFModelDiagram object.
      /// </summary>
      /// <returns>The EFModelDiagram object.</returns>
      public EFModelDiagram GetDiagram() { return diagram; }

      /// <summary>
      /// Sets the EFModelDiagram object.
      /// </summary>
      /// <param name="d">The EFModelDiagram object to be set.</param>
      public void SetDiagram(EFModelDiagram d)
      {
         diagram = d;
      }
   }
}