//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
//
//     Produced by Entity Framework Visual Editor v4.2.3.2
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

namespace Credit.API.Domain_RE.Models_RE
{
   public partial class Inquiries
   {
      partial void Init();

      /// <summary>
      /// Default constructor. Protected due to required properties, but present because EF needs it.
      /// </summary>
      protected Inquiries()
      {
         Init();
      }

      /// <summary>
      /// Replaces default constructor, since it's protected. Caller assumes responsibility for setting all required values before saving.
      /// </summary>
      public static Inquiries CreateInquiriesUnsafe()
      {
         return new Inquiries();
      }

      /// <summary>
      /// Public constructor with required data
      /// </summary>
      /// <param name="inquiryid"></param>
      /// <param name="creditprofileid">Foreign key for CreditProfiles.Inquiries &lt;--&gt; Inquiries.CreditProfile. </param>
      /// <param name="creditprofile"></param>
      public Inquiries(Guid inquiryid, Guid creditprofileid, global::Credit.API.Domain_RE.Models_RE.CreditProfiles creditprofile)
      {
         this.InquiryId = inquiryid;

         this.CreditProfileId = creditprofileid;

         if (creditprofile == null) throw new ArgumentNullException(nameof(creditprofile));
         this.CreditProfile = creditprofile;
         creditprofile.Inquiries.Add(this);

         Init();
      }

      /// <summary>
      /// Static create function (for use in LINQ queries, etc.)
      /// </summary>
      /// <param name="inquiryid"></param>
      /// <param name="creditprofileid">Foreign key for CreditProfiles.Inquiries &lt;--&gt; Inquiries.CreditProfile. </param>
      /// <param name="creditprofile"></param>
      public static Inquiries Create(Guid inquiryid, Guid creditprofileid, global::Credit.API.Domain_RE.Models_RE.CreditProfiles creditprofile)
      {
         return new Inquiries(inquiryid, creditprofileid, creditprofile);
      }

      /*************************************************************************
       * Properties
       *************************************************************************/

      /// <summary>
      /// Max length = 10
      /// </summary>
      [MaxLength(10)]
      [StringLength(10)]
      public string Amount { get; set; }

      /// <summary>
      /// Indexed, Required
      /// Foreign key for CreditProfiles.Inquiries &lt;--&gt; Inquiries.CreditProfile. 
      /// </summary>
      [Required]
      [System.ComponentModel.Description("Foreign key for CreditProfiles.Inquiries <--> Inquiries.CreditProfile. ")]
      public Guid CreditProfileId { get; set; }

      /// <summary>
      /// Max length = 12
      /// </summary>
      [MaxLength(12)]
      [StringLength(12)]
      public string Date { get; set; }

      /// <summary>
      /// Identity, Required
      /// </summary>
      [Key]
      [Required]
      public Guid InquiryId { get; set; }

      /// <summary>
      /// Max length = 4
      /// </summary>
      [MaxLength(4)]
      [StringLength(4)]
      public string Kob { get; set; }

      /// <summary>
      /// Max length = 10
      /// </summary>
      [MaxLength(10)]
      [StringLength(10)]
      public string SubscriberCode { get; set; }

      /// <summary>
      /// Max length = 100
      /// </summary>
      [MaxLength(100)]
      [StringLength(100)]
      public string SubscriberName { get; set; }

      /// <summary>
      /// Max length = 5
      /// </summary>
      [MaxLength(5)]
      [StringLength(5)]
      public string Terms { get; set; }

      /// <summary>
      /// Max length = 5
      /// </summary>
      [MaxLength(5)]
      [StringLength(5)]
      public string Type { get; set; }

      /*************************************************************************
       * Navigation properties
       *************************************************************************/

      /// <summary>
      /// Required
      /// </summary>
      public virtual global::Credit.API.Domain_RE.Models_RE.CreditProfiles CreditProfile { get; set; }

   }
}
