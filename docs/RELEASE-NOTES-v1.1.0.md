# Cirreum.Persistence.SqlServer 1.1.0 — the credential block reaches SQL Server

## Why this release exists

The provider-credential wave gave every service-provider instance a shared `Credential` block
(whose identity acquires tokens) and hoisted `Identifier` (which Entra tenant). SQL Server was
deliberately deferred — and in the meantime its settings, which inherit from
`ServiceProviderInstanceSettings`, have been *binding* both values and ignoring them. An
operator configuring `Credential: { Mode: ManagedIdentity, IdentityId: … }` got a bare
`DefaultAzureCredential` with no tenant pinning and no identity selection — no error, no
effect. This release closes that gap and brings SQL Server into line with the Azure cohort
(Secrets, Cosmos, Storage, Service Bus, Email, SMS).

## What's new

**Credential selection.** With `UseAzureAuthentication = true`, the factory maps
`Credential.Mode` exactly as the cohort does:

| Mode | Identity used |
|------|---------------|
| `Default` (or no block) | The default chain; `IdentityId` pins the managed-identity leg |
| `ManagedIdentity` | System-assigned, or the user-assigned identity named by `IdentityId` |
| `Developer` | Visual Studio / Azure CLI / Azure PowerShell only |

An unmapped future mode throws at startup rather than silently degrading to `Default`.

**Tenant pinning.** The instance's `Identifier` names the Entra tenant across every mode.

**A contradiction guard.** A `Credential` block on an instance with
`UseAzureAuthentication = false` is rejected at registration — the block selects a token
identity, so on a connection-string-authenticated instance it could only bind and do nothing.

**One credential per factory.** The factory previously constructed a new
`DefaultAzureCredential` for *every connection it opened*, defeating Azure.Identity's internal
token caching. The credential is now created once per factory; per-connection token
acquisition is a cache read until the token nears expiry. Health checks flow through the same
factory and inherit all of the above.

## Compatibility

Fully backward compatible for existing configurations: no `Credential` block and no
`Identifier` produces the same default-chain behavior as before (now with the credential
instance reused). The two new behaviors are deliberate: a value-carrying `Credential` block now
*works*, and a contradictory one now *throws at registration* instead of binding silently.

## See also

- `docs/CHANGELOG.md` — the enumerated changes
- The provider-credential design (`CredentialMode` / `CredentialSettings` in
  `Cirreum.Providers`) — the shared taxonomy this release adopts
