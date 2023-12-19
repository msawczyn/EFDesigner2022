using System.Collections.Generic;

namespace Sawczyn.EFDesigner.EFModel.EditingOnly
{
   public partial class GeneratedTextTransformation
   {
      #region Template

      // EFDesigner v4.2.7
      // Copyright (c) 2017-2024 Michael Sawczyn
      // https://github.com/msawczyn/EFDesigner

      /// <summary>
      /// A model generator for Entity Framework Core 3.x
      /// </summary>
      public class EFCore3ModelGenerator : EFCore2ModelGenerator
      {
         /// <summary>
         /// Initializes a new instance of the EFCore3ModelGenerator class.
         /// </summary>
         /// <param name="host">The instance of the text template transformation engine.</param>
         public EFCore3ModelGenerator(GeneratedTextTransformation host) : base(host) { }

         /// <summary>
         /// Writes the delete behavior for a bidirectional association's source role to the list of segments.
         /// </summary>
         /// <param name="association">The bidirectional association.</param>
         /// <param name="segments">The list of segments to add the delete behavior to.</param>
         protected override void WriteSourceDeleteBehavior(BidirectionalAssociation association, List<string> segments)
         {
            if (!association.Source.IsDependentType
             && !association.Target.IsDependentType
             && ((association.TargetRole == EndpointRole.Principal) || (association.SourceRole == EndpointRole.Principal)))
            {
               DeleteAction deleteAction = association.SourceRole == EndpointRole.Principal
                                              ? association.SourceDeleteAction
                                              : association.TargetDeleteAction;

               switch (deleteAction)
               {
                  case DeleteAction.None:
                     segments.Add("OnDelete(DeleteBehavior.NoAction)");

                     break;

                  case DeleteAction.Cascade:
                     segments.Add("OnDelete(DeleteBehavior.Cascade)");

                     break;
               }
            }
         }

         /// <summary>
         /// Writes the delete behavior for the target class of the given association into a list of segments.
         /// </summary>
         /// <param name="association">The Association object to write delete behavior for.</param>
         /// <param name="segments">The list of segments to contain the generated code.</param>
         protected override void WriteTargetDeleteBehavior(Association association, List<string> segments)
         {
            if (!association.Source.IsDependentType
             && !association.Target.IsDependentType
             && ((association.TargetRole == EndpointRole.Principal) || (association.SourceRole == EndpointRole.Principal)))
            {
               DeleteAction deleteAction = association.SourceRole == EndpointRole.Principal
                                              ? association.SourceDeleteAction
                                              : association.TargetDeleteAction;

               switch (deleteAction)
               {
                  case DeleteAction.None:
                     segments.Add("OnDelete(DeleteBehavior.NoAction)");

                     break;

                  case DeleteAction.Cascade:
                     segments.Add("OnDelete(DeleteBehavior.Cascade)");

                     break;
               }
            }
         }
      }

#endregion Template
   }
}