using PhoneNumbers;

namespace GraduationProject.Services
{
    public class PhoneNumberService
    {
        public static (string, string)? IsValidPhoneNumber(string phoneNumber)
        {
            var phoneNumberUtil = PhoneNumberUtil.GetInstance();
            try
            {
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
