using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace UserInterface.Services;

internal class ValidationException : UserException
{
    public ValidationException(string message) : base(message) { }
}
internal static class DataValidator
{
    public static bool ValidatePassword(string password)
    {
        bool isValid = 
            password.Length >= 6 &&
            password.Length <= 256 &&
            password.Any(char.IsDigit) &&
            password.Any(char.IsUpper) &&
            password.Any(char.IsLower);

        if (isValid)
            return true;
        else
            throw new ValidationException("The password must be from 6 to 256 characters long and contain at least 1 digit, 1 uppercase and 1 lowercase letters.");
    }

    public static bool ValidateEmail(string email)
    {
        bool isValid = email.Length < 256 && new EmailAddressAttribute().IsValid(email);

        if (isValid)
            return true;
        else
            throw new ValidationException("Provided email is invalid.");
    }
}
