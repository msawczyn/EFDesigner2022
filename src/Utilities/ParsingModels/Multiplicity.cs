namespace ParsingModels
{
   /// <summary>
   /// Represents the possible multiplicities of a relation between entities.
   /// </summary>
   public enum Multiplicity
   {
      /// <summary>
      /// Represents an entity that can have zero or many instances.
      /// </summary>
      ZeroMany,
      /// <summary>
      /// Represents an entity that can have one instance.
      /// </summary>
      One,
      /// <summary>
      /// Represents an entity that can have zero or one instance.
      /// </summary>
      ZeroOne
   }
}