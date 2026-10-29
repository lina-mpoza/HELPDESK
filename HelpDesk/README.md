# IT Helpdesk System

A role-based IT helpdesk application built for **O.R. Tambo District Municipality**. Staff log IT problems, the Helpdesk Manager assigns them to technicians, and technicians resolve them with two-step verification from the reporter.

Built from scratch with **ASP.NET Core (C#)** on the backend, **Microsoft SQL Server** for the database, and plain **HTML, CSS, and JavaScript** on the frontend. No JavaScript frameworks, no build step.

---

## Table of Contents

- [About](#about)
- [Features](#features)
- [Ticket Workflow](#ticket-workflow)
- [Roles and Permissions](#roles-and-permissions)
- [Project Structure](#project-structure)
- [Database](#database)
- [API Endpoints](#api-endpoints)
- [Running Locally](#running-locally)
- [Configuration](#configuration)
- [Using the App](#using-the-app)
- [Attachments](#attachments)
- [Email Sending](#email-sending)
- [Password Reset](#password-reset)
- [Known Limitations](#known-limitations)
- [Future Enhancements](#future-enhancements)
- [Credits](#credits)
- [License](#license)

---

## About

This project serves three purposes:

1. **Provide** a working IT helpdesk system for municipal staff to report and track IT problems.
2. **Enforce** a clear workflow: staff report, admin assigns, technicians fix, staff confirm, admin closes.
3. **Demonstrate** full-stack development across a C# backend, a SQL Server database, and a hand-built frontend.

The system is designed around three user roles. Each role sees a different dashboard and can only perform actions permitted to that role.

---

## Features

### General Staff

- Sign up with a username, email, and password
- Submit tickets with a description, urgency, and optional screenshot
- See all their own tickets with live statuses
- Edit their ticket while it is still Open
- Cancel their own Open tickets
- Confirm Fixed or Not Fixed when a technician marks a ticket Resolved
- Update their own profile and change their password

### Technician

- See only the tickets assigned to them
- Click Start to begin working on a ticket
- Click Resolve and write a short note on what was done
- See a personal activity timeline
- Click any ticket ID to see its full history
- Update their own profile and change their password

### Admin (Helpdesk Manager)

- See an overview of ticket counts by status
- See every ticket in the system with filters
- Assign Open tickets to technicians with instructions and a deadline
- Close tickets the staff have Confirmed
- Force-close Resolved tickets that staff have not responded to
- Reopen Closed or Cancelled tickets
- Manage Users: see all accounts, edit names and contact details, add technicians
- Add Technician form with credentials emailed to the technician
- See a full activity log of every ticket action
- Update their own profile and change their password

### Attachments

- Upload screenshots when submitting a ticket
- Thumbnails appear in every ticket list
- Click a thumbnail to see the full image

### Account Recovery

- Forgot Password flow with a 6-digit OTP sent by email
- The OTP expires after 10 minutes
- Passwords can be changed from the profile page

---

## Database

The database is called `MunicipalHelpdesk` and runs on Microsoft SQL Server. The main tables are:

- **Persons:** Login details for every user.
- **Staff:** Employee records linked to a person.
- **GeneralStaff / UserSupportStaff / ITAdmins:** Marks the specific role of a staff member.
- **IssuesTickets:** The tickets themselves.
- **TicketAssignment:** Which technician got which ticket.
- **TicketActivity:** An immutable history of every ticket action.
- **Statuses:** The list of possible statuses.

---

## Running Locally

### Prerequisites
- .NET 10 SDK or newer
- Microsoft SQL Server (Express works fine)
- Visual Studio 2026

### Setup Steps
1. Clone this repository.
2. Create the database `MunicipalHelpdesk` and run the SQL scripts to create the tables and lookup data.
3. Open `appsettings.json` and configure your local connection string (see [Configuration](#configuration)).
4. In Visual Studio, press **F5**.
5. Or from the command line: `dotnet run --project HelpDesk`

### Where to find the pages
- **Home:** `/`
- **Login:** `/pages/login.html`
- **Admin Dashboard:** `/pages/admin/dashboard.html`
- **Staff Dashboard:** `/pages/staff/dashboard.html`
- **Technician Dashboard:** `/pages/technician/dashboard.html`

---

## Configuration

Open `appsettings.json` in the `HelpDesk` folder and update the following sections:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=YOUR-SERVER\\SQLEXPRESS;Initial Catalog=MunicipalHelpdesk;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
  },
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UseSsl": true,
    "Username": "your.email@gmail.com",
    "Password": "your-gmail-app-password",
    "FromName": "O.R. Tambo IT Helpdesk",
    "FromEmail": "your.email@gmail.com"
  }
}