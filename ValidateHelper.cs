using System.ComponentModel.DataAnnotations;

namespace YouRock
{
    public static class ValidateHelper
    {
        public static bool IsEmail(string email)
        {
            return new EmailAddressAttribute().IsValid(email);
        }

        public static object IfNullReturnStringNull(this object data)
        {
            if (data.ToString().Equals(string.Empty))
            {
                return "null";
            }

            return data;
        }
    }
}