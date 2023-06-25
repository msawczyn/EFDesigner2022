using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.Modeling;

using Sawczyn.EFDesigner.EFModel.Extensions;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <summary>
   /// Provides element name based on a model attribute name.
   /// </summary>
   public class ModelAttributeNameProvider : ElementNameProvider
   {
      /// <summary>
      /// Sets unique name on the element given base name and list of sibling model elements.
      /// </summary>
      /// <param name="element">Element to set name for.</param>
      /// <param name="baseName">Base name from which unique name is to be derived.</param>
      /// <param name="siblings">List of sibling elements which can be used to make the name unique. The list does not include the passed element.</param>
      /// <exception cref="T:System.NotSupportedException">DomainProperty is not of type string, please override this method to set unique name on the given model element..</exception>
      protected override void CustomSetUniqueNameCore(ModelElement element, string baseName, IList<ModelElement> siblings)
      {
         base.CustomSetUniqueNameCore(element, baseName, Siblings(element as ModelAttribute));
      }

      /// <summary>Sets unique name on an element.</summary>
      /// <param name="element">Element to assign an unique name.</param>
      /// <param name="container">Container embedding the element.</param>
      /// <param name="embeddedDomainRole">Role played by the element in embedding relationship.</param>
      /// <param name="baseName">String from which generated name should be derived.</param>
      /// <exception cref="T:System.ArgumentNullException">element, container or embeddedDomainRole is a null reference.</exception>
      /// <exception cref="T:System.InvalidOperationException">When called outside of modeling transaction context,
      /// name property is calculated or other modeling constraints are not satisfied.</exception>
      /// <exception cref="T:System.NotSupportedException">There are more than <see cref="F:System.UInt64.MaxValue" /> elements in container.</exception>
      public override void SetUniqueName(ModelElement element, ModelElement container, DomainRoleInfo embeddedDomainRole, string baseName)
      {
         base.SetUniqueName(element, container, embeddedDomainRole, "Property");
      }

      // ReSharper disable once RedundantAssignment
      /// <summary>
      /// Sets the unique name of a ModelElement, given a base name and a dictionary of sibling names.
      /// </summary>
      /// <param name="element">The ModelElement to set the name for.</param>
      /// <param name="baseName">The base name to use for the new name.</param>
      /// <param name="siblingNames">A dictionary of sibling names to check against.</param>
      protected override void SetUniqueNameCore(ModelElement element, string baseName, IDictionary<string, ModelElement> siblingNames)
      {
         siblingNames = Siblings(element as ModelAttribute).GroupBy(x => (x as ModelAttribute)?.Name ?? (x as Association)?.Name)
                                                           .ToDictionary(g => g.Key, g => g.First());

         base.SetUniqueNameCore(element, baseName, siblingNames);
      }

      /// <summary>
      /// Returns a list of ModelElement objects that are siblings of the given ModelAttribute object
      /// </summary>
      /// <param name="modelAttribute">The ModelAttribute object whose siblings are to be returned</param>
      /// <returns>A list of ModelElement objects that are siblings of the given ModelAttribute object</returns>
      public static IList<ModelElement> Siblings(ModelAttribute modelAttribute)
      {
         List<ModelClass> inheritanceTree = modelAttribute.ModelClass.AllSuperclasses
                                                          .Union(modelAttribute.ModelClass.AllSubclasses)
                                                          .ToList();

         List<ModelAttribute> attributes = inheritanceTree.SelectMany(c => c.Attributes)
                                                          .Union(modelAttribute.ModelClass.AllAttributes)
                                                          .Except(new[] {modelAttribute})
                                                          .Distinct()
                                                          .ToList();

         List<Association> associations = modelAttribute.Store.GetAll<Association>()
                                                        .Where(association => inheritanceTree.Contains(association.Source))
                                                        .Distinct()
                                                        .ToList();

         return attributes.Cast<ModelElement>().Union(associations).ToList();
      }
   }
}