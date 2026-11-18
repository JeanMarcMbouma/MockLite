# Analyzer status

MockLite 2.x currently reports these analyzer diagnostics:

- **ML002** — async returns mismatch.
- **ML003** — non-mockable class method in a recognized MockLite expression.
- **ML005** — ambiguous interface overload in a recognized MockLite expression.

**ML001** and **ML004** remain reserved diagnostic IDs. Their descriptors are retained for release-tracking compatibility, but they are not returned by `MockLiteAnalyzer.SupportedDiagnostics` and are not reported. They require proper cross-statement data-flow analysis before they can be enabled without false positives.
