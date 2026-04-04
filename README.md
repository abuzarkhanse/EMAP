# Editorial Management and Automation Platform (EMAP)

An integrated platform for managing **Final Year Projects (FYP), Thesis workflows, and Research publication processes** in universities.

This system automates academic editorial workflows such as proposal submission, supervisor allocation, evaluation, review management, and final publication.

---

# Project Overview

The **Editorial Management and Automation Platform (EMAP)** is designed to replace manual academic workflows (emails, spreadsheets, paperwork) with a **centralized web-based platform**.

Universities often manage:

- Final Year Projects (FYP)
- Thesis supervision
- Research paper submissions
- Proposal defenses
- Evaluation and grading

Most institutions still handle these processes manually which leads to:

- Delays
- Lack of transparency
- Data loss
- Administrative workload

EMAP provides a **fully automated system** that manages the complete academic lifecycle from **proposal submission to publication**. :contentReference[oaicite:2]{index=2}

---

# Modules

The system consists of **three main modules**.

## 1. FYP Management Module

Handles the complete Final Year Project workflow.

Features:

- FYP batch call announcement
- Supervisor selection
- Student project proposals
- Supervisor approval / rejection
- Proposal defense scheduling
- Defense committee evaluation
- Progress tracking
- Midterm evaluation
- Final project submission

Workflow:
FYP Call → Supervisor Selection → Proposal Submission
→ Proposal Review → Defense Scheduling
→ Evaluation → Project Development
→ Final Submission


---

## 2. Thesis Management Module

Automates thesis supervision and submission processes.

Features:

- Thesis proposal submission
- Supervisor assignment
- Chapter submissions
- Feedback and evaluation
- Thesis defense scheduling
- Final approval

---

## 3. Research Management Module

Handles **research paper submissions and peer review workflows**.

Features:

- Research article submission
- Editorial review
- Reviewer assignment
- Peer review process
- Decision management
- Publication archive

---

# System Architecture

The system follows a **modular web architecture**:
Presentation Layer
|
ASP.NET MVC Web Application
|
Application Layer (Services & Controllers)
|
Domain Layer (Business Logic)
|
Infrastructure Layer (Database & Identity)
|
Microsoft SQL Server


---

# Technology Stack

Backend:
- ASP.NET MVC / .NET Framework
- C#

Frontend:
- Razor Views
- HTML
- CSS
- Bootstrap
- JavaScript

Database:
- Microsoft SQL Server
- Entity Framework

Authentication:
- ASP.NET Identity

Development Tools:
- Visual Studio
- GitHub
- SQL Server Management Studio

---

# User Roles

The system supports **role-based access control**.

| Role | Responsibilities |
|-----|----------------|
| Student | Submit proposals, projects, research papers |
| Supervisor | Review proposals, supervise projects |
| Reviewer | Review research papers |
| FYP Coordinator | Manage FYP workflow and defense |
| Admin | System management |

---

# Current Progress

The following components have been implemented so far:

### Completed

✔ Database schema design  
✔ Entity Relationship Diagrams (ERD)  
✔ System architecture design  
✔ Literature review (SLR)  
✔ FYP module workflow implementation  
✔ Supervisor management  
✔ Proposal submission module  
✔ Defense committee management  
✔ Role-based authentication  

### In Progress

- Thesis module development
- Research submission system
- Peer review workflow
- Notification system
- Dashboard analytics

### Planned

- Plagiarism detection integration
- Reviewer recommendation system
- Email automation
- Report generation
- Institutional repository integration

---

# Research Foundation

This project is based on research into **Editorial Management Systems, Journal Management Systems, and Academic Workflow Platforms**.

Existing systems such as:

- Open Journal Systems (OJS)
- Editorial Manager
- ConfiChair

provide editorial workflows but lack support for **academic project management**.

EMAP aims to bridge this gap by integrating **FYP, Thesis, and Research workflows in one system**. :

---

# Project Team

Final Year Project – BS Software Engineering  
Pak-Austria Fachhochschule: Institute of Applied Sciences & Technology

Students:

- Abuzar Khan
- Sufyan Humam Mushtaq
- Jawad-ul-Islam

Supervisor:

Dr. Rashid Naseem

---

# Future Improvements

Future versions of EMAP may include:

- AI-based reviewer assignment
- Machine learning for project recommendations
- Blockchain-based peer review transparency
- Integration with ORCID and DOI systems
- Large language model support for research analysis

---

# License

This project is developed for academic purposes as part of a **Final Year Project (FYP)**.

---

# Repository Status

Project Status: **In Development**
