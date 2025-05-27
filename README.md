# Withdrawal

## Approach outline
I have restructured the logic to use onion architeture and C# as a programming language.
I decided to use Dapper for db queries 
I used clean architecture for easy maintainability
I think using SQL for updates to prevent race conditions and promote consistency 
Implimented Idempotancy to prevent same requests being processed mulple times
Added Outbox pattern for reliable messaging and fault tolerance.

# To be implimented in future
I think adding OpenTelemetry will be beneficial for observability
Unit tests and Intergration tests

# Summary
I did not focus on making the code work but implemented just enough to sell an idea of what could be done to improve the code,
The code provided handles audits to the logging service, it also ensures eventual consistency through the use of outbox pattern, makes it easy to scale, maintain and adding unit tests.
