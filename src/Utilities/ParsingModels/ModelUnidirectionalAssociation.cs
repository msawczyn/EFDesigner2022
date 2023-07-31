using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace ParsingModels
{
   /// <summary>
   /// Represents a unidirectional association between two classes.
   /// </summary>
   [DebuggerDisplay("{SourceClassName} <--> {TargetClassName}.{TargetPropertyName}")]
   public class ModelUnidirectionalAssociation
   {
      /// <summary>
      /// Gets or sets the name of the source class.
      /// </summary>
      public string SourceClassName { get; set; }
      /// <summary>
      /// The namespace of the source class.
      /// </summary>
      public string SourceClassNamespace { get; set; }

      /// <summary>
      /// Gets full name of the source class.
      /// </summary>
      public string SourceClassFullName
      {
         get
         {
            return string.IsNullOrWhiteSpace(SourceClassNamespace)
                      ? $"global::{SourceClassName}"
                      : $"global::{SourceClassNamespace}.{SourceClassName}";
         }
      }

      /// <summary>
      /// Gets or sets the source multiplicity.
      /// </summary>
      public Multiplicity SourceMultiplicity { get; set; }
      /// <summary>
      /// Gets or sets the source role of the association
      /// </summary>
      public AssociationRole SourceRole { get; set; } = AssociationRole.NotSet;

      /// <summary>
      /// Gets or sets the name of the target class.
      /// </summary>
      public string TargetClassName { get; set; }
      /// <summary>
      /// Gets or sets the namespace of the target class.
      /// </summary>
      public string TargetClassNamespace { get; set; }

      /// <summary>
      /// Gets the full name of the target class.
      /// </summary>
      public string TargetClassFullName
      {
         get
         {
            return string.IsNullOrWhiteSpace(TargetClassNamespace)
                      ? $"global::{TargetClassName}"
                      : $"global::{TargetClassNamespace}.{TargetClassName}";
         }
      }

      /// <summary>
      /// Gets or sets the target multiplicity of the element.
      /// </summary>
      public Multiplicity TargetMultiplicity { get; set; }
      /// <summary>
      /// Gets or sets the type name of the target property.
      /// </summary>
      public string TargetPropertyTypeName { get; set; }
      /// <summary>
      /// Gets or sets the name of the target property.
      /// </summary>
      public string TargetPropertyName { get; set; }
      /// <summary>
      /// Gets or sets the summary comments for the target.
      /// </summary>
      public string TargetSummary { get; set; }
      /// <summary>
      /// Gets or sets the description comments for the target.
      /// </summary>
      public string TargetDescription { get; set; }
      /// <summary>
      /// Gets or sets the name of the declared foreign key poperty for the association, if any.
      /// </summary>
      public string ForeignKey { get; set; }
      /// <summary>
      /// Gets or sets the target role.
      /// </summary>
      public AssociationRole TargetRole { get; set; } = AssociationRole.NotSet;

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

         return Equals((ModelUnidirectionalAssociation)obj);
      }

      /// <summary>
      ///    Determines whether two instances of ModelUnidirectionalAssociation are equal.
      /// </summary>
      /// <param name="other">The ModelUnidirectionalAssociation instance to compare with the current instance.</param>
      /// <returns>true if the specified ModelUnidirectionalAssociation is equal to the current ModelUnidirectionalAssociation; otherwise, false.</returns>
      protected bool Equals(ModelUnidirectionalAssociation other)
      {
         return (SourceClassName == other.SourceClassName)
             && (SourceClassNamespace == other.SourceClassNamespace)
             && (SourceMultiplicity == other.SourceMultiplicity)
             && (SourceRole == other.SourceRole)
             && (TargetClassName == other.TargetClassName)
             && (TargetClassNamespace == other.TargetClassNamespace)
             && (TargetMultiplicity == other.TargetMultiplicity)
             && (TargetPropertyTypeName == other.TargetPropertyTypeName)
             && (TargetPropertyName == other.TargetPropertyName)
             && (TargetRole == other.TargetRole);
      }

      /// <summary>Serves as the default hash function.</summary>
      /// <returns>A hash code for the current object.</returns>
      public override int GetHashCode()
      {
         unchecked
         {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            int hashCode = SourceClassName.GetHashCode();
            hashCode = (hashCode * 397) ^ SourceClassNamespace.GetHashCode();
            hashCode = (hashCode * 397) ^ (int)SourceMultiplicity;
            hashCode = (hashCode * 397) ^ (int)SourceRole;
            hashCode = (hashCode * 397) ^ TargetClassName.GetHashCode();
            hashCode = (hashCode * 397) ^ TargetClassNamespace.GetHashCode();
            hashCode = (hashCode * 397) ^ (int)TargetMultiplicity;
            hashCode = (hashCode * 397) ^ TargetPropertyTypeName.GetHashCode();
            hashCode = (hashCode * 397) ^ TargetPropertyName.GetHashCode();
            hashCode = (hashCode * 397) ^ (int)TargetRole;
            // ReSharper restore NonReadonlyMemberInGetHashCode

            return hashCode;
         }
      }

      /// <summary>
      ///    Returns a value that indicates whether the values of two
      ///    <see cref="T:ParsingModels.ModelUnidirectionalAssociation" /> objects are equal.
      /// </summary>
      /// <param name="left">The first value to compare.</param>
      /// <param name="right">The second value to compare.</param>
      /// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
      public static bool operator ==(ModelUnidirectionalAssociation left, ModelUnidirectionalAssociation right) { return Equals(left, right); }

      /// <summary>
      ///    Returns a value that indicates whether two <see cref="T:ParsingModels.ModelUnidirectionalAssociation" />
      ///    objects have different values.
      /// </summary>
      /// <param name="left">The first value to compare.</param>
      /// <param name="right">The second value to compare.</param>
      /// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
      public static bool operator !=(ModelUnidirectionalAssociation left, ModelUnidirectionalAssociation right) { return !Equals(left, right); }
   }
}