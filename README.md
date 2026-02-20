# COBOL Banker

A legacy "green screen" banking terminal simulator built for demoing Copilot agent interaction with local command-line applications.

> **Status:** Design phase — see [DESIGN.md](DESIGN.md) for the living design document.

## What is this?

COBOL Banker is a single-file executable that mimics the kind of text-based banking terminal an associate at a bank might use daily. Think IBM 3270 / AS/400 green screens — menu-driven, fixed-width, no mouse.

It's designed as a demo prop to showcase how a Copilot agent can read terminal output, navigate menus, and perform workflows in a locally installed application.

## Features (Planned)

- Simulated teller login
- Customer lookup (by name, account number, or last-4 SSN)
- Account inquiry (balance, type, status)
- Fund transfers with confirmation
- Account maintenance (status changes, contact updates, notes)
- Transaction history
- Pre-seeded sample data (resets every launch)

## License

MIT
