# Implicit Communication Inventory

This document will be populated with the results of the service analysis.

## Discovered Interactions

| Source Service | Target Service | Trigger | Interaction Type | Proposed Message(s) |
| :--- | :--- | :--- | :--- | :--- |
| (External) | OrderService | `POST /api/orders` | HTTP | `CreateOrderCommand` |
| OrderService | PaymentService | `POST /api/payments` | Command | `ProcessPaymentCommand` |
| PaymentService | NotificationService | (Implied) | Event | `PaymentCompletedEvent` |
| OrderService | CustomerService | `GET /api/customers/{id}` | Request/Response | `GetCustomerDetailsRequest` / `CustomerDetailsResponse` |