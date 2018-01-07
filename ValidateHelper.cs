using System.ComponentModel.DataAnnotations;

namespace YouRock
{
    public class ValidateHelper
    {
        public static bool IsEmail(string email)
        {
            return new EmailAddressAttribute().IsValid(email);
        }
    }
}