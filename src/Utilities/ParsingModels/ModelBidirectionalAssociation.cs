using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace ParsingModels
{
   /// <summary>
   /// Represents a bidirectional association between two model elements.
   /// </summary>
   [DebuggerDisplay("{SourceClassName}.{SourcePropertyName} <--> {TargetClassName}.{TargetPropertyName}")]
   public class ModelBidirectionalAssociation : ModelUnidirectionalAssociation
   {
      /// <summary>
      /// Gets or sets the name of the type of the source property.
      /// </summary>
      public string SourcePropertyTypeName { get; set; }
      ///<summary>
      ///Gets or sets the name of the source property.
      ///</summary>
      public string SourcePropertyName { get; set; }
      /// <summary>
      /// Gets or sets the summary of the source.
      /// </summary>
      public string SourceSummary { get; set; }
      /// <summary>
      /// Gets or sets the description of the data source.
      /// </summary>
      public string SourceDescription { get; set; }
      /// <summary>
      /// Gets or sets the name of the join table, if any.
      /// </summary>
      public string JoinTableName { get; set; }

      /// <summary>
      /// Gets or sets the name of the column in the join table that references the source.
      /// </summary>
      public string End1ColumnName { get; set; }

      /// <summary>
      /// Gets or sets the name of the column in the join table that references the target.
      /// </summary>
      public string End2ColumnName { get; set; }

      /// <summary>Determines whether the specified object is equal to the current object.</summary>
      /// <param name="obj">The object to compare with the current object.</param>
      /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
      public override bool Equals(object obj)
      {
         if (ReferenceEquals(null, obj))
            return false;

         if (ReferenceEquals(this, obj))
            return true;

         // ReSharper disable once ConvertIfStatementToReturnStatement
         if (obj.GetType() != GetType())
            return false;

         return Equals((ModelBidirectionalAssociation)obj);
      }

      /// <summary>
      /// Returns a bool indicating whether this instance's values are identical to a specified ModelBidirectionalAssociation's values.
      /// </summary>
      /// <param name="other">The ModelBidirectionalAssociation instance to compare to this instance.</param>
      /// <returns>True if the values of the provided ModelBidirectionalAssociation are identical to this instance's values; otherwise, false.</returns>
      protected bool Equals(ModelBidirectionalAssociation other)
      {
         return base.Equals(other)
             && (SourcePropertyTypeName == other.SourcePropertyTypeName)
             && (SourcePropertyName == other.SourcePropertyName);
      }

      /// <summary>Serves as the default hash function.</summary>
      /// <returns>A hash code for the current object.</returns>
      public override int GetHashCode()
      {
         unchecked
         {
            int hashCode = base.GetHashCode();
            // ReSharper disable NonReadonlyMemberInGetHashCode
            hashCode = (hashCode * 397) ^ SourcePropertyTypeName.GetHashCode();
            hashCode = (hashCode * 397) ^ SourcePropertyName.GetHashCode();
            // ReSharper restore NonReadonlyMemberInGetHashCode

            return hashCode;
         }
      }

      /// <summary>
      /// Returns the inverse of the bidirectional association.
      /// </summary>
      public ModelBidirectionalAssociation Inverse()
      {
         return
            new ModelBidirectionalAssociation
            {
               SourceClassName = TargetClassName,
               SourceClassNamespace = TargetClassNamespace,
               SourceMultiplicity = TargetMultiplicity,
               SourcePropertyTypeName = TargetPropertyTypeName,
               SourcePropertyName = TargetPropertyName,
               TargetClassName = SourceClassName,
               TargetClassNamespace = SourceClassNamespace,
               TargetMultiplicity = SourceMultiplicity,
               TargetPropertyTypeName = SourcePropertyTypeName,
               TargetPropertyName = SourcePropertyName
            };
      }

      /// <summary>
      ///    Returns a value that indicates whether the values of two
      ///    <see cref="T:ParsingModels.ModelBidirectionalAssociation" /> objects are equal.
      /// </summary>
      /// <param name="left">The first value to compare.</param>
      /// <param name="right">The second value to compare.</param>
      /// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
      public static bool operator ==(ModelBidirectionalAssociation left, ModelBidirectionalAssociation right) { return Equals(left, right); }

      /// <summary>
      ///    Returns a value that indicates whether two <see cref="T:ParsingModels.ModelBidirectionalAssociation" />
      ///    objects have different values.
      /// </summary>
      /// <param name="left">The first value to compare.</param>
      /// <param name="right">The second value to compare.</param>
      /// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
      public static bool operator !=(ModelBidirectionalAssociation left, ModelBidirectionalAssociation right) { return !Equals(left, right); }
   }
}