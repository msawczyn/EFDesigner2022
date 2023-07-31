using System.Collections.Generic;
using System.Windows.Forms;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   /// Provides extension methods for the TreeView control.
   /// </summary>
   public static class TreeViewExtensions
   {
      /// <summary>
      /// Gets all the nodes of the TreeView
      /// </summary>
      /// <param name="self">The TreeView from which the nodes are to be retrieved</param>
      /// <returns>A list of all the TreeView nodes</returns>
      public static List<TreeNode> GetAllNodes(this TreeView self)
      {
         List<TreeNode> result = new List<TreeNode>();

         foreach (TreeNode child in self.Nodes)
            result.AddRange(child.GetAllNodes());

         return result;
      }

      /// <summary>
      /// Gets all the nodes of a tree node.
      /// </summary>
      /// <param name="self">The tree node.</param>
      /// <returns>A list of all the tree nodes.</returns>
      public static List<TreeNode> GetAllNodes(this TreeNode self)
      {
         List<TreeNode> result = new List<TreeNode> { self };

         foreach (TreeNode child in self.Nodes)
            result.AddRange(child.GetAllNodes());

         return result;
      }
   }
}