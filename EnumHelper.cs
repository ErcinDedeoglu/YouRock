using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace YouRock
{
    public static class EnumHelper
    {
        //USAGE: public enum MyEnum { [Display(Name="ONE")]One, Two }
        public static string DisplayName<T>(this T enumValue) where T : IComparable, IFormattable, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("Argument must be of type Enum");
            }

            try
            {
                return enumValue.GetType() // GetType causes exception if DisplayAttribute.Name is not set
                    .GetMember(enumValue.ToString(CultureInfo.InvariantCulture))
                    .First()
                    .GetCustomAttribute<DisplayAttribute>()
                    .GetName();
            }
            catch // If there's no DisplayAttribute.Name set, just return the ToString value
            {
                return enumValue.ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}