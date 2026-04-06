# FlickList

A passion project and a portfolio piece that integrates with [The Movie Database (TMDB) API](https://www.themoviedb.org/) 
to allow users to track their personal movie ownership.

> **Note:** This repository represents the Minimum Viable Product (MVP) and is still in early development.

---

## Overview

FlickList lets you keep track of your movie ownership across physical and digital formats.

**Physical Formats**
- DVD
- Blu-ray
- 4K Blu-ray

**Digital Retailers**
- Apple TV
- Movies Anywhere
- Fandango at Home
- YouTube
- Amazon Prime Video

---

## Live Demo

**[FlickList Live Demo - Angular App](https://brave-mud-06345141e.2.azurestaticapps.net/)**

Explore the app without local setup. Use the provided demo account or create a new account, search for movies on TMDB, 
and start tracking your movie collection.

> **Note:** The API is hosted on Azure's free tier and may take up to 1 minute to respond on the first request after 
> inactivity (cold start). Subsequent requests will be much faster. Saved data may be periodically cleared in the demo 
> environment.

---

## Why This Project Exists

As a full-stack software engineer, this project demonstrates my ability to design and implement a modern backend system 
as well as front-end frameworks.

The Web API is built using a **Clean Architecture**. Most endpoints are implemented using **Minimal APIs**, with one 
controller-based endpoint included to demonstrate familiarity with both approaches.

**Current technology choices** (intended for quick setup and development):
- **.NET Core 10**
- **EF Core**
- **SQLite** — simple local database
- **MediatR** — request handling
- **Serilog** — logging
- **Scalar** — API documentation
- **Redis cloud** — Caching TMDB API responses
- **Angular v21** — front-end framework
- **Azure** — cloud hosting and deployment via GitHub Actions

---

## Project Goals

The long-term goal is to evolve this into a full ecosystem:

**Web API**
- Logging (Complete)
- Rate limiting (Complete)
- Scalar / OpenAPI documentation (Complete)
- Centralized exception handling (Complete)
- Remove MediatR and implement custom request handling

**Web Applications** — multiple front-end implementations of the same API:
- Angular (MVP complete)
- React
- Vue
- Blazor

**Infrastructure**
- Email service for account creation and password resets
- PostgreSQL database
- Caching strategy for TMDB API responses (Complete)
- OpenTelemetry for logging and monitoring
- Docker containerization

---

## Getting Started

Visit the backend [README](https://github.com/nrobinson-dev/my-movies-app/tree/main/src/backend/README.md) for setup instructions.