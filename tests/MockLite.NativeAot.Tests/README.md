# MockLite Native AOT tests

This executable test project checks generated setup, invocation verification,
factory resolution, and the error for an unregistered contract. It exits
nonzero on a failed assertion.

With the .NET SDK and Native AOT prerequisites installed, publish and run from
the repository root. This example targets Linux x64.

```sh
dotnet publish tests/MockLite.NativeAot.Tests -c Release -r linux-x64 -p:PublishAot=true -warnaserror -o /tmp/mocklite-native-aot
/tmp/mocklite-native-aot/MockLite.NativeAot.Tests
```

Publishing and executing the native binary is the validation step. A managed
build or `dotnet test` alone does not exercise Native AOT. CI runs these same
commands. The packed NuGet consumer smoke test is separate.
