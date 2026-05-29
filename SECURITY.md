# Security Policy

## Supported Versions

This repository contains defensive artifacts (guards, skills, patterns) rather than runtime libraries. We support the latest content on the `main` branch.

## Reporting a Vulnerability

If you discover a security issue in any artifact, skill, or template — **do not open a public Issue**.

Instead, contact the maintainer directly:

- **Telegram**: [@svetkis](https://t.me/svetkis)
- **Channel**: [@kot_review](https://t.me/kot_review)

Please include:
- Description of the vulnerability
- Affected files or skills
- Steps to reproduce (if applicable)
- Suggested fix (optional)

## Disclosure Policy

We follow a **responsible disclosure** approach:

1. Acknowledgment within 48 hours
2. Fix or mitigation within 7 days
3. Public disclosure (with credit to the reporter) after the fix is merged

## Scope

Security issues within scope include:
- Vulnerabilities in test patterns or CI templates that could lead to unsafe defaults
- Insecure code examples in `examples/DemoProject/`
- Misleading security guidance in `skills/security-audit/`

Out of scope:
- Third-party dependencies (report to their maintainers)
- Hypothetical issues without a practical exploit path
