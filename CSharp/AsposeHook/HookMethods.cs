namespace STD.Hook
{
    [Flags]
    public enum HookMethods
    {
        Invoke = 1,
        ParseExact = 2,
        DateTimeOpGreaterThan = 4,
        StringCompare = 8,
        StringIndexOf = 16,
        XmlElementInnerText = 32
    }
}
