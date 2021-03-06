//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Data.Entity.Spatial;

namespace Testing
{
   /// <summary>
   /// x
   /// </summary>
   [System.ComponentModel.Description("x")]
   public partial class UParentOptional: global::Testing.HiddenEntity
   {
      partial void Init();

      /// <summary>
      /// Default constructor. Protected due to required properties, but present because EF needs it.
      /// </summary>
      protected UParentOptional(): base()
      {
         PropertyInChild = "hello";
         UChildCollection = new System.Collections.ObjectModel.ObservableCollection<global::Testing.UChild>();

         Init();
      }

      /// <summary>
      /// Replaces default constructor, since it's protected. Caller assumes responsibility for setting all required values before saving.
      /// </summary>
      public static UParentOptional CreateUParentOptionalUnsafe()
      {
         return new UParentOptional();
      }

      /// <summary>
      /// Public constructor with required data
      /// </summary>
      /// <param name="uchildrequired"></param>
      public UParentOptional(global::Testing.UChild uchildrequired)
      {
         this.PropertyInChild = "hello";
         if (uchildrequired == null) throw new ArgumentNullException(nameof(uchildrequired));
         this.UChildRequired = uchildrequired;

         UChildCollection = new System.Collections.ObjectModel.ObservableCollection<global::Testing.UChild>();
         Init();
      }

      /// <summary>
      /// Static create function (for use in LINQ queries, etc.)
      /// </summary>
      /// <param name="uchildrequired"></param>
      public static UParentOptional Create(global::Testing.UChild uchildrequired)
      {
         return new UParentOptional(uchildrequired);
      }

      /*************************************************************************
       * Properties
       *************************************************************************/

      /// <summary>
      /// Default value = &quot;hello&quot;
      /// </summary>
      public string PropertyInChild { get; set; }

      /*************************************************************************
       * Navigation properties
       *************************************************************************/

      /// <summary>
      /// x
      /// </summary>
      [Description("x")]
      public virtual global::Testing.UChild UChildOptional { get; set; }

      /// <summary>
      /// h
      /// </summary>
      [Description("h")]
      public virtual ICollection<global::Testing.UChild> UChildCollection { get; private set; }

      /// <summary>
      /// Required
      /// </summary>
      public virtual global::Testing.UChild UChildRequired { get; set; }

   }
}

