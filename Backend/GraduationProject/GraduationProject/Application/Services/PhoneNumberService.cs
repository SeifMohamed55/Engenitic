using PhoneNumbers;

namespace GraduationProject.Application.Services
{
    public class PhoneNumberService
    {
        public static (string, string)? IsValidPhoneNumber(string phoneNumber, string countryCode)
        {
            var phoneNumberUtil = PhoneNumberUtil.GetInstance();
            try
            {
                phoneNumber = string.Concat(countryCode, " ", phoneNumber);
                // Parse the phone number
                var parsedNumber = phoneNumberUtil.Parse(phoneNumber, null);

                // Validate the phone number
                if (phoneNumberUtil.IsValidNumber(parsedNumber))
                {
                    return (phoneNumberUtil.Format(parsedNumber, PhoneNumberFormat.E164), phoneNumberUtil.GetRegionCodeForCountryCode(parsedNumber.CountryCode));
                }
            }
            catch (NumberParseException)
            {
                // Parsing failed, so the phone number is invalid
            }
            return null;

        }
    }
}
