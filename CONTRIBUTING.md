# Contributing to the Cephable Unity Plugin

Thanks for your interest in contributing! This plugin helps developers make their games accessible to millions of players who use Cephable's adaptive controls. Every contribution — code, docs, bug reports, or feature ideas — helps push that forward.

## Ways to Contribute

- **Report a bug** — open a [bug report](https://github.com/Cephable/Cephable-Unity-Plugin/issues/new?template=bug_report.md)
- **Request a feature** — open a [feature request](https://github.com/Cephable/Cephable-Unity-Plugin/issues/new?template=feature_request.md)
- **Improve documentation** — README, code comments, samples
- **Submit a pull request** — bug fixes, new features, sample scenes
- **Share your game** — built something using the plugin? Let us know at [support@cephable.com](mailto:support@cephable.com)

---

## Before You Start

For anything beyond a small fix or doc tweak, please open an issue first to discuss the change. This avoids duplicated work and helps us agree on the approach before code is written.

For security issues, **do not open a public issue** — see [SECURITY.md](SECURITY.md) for the private disclosure process.

---

## Development Setup

### Requirements

- Unity 2020.3.49f1 or later
- A [Cephable developer account](https://developers.cephable.com/) with a Client ID, Client Secret, and Device Type ID for testing
- Git

### Getting the Code

```bash
git clone https://github.com/Cephable/Cephable-Unity-Plugin.git
cd Cephable-Unity-Plugin
```

### Testing Your Changes

1. Open the `Cephable.Plugin` folder in Unity (or add it as a local package via Package Manager → Add package from disk).
2. Use the included sample scene to verify your changes against a real Cephable device.
3. Test the three command-handling paths (Unity Events, C# events, simulated input polling) if your change touches `VirtualController`.

---

## Pull Request Guidelines

### Branching

- Branch from `main`
- Use a descriptive branch name (e.g. `fix/oauth-redirect-timeout`, `feature/macro-record-mode`)

### Commit Messages

- Write a clear, present-tense summary on the first line (under 72 chars)
- Explain the *why* in the body if the change isn't self-evident
- Reference related issues (`Fixes #123`, `Refs #456`)

### Code Style

- Match the existing C# style in the codebase
- Keep public API changes minimal and document them in the README
- Don't introduce new third-party dependencies without discussing first — this plugin ships into other developers' projects, so dependency footprint matters

### Before Submitting

- [ ] Code compiles cleanly in Unity 2020.3.49f1
- [ ] You've tested against a real Cephable device (not just unit-tested in isolation)
- [ ] README is updated if you changed public API or added features
- [ ] No secrets, OAuth tokens, or personal Client IDs committed
- [ ] Sample scenes still work

### Submitting

1. Push your branch to your fork
2. Open a pull request against `main`
3. Fill out the PR template
4. A maintainer will review — please be patient, and respond to feedback when you can

---

## Reporting Bugs

Use the [bug report template](https://github.com/Cephable/Cephable-Unity-Plugin/issues/new?template=bug_report.md) and include:

- Unity version and target platform
- Plugin version (from `package.json`)
- A minimal reproduction (steps, scene setup, or repo link)
- What you expected vs. what happened
- Relevant console output (with secrets redacted)

---

## Code of Conduct

Be respectful. We're building tools that make games playable for people who can't use a standard controller — that mission attracts contributors from many backgrounds and experience levels. Assume good faith, give helpful feedback, and keep discussions focused on the work.

Harassment, discrimination, or dismissive behavior toward contributors or users will not be tolerated. Report concerns to [support@cephable.com](mailto:support@cephable.com).

---

## Questions?

- General developer questions: [developers.cephable.com](https://developers.cephable.com/)
- Plugin-specific discussion: open a [GitHub Discussion](https://github.com/Cephable/Cephable-Unity-Plugin/discussions) or issue
- Direct contact: [support@cephable.com](mailto:support@cephable.com)
