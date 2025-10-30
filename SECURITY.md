# Security Policy

## 🔒 Security Policy

We take the security of JetMapper seriously. This document explains how to report security vulnerabilities and how they are handled.

## 📋 Supported Versions

Security updates are currently provided for the following versions:

| Version | Supported             |
| ------- | --------------------- |
| 1.2.x   | :white_check_mark:    |
| 1.1.x   | :white_check_mark:    |
| 1.0.x   | :x:                   |
| < 1.0   | :x:                   |

## 🚨 Reporting a Vulnerability

### Please do not report security vulnerabilities as public issues!

If you discover a security vulnerability, please follow these steps:

### 1. Private Disclosure

Send security issues to **mennansevim@gmail.com**.

Include in your email:

- Detailed description of the vulnerability
- Steps to reproduce the issue
- Affected versions
- Potential impact analysis
- Suggested fix, if available

### 2. Expected Response Time

- **Within 24 hours**: Initial response and acknowledgment
- **Within 48 hours**: Assessment of the issue
- **Within 7 days**: Fix plan and estimated timeline
- **Within 30 days**: Fix release (sooner for critical issues)

### 3. Coordinated Disclosure

After the security vulnerability is fixed:

1. Fix is released
2. Security advisory is published
3. Your contribution is credited (if you wish)

## 🛡️ Security Best Practices

We recommend the following security practices when using JetMapper:

### 1. Sensitive Data Handling

```csharp
// Ignore sensitive properties
var dto = user.Builder()
    .MapTo<UserDto>()
    .Ignore(d => d.Password)
    .Ignore(d => d.CreditCard)
    .Ignore(d => d.SSN)
    .Create();
```

### 2. Type Safety

```csharp
// Validate before mapping
ValidationResult validation = MappingValidator.ValidateMapping<Source, Destination>();
if (!validation.IsValid)
{
    // Handle errors
}
```

### 3. Stay Updated

```bash
# Get the latest security updates
dotnet add package JetMapper
```

### 4. Dependency Check

Regularly update all dependencies in your project:

```bash
dotnet list package --outdated
```

## 🔍 Security Checklist

Before going to production:

- [ ] Are sensitive data excluded from mapping?
- [ ] Is type validation performed?
- [ ] Is the latest JetMapper version being used?
- [ ] Do custom converters perform input validation?
- [ ] Do lifecycle hooks contain secure code?

## 📝 Security Updates

To stay informed about security updates:

1. "Watch" this repository
2. Follow the [GitHub Security Advisories](https://github.com/mennansevim/jet-mapper/security/advisories) page
3. Check [Release Notes](https://github.com/mennansevim/jet-mapper/releases)

## 🏆 Hall of Fame

Contributors who responsibly report security vulnerabilities (with their permission):

- *No reports yet*

## 📞 Contact

For security questions:

- **Email**: mennansevim@gmail.com
- **Optional**: You can send encrypted email using PGP Key

## ⚖️ Policy Changes

This security policy may be updated from time to time. Notifications will be made for significant changes.

---

**Last Updated**: October 2025

Thank you for helping us improve our security! 🙏
