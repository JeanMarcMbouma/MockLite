using System;

namespace MockLite;

public sealed class VerificationException(string message) : Exception(message);