# RydrSafe - Technical Product Specification

## Vision

RydrSafe is a community-driven e-hailing safety platform that helps passengers verify drivers before entering a vehicle.

Users upload screenshots from ride-hailing applications (Uber, Bolt, inDrive, etc.) and the platform extracts driver and vehicle information using OCR.

The extracted details are matched against a database of previously reported drivers and vehicles.

If a driver has been flagged before, the user is warned and moderators are notified.

Images are used only for processing and are not permanently stored.

---

# Problem Statement

Passengers often have limited information about the safety history of drivers assigned through e-hailing applications.

RydrSafe provides a community-driven verification system that enables users to identify potentially dangerous drivers before a trip begins.

---

# Core Objectives

* Improve passenger safety
* Detect previously reported drivers
* Build a community-driven safety database
* Notify moderators of repeat offenders
* Provide driver risk assessments in real-time

---

# Technology Stack

## Frontend

* React 19
* TypeScript
* Vite
* React Router
* Tailwind CSS
* shadcn/ui
* TanStack Query
* React Hook Form
* Zod
* Sonner (notifications)
* Lucide React

## Backend

* ASP.NET Core 9 Web API
* C#
* Clean Architecture
* Entity Framework Core
* JWT Authentication
* SignalR
* FluentValidation

## OCR

* Tesseract OCR (Open Source)

## Database

* PostgreSQL

## Deployment

Frontend:

* Vercel or Azure Static Web Apps

Backend:

* Azure App Service

Database:

* Azure PostgreSQL

---

# System Architecture

Frontend (React + Vite)
|
v
ASP.NET Core API
|
v
OCR Service (Tesseract)
|
v
Driver Matching Engine
|
v
PostgreSQL Database

SignalR handles real-time notifications.

---

# User Roles

## Passenger

Can:

* Register account
* Login
* Upload driver screenshots
* Search drivers
* Submit reports
* View verification results

---

## Moderator

Can:

* Review reports
* View flagged drivers
* Receive notifications
* Approve reports
* Reject reports

---

## Admin

Can:

* Manage users
* Manage moderators
* View analytics
* Configure system settings

---

# Functional Requirements

## Authentication

### Register

Fields:

* Full Name
* Email
* Password

### Login

Fields:

* Email
* Password

### Security

* JWT Authentication
* Refresh Tokens
* Role-Based Authorization

---

# Driver Verification

## Upload Verification

User uploads:

* Driver Screenshot
* Driver Details Screenshot
* Vehicle Screenshot

Supported formats:

* jpg
* jpeg
* png
* webp

Maximum file size:

10MB

---

## OCR Processing

System extracts:

* Driver Name
* Vehicle Registration Number
* Vehicle Make
* Vehicle Model
* Phone Number (if visible)

The image is processed and discarded after extraction.

No image storage is required.

---

# Driver Matching

After extraction:

1. Search database by registration number
2. Search database by phone number
3. Search database by driver name

Use fuzzy matching to handle OCR mistakes.

Examples:

"Mokoena"

should match

"Mok0ena"

and

"Mokoina"

using Levenshtein Distance.

---

# Verification Response

Example:

{
"driverName": "John Smith",
"registrationNumber": "ABC123GP",
"status": "Flagged",
"riskScore": 82,
"reportCount": 4
}

---

# Risk Assessment

Risk score is calculated using:

* Number of reports
* Severity of reports
* Report frequency
* Report verification status

Status Levels:

Safe = 0-29

Under Review = 30-59

Flagged = 60-79

High Risk = 80-100

---

# Driver Reports

Users can report:

* Reckless Driving
* Harassment
* Assault
* Theft
* Fraud
* Unsafe Vehicle
* Intoxicated Driving
* Other

Report fields:

* Driver Name
* Registration Number
* Category
* Severity
* Description
* Incident Date

Severity:

* Low
* Medium
* High
* Critical

---

# Moderator Dashboard

Dashboard Features:

## Driver Management

* View Drivers
* Search Drivers
* View Risk Scores

## Reports

* View Reports
* Approve Reports
* Reject Reports
* Escalate Reports

## Notifications

Receive alerts when:

* Flagged drivers appear again
* High-risk drivers are detected
* Multiple reports are received

---

# Real-Time Notifications

Use SignalR.

Triggers:

* Driver Match Found
* Driver Flagged
* New Critical Report

Notification Example:

"Flagged driver ABC123GP was matched during verification."

---

# Database Design

## Users

Id UUID

FullName VARCHAR(255)

Email VARCHAR(255)

PasswordHash TEXT

Role VARCHAR(50)

CreatedAt TIMESTAMP

---

## Drivers

Id UUID

DriverName VARCHAR(255)

PhoneNumber VARCHAR(50)

RiskScore INTEGER

Status VARCHAR(50)

CreatedAt TIMESTAMP

UpdatedAt TIMESTAMP

---

## Vehicles

Id UUID

DriverId UUID

RegistrationNumber VARCHAR(50)

Make VARCHAR(100)

Model VARCHAR(100)

Color VARCHAR(50)

CreatedAt TIMESTAMP

---

## Reports

Id UUID

DriverId UUID

UserId UUID

Category VARCHAR(100)

Severity VARCHAR(50)

Description TEXT

Status VARCHAR(50)

CreatedAt TIMESTAMP

---

## Notifications

Id UUID

UserId UUID

Title VARCHAR(255)

Message TEXT

IsRead BOOLEAN

CreatedAt TIMESTAMP

---

# API Endpoints

## Authentication

POST /api/auth/register

POST /api/auth/login

POST /api/auth/refresh-token

---

## Verification

POST /api/verification/upload

GET /api/verification/history

---

## Drivers

GET /api/drivers

GET /api/drivers/{id}

GET /api/drivers/search

---

## Reports

POST /api/reports

GET /api/reports

GET /api/reports/{id}

PUT /api/reports/{id}/approve

PUT /api/reports/{id}/reject

---

## Notifications

GET /api/notifications

PUT /api/notifications/{id}/read

---

# Frontend Pages

## Public

* Login
* Register

## Passenger

* Dashboard
* Verify Driver
* Report Driver
* Verification History
* Profile

## Moderator

* Dashboard
* Reports
* Drivers
* Notifications

## Admin

* Dashboard
* Users
* Moderators
* Analytics

---

# Frontend Folder Structure

src/

api/

components/

features/
auth/
verification/
reports/
drivers/
notifications/

hooks/

layouts/

pages/

routes/

services/

types/

utils/

---

# Non-Functional Requirements

Performance:

* API Response < 500ms
* OCR Processing < 5 seconds

Security:

* HTTPS Only
* JWT Authentication
* Input Validation
* Audit Logging

Scalability:

* 10,000+ Drivers
* 100,000+ Reports

Accessibility:

* Responsive Design
* Mobile Friendly
* Dark Mode Support

---

# Future Enhancements

Phase 2

* Mobile App
* Panic Button
* Emergency Contacts
* AI Risk Predictions
* Driver Watchlists

Phase 3

* Ride-Hailing Integrations
* Community Safety Maps
* Predictive Safety Analytics

---

# Claude Code Instructions

Generate a production-ready monorepo containing:

/frontend
React + Vite + TypeScript

/backend
ASP.NET Core 9 Web API

/database
PostgreSQL migrations

/docs
project documentation

Use Clean Architecture, CQRS, Repository Pattern, JWT Authentication, Entity Framework Core, SignalR, PostgreSQL, and Tesseract OCR.

Generate all necessary entities, DTOs, repositories, services, API endpoints, database migrations, React pages, reusable UI components, route guards, and authentication flows.
