using System;

namespace MockLite;

public static class It
{
    public static T IsAny<T>() => default!;
    public static T Matches<T>(Predicate<T> predicate) => default!;
}
