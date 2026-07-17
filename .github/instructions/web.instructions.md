---
applyTo: "src/web"
---

- This is a Dotnet 10.0 Blazor WASM app running in Azure Static Web App with Function backend running the latest version of Bootstrap
- It is the very basis of a chat bot style app and in the starting tempalte, the chat agent simple calls the functrion app with a prompt and it respond with a greeting without any backend integration
- The product vision is the "Single Page View concept" described in `.github/copilot-instructions.md`: a single, no-scroll page filled with agent-generated content, with clickable sections that let the user drill deeper using browser-like navigation (back/forward/refresh), rather than a traditional scrolling chat transcript

## Razor / Blazor coding rules

- **Never use Razor directive keywords as variable names** in `.razor` files. The following names are reserved by the Razor compiler and will cause build errors (RZ2005, RZ9979, RZ1011) when referenced with `@` syntax in markup: `section`, `page`, `model`, `inject`, `layout`, `functions`, `code`, `inherits`, `namespace`, `using`, `implements`, `attribute`, `addTagHelper`, `removeTagHelper`, `tagHelperPrefix`. Use descriptive alternatives (e.g. `sec` instead of `section`, `item` instead of `page`).
- **Always wrap non-trivial Razor inline expressions in parentheses**: prefer `@(variable.Property)` over `@variable.Property` when the variable name could be ambiguous or resembles a directive keyword. This makes the intent explicit and avoids parser confusion across SDK versions.
- **Validate the Azure Static Web Apps build locally** by running `dotnet build src/web/web.csproj` and `dotnet publish src/web/web.csproj -c Release` before pushing. Oryx may use an earlier .NET 10 patch than your local SDK; avoid constructs that differ between patch versions.
