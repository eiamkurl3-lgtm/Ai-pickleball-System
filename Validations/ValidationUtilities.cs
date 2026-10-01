namespace AI_Based_Pickleball_Tournament_Management.Validations
{
    public static class ValidationUtilities
    {
        public static string NonEmptyMessage = "The Field {PropertyName} is Required";
        public static string MessageMaxLength = "Field {PropertyName} must be less than {MaxLength} Characters";
        public static string MinLengthMessage = "Field {PropertyName} must be at least {MinLength} Characters";
        public static string NotNullMessage = "The Field {PropertyName} is Required";
        public static string UpperCaseMessage = "firstletter must be uppercase {PropertyName}";
        public static string InvalidEmailMessage = "A valid email address is required.";
        public static string InvalidUserNameMessage = "UserName may only contain letters, digits, and the characters . _ @ -";

        public static string GreaterThanDate(DateTime value) =>
            $"The field must be greater than or equal to {value:yyyy-MM-dd}";

        public static string LessThanDate(DateTime value) =>
            $"The field must be less than or equal to {value:yyyy-MM-dd}";

        public static string GreaterThan(int value, string propertyName) =>
            $"The field {propertyName} must be greater than {value}";

        public static string GreaterThanOrEqualTo(int value, string propertyName) =>
            $"The field {propertyName} must be greater than or equal to {value}";

        public static string Between(decimal from, decimal to, string propertyName) =>
            $"The field {propertyName} must be between {from} and {to}";

        public static string OneOf(string propertyName, IEnumerable<string> allowedValues) =>
            $"Field {propertyName} must be one of: {string.Join(", ", allowedValues)}";

        public static bool FirstLetterIsUppercase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;
            var firstletter = value[0].ToString();
            return firstletter == firstletter.ToUpper();
        }
    }
}
