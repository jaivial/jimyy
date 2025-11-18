# Expression Syntax Documentation

## Overview

The Expression Evaluator provides a powerful and secure way to create dynamic values in workflow parameters using JavaScript-like expressions. Expressions are enclosed in double curly braces: `{{ expression }}`.

## Expression Syntax

### Basic Expressions

```javascript
// Simple values
{{ "Hello World" }}
{{ 42 }}
{{ true }}

// Math operations
{{ 10 + 20 }}
{{ price * 1.1 }}
{{ Math.round(value * 100) / 100 }}

// String concatenation
{{ firstName + " " + lastName }}
{{ "Total: $" + total }}

// Conditional (ternary) operator
{{ age >= 18 ? "Adult" : "Minor" }}
{{ status === "active" ? "Yes" : "No" }}
```

## Context Variables

### $node - Access Previous Node Results

Access output data from previous nodes in the workflow:

```javascript
// Access data from a specific node
{{ $node.HttpRequest.data.userId }}
{{ $node.DatabaseQuery.data.result[0].name }}
{{ $node.Transform.data.processedValue }}

// Access nested properties
{{ $node.ApiCall.data.response.items[0].id }}
{{ $node.JsonParser.data.user.email }}
```

### $workflow - Access Workflow Metadata

Access workflow-level information and variables:

```javascript
// Workflow ID and name
{{ $workflow.id }}
{{ $workflow.name }}

// Workflow variables
{{ $workflow.variables.apiEndpoint }}
{{ $workflow.variables.maxRetries }}
{{ $workflow.variables.enableLogging }}
```

### $env - Access Environment Variables

Access environment variables and configuration:

```javascript
// Environment variables
{{ $env.API_KEY }}
{{ $env.DATABASE_URL }}
{{ $env.MAX_CONNECTIONS }}

// Use in strings
{{ "Bearer " + $env.AUTH_TOKEN }}
```

### $json - Access Current Item Data

Access the current JSON data being processed:

```javascript
// Direct property access
{{ $json.name }}
{{ $json.email }}
{{ $json.age }}

// Nested properties
{{ $json.address.city }}
{{ $json.user.profile.avatar }}
{{ $json.items[0].price }}
```

## Built-in Functions

### Type Conversion Functions

```javascript
// Convert to number
{{ toNumber("123") }}          // → 123
{{ toNumber("45.67") }}         // → 45.67

// Convert to string
{{ toString(123) }}             // → "123"
{{ toString(true) }}            // → "true"

// Convert to integer
{{ toInt("123.45") }}           // → 123
{{ toInt(45.67) }}              // → 46

// Convert to boolean
{{ toBoolean("true") }}         // → true
{{ toBoolean(1) }}              // → true

// Convert to date
{{ toDate("2024-01-15") }}      // → DateTime object
```

### String Functions

```javascript
// Case conversion
{{ toUpper("hello") }}          // → "HELLO"
{{ toLower("WORLD") }}          // → "world"

// Trim whitespace
{{ trim("  hello  ") }}         // → "hello"

// Substring
{{ substring("Hello World", 0, 5) }}      // → "Hello"
{{ substring("Hello World", 6) }}         // → "World"

// Replace
{{ replace("Hello World", "World", "JavaScript") }}  // → "Hello JavaScript"

// Split
{{ split("a,b,c", ",") }}       // → ["a", "b", "c"]

// Contains, starts with, ends with
{{ contains("Hello World", "World") }}    // → true
{{ startsWith("Hello", "He") }}           // → true
{{ endsWith("Hello", "lo") }}             // → true

// Length
{{ length("Hello") }}           // → 5

// Regex match
{{ regexMatch("abc123", "\\d+") }}        // → true
```

### Math Functions

```javascript
// Rounding
{{ round(3.14159, 2) }}         // → 3.14
{{ floor(3.9) }}                // → 3
{{ ceil(3.1) }}                 // → 4

// Absolute value
{{ abs(-5) }}                   // → 5

// Min/Max
{{ min(10, 20) }}               // → 10
{{ max(10, 20) }}               // → 20

// Random number (0-1)
{{ random() }}                  // → 0.742891...

// Standard Math object also available
{{ Math.sqrt(16) }}             // → 4
{{ Math.pow(2, 3) }}            // → 8
{{ Math.PI }}                   // → 3.14159...
```

### Date/Time Functions

```javascript
// Current date/time
{{ now() }}                     // → Current DateTime
{{ utcNow() }}                  // → Current UTC DateTime
{{ today() }}                   // → Today's date (no time)

// Format date
{{ formatDate(now(), "yyyy-MM-dd") }}           // → "2024-01-15"
{{ formatDate(now(), "MM/dd/yyyy HH:mm:ss") }}  // → "01/15/2024 14:30:00"

// Add to dates
{{ addDays(today(), 7) }}       // → 7 days from today
{{ addHours(now(), 2) }}        // → 2 hours from now
{{ addMinutes(now(), 30) }}     // → 30 minutes from now

// Extract date parts
{{ year(now()) }}               // → 2024
{{ month(now()) }}              // → 1
{{ day(now()) }}                // → 15

// Date operations with JavaScript
{{ new Date().toISOString() }}  // → "2024-01-15T14:30:00.000Z"
{{ new Date().getTime() }}      // → 1705328400000
```

### JSON Functions

```javascript
// Parse JSON string
{{ parseJson('{"name":"John","age":30}') }}

// Convert to JSON string
{{ toJson({ name: "John", age: 30 }) }}  // → '{"name":"John","age":30}'

// Get nested property
{{ getJsonProperty($json, "user.profile.email") }}
```

### Array/Collection Functions

```javascript
// Check if empty
{{ isEmpty([]) }}               // → true
{{ isEmpty("") }}               // → true
{{ isEmpty(null) }}             // → true

// Array length
{{ arrayLength([1, 2, 3]) }}    // → 3

// Check if null
{{ isNull(null) }}              // → true
{{ isNull(42) }}                // → false

// Default value if null
{{ default(null, "default value") }}  // → "default value"
{{ default(42, "default value") }}    // → 42
```

### Utility Functions

```javascript
// Generate UUID
{{ uuid() }}                    // → "a1b2c3d4-e5f6-..."

// Base64 encoding
{{ base64Encode("Hello World") }}     // → "SGVsbG8gV29ybGQ="
{{ base64Decode("SGVsbG8gV29ybGQ=") }} // → "Hello World"
```

## Complex Examples

### Example 1: Data Transformation

```javascript
// Transform user data
{{
  toUpper($node.GetUser.data.firstName) + " " +
  toUpper($node.GetUser.data.lastName)
}}

// Calculate discounted price
{{ round($node.GetProduct.data.price * 0.9, 2) }}

// Format full address
{{
  $json.address.street + ", " +
  $json.address.city + ", " +
  $json.address.state + " " +
  $json.address.zip
}}
```

### Example 2: Conditional Logic

```javascript
// Check user age
{{ $node.GetUser.data.age >= 21 ? "Allowed" : "Denied" }}

// Status based on multiple conditions
{{
  $json.status === "active" && $json.verified === true
    ? "Active User"
    : "Inactive User"
}}

// Complex nested conditions
{{
  $json.type === "premium"
    ? $json.price * 0.8
    : $json.type === "standard"
    ? $json.price * 0.9
    : $json.price
}}
```

### Example 3: Date Operations

```javascript
// Check if date is in the past
{{ toDate($json.expiryDate) < now() }}

// Format date for API
{{ formatDate(addDays(today(), 30), "yyyy-MM-dd") }}

// Calculate age
{{ year(now()) - year(toDate($json.birthDate)) }}
```

### Example 4: Working with Arrays

```javascript
// Access array elements
{{ $node.GetOrders.data.orders[0].id }}

// Get first item from array
{{ $json.items[0].name }}

// Check array length
{{ arrayLength($node.GetItems.data.items) > 0 ? "Has items" : "Empty" }}
```

### Example 5: API Integration

```javascript
// Build API URL
{{ $env.API_BASE_URL + "/users/" + $json.userId }}

// Build Authorization header
{{ "Bearer " + $env.API_TOKEN }}

// Build query parameters
{{
  "?page=" + $workflow.variables.currentPage +
  "&limit=" + $workflow.variables.pageSize
}}
```

### Example 6: String Manipulation

```javascript
// Clean and format email
{{ toLower(trim($json.email)) }}

// Extract domain from email
{{ split($json.email, "@")[1] }}

// Build full name with title
{{
  $json.title + " " +
  $json.firstName + " " +
  $json.lastName
}}

// Generate slug
{{ toLower(replace($json.title, " ", "-")) }}
```

## Security & Limitations

### Allowed Operations

- Basic JavaScript expressions (math, strings, conditionals)
- Access to context variables ($node, $workflow, $env, $json)
- Built-in helper functions
- Standard JavaScript Math, Date, String operations
- Array/object access and manipulation

### Forbidden Operations

For security reasons, the following are **NOT** allowed:

- File system operations (read, write, delete)
- Network operations (fetch, http, ajax)
- Process/system access (process, require, import)
- Code evaluation (eval, Function constructor)
- DOM/Browser APIs (document, window)
- Timers (setTimeout, setInterval)

### Execution Limits

- **Timeout**: 5 seconds maximum execution time
- **Recursion**: Maximum 100 levels deep
- **Statements**: Maximum 10,000 statements
- **Expression Length**: Maximum 10,000 characters
- **Nesting**: Maximum 10 levels of nested braces

## Error Handling

If an expression fails to evaluate, an error will be thrown with a descriptive message:

```javascript
// Missing node
{{ $node.NonExistentNode.data.field }}
// Error: Cannot read property 'data' of undefined

// Invalid syntax
{{ 10 / 0 }}
// Result: Infinity (JavaScript behavior)

// Type error
{{ toNumber("not a number") }}
// Result: 0 (safe fallback)
```

## Best Practices

1. **Keep expressions simple**: Break complex logic into multiple nodes
2. **Use variables**: Store reusable values in workflow variables
3. **Handle null values**: Use `default()` or conditional checks
4. **Validate data**: Check for null/undefined before accessing properties
5. **Use appropriate functions**: Use built-in functions for common operations
6. **Test expressions**: Test with sample data before deploying

## Examples in Workflow Nodes

### HTTP Request Node - Dynamic URL

```javascript
URL: {{ $env.API_BASE_URL + "/api/v1/users/" + $json.userId }}
```

### HTTP Request Node - Headers

```javascript
Authorization: {{ "Bearer " + $env.API_TOKEN }}
Content-Type: application/json
X-Request-ID: {{ uuid() }}
```

### Set Variable Node - Transform Data

```javascript
{
  "fullName": {{ $json.firstName + " " + $json.lastName }},
  "email": {{ toLower(trim($json.email)) }},
  "age": {{ year(now()) - year(toDate($json.birthDate)) }},
  "isAdult": {{ $json.age >= 18 }}
}
```

### If Node - Conditional Logic

```javascript
Condition: {{ $node.GetUser.data.role === "admin" && $node.GetUser.data.active === true }}
```

This expression system provides a powerful yet secure way to create dynamic workflows with minimal code.
