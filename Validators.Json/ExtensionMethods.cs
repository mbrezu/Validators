namespace Validators.Json
{
    public static class ExtensionMethods
    {
        internal static string GetFullNameOrName(this Type type) => type.FullName ?? type.Name;

        private static Type? GetFirstDictionaryOfString(this Type type)
        {
            static bool isDictionaryOfString(Type type)
                => type.IsGenericType
                    && type.GetGenericTypeDefinition() == typeof(IDictionary<,>)
                    && type.GetGenericArguments().First() == typeof(string);
            return Array.Find(type.GetInterfaces(), isDictionaryOfString);
        }

        internal static bool IsDictionaryOfString(this Type type)
            => type.GetFirstDictionaryOfString() != null;

        internal static Type GetDictionaryOfStringValueType(this Type type)
            => type.GetFirstDictionaryOfString()!.GetGenericArguments().ElementAt(1);

        private static Type? GetFirstIEnumerable(this Type type)
        {
            static bool isEnumerable(Type type)
                => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
            return Array.Find(type.GetInterfaces(), isEnumerable);
        }

        internal static bool IsArray(this Type type) => type.GetFirstIEnumerable() != null;

        internal static Type GetArrayElementType(this Type type)
            => type.GetFirstIEnumerable()!.GetGenericArguments().ElementAt(0);
    }
}
