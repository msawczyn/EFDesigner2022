using Microsoft.VisualStudio.Modeling;

// ReSharper disable UnusedParameter.Local
// ReSharper disable ConvertIfStatementToReturnStatement

namespace Sawczyn.EFDesigner.EFModel
{
   partial class UnidirectionalAssociationBuilder
   {
      private static bool CanAcceptModelAttributeAndModelAttributeAsSourceAndTarget(ModelAttribute sourceModelAttribute, ModelAttribute targetModelAttribute)
      {
         return CanAcceptModelClassAndModelClassAsSourceAndTarget(sourceModelAttribute.ModelClass, targetModelAttribute.ModelClass);
      }

      private static bool CanAcceptModelAttributeAndModelClassAsSourceAndTarget(ModelAttribute sourceModelAttribute, ModelClass targetModelClass)
      {
         return CanAcceptModelClassAndModelClassAsSourceAndTarget(sourceModelAttribute.ModelClass, targetModelClass);
      }

      private static bool CanAcceptModelAttributeAsSource(ModelAttribute candidate)
      {
         return CanAcceptModelClassAsSource(candidate.ModelClass);
      }

      private static bool CanAcceptModelAttributeAsTarget(ModelAttribute candidate)
      {
         return CanAcceptModelClassAsTarget(candidate.ModelClass);
      }

      private static bool CanAcceptModelClassAndModelAttributeAsSourceAndTarget(ModelClass sourceModelClass, ModelAttribute targetModelAttribute)
      {
         return CanAcceptModelClassAndModelClassAsSourceAndTarget(sourceModelClass, targetModelAttribute.ModelClass);
      }

      private static bool CanAcceptModelClassAndModelClassAsSourceAndTarget(ModelClass sourceModelClass, ModelClass targetModelClass)
      {
         // valid unidirectional associations:
         // EF6 - entity to entity, entity to dependent
         // EFCore - entity to entity, entity to dependent
         // EFCore5Plus - entity to entity, entity to dependent, dependent to dependent, keyless to entity

         if (sourceModelClass == targetModelClass)
            return true;

         ModelRoot modelRoot = sourceModelClass.ModelRoot;

         System.Diagnostics.Debug.WriteLine($"Source: {sourceModelClass?.Name}, Is Entity: {sourceModelClass?.IsEntity()}, Is Dependent: {sourceModelClass?.IsDependent()}, Is DependentType: {sourceModelClass?.IsDependentType}, Is Keyless: {sourceModelClass?.IsKeyless()}, Is Keyless Type: {sourceModelClass?.IsKeylessType()}");
         System.Diagnostics.Debug.WriteLine($"Target: {targetModelClass?.Name}, Is Entity: {targetModelClass?.IsEntity()}, Is Dependent: {targetModelClass?.IsDependent()}, Is DependentType: {targetModelClass?.IsDependentType}, Is Keyless: {targetModelClass?.IsKeyless()}, Is Keyless Type: {targetModelClass?.IsKeylessType()}");
         switch ( modelRoot.EntityFrameworkVersion )
         {
            case EFVersion.EF6:
               return
                  sourceModelClass.IsEntity() && targetModelClass.IsEntity()
               || sourceModelClass.IsEntity() && targetModelClass.IsDependent();
            case EFVersion.EFCore when !modelRoot.IsEFCore5Plus:
               return
                  sourceModelClass.IsEntity() && targetModelClass.IsEntity()
               || sourceModelClass.IsEntity() && targetModelClass.IsDependent();
            case EFVersion.EFCore when modelRoot.IsEFCore5Plus:
               return
                  sourceModelClass.IsEntity() && targetModelClass.IsEntity()
               || sourceModelClass.IsEntity() && targetModelClass.IsDependent()
               || sourceModelClass.IsDependent() && targetModelClass.IsDependent()
               || sourceModelClass.IsKeyless() && targetModelClass.IsEntity();
         }

         return false;
      }

      private static bool CanAcceptModelClassAsSource(ModelClass sourceCandidate)
      {
         // valid unidirectional associations:
         // EF6 - entity to entity, entity to dependent
         // EFCore - entity to entity, entity to dependent
         // EFCore5Plus - entity to entity, entity to dependent, dependent to dependent, keyless to entity

         ModelRoot modelRoot = sourceCandidate.ModelRoot;

         switch (modelRoot.EntityFrameworkVersion)
         {
            case EFVersion.EF6:
               return sourceCandidate.IsEntity();
            case EFVersion.EFCore when !modelRoot.IsEFCore5Plus:
               return sourceCandidate.IsEntity();
            case EFVersion.EFCore when modelRoot.IsEFCore5Plus:
               return sourceCandidate.IsEntity() || sourceCandidate.IsDependentType || sourceCandidate.IsKeylessType();
         }

         return false;
      }

      private static bool CanAcceptModelClassAsTarget(ModelClass targetCandidate)
      {
         // valid unidirectional associations:
         // EF6 - entity to entity, entity to dependent
         // EFCore - entity to entity, entity to dependent
         // EFCore5Plus - entity to entity, entity to dependent, dependent to dependent, keyless to entity

         ModelRoot modelRoot = targetCandidate.ModelRoot;

         switch ( modelRoot.EntityFrameworkVersion )
         {
            case EFVersion.EF6:
               return targetCandidate.IsEntity() || targetCandidate.IsDependent();
            case EFVersion.EFCore when !modelRoot.IsEFCore5Plus:
               return targetCandidate.IsEntity() || targetCandidate.IsDependent();
            case EFVersion.EFCore when modelRoot.IsEFCore5Plus:
               return targetCandidate.IsEntity() || targetCandidate.IsDependent();
         }

         return false;
      }

      private static ElementLink ConnectModelAttributeToModelAttribute(ModelAttribute sourceAccepted, ModelAttribute targetAccepted)
      {
         return ConnectModelClassToModelClass(sourceAccepted.ModelClass, targetAccepted.ModelClass);
      }

      private static ElementLink ConnectModelAttributeToModelClass(ModelAttribute sourceAccepted, ModelClass targetAccepted)
      {
         return ConnectModelClassToModelClass(sourceAccepted.ModelClass, targetAccepted);
      }

      private static ElementLink ConnectModelClassToModelAttribute(ModelClass sourceAccepted, ModelAttribute targetAccepted)
      {
         return ConnectModelClassToModelClass(sourceAccepted, targetAccepted.ModelClass);
      }

      private static ElementLink ConnectModelClassToModelClass(ModelClass sourceAccepted, ModelClass targetAccepted)
      {
         ElementLink result = new UnidirectionalAssociation(sourceAccepted, targetAccepted);

         if (DomainClassInfo.HasNameProperty(result))
            DomainClassInfo.SetUniqueName(result);

         return result;
      }

      private static ElementLink ConnectSourceToTarget(ModelElement source, ModelElement target)
      {
         if (source is ModelAttribute sourceAttribute)
         {
            if (target is ModelAttribute targetAttribute)
               return ConnectModelAttributeToModelAttribute(sourceAttribute, targetAttribute);

            if (target is ModelClass targetClass)
               return ConnectModelAttributeToModelClass(sourceAttribute, targetClass);
         }

         if (source is ModelClass sourceClass)
         {
            if (target is ModelAttribute targetAttribute)
               return ConnectModelClassToModelAttribute(sourceClass, targetAttribute);

            if (target is ModelClass targetClass)
               return ConnectModelClassToModelClass(sourceClass, targetClass);
         }

         return null;
      }
   }
}