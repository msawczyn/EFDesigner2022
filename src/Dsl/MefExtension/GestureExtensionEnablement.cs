  
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using ExtensionEnablement = global::Microsoft.VisualStudio.Modeling.Diagrams.ExtensionEnablement;
using Sawczyn.EFDesigner.EFModel.ExtensionEnablement;

namespace Sawczyn.EFDesigner.EFModel
{
	/// <summary>
	/// Partial implementation that instantiates an appropriate GestureExtensionController for this designer
	/// </summary>
	partial class EFModelDiagram
	{
		/// <summary>
		/// Instantiates a GestureExtensionController for this designer.
		/// </summary>
		/// <returns>IGestureExtension implementation pertinent to this designer</returns>
		protected override ExtensionEnablement::IGestureExtensionController CreateGestureExtensionController()
		{
			return new EFModelGestureExtensionController();
		}
	}
}

