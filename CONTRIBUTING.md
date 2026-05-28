# Contributing to Dermalog

Project-wide engineering conventions. Source of truth — anything that contradicts this needs a deliberate decision and an update here.

---

## Tech Stack

### Mobile (`mobile/`)
- **Expo** + **React Native** + **TypeScript**
- **Expo Router** for file-based routing (App-Router style)
- **NativeWind** for styling (Tailwind classes on RN primitives)
- **TanStack Query** for server state
- **Zustand** or **Jotai** for client state (decide on first need; avoid Redux)
- **expo-camera** + **expo-image-picker** for photo capture
- **expo-secure-store** for tokens
- **Jest** + **React Native Testing Library**
- **pnpm** package manager
- Distribution: **EAS Build** + App Store / Play Store

### API (`api/`)
- **ASP.NET Core 9** Web API
- **Postgres** via **EF Core**
- **S3** for photo storage (pre-signed URLs for uploads)
- **AWS Bedrock** for Claude (vision comparison + journal structuring)
- **xUnit** + **Testcontainers** for integration tests
- **CSharpier** for formatting (merge-blocking on CI)
- OpenAPI generated → mobile uses it for typed client

### Infra (`terraform/`)
- **OpenTofu** for all AWS resources
- Services: RDS Postgres, S3, App Runner (or ECS — TBD), IAM roles, Secrets Manager, CloudWatch
- Single AWS account, single region (`eu-west-2` unless reason otherwise)

---

## Mobile Conventions

### Folder layout
```
mobile/
├── app/                       # Expo Router screens
│   ├── (tabs)/
│   │   ├── index.tsx          # Home
│   │   ├── capture.tsx        # New entry
│   │   └── journal.tsx        # History
│   └── _layout.tsx
├── components/
│   ├── ui/                    # Pure primitives (Button, Card, Input) — never modified inline
│   └── <feature>/             # Feature wrappers with business logic
├── features/                  # Feature modules (capture/, journal/, etc.)
│   └── capture/
│       ├── api.ts             # TanStack Query hooks
│       ├── types.ts           # Feature-defined types
│       └── components/
├── lib/                       # Cross-cutting utilities
├── hooks/                     # Cross-cutting hooks
└── generated/                 # OpenAPI types — API-layer only
```

### Component rules
- **Two-folder split**: `/components/ui` = pure primitives (no business logic, no state, no API calls); `/components/<feature>` = wrappers that add logic.
- Wrap, don't modify. If a UI primitive needs feature-specific behavior, write a wrapper.
- Kebab-case filenames. PascalCase component exports.
- Use `cn()` (or `clsx` + `tailwind-merge`) for conditional classes with NativeWind.

### State
- **Server state → TanStack Query** (`useQuery`, `useMutation`, `queryOptions`).
- **Client state → React state** until cross-screen, then Zustand/Jotai.
- Never `fetch` + `useState` for server data.

### Types
- `generated/` is API-layer only. Components consume **feature-defined types** mapped from generated types at the query boundary (via `select`).
- Prevents UI from breaking when API shapes shift.

---

## API Conventions

### Layered structure
Single ASP.NET Core project, organized by technical concern:

```
api/
├── Controllers/         # HTTP endpoints. Thin — bind, call service, translate result.
├── Services/            # Business logic. Return ServiceResult<T>.
├── Data/                # EF DbContext, Configurations, query helpers.
├── Domain/              # Entities, value objects, domain enums.
├── Models/              # DTOs (requests, responses) — never expose Domain types over HTTP.
├── Infrastructure/      # External integrations (S3 client, Bedrock client, email, etc.).
├── Validators/          # FluentValidation classes — one per request DTO.
├── Migrations/          # EF migrations checked into source.
├── Program.cs           # DI registration, middleware pipeline.
└── appsettings*.json
```

Group files inside each folder by feature when it helps (`Controllers/Photos/PhotosController.cs`), but don't force a parallel structure across folders — let it emerge.

### Services
- Return `ServiceResult<T>` (Success/Failure with reason). Never throw for expected failure cases (validation, not-found, conflicts).
- Controllers thin — translate `ServiceResult` to HTTP responses via `result.ToProblem()` extension, nothing else. No business logic in controllers.
- Rethrow with `throw;` only. Never `throw new X(..., ex)` (loses stack).

### Repositories
- One repository per aggregate root (e.g. `IPhotoRepository`, `IJournalRepository`) when there's a second consumer.
- Repositories return Domain entities, not DTOs. DTO mapping happens in services.
- Skip the repository if a service has a one-off query — call EF directly. Don't add ceremony for a single use.

### Validation
- FluentValidation, one validator per request DTO in `Validators/`.
- Auto-registered via `AddValidatorsFromAssemblyContaining<Program>()` — never manually injected or invoked.
- Runs in the MVC pipeline before the controller method; returns 400 with validation problem details automatically.
- Services do not need to re-validate inputs that go through this pipeline.

### EF Core
- `DbContext` is scoped per-request, **NOT thread-safe**. Never `Task.WhenAll` parallel queries on the same context.
- Migrations checked into source. Apply on deploy, not at runtime.
- Configure entities via `IEntityTypeConfiguration<T>` in `Data/Configurations/`, not via attributes on Domain types (keeps Domain pure).

### Cross-cutting concerns
- Logging, auth, rate limiting → middleware in `Program.cs`.
- Notifications, audit, etc. → start with direct injection (`INotificationSender`, `IAuditWriter`). Move to an event bus only when handlers proliferate enough to justify it. Don't over-engineer upfront.

### LLM integration
- All Bedrock calls go through `Infrastructure/Bedrock/` (or `Services/AI/`). One place to manage retries, prompt caching, rate limiting.
- **Prompt caching** on system prompts — saves ~90% on repeated calls.
- **Structured outputs** via tool use / JSON mode for journal parsing.
- Mobile never calls Bedrock or Anthropic directly. Never expose model identifiers or prompts in the mobile bundle.

---

## Testing

### Mobile
- Jest + React Native Testing Library for components.
- Mock the API layer at the TanStack Query boundary, not deeper.
- E2E with Maestro or Detox (decide when first needed).

### API
- xUnit for unit tests.
- **Testcontainers (Postgres) for integration tests** — never EF InMemory (different semantics from real Postgres).
- `IntegrationTestBase` pattern with a shared `WebApplicationFactory` per test class. AWS clients are mocked via `Moq`.

### Coverage
- Aim for meaningful coverage, not a number. Domain logic and service result paths first; controllers thin enough that they don't need separate tests.

---

## Security

- Bedrock credentials only in AWS (IAM role on the API runtime). Never in `.env` files committed to the repo.
- Photo S3 bucket: private. Mobile gets pre-signed URLs (15-minute TTL) from the API.
- Auth: TBD (Cognito vs. Clerk vs. roll-your-own). Decide before the first user-data slice.
- All inputs to LLM prompts sanitized — assume prompt-injection from journal text and treat LLM output as untrusted.

---

## Deployment

- Mobile: EAS Build for iOS/Android. TestFlight for internal, App Store / Play Store for prod.
- API: App Runner or ECS (Fargate) — terraform-managed. CI builds container, pushes to ECR, deploys.
- Infra: `tofu plan` in CI on PRs; `tofu apply` on merge to main (after manual approval).

---

## CI

- GitHub Actions, path-filtered workflows:
  - `mobile/**` → mobile lint/test/typecheck
  - `api/**` → dotnet build/test/csharpier
  - `terraform/**` → tofu fmt/validate/plan
- SonarCloud projects per pillar (mobile and api separate).
- Branch protection: main requires PR + green CI + 1 reviewer (or self-review for solo work).

---

## What lives where

| Concern | Mobile | API | Infra |
|---|---|---|---|
| Photo capture | ✓ |  |  |
| Photo storage |  |  | S3 (terraform) |
| Photo upload | ✓ (pre-signed URL) | issues URL |  |
| Journal text input | ✓ |  |  |
| Journal NL parsing |  | ✓ (Bedrock) |  |
| Photo comparison |  | ✓ (Bedrock) |  |
| User data |  | ✓ (Postgres) | RDS (terraform) |
| Auth tokens | secure-store | validate / issue | Cognito (terraform) |
