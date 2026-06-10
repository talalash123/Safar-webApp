#  Safar | Smart Railway Intelligence Ecosystem

> Safar (Urdu for "Journey") is a highly scalable, enterprise-grade web application engineered to modernize railway travel. Built on the bleeding edge of the .NET ecosystem and utilizing a document-based NoSQL architecture, Safar is designed to handle thousands of concurrent booking transactions with sub-millisecond latency.

Safar represents a paradigm shift in travel software by embedding Artificial Intelligence directly into the core business logic using ML.NET. It autonomously manages ticket pricing based on supply-and-demand regression models, ensures passenger safety through algorithmic seating assignments, and handles Tier-1 customer support via a Natural Language Processing text-classification pipeline.

---

## 📖 Table of Contents
* [Detailed Feature Specification](#-detailed-feature-specification)
* [Technology Stack & Dependencies](#-technology-stack--dependencies)
* [System Architecture & Folder Structure](#-system-architecture--folder-structure)
* [Database Schema](#-database-schema)
* [Machine Learning Implementation](#-machine-learning-implementation)
* [API & Routing](#-api--routing)
* [Installation & Local Setup](#-installation--local-setup)
* [Docker Deployment](#-docker-deployment)
* [CI/CD Pipeline](#-cicd-pipeline)
* [Security & UI/UX](#-security--uiux)
* [Recent Updates](#-recent-updates)
* [Contributors & License](#-contributors--license)

---

##  Detailed Feature Specification

### Customer-Facing Features
* **Frictionless Route Discovery:** Search interface with asynchronous fetching for available trains, transit times, and dynamic pricing.
* **Interactive Seat Mapping:** Visual representation of train bogeys.
  * *Economy:* 72 seats (Standard legroom, high density).
  * *Business:* 48 seats (Enhanced comfort, power outlets).
  * *Executive:* 30 seats (Premium reclining seats, maximum privacy).
* **Smart Booking Flow:** Multi-step checkout capturing demographics required for the AI seating engine.
* **Digital Ticketing:** Automatic PNR generation (e.g., SAFAR-9A8B7C) with a scannable QR code e-ticket.
* **Customer Dashboard:** Secure portal to view journeys, download tickets, and initiate cancellations.

### Administrator Capabilities
* **Fleet Management:** Full CRUD operations for rolling stock, bogey configurations, and operational parameters.
* **Dynamic Scheduling:** Calendar management for operational days, layover times, and segment pricing.
* **Manifest Generation:** One-click passenger manifests highlighting special assistance (seniors) or security considerations (solo females).
* **System Analytics:** Real-time visibility into system load, revenue, and active ML modifiers.

---

##  Technology Stack & Dependencies

| Layer | Technology | Description |
| :--- | :--- | :--- |
| **Backend** | .NET 8.0 (ASP.NET Core) | High-performance cross-platform server framework. C# 11.0. |
| **Frontend** | Razor Pages, HTML5/CSS3 | Server-side rendered views, Bootstrap 5.3.x, jQuery. |
| **Database** | MongoDB & Entity Framework | Document-based NoSQL and strongly-typed SQL mappings. |
| **AI / ML** | Microsoft.ML (3.0.1) | FastTree regression models and intent classification. |
| **Deployment** | Docker | Standardized multi-stage container deployment. |

---

##  System Architecture & Folder Structure

The application follows a clean, feature-slice structure standard in modern Razor Page applications.

```text
Safar-webApp/
├── Models/              # Domain Entities & BSON Mappings (Train, Schedule, Booking, User)
├── Services/            # Core Business & AI Logic (Pricing, Seating, Chatbot)
├── Pages/               # Presentation Layer (UI)
│   ├── Admin/           # Restricted Operations (Dashboard, Fleet CRUD)
│   ├── Customer/        # Public/Authenticated User Flows (Search, Checkout, Ticket)
│   └── Shared/          # Global UI Components (_Layout, Error handling)
├── wwwroot/             # Static Web Assets (Custom theme, Inter font, vendor libs)
├── appsettings.json     # Global Configurations
└── Program.cs           # Application Builder & DI Container
Machine Learning Implementation
Safar's intelligence resides native to .NET (no external Python microservices required).

Dynamic Pricing Engine (Regression): Uses FastTree to maximize revenue. Factors in base fare, remaining seats, days until departure, and holidays to apply Surge Pricing or Clearance Discounts.

Smart Seating Arrangement: A heuristic constraint satisfaction engine that distributes passengers for safety and comfort (e.g., grouping seniors away from doors, protecting solo female travelers, and blocking contiguous seats for families).

Intent-Driven Support Chatbot: Uses SdcaMaximumEntropy to predict user intent (BookingHelp, CancellationHelp) and map responses, deflecting L1 support tickets.

 API & Routing
GET /Customer/SearchResults - Queries MongoDB, applies ML pricing via DynamicPricingService.

GET /Customer/SelectSeats - Renders bogey map via SeatingArrangementService.

POST /Customer/Checkout - Validates payment, inserts document, decrements seats.

GET /Admin/Dashboard - Aggregates MongoDB data for charting (Requires Auth).

 Installation & Local Setup
Prerequisites
.NET 8.0 SDK

MongoDB Community Server (v5.0+) running on port 27017

Git CLI
CI/CD Pipeline
Safar uses GitHub Actions for continuous integration. Upon push or PR to main, the workflow (.github/workflows/dotnet.yml) automatically restores dependencies, builds in Release mode, and runs xUnit integration tests utilizing Testcontainers for isolated MongoDB instances.

Security & UI/UX
Security: Natively handles NoSQL Injection via LINQ expressions. Built-in ASP.NET Core Cookie Authentication secures the Admin panel. Razor Pages automatically HTML-encodes rendering to prevent XSS.

UI/UX: Custom design system built on Bootstrap 5, themed to represent a clean, minimalist aesthetic (Pakistani Green and White). Driven by typography (Inter font) and subtle hover transformations.

 Recent Updates & Commit History
Fix: Solved ticket confirmation fare amount and database decimal precision mapping.

Enhancement: Upgraded Admin sidebar aesthetics and resolved CSS compilation during building.

Design: Added premium layout enhancements for customer routing workflows. Initial Release of Safar Premium UI core design.
