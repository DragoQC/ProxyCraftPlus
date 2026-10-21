---
name: modern-csharp-coding-standards
description: Write ProxyCraftPlus C# and Razor server code using explicit types, async I/O, nullable references, and the existing MVC/API/service/EF Core structure.
---

# Modern C# for ProxyCraftPlus

Read the root `AGENTS.md` and relevant existing code before editing.

- Target zero `var` in authored C# and Razor blocks. Use explicit locals, loop variables, `out` arguments, patterns, deconstruction, and lambda parameter types. Do not use anonymous types, `dynamic`, or broad `object` declarations to evade the rule.
- Keep nullable reference types enabled. Use named record DTOs and existing EF entity classes; keep `User : BaseModel` and shared UTC timestamp handling.
- Keep async I/O asynchronous and propagate cancellation. Do not block on tasks with `.Result` or `.Wait()`.
- Put authoritative behavior in `Services/`, HTTP contracts in `Api/Contracts/`, API endpoints in `Api/Controllers/`, persistence in `Data/`, and rendering in `Controllers/` and `Views/`.
- Prefer straightforward code and existing helpers. Do not introduce mediators, repositories, custom result frameworks, mapping libraries, or value-object layers without a concrete request.
- Preserve cookie authentication, CSRF checks, password hashing, rate limiting, protected 2FA secrets, atomic code consumption, and session invalidation.
- Inspect `ProxyCraftPlus.csproj`, `packages.lock.json`, and restored assets before using package APIs. Use the pinned versions; dependencies and later database schema changes need approval.
- Verify builds and run `dotnet format style --no-restore --verify-no-changes --diagnostics IDE0008`. Check Razor code and explicit lambda types manually as well.

Adapted from the modern C# guidance used for ProxyCraft, with this project's explicit-type and existing-pattern requirements taking precedence.
