namespace ParsingModels
{
   /// <summary>
   /// Represents the roles that an entity can have in an association.
   /// </summary>
   public enum AssociationRole
   {
      /// <summary>
      /// Enum representing a unset value.
      /// </summary>
      NotSet,
      /// <summary>
      /// Enum representing the principal role.
      /// </summary>
      Principal,
      /// <summary>
      /// Enum representing the dependent role.
      /// </summary>
      Dependent,
      /// <summary>
      /// Enum representing that roles are not applicable in the current association
      /// </summary>
      NotApplicable
   }
}