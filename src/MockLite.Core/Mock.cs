using System;

namespace MockLite;

public static class Mock
{
    public static T Of<T>() where T : class
    {
        // Try generated type MockXxx in same namespace
        var ifaceName = typeof(T).Name;
        var baseNs = typeof(T).Namespace;
        var mockName = ifaceName.Length > 1 && ifaceName[0] == 'I' && char.IsUpper(ifaceName[1])
            ? $"Mock{ifaceName.Substring(1)}"
            : $"Mock{ifaceName}";
        var fullName = string.IsNullOrEmpty(baseNs) ? mockName : $"{baseNs}.{mockName}";

        var generated = Type.GetType(fullName);
        if (generated is not null) return (T)Activator.CreateInstance(generated)!;

        // Fallback: DispatchProxy-based proxy for quick use
        return RuntimeProxy.Create<T>();
    }
}
