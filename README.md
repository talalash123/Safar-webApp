📖 Comprehensive Table of Contents
Executive Summary & Project Vision

Detailed Feature Specification

Technology Stack & Dependencies

Deep Dive: System Architecture & Folder Structure

Database Schema & Data Dictionary

Machine Learning Implementation Guide

API & Routing Documentation

Setup & Installation Instructions

Docker & Production Deployment

CI/CD Pipeline Configuration

Configuration Management

Testing Strategy

Security Considerations

UI/UX Theming & Design System

Troubleshooting Common Issues

Contributing Guidelines

Roadmap & Future Enhancements

License & Acknowledgements

1. Executive Summary & Project Vision
Safar (Urdu for "Journey") is a highly scalable, enterprise-grade web application engineered to modernize railway travel in Pakistan and beyond. Built on the bleeding edge of the .NET ecosystem (.NET 8.0) and utilizing a document-based NoSQL architecture via MongoDB, Safar is designed to handle thousands of concurrent booking transactions with sub-millisecond latency.

Safar represents a paradigm shift in travel software by embedding Artificial Intelligence directly into the core business logic. Using ML.NET, Safar autonomously manages ticket pricing based on supply-and-demand regression models, ensures passenger safety and comfort through algorithmic seating assignments, and handles Tier-1 customer support via a Natural Language Processing text-classification pipeline.

2. Detailed Feature Specification
2.1 Customer Facing Features
Frictionless Route Discovery: An intuitive search interface allowing users to input source, destination, and temporal parameters. Results are fetched asynchronously, displaying available trains, transit times, and dynamic pricing.

Interactive Seat Mapping: A visual representation of train bogeys. Users can select seats across three tiers:

Economy: 72 seats per bogey (Standard legroom, high density).

Business: 48 seats per bogey (Enhanced comfort, power outlets).

Executive: 30 seats per bogey (Premium reclining seats, maximum privacy).

Smart Booking Flow: Multi-step checkout process capturing essential demographics (CNIC, Age, Gender) required for the AI seating engine.

Digital Ticketing: Automatic generation of a unique PNR (e.g., SAFAR-9A8B7C) accompanied by an e-ticket featuring a scannable QR code.

Customer Dashboard: A secure portal for users to view upcoming journeys, download tickets, and initiate automated cancellations.

2.2 Administrator Capabilities
Fleet Management: Full CRUD operations for rolling stock. Admins can define new train models, assign bogey configurations, and set base operational parameters.

Dynamic Scheduling: A robust calendar management system to define operational days, layover times at intermediate stations, and segment-specific base pricing.

Manifest Generation: One-click generation of passenger manifests for train conductors, highlighting passengers requiring special assistance (seniors) or security considerations (solo females).

System Analytics: Real-time visibility into system load, total revenue generated, and active ML-driven price modifiers.

3. Technology Stack & Dependencies
The application relies on a carefully curated stack to ensure high performance, security, and developer ergonomics.

3.1 Backend & Runtime
.NET SDK (8.0.x): Cross-platform runtime and compilation.

ASP.NET Core 8.0: Web framework and HTTP pipeline.

Razor Pages: Page-based MVVM routing and server-side rendering.

C# 11.0: Primary backend programming language.

3.2 Frontend & UI/UX
Bootstrap 5.3.x: Core CSS framework, responsive grid, UI components.

jQuery 3.6.x: DOM manipulation, AJAX requests.

jQuery Validation: Client-side form validation mapping to C# Data Annotations.

Google Fonts (Inter): Primary typography.

HTML5/CSS3: Semantic structure and custom styling (site.css).

3.3 Database & ODM
MongoDB Community Server (5.0+): Primary NoSQL datastore.

MongoDB C# Driver (3.9.0): Official driver for database connectivity.

MongoDB.Bson: BSON serialization and mapping attributes.

3.4 Machine Learning & AI
Microsoft.ML (3.0.1): Core Machine Learning pipeline and context.

Microsoft.ML.FastTree: Decision tree regression models (used for Dynamic Pricing).

4. Deep Dive: System Architecture
The application follows a clean, feature-slice folder structure standard in modern Razor Page applications.

Plaintext
Safar-webApp/
├── Models/                             # Domain Entities & BSON Mappings
│   ├── Train.cs                        # Train configuration & capacities
│   ├── Schedule.cs                     # Routing, timing, and base pricing
│   ├── Booking.cs                      # Transactional records & PNRs
│   └── User.cs                         # Authentication records
├── Services/                           # Core Business & AI Logic
│   ├── DynamicPricingService.cs        # Regression ML pipeline
│   ├── SeatingArrangementService.cs    # Constraint satisfaction engine
│   └── ChatbotService.cs               # Text classification NLP engine
├── Pages/                              # Presentation Layer (UI)
│   ├── Admin/                          # Restricted Area
│   │   ├── Dashboard.cshtml            
│   │   ├── Bookings.cshtml             
│   │   ├── Schedules/                  
│   │   │   ├── Create.cshtml
│   │   │   ├── Edit.cshtml
│   │   │   └── Index.cshtml
│   │   └── Trains/                     
│   │       ├── Add.cshtml
│   │       ├── Edit.cshtml
│   │       └── Index.cshtml
│   ├── Customer/                       # Public/Authenticated User Flows
│   │   ├── Index.cshtml                # Search landing page
│   │   ├── SearchResults.cshtml        # ML Pricing integration here
│   │   ├── SelectSeats.cshtml          # ML Seating integration here
│   │   ├── Checkout.cshtml             
│   │   └── TicketConfirmation.cshtml   
│   ├── Shared/                         # Global UI Components
│   │   ├── _Layout.cshtml              
│   │   ├── _AdminLayout.cshtml         
│   │   ├── _ValidationScriptsPartial.cshtml
│   │   └── Error.cshtml                
│   ├── Index.cshtml                    
│   └── Login.cshtml                    
├── wwwroot/                            # Static Web Assets
│   ├── css/
│   │   └── site.css                    # Custom Pakistani Green theme
│   ├── js/
│   │   └── site.js                     
│   └── lib/                            # Vendor libraries (Bootstrap/jQuery)
├── Properties/
│   └── launchSettings.json             
├── appsettings.json                    # Global Configurations
├── Program.cs                          # Application Builder & DI Container
└── Dockerfile                          # Container definition
5. Database Schema & Data Dictionary
Safar leverages MongoDB's flexible schema. Core collections include:

5.1 Trains Collection
Defines the physical attributes of rolling stock.

JSON
{
  "_id": ObjectId("64a7c8f001"),
  "TrainName": "Karakoram Express",
  "TrainCode": "KK-101",
  "TotalCapacity": 450,
  "Bogies": [
    {
      "Class": "Economy",
      "SeatCount": 72,
      "BogeyId": "E1"
    },
    {
      "Class": "Business",
      "SeatCount": 48,
      "BogeyId": "B1"
    }
  ],
  "OperatingDays": ["Monday", "Wednesday", "Friday", "Sunday"],
  "IsActive": true,
  "CreatedAt": ISODate("2024-01-01T00:00:00Z")
}
5.2 Schedules Collection
Maps trains to physical routes, times, and base economic values.

JSON
{
  "_id": ObjectId("64a7d9a002"),
  "TrainId": ObjectId("64a7c8f001"),
  "Origin": "Lahore",
  "Destination": "Karachi",
  "DepartureTime": ISODate("2024-08-15T08:00:00Z"),
  "ArrivalTime": ISODate("2024-08-16T02:00:00Z"),
  "BaseFare": {
    "Economy": 1500.00,
    "Business": 3500.00,
    "Executive": 5000.00
  },
  "AvailableSeats": 450,
  "Status": "Scheduled"
}
5.3 Bookings Collection
The immutable record of a transaction and seating state.

JSON
{
  "_id": ObjectId("64b1e2c003"),
  "PNR": "SAFAR-8X9Y2Z",
  "ScheduleId": ObjectId("64a7d9a002"),
  "CustomerDetails": {
    "Name": "Ahmad Khan",
    "CNIC": "35202-1234567-1",
    "Phone": "+923001234567",
    "Age": 65,
    "Gender": "Male",
    "IsWithFamily": false
  },
  "AssignedSeats": ["E1-12A", "E1-12B"],
  "TotalAmountPaid": 3000.00,
  "PaymentStatus": "Completed",
  "PaymentMethod": "JazzCash",
  "BookingDate": ISODate("2024-08-10T14:30:00Z")
}
6. Machine Learning Implementation Guide
Safar's intelligence resides in the Services/ directory. All ML models are built native to .NET using ML.NET, meaning no external Python microservices are required.

6.1 Dynamic Pricing Engine (Regression)
Goal: Maximize revenue and ensure train occupancy by adjusting prices based on context.

Algorithm: FastTree (Gradient Boosting Decision Tree).

Inputs: Base Fare, Remaining Seats, Days Until Departure, Event/Holiday Flag.

Behavior: * High Demand + Close Departure + Holiday = Surge Pricing (e.g., +40% fare).

Low Demand + Close Departure = Clearance Discount (e.g., -30% fare to fill seats).

Integration: Injected into SearchResults.cshtml.cs. The model evaluates live state parameters during the page load to render live, optimized prices.

6.2 Smart Seating Arrangement (Constraint Logic)
Goal: Automate passenger distribution for safety, cultural norms, and comfort.

Methodology: Heuristic constraint satisfaction engine.

Execution Flow:

System queries Bookings collection for the specific ScheduleId.

Constructs a 2D map of currently occupied seats and associated demographic data.

Analyzes the new CustomerDetails from the checkout form.

Seniors (>60): Searches for proximity to other seniors, prioritizing low-traffic zones away from bogey doors.

Solo Females: Searches for proximity to other female passengers or families, strictly avoiding isolated seats next to solo males.

Families: Searches for contiguous seating blocks.

Yields the safest, most logical seat IDs dynamically.

6.3 Intent-Driven Support Chatbot (Classification)
Goal: Deflect common L1 support tickets.

Algorithm: SdcaMaximumEntropy (Stochastic Dual Coordinate Ascent).

Pipeline: User Input (Text) -> Text Featurization -> Intent Prediction (BookingHelp, CancellationHelp, PaymentHelp) -> Mapped Response.

Integration: Accessed via an asynchronous AJAX endpoint that the client-side JavaScript (site.js) polls when a user interacts with the floating chat widget.

7. API & Routing Documentation
Because Safar is built on Razor Pages, routing is inherently directory-based.

Customer Routes
GET / - Search landing page.

GET /Customer/SearchResults?origin=X&dest=Y&date=Z - Queries MongoDB for schedules, applies ML pricing via DynamicPricingService.

GET /Customer/SelectSeats?scheduleId=X - Renders bogey map. Uses SeatingArrangementService to map recommended vs taken seats.

POST /Customer/Checkout - Validates payment, inserts document into Bookings collection, decrements available seats.

GET /Customer/TicketConfirmation?pnr=X - Retrieves specific booking and renders printable e-ticket.

Admin Routes (Requires Authentication)
GET /Admin/Dashboard - Aggregates MongoDB data (total sales, passenger count) for charting.

GET /Admin/Trains/Index - Lists all rolling stock.

POST /Admin/Trains/Add - Inserts new train document.

POST /Admin/Schedules/Create - Creates a new route instance.

GET /Admin/Bookings - Global manifest viewer.

8. Setup & Installation Instructions
Follow these instructions strictly to achieve a stable development environment.

8.1 Prerequisites
.NET 8.0 SDK

MongoDB Community Server (v5.0 or higher) running locally on port 27017.

Git CLI.

8.2 Bare-Metal Local Setup
Clone the Repository

Bash
git clone https://github.com/YOUR-USERNAME/Safar-webApp.git
cd Safar-webApp
Restore NuGet Packages

Bash
dotnet restore
Validate Database Connection
Open appsettings.Development.json and ensure your connection string is correct:

JSON
"MongoDbSettings": {
  "ConnectionString": "mongodb://localhost:27017",
  "DatabaseName": "SafarDB"
}
Build and Run

Bash
dotnet build
dotnet watch run
Navigate to https://localhost:71xx in your browser.

9. Docker & Production Deployment
Safar is container-native. To deploy to a production environment (like AWS ECS, DigitalOcean Droplets, or Azure), use the provided Docker configurations.

9.1 The Dockerfile
Located in the project root:

Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Safar-webApp.csproj", "."]
RUN dotnet restore "./Safar-webApp.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Safar-webApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Safar-webApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Safar-webApp.dll"]
9.2 Docker Compose (Production Environment)
Create a docker-compose.yml on your production server:

YAML
version: '3.8'

services:
  safar-web:
    image: your-dockerhub-username/safar-webapp:latest
    restart: always
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - MongoDbSettings__ConnectionString=mongodb://safar-mongo:27017
    ports:
      - "5000:80"
    depends_on:
      - safar-mongo

  safar-mongo:
    image: mongo:6.0
    restart: always
    volumes:
      - safar_mongo_data:/data/db
    ports:
      - "27017:27017"

volumes:
  safar_mongo_data:
Run docker-compose up -d to spin up the entire isolated network. Note: In production, place an Nginx reverse proxy in front of port 5000 to handle SSL/TLS termination.

10. CI/CD Pipeline Configuration
Safar uses GitHub Actions for continuous integration. Create a file at .github/workflows/dotnet.yml:

YAML
name: .NET Core CI

on:
  push:
    branches: [ "main" ]
  pull_request:
    branches: [ "main" ]

jobs:
  build:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v3
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    - name: Restore dependencies
      run: dotnet restore
    - name: Build
      run: dotnet build --no-restore -c Release
    - name: Test
      run: dotnet test --no-build --verbosity normal
11. Configuration Management
Application settings are managed via the IConfiguration provider.

appsettings.json
JSON
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "SafarDB"
  },
  "PaymentGateway": {
    "Provider": "JazzCash",
    "MerchantId": "TEST_MERCHANT_ID",
    "Password": "TEST_PASSWORD",
    "Environment": "Sandbox"
  }
}
12. Testing Strategy
Unit Testing (xUnit): Placed in a separate SafarWebApp.Tests project. Core focus is testing the logic inside the Services/ folder (e.g., ensuring SeatingArrangementService.SuggestBestSeat() returns the correct adjacent string ID).

Integration Testing: Uses Testcontainers to spin up a temporary MongoDB instance, run actual InsertOneAsync commands, and tear the container down, ensuring the Bson mappings are structurally sound.

13. Security Considerations
NoSQL Injection: Handled natively by the MongoDB C# driver. Always use LINQ expressions (e.g., Find(x => x.Id == id)) rather than raw string queries.

Authentication: Built-in ASP.NET Core Cookie Authentication is utilized for the Admin panel.

Cross-Site Scripting (XSS): Razor Pages automatically HTML-encodes all @ outputs.

Sensitive Data: Customer CNICs and Phone Numbers should never be logged to the console or file logs. Ensure logging levels in production are set to Warning or Error.

14. UI/UX Theming & Design System
The application uses a custom design system built on Bootstrap 5, themed to represent Pakistani national colors (Green and White).

Core CSS Variables (wwwroot/css/site.css):

CSS
:root {
    --safar-primary-green: #22c55e;
    --safar-dark-green: #004d26;
    --safar-accent-gold: #eab308;
    --safar-bg-light: #f8fafc;
    --safar-text-slate: #334155;
    --safar-text-muted: #64748b;
    --font-family-base: 'Inter', sans-serif;
}

body {
    font-family: var(--font-family-base);
    background-color: var(--safar-bg-light);
    color: var(--safar-text-slate);
}

/* Primary Button Styling */
.btn-safar-primary {
    background-color: var(--safar-primary-green);
    border: none;
    color: white;
    font-weight: 600;
    border-radius: 8px;
    padding: 10px 24px;
    transition: all 0.3s ease;
}

.btn-safar-primary:hover {
    background-color: var(--safar-dark-green);
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(34, 197, 94, 0.3);
}
15. Troubleshooting Common Issues
Error: A timeout occurred after 30000ms selecting a server

Fix: Your backend cannot reach MongoDB. Ensure the MongoDB service is running locally, or if using Docker, ensure the safar-mongo container is healthy.

Error: System.DllNotFoundException: Unable to load shared library 'LdaNative'

Fix: ML.NET requires native C++ redistributables. On Windows, install the latest Visual C++ Redistributable. On Linux, run sudo apt-get install libomp-dev.

Issue: CSS/JS changes are not reflecting in the browser.

Fix: Your browser is caching the static files. Press Ctrl + F5 to hard refresh, or append asp-append-version="true" to your <script> and <link> tags in _Layout.cshtml.

16. Contributing Guidelines
Fork the repository.

Create a feature branch: git checkout -b feature/new-ml-model

Commit your changes: git commit -m "Add weather data to pricing model"

Push to the branch: git push origin feature/new-ml-model

Open a Pull Request detailing your changes.

17. Roadmap & Future Enhancements
Phase 2 (Next Quarter): Migrate ML model training from offline static arrays to a hosted background service (IHostedService) that continuously learns from live database streams.

Phase 3: Introduce a Redis caching layer for the Schedules collection to handle extremely high load during holiday surges.

Phase 4: Develop a RESTful Web API layer alongside the Razor Pages to support upcoming native Android/iOS mobile applications.

18. License & Acknowledgements
This project is licensed under the MIT License. You are free to use, modify, and distribute this software for educational, personal, or commercial purposes.

Acknowledgements to the open-source community, the .NET Foundation for ML.NET, and MongoDB for comprehensive driver documentation.




# Safar-webApp
🛠️ Tech Sta
ck
&

De
p
e
n
d
e
n
c
i
e
s
L
a
y
e
r
T
e
c
h
no
l
o
g
y
D
e
sc
r
i
p
t
i
o
n
B
a
c
k
e
n
d
F
r
a
m
e
w
o
r
k
.
N
ET
8
.
0
(
A
SP
.
N
E
T
C
o
r
e
)H
i
g
h-
p
e
r
f
or
m
a
n
ce
c
ro
s
s
-
p
l
a
t
f
or
m
se
r
v
e
r
f
ra
m
e
w
or
k
.
F
ro
n
t
e
n
d
E
n
g
in
e
R
a
z
o
r
P
a
g
e
s
&
H
TM
L
5
S
e
r
v
er
-
s


i
d
e
r
e
n
d
e
r
e
d 
v
i
e
w
s
o
pt
i
m
i
z
ed
w

it
h
c
u
s
t
o
m
C
S
S

wo
r
k
f
l
ow
s
.D
a
t
a
b
a
s
e
OR
M
E
n
t
i
ty
F
r
a
m
e
wo
r
k
C
o
r
e
S
t
r
o
n
gl
y
-
t
y
pe
d
S
Q
L
m
ap
p
i
n
g
s
wi
t
h
ex
a
c
t
d
e
c
i
m
a
l/
c
u
r
r
e
n
c
y
s
c
a
le
s
.
C
on
t


a
i
n
e
riz
a
ti
o
nD
o
ck
er
S
t
a
n
da
r
d
i
z
e
d
m
ul
t
i
-
st
a
g
e
co
n
ta
i
n
e
r
de
p
l
o
y

m
e
nt
.
🔧
I
n
s
t
a
l
la
ti
o
n
& L
o
c
a
l 
S
e
t
u
p
P
r
er

eq
u
i
s
it
e
s
Ma
k
e
s
u

re
y
o
u
h
a
v
e
t
h

e
fo

l
lo

w
i
n
g
i
n
s
t
a
l
l
e
d
on
y
o
u
r
m
a
ch

in
e
:
.
N
E
T
8
.
0
S



DK
D

o
c
k
e
r
D
e
s
k
t
op
(

O
pt
i
o
n
a
l
,
f
o
r
c
o
n
t
a
i
n
e
ri
z
e
d
r

un
)
I
D
E
:

V
i
su
a
l 
S
t
u
d
i
o
2
0
2
2
o
r
V
S
C
o
d

eS
t
e
p
s
t
o
R
u

nC
l
o
n

e 
t

he
R
e
p
o

s
it
o
r
y
B
a
s
h
g
i
t
c
l
o
n
e

[
h
tt
p

s:
/
/
g
i
t
h
u
b
.c
o
m
/
ta
l
a
l
as
h
1
2
3
/
Sa
f
a
r
.
g
it
]

(
ht
t
ps
:
/
/
g
it
h
u
b
.c
o
m
/
t
a
l
a
l
a
s
h
1
2
3
/
S
a
f
ar.
g
i
t
)


c


d
S
a
f
a
r


C




o
n
f
i
g
ur
e
D
at
a
b
a
s
e
C
o
n
n
e
c
t
i
o
nO
p
e
n

ap
p
se
t
t
i

ng
s
.
j
so

n
a
nd
a
d
j
u
s
t
th
e
co

nn
e
c
t
i
o
n
s
t
r
i
n
g
t
o
p
oint to your local database server:JSON"ConnectionStrings": {
 
  
  "
  DefaultConnection": "Server=YOUR_SERVER;Database=SafarDb;Trusted_Connection=True;TrustServerCertificate=True;"
}


Apply Database MigrationsRun the following commands in your terminal or Package Manager Console to set up your schemas:Bashdotnet ef database update
B
uild and Run the ProjectBashdotnet build
dotnet run --launch-profile https
O
p
en your browser and navigate to https:
//localhost:7051 (or the port specified in your console).🐳 Docker DeploymentTo build and run the application using Docker c

o


ntainers:Build the Docker ImageBashdocker build -t safar-app .




R
un th
e
C
o
nt
a
i
n
er
B
a
s
h
d
ock
er
r
u
n
-d
-
p
8
0
8
0
:
80
-
-
n
a
me
s
a
f
a
r-
b
o
o
k
in
g
-s
y
s
t
e
m 
s
a
f
a
r
-
a
p
p







A
c
c
es
s
t
h
e
s
y
s
t
e
m
vi
a
h
tt
p:
/
/
l
o
ca
lh
o
s
t
:
8
0
8
0
.
📌
R
ec
e
n
t
U
p
d
a
t
e
s
& C
o
m
m
i
t
H
i
s
t
o
r
y
F
i
x
:

S
o


lv
e
d
t
ic
k
et
c
o
n
f
i
r
m
at
i
o
n
f
a
re

am
o
un
t
a
n
d
d
a
t
a
b
a
s
e
d
ec
i
ma
l
p
re
c
is
i
o
n 
map
p
i
n
g
.E
n
h
a
n
c
e
m
e
n
t:
U
p
g
r
ad
e
d
A
d
m
in
s
i
d
eb
a
r
a
e
s
t
h
e
t
ic
s
a
n
d
r
e
s
o
l
v
e
d
C
S
S
c
o
m
p
i
l
at
io
n
d
u
ri
n
g
b
u
i
ld
i
n
g
.
D
e
s
i

gn
:
Ad
d
e
d
pr
e
m

i
u
m 
l
ay
o
u
t 
e
n
h
an



c
em
e
n
t
s
f
o
r
cu
s
to
m
e
r
r
o
u
t
in
g
w


or
kflows.Initial Release: Completed Safar Premium UI design core implementation."""with open("README.md", "w", encoding="utf-8") as f:f.write(readme_content)print("README.md successfully created.")
