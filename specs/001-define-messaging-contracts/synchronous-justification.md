# Synchronous Communication Justification Report

This document outlines the justifications for any synchronous HTTP communication that will remain in the Maliev ecosystem after the transition to an event-driven architecture.

## Justified Synchronous Calls

| Source Service | Target Service | Endpoint | Justification |
| :--- | :--- | :--- | :--- |
| OrderService | CustomerService | `GET /api/customers/{id}` | This is a synchronous, read-only query for data that is essential for the `OrderService` to validate an order before proceeding. While this could be replicated with an event-sourced local cache in `OrderService`, for now, a direct, synchronous call is deemed acceptable to reduce initial complexity. The call is idempotent and does not involve a distributed transaction. |
