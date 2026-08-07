# Postgirl

> Standalone desktop HTTP client for API testing and web server communication analysis. BETA

---

## 📖 Overview

**Postgirl** is a Windows desktop application built with WPF and .NET, designed to help developers test APIs and inspect HTTP communication in a fully local environment.

It focuses on robustness, transparency, and predictable behavior during HTTP interactions.

---

## 🎯 Purpose

Postgirl helps developers:

- Test REST APIs
- Inspect HTTP request/response cycles
- Debug backend services
- Validate headers and payloads
- Analyze web server communication behavior

The emphasis is on control and clarity rather than cloud-driven workflows.

---

## 🔒 Fully Standalone

Postgirl operates entirely on your local machine.

| Feature | Status |
|----------|--------|
| Cloud connectivity | ❌ None |
| Telemetry | ❌ None |
| External data storage | ❌ None |
| Local-only operation | ✅ Yes |

All request configurations and data remain strictly on the user's computer.

Ideal for:

- Internal APIs
- Sensitive development environments
- Offline workflows
- Secure backend testing

---

## 🧱 Design Principles

- Robust behavior over minimalism  
- Transparent HTTP handling  
- Developer-focused workflow  
- No hidden logic  
- No unnecessary abstraction  

---

## 🚀 Current Features

- Support for common HTTP methods (GET, POST, PUT, DELETE, etc.)
- Custom header configuration
- Request body editor
- Response viewer
- Status code inspection
- Fully local execution

---

## 🛣 Roadmap

### 🔄 In Progress / Planned

- [ ] Internal variable handling
- [ ] Environment-based URL resolution
- [ ] Scripted request execution
- [ ] Automated request sequences
- [ ] Response validation scripting

---

## 🌍 Environment Concept (Planned)

Example:

```http
{{baseUrl}}/api/users
