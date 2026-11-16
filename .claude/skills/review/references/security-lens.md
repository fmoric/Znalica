# Security Lens

Read this when a change touches input handling, authentication or authorization, secrets, files, URLs,
queries, serialization, code that feeds external text to a model — or, in AL, permissions, `IsolatedStorage`,
events or outbound HTTP. It sharpens the review's **Security** dimension; it does not replace the others.

## Judge each candidate the same way

1. **Source** — where does data from someone less trusted enter? A request parameter, body or header, an
   uploaded file, a URL, a queue message, a field a user can edit, an external API's response, a fetched
   page, a model's output.
2. **Sink** — where could that data do harm? A query, a shell or process call, a file path, HTML or a
   template, a redirect or a server-side fetch, a deserializer, a permission or ownership decision, a log.
3. **Guard** — what between the two should stop it: an allowlist, a bound parameter, output encoding, an
   authorization check **on the object**, a canonicalised path checked against its root, a size or type
   limit. Is it missing, bypassable, or applied after the data was already used?
4. **False-positive test — before it is a finding, try to clear it.** Can someone less trusted actually
   control the source? Does this path reach the sink? Does a framework default already guard it
   (parameterizing ORM, auto-escaping templates, CSRF middleware)? Is the "secret" a test fixture or a public
   identifier? If any answer clears it, it is not a finding — at most a 🔵 that says why it was cleared.
5. **Report** — the source and the sink as `file:line`, the missing guard in one sentence, the fix, and a
   severity from impact × reachability.

**A finding names a path from a source to a sink.** "Input is not validated" with no sink is a 🔵 at most.

## Web and API code — TypeScript, Python and similar

| Class | Source → sink | Guard to look for | Not a finding when |
|---|---|---|---|
| Injection — SQL, NoSQL, command | request data → query text or a shell | bound parameters or the ORM's binding; an argument list instead of a shell string; an allowlist for identifiers | the value is a constant or a bound parameter; the process gets a fixed argument list |
| Cross-site scripting, template injection | user text → HTML or a template | escaping at output; no raw-HTML escape hatch (`innerHTML`, `dangerouslySetInnerHTML`, a "safe" filter) on user data; user text is never the template itself | an auto-escaping template with no raw marker; text set as text, not markup |
| Broken access control — object ids | an id from the request → an object lookup | an ownership or tenant check on the object, not only a login check on the route | the object is public by design; the query is already scoped to the caller's user or tenant |
| Mass assignment | a request body → a model update | an explicit list of the fields a caller may set | the body binds to a schema that holds only those fields |
| Server-side request forgery, open redirect | a user-supplied URL → a server-side fetch or a redirect | a host and scheme allowlist, checked after resolving; redirects not followed into internal addresses | the URL comes from configuration the team controls; the redirect is a relative path from a fixed list |
| Path traversal, unsafe upload | a filename or path → the file system | canonicalise, then check it is under the intended root; server-generated names; type and size limits | the path is built from a server-generated id |
| Unsafe deserialization | bytes from outside → a loader that can build arbitrary objects or run code | a data-only loader and a schema | the data is internal and signed |
| Tokens and sessions | a token → the caller's identity | signature verified with a pinned algorithm, expiry checked, the key outside the code | an obviously fake key in a test |
| Secrets in code or logs | a secret → source, a log line, an error message | a secret store or environment variable; redaction | a placeholder, or an example file with names only |
| Race conditions | concurrent requests → check-then-act | a transaction, a lock or an idempotency key | there is only ever one writer on this path |
| Prompt injection | a fetched page, issue or email → a model that holds tools | external text treated as data; no tool authority taken from it; confirmation before an outward act | the text never reaches a model that can act |

## Business Central — AL

**If you hold the knowledge server, search it first** for the class in front of you — topics such as
*IsolatedStorage*, *SecretText*, *permission set wildcard*, *InherentPermissions*, *IntegrationEvent
secrets*, *RecordRef caller table*, *user-configurable URL*, *CommitBehavior* — and cite the article you
used. Without it, apply the checks below and say the knowledge base was not consulted.

| Class | Look for | Not a finding when |
|---|---|---|
| Secrets | a credential, key or token in a table field, a plain `Text` variable, a label or an event parameter — it belongs in `SecretText`, stored in `IsolatedStorage` | the value is a public identifier; a test codeunit with a fake value |
| Who can reach the secret | a **public** procedure that reads or writes `IsolatedStorage`. `Access = Internal` is API hygiene, not an authorization check | the procedure is `local` or `internal` and its callers are checked |
| Permissions | a wildcard grant in a permission set; rights beyond what the user's role is entitled to; `InherentPermissions` wider than the procedure needs; direct table rights where code-mediated (indirect) access was meant | a permission set used only by tests |
| Event surface | an `IntegrationEvent` that passes a secret, or exposes a security guard as a `var` parameter a subscriber can flip | the parameter decides nothing about security |
| Dynamic table access | a public procedure that opens a `RecordRef` on a table number the caller supplies | the table number is fixed or checked against a list |
| Outbound HTTP | a URL read from a table field and called unchecked; a response from an unauthenticated endpoint used without validation; an API key where the service offers OAuth2 | the URL is a constant in code |
| Transaction integrity | an operation that must be atomic while it raises events a third-party subscriber could commit inside | no event is raised inside it, or it is protected against subscriber commits |
| Output | HTML assembled from user data — AL has no built-in HTML encoder | the output is not HTML |

**Classify before you grade.** Personal data stored where it should not be — an error text written to a
table, for instance — is a **privacy** finding, not a security one; report it under its own name.
