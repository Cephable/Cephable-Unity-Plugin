# Security Policy

## Reporting a Vulnerability

If you discover a security vulnerability in the Cephable Unity Plugin, **please do not open a public GitHub issue**. Public disclosure before a fix is available puts users at risk.

Instead, report it privately to:

**[security@cephable.com](mailto:security@cephable.com)**

Please include:

- A description of the vulnerability and its potential impact
- Steps to reproduce (or a proof of concept, if available)
- The plugin version affected (from `package.json`)
- Unity version and platform, if relevant
- Your name and contact info, so we can follow up (optional — anonymous reports are accepted)

## What to Expect

- **Acknowledgement** within 3 business days
- **Initial assessment** within 7 business days, including severity classification and a rough remediation timeline
- **Ongoing updates** as we investigate and develop a fix
- **Coordinated disclosure** — we'll work with you on a timeline for public announcement once a fix is released
- **Credit** in the release notes for the fix, if you'd like to be named

## Scope

This security policy covers the code in this repository — the Cephable Unity Plugin and its sample scripts.

It does **not** cover:

- The Cephable backend services (`services.cephable.com`) — report those to [security@cephable.com](mailto:security@cephable.com) as well, but they're outside this repo
- The Cephable mobile or desktop apps
- Third-party Unity packages or dependencies — please report those to their respective maintainers

## What Counts as a Vulnerability

Examples we want to hear about:

- OAuth token handling flaws (leakage, improper storage, replay)
- Code injection via crafted SignalR payloads
- Unsafe deserialization of macro or command data
- Local privilege escalation through the plugin
- Insecure defaults that expose user credentials or session tokens

Examples that are generally **not** security issues:

- Bugs that crash the game without affecting credentials or other users
- Missing rate limiting on the player's own local plugin instance
- Issues that require the attacker to already have full control of the user's machine

When in doubt, send it — we'd rather review a non-issue than miss a real one.

## Safe Harbor

We will not pursue legal action against researchers who:

- Make a good-faith effort to follow this policy
- Avoid privacy violations, destruction of data, and disruption of Cephable services
- Give us reasonable time to fix the issue before public disclosure
- Don't exploit the issue beyond what's needed to demonstrate it

Thank you for helping keep Cephable and our players safe.
