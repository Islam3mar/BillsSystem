<div align="center">

# 🧾 Bills System

**A billing and invoicing platform with inventory control, client credit limits, online payments via Stripe, bilingual PDF invoices, and automated payment reminders.**

[![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![EF Core](https://img.shields.io/badge/EF%20Core-10-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)
[![Stripe](https://img.shields.io/badge/Stripe-635BFF?logo=stripe&logoColor=white)](https://stripe.com/)

[**🔗 Live Demo**](https://bills.runasp.net) · [Features](#-features) · [Architecture](#-architecture) · [Getting Started](#-getting-started) · [Configuration](#%EF%B8%8F-configuration)

</div>

---

## 📖 Overview

**Bills System** is a full-stack ASP.NET Core MVC application for managing sales invoices end to end: catalog and stock, clients with credit limits, multi-payment bills, online card payments, PDF invoices, and automated email reminders for due and overdue bills.

It was built as part of my internship at **Pioneers Solutions**, with a focus on **clean architecture**, **data integrity** (concurrency, idempotency, snapshots), and **production-grade payment handling**.

## ✨ Features

### 🧮 Billing & Inventory
- Create bills with multiple line items, each with its own **value or percentage discount**, plus a bill-level discount.
- **Stock validation** on every sale; stock is deducted on creation and **restored automatically** when a bill is deleted.
- **Item snapshots**: item name, type, company, unit, and prices are copied into each bill line, so historical invoices never change when the catalog does.
- **Soft delete** for bills: they disappear from screens and reports but remain in the database.
- Full catalog management: **Companies, Units, Item Types, Categories, Items**.

### 👥 Clients & Credit Control
- Client management with **Egyptian mobile number validation** (Vodafone, Etisalat, Orange, WE prefixes).
- Per-client **maximum credit limit**, calculated over the client's total outstanding balance.
- Bills that would exceed the limit are **rejected** with a clear message, and an alert is raised.
- Clients receive an **email warning** when their debt reaches a configurable percentage of their limit (default 90%), with a cooldown to avoid spamming.

### 💳 Payments
- Record **cash** payments or let the client pay **online via Stripe Checkout**.
- Multiple partial payments per bill; paid / remaining amounts are always kept in sync.
- **Void a payment** with a reason (guarded so voiding can't push a client past their credit limit).

### 🔐 Stripe Integration
- Secure webhook endpoint (`/webhooks/stripe`) with **signature verification**.
- **Idempotent processing**: each payment is keyed by the Stripe session ID, so retried webhooks never double-record a payment.
- Handles `checkout.session.completed`, `checkout.session.async_payment_succeeded`, `charge.refunded`, and `charge.dispute.created`.
- **Refunds**: a full refund automatically voids the payment; a partial refund notifies the admin.
- **Disputes**: recorded as admin alerts.
- Currency mismatch protection: payments in an unexpected currency are never recorded with wrong amounts.

### 📄 PDF Invoices
- Professional A4 invoices generated with **QuestPDF**.
- **Arabic and English** text rendering using embedded **Cairo** fonts (Lato as the primary Latin font, Cairo as fallback).
- View in browser or download.

### ⏰ Automated Reminders
- A **BackgroundService** sends reminders before the due date and repeated overdue alerts until the bill is settled.
- **Catch-up logic**: it checks on startup and on a fixed interval, so reminders missed while the app was down are sent as soon as it returns.
- Failed sends are retried after a configurable delay and **never block** the bills behind them.
- Per-cycle email cap and automatic cleanup of old notifications.

### 📊 Dashboard, Reports & Notifications
- Dashboard with key totals: clients, items, companies, bills, all-time outstanding balance, and current-month net sales.
- **Sales report** with date filters (by bill, by item, by client) and a print-friendly layout.
- **Activity feed** for every create / update / delete / alert event across the system.

## 🏗 Architecture

The solution follows **Clean Architecture** with four projects and strict dependency direction:

```
┌──────────────────────────────────────────────┐
│  BillsSystem.Web         (MVC, Controllers,  │
│                           Views, ViewModels) │
└──────────────────────┬───────────────────────┘
                       │ depends on
┌──────────────────────▼───────────────────────┐
│  BillsSystem.Infrastructure                  │
│  EF Core · Repositories · Stripe · SMTP ·    │
│  QuestPDF · Background services              │
└──────────────────────┬───────────────────────┘
                       │ depends on
┌──────────────────────▼───────────────────────┐
│  BillsSystem.Application                     │
│  Services · DTOs · Validators · Interfaces   │
└──────────────────────┬───────────────────────┘
                       │ depends on
┌──────────────────────▼───────────────────────┐
│  BillsSystem.Domain                          │
│  Entities · Enums · Specifications ·         │
│  Repository contracts                        │
└──────────────────────────────────────────────┘
```

**Patterns & practices**

| Pattern | Where |
|---|---|
| Repository + Unit of Work | `Domain/Interfaces`, `Infrastructure/Repositories` |
| Specification | `Domain/Specifications` + `SpecificationEvaluator` for paging and filtering |
| Dependency Injection | `ApplicationServicesRegistrations`, `InfrastructureServicesRegistrations` |
| FluentValidation | One validator per input DTO (`Application/Validators`) |
| Optimistic concurrency | `RowVersion` on `Bill` and `Item`, with user-friendly conflict handling |
| Double-submit protection | `SubmissionId` on bill creation |
| Background processing | `BillReminderBackgroundService` (hosted service) |

## 🛠 Tech Stack

| Area | Technology |
|---|---|
| Framework | ASP.NET Core MVC on **.NET 10** |
| Data | Entity Framework Core 10, SQL Server |
| Validation | FluentValidation 12 |
| Payments | Stripe.net (Stripe Checkout + Webhooks) |
| Email | MailKit (SMTP) |
| PDF | QuestPDF |
| Security | Cookie authentication, BCrypt password hashing, built-in Rate Limiter |
| Frontend | Razor Views, Bootstrap, JavaScript, custom CSS (theme + print styles) |

## 🔒 Security

- **Secure by default**: a fallback authorization policy protects every endpoint unless explicitly marked `[AllowAnonymous]`.
- **Anti-forgery tokens** are validated automatically on all state-changing requests (the Stripe webhook is exempt and protected by signature verification instead).
- **Login rate limiting**: 5 attempts per minute per IP.
- **BCrypt**-hashed admin password; **HttpOnly**, **Secure** auth cookies with sliding expiration.
- Security headers: `X-Content-Type-Options`, `X-Frame-Options: DENY`, `Referrer-Policy`, `Permissions-Policy`, plus **HSTS** outside Development.
- **Data Protection keys** persisted to disk so sessions survive app pool recycles.
- Forwarded headers are restricted to configured proxies, so `X-Forwarded-For` can't be spoofed to bypass rate limiting.
- Secrets are kept out of source control (User Secrets in development).

## 📁 Project Structure

```
BillsSystem/
├── BillsSystem.Domain/
│   ├── Common/              # BaseEntity, Money, AppClock
│   ├── Entities/            # Bill, BillItem, Client, Payment, Item, Notification, ...
│   ├── Enums/               # PaymentMethod, ReminderType, DiscountType, ...
│   ├── Interfaces/          # Repository & UnitOfWork contracts
│   └── Specifications/      # Paging / filtering specifications
├── BillsSystem.Application/
│   ├── DTOs/                # Inputs, results, settings
│   ├── Interfaces/          # Service contracts
│   ├── Services/            # Bill, Client, Item, Reminder, Notification, ...
│   └── Validators/          # FluentValidation rules
├── BillsSystem.Infrastructure/
│   ├── Background/          # Reminder background service
│   ├── Data/                # DbContext, entity configurations
│   ├── Email/               # SMTP email service
│   ├── Migrations/
│   ├── Payments/            # Stripe Checkout service
│   ├── Pdf/                 # QuestPDF invoice + embedded Cairo fonts
│   └── Repositories/
└── BillsSystem.Web/
    ├── Controllers/         # Bills, Clients, Items, Reports, Stripe webhook, ...
    ├── Views/
    ├── ViewModels/
    └── wwwroot/
```

## 🚀 Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, Express, or full)
- *(Optional)* A [Stripe](https://stripe.com) account in test mode and the [Stripe CLI](https://stripe.com/docs/stripe-cli) for online payments
- *(Optional)* An SMTP account (e.g. Gmail app password) for reminders

### Run locally

```bash
# 1. Clone
git clone https://github.com/Islam3mar/BillsSystem.git
cd BillsSystem

# 2. Set your secrets (never commit these)
cd BillsSystem.Web
dotnet user-secrets set "Stripe:SecretKey" "sk_test_..."
dotnet user-secrets set "Stripe:PublishableKey" "pk_test_..."
dotnet user-secrets set "Stripe:WebhookSecret" "whsec_..."
dotnet user-secrets set "Email:Password" "<smtp-app-password>"

# 3. Run (the database is created and migrated automatically in Development)
dotnet run
```

In **Development**, the app applies migrations automatically on startup. Update the connection string in `appsettings.json` if you are not using a local default SQL Server instance.

### Test Stripe webhooks locally

```bash
stripe listen --forward-to https://localhost:<port>/webhooks/stripe
```

Copy the `whsec_...` signing secret printed by the CLI into `Stripe:WebhookSecret`.

## ⚙️ Configuration

| Key | Description |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `AdminCredentials:Username` / `PasswordHash` | Admin login. The password must be stored as a **BCrypt hash** |
| `Stripe:SecretKey`, `PublishableKey`, `WebhookSecret` | Stripe API keys and webhook signing secret |
| `Stripe:Currency` | Payment currency (default `usd`) |
| `Email:Enabled`, `Host`, `Port`, `UserName`, `Password`, `FromEmail`, `FromName` | SMTP settings |
| `Reminders:Enabled` | Turn automated reminders on or off |
| `Reminders:DaysBeforeDue` | Days before the due date to send the first reminder (default `3`) |
| `Reminders:OverdueRepeatEveryDays` | Overdue reminder repeat interval (default `3`) |
| `Reminders:CheckIntervalMinutes` | How often the background service runs (default `60`) |
| `Reminders:CreditWarningPercent` | Credit usage % that triggers the client warning email (default `90`) |
| `App:PublicBaseUrl` | Public HTTPS base URL used for Stripe return links (optional) |
| `RunMigrationsOnStartup` | Set to `true` to apply migrations automatically in Production (back up first) |
| `ForwardedHeaders:KnownProxies` / `KnownNetworks` | Trusted reverse proxies |

> In production, supply secrets through environment variables or your host's secret store, not through files in the repository.

## 🧪 Core Business Rules

- A bill is rejected if any item exceeds available stock.
- A bill is rejected if it pushes the client over their credit limit.
- Payments can never be recorded twice for the same Stripe session.
- Voiding a payment is blocked if it would put the client over their credit limit.
- Deleting a bill restores stock and hides the bill without destroying its data.
- Bills without a due date never trigger reminders.

## 🗺 Screenshots

> _Add screenshots here: dashboard, create bill, bill details, PDF invoice, sales report._

```md
![Dashboard](docs/screenshots/dashboard.png)
```

## 👤 Author

**Islam Omar**, Back-End Developer (.NET)

[![LinkedIn](https://img.shields.io/badge/LinkedIn-0A66C2?logo=linkedin&logoColor=white)](https://linkedin.com/in/islam-omar-3ba867266)
[![GitHub](https://img.shields.io/badge/GitHub-181717?logo=github&logoColor=white)](https://github.com/Islam3mar)
[![Portfolio](https://img.shields.io/badge/Portfolio-1F5F7A?logoColor=white)](https://islam3mar.github.io/portfolio/)
[![Email](https://img.shields.io/badge/Email-D14836?logo=gmail&logoColor=white)](mailto:islam3marahmed@gmail.com)

Built during my internship at **Pioneers Solutions**.
