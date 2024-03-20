//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
//
//     Produced by Entity Framework Visual Editor v4.2.5.1
//     Source:                    https://github.com/msawczyn/EFDesigner
//     Visual Studio Marketplace: https://marketplace.visualstudio.com/items?itemName=michaelsawczyn.EFDesigner
//     Documentation:             https://msawczyn.github.io/EFDesigner/
//     License (MIT):             https://github.com/msawczyn/EFDesigner/blob/master/LICENSE
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

namespace Ex4_ModelInvoice
{
   public partial class InvoiceHeaders
   {
      partial void Init();

      /// <summary>
      /// Default constructor
      /// </summary>
      public InvoiceHeaders()
      {
         InvoiceDetails = new System.Collections.Generic.HashSet<global::Ex4_ModelInvoice.InvoiceDetails>();

         Init();
      }

      /*************************************************************************
       * Properties
       *************************************************************************/

      /// <summary>
      /// Identity, Indexed, Required
      /// Unique identifier
      /// </summary>
      [Key]
      [Required]
      [System.ComponentModel.Description("Unique identifier")]
      public long Id { get; set; }

      public decimal? Total { get; set; }

      /*************************************************************************
       * Navigation properties
       *************************************************************************/

      public virtual ICollection<global::Ex4_ModelInvoice.InvoiceDetails> InvoiceDetails { get; private set; }

   }
}

