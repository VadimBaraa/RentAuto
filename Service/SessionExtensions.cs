namespace RentAutoWeb.Services
{
    public static class SessionExtensions
    {
        public static void SetDouble(this ISession session, string key, double value)
        {
            session.SetString(key, value.ToString());
        }

        public static double? GetDouble(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? 0 : double.Parse(value);
        }
    }
}