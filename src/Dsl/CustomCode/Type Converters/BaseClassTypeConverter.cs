using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

using Microsoft.VisualStudio.Modeling;

namespace Sawczyn.EFDesigner.EFModel
{
   /// <inheritdoc />
   public class BaseClassTypeConverter : TypeConverterBase
   {
      /// <summary>Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.</summary>
      /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
      /// <param name="sourceType">A <see cref="T:System.Type" /> that represents the type you want to convert from. </param>
      /// <returns>
      /// <see langword="true" /> if this converter can perform the conversion; otherwise, <see langword="false" />.</returns>
      public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
      {
         return (sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType);
      }

      /// <summary>Converts the given object to the type of this converter, using the specified context and culture information.</summary>
      /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
      /// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> to use as the current culture. </param>
      /// <param name="value">The <see cref="T:System.Object" /> to convert. </param>
      /// <returns>An <see cref="T:System.Object" /> that represents the converted value.</returns>
      /// <exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
      public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
      {
         return value?.ToString();
      }

      /// <summary>Returns a collection of standard values for the data type this type converter is designed for when provided with a format context.</summary>
      /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context that can be used to extract additional information about the environment from which this converter is invoked. This parameter or properties of this parameter can be <see langword="null" />. </param>
      /// <returns>A <see cref="T:System.ComponentModel.TypeConverter.StandardValuesCollection" /> that holds a standard set of valid values, or <see langword="null" /> if the data type does not support a standard set of values.</returns>
      public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
      {
         Store store = GetStore(context.Instance);

         List<string> invalidOptions = new List<string>();

         if (context.Instance is Array shapeArray)
         {
            invalidOptions.AddRange(shapeArray.OfType<ClassShape>()
                                              .Where(s => s.Subject is ModelClass)
                                              .Select(s => (s.Subject as ModelClass).Name));
         }
         else
         {
            string targetClassName = ((context.Instance as ClassShape)?.Subject as ModelClass)?.Name;

            if (targetClassName != null)
               invalidOptions.Add(targetClassName);
         }

         List<string> validNames = store.ElementDirectory
                                        .FindElements<ModelClass>()
                                        .Where(e => !invalidOptions.Contains(e.Name))
                                        .OrderBy(c => c.Name)
                                        .Select(c => c.Name)
                                        .ToList();

         validNames.Insert(0, null);

         return new StandardValuesCollection(validNames);
      }

      /// <summary>Returns whether the collection of standard values returned from <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues" /> is an exclusive list of possible values, using the specified context.</summary>
      /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
      /// <returns>
      /// <see langword="true" /> if the <see cref="T:System.ComponentModel.TypeConverter.StandardValuesCollection" /> returned from <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues" /> is an exhaustive list of possible values; <see langword="false" /> if other values are possible.</returns>
      public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
      {
         return true;
      }

      /// <summary>Returns whether this object supports a standard set of values that can be picked from a list, using the specified context.</summary>
      /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
      /// <returns>
      /// <see langword="true" /> if <see cref="M:System.ComponentModel.TypeConverter.GetStandardValues" /> should be called to find a common set of values the object supports; otherwise, <see langword="false" />.</returns>
      public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
      {
         return true;
      }
   }
}