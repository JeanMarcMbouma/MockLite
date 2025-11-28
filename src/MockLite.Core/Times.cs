using System;

namespace MockLite;

public static class Times
{
    public static Func<int, bool> Once => c => c == 1;
    public static Func<int, bool> Never => c => c == 0;
    public static Func<int, bool> Exactly(int n) => c => c == n;
    public static Func<int, bool> AtLeast(int n) => c => c >= n;
    public static Func<int, bool> AtMost(int n) => c => c <= n;
}

