using System;
using System.Collections.Generic;
using AutoFixture;
using AutoFixture.Kernel;

namespace BbQ.MockLite.AutoFixture;

/// <summary>Registers configurable MockLite instances with an AutoFixture fixture.</summary>
public static class FixtureMockExtensions
{
    /// <summary>Freezes a runtime mock and supplies its object for requests for <typeparamref name="T"/>.</summary>
    public static Mock<T> FreezeMock<T>(this IFixture fixture) where T : class
    {
        ArgumentNullException.ThrowIfNull(fixture);
        var customizations = fixture.Customizations;
        var existing = FindFrozenMock<T, Mock<T>>(customizations);
        if (existing is not null)
            return existing;

        var mock = Mock.Create<T>();
        customizations.Insert(0, new FrozenMock(typeof(T), typeof(Mock<T>), mock.Object, mock));
        return mock;
    }

    /// <summary>Freezes a concrete mock and supplies it for requests for either of its registered types.</summary>
    public static TMock FreezeMock<T, TMock>(this IFixture fixture)
        where T : class
        where TMock : class, T, new()
    {
        ArgumentNullException.ThrowIfNull(fixture);
        var customizations = fixture.Customizations;
        var existing = FindFrozenMock<T, TMock>(customizations);
        if (existing is not null)
            return existing;

        var mock = new TMock();
        customizations.Insert(0, new FrozenMock(typeof(T), typeof(TMock), mock, mock));
        return mock;
    }

    private static TMock? FindFrozenMock<T, TMock>(IList<ISpecimenBuilder> customizations)
        where TMock : class
    {
        int count = customizations.Count;
        for (int i = 0; i < count; i++)
        {
            if (customizations[i] is not FrozenMock frozen)
                continue;

            if (frozen.Contract == typeof(T) && frozen.HandleType == typeof(TMock))
                return (TMock)frozen.Handle;

            if (frozen.Contract == typeof(T) || frozen.HandleType == typeof(TMock) ||
                frozen.Contract == typeof(TMock) || frozen.HandleType == typeof(T))
            {
                throw new InvalidOperationException(
                    $"A different MockLite registration already uses '{typeof(T)}' or '{typeof(TMock)}' in this fixture.");
            }
        }

        return null;
    }

    private sealed class FrozenMock(Type contract, Type handleType, object dependency, object handle)
        : ISpecimenBuilder
    {
        // NoSpecimen is an immutable marker. Reuse it for unrelated fixture requests.
        private static readonly NoSpecimen Miss = new();

        public Type Contract { get; } = contract;
        public Type HandleType { get; } = handleType;
        public object Handle { get; } = handle;

        public object Create(object request, ISpecimenContext context)
        {
            if (request is SeededRequest seeded)
                request = seeded.Request;

            if (request is Type type)
            {
                if (type == Contract)
                    return dependency;
                if (type == HandleType)
                    return Handle;
            }

            return Miss;
        }
    }
}
