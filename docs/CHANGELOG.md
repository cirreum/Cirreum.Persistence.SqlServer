# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.1.1] - 2026-08-17

### Updated

- Updated NuGet packages.

## [1.1.0] - 2026-08-04

### Added

- **The inherited `Credential` block is honored.** The connection factory now selects the
  Entra token identity from `Credential.Mode` — `Default` (chain, with `IdentityId` pinning
  the managed-identity leg), `ManagedIdentity` (system-assigned or the user-assigned identity
  named by `IdentityId`), `Developer` (Visual Studio / Azure CLI / Azure PowerShell) — with a
  throwing discard on unmapped modes. Previously the block bound on
  `SqlServerInstanceSettings` (inherited since the provider-credential wave) and was silently
  ignored: the factory hardcoded a bare `DefaultAzureCredential`.
- **The inherited `Identifier` is honored as the Entra tenant** across every credential mode.
- **Contradiction guard at registration**: a `Credential` block on an instance with
  `UseAzureAuthentication = false` throws instead of binding and doing nothing.

### Changed

- **One credential per factory instead of one per connection.** The factory previously
  constructed a new `DefaultAzureCredential` for every connection it opened, defeating
  Azure.Identity's internal token caching; the credential is now created once, so
  per-connection token acquisition is a cache read until the token nears expiry.
- Settings, registrar, and README documentation rewritten around credential selection —
  they previously promised only a bare `DefaultAzureCredential`.

### Updated

- Updated NuGet packages (Cirreum spine 4.2.0 wave: `Cirreum.Contracts` 4.2.0 /
  `Cirreum.Domain` 4.2.0).

## [1.0.44] - 2026-07-31

### Updated

- Updated NuGet packages (Cirreum spine 4.0.1 wave: `Cirreum.Contracts` 4.0.1 / `Cirreum.Domain` 4.0.1 / `Cirreum.Kernel` 2.0.1 / `Cirreum.AuthenticationProvider` 2.0.3).

## [1.0.43] - 2026-07-30

### Updated

- Re-pinned `Cirreum.Domain` `2.0.0` → `3.0.0` — restores operation-authorization enforcement
  (the fail-open intercept fix shipped in Domain 2.0.1/3.0.0) and adopts the `IPolicyAuthorizer`
  vocabulary; see Cirreum.Domain `MIGRATION-v3.md`.

## [1.0.42] - 2026-07-29

### Updated

- Updated NuGet packages.

## [1.0.41] - 2026-07-27

### Updated

- Updated NuGet packages.

## [1.0.40] - 2026-07-24

### Updated

- Updated NuGet packages.

## [1.0.39] - 2026-07-24

### Updated

- Updated NuGet packages.

## [1.0.38] - 2026-07-20

### Updated

- Updated NuGet packages.

## [1.0.37] - 2026-07-19

### Updated

- Updated NuGet packages.

## [1.0.34] - 2026-07-04

### Updated

- Updated NuGet packages.

## [1.0.33] - 2026-07-04

### Updated

- Updated NuGet packages.

## [1.0.32] - 2026-07-04

### Updated

- Updated NuGet packages.

## [1.0.31] - 2026-05-10

### Updated

- Updated NuGet packages.

## [1.0.30] - 2026-05-07

### Updated

- Updated NuGet packages.

## [1.0.29] - 2026-05-01

### Updated

- Updated NuGet packages.
