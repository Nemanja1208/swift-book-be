# Backend Architecture & Frontend Integration Guide

## Table of Contents
1. [Backend Architecture Overview](#backend-architecture-overview)
2. [Authentication System Details](#authentication-system-details)
3. [Required Backend Changes for Frontend Integration](#required-backend-changes-for-frontend-integration)
4. [Frontend TypeScript Types](#frontend-typescript-types)
5. [API Integration Guide for Frontend](#api-integration-guide-for-frontend)

---

## Backend Architecture Overview

### Project Structure

Your .NET Clean Architecture backend follows a 4-layer architecture:

```
CleanArchitectureBE-DOTNET/
├── API/                          # Presentation Layer
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   └── BankAccountsController.cs
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs
│   └── Program.cs
│
├── Application/                  # Application Layer (Business Logic)
│   ├── Auth/
│   │   └── Dtos/
│   │       └── UserDtos.cs
│   ├── Common/
│   │   ├── Behaviors/           # MediatR Pipeline Behaviors
│   │   │   ├── ValidationBehavior.cs
│   │   │   └── LoggingBehavior.cs
│   │   ├── Interfaces/          # Service Interfaces
│   │   └── Mappings/            # AutoMapper Profiles
│   └── Features/                # CQRS Commands/Queries
│
├── Infrastructure/               # Infrastructure Layer
│   ├── Database/
│   │   └── AppDbContext.cs
│   ├── Repositories/            # Data Access
│   ├── Services/
│   │   ├── AuthService.cs
│   │   └── UserContextService.cs
│   └── Configuration/
│       └── JwtSettings.cs
│
└── Domain/                       # Domain Layer (Core Business)
    ├── Models/
    │   ├── Users/
    │   │   ├── User.cs
    │   │   ├── Role.cs
    │   │   └── UserRole.cs
    │   ├── Accounts/
    │   │   └── BankAccount.cs
    │   └── Common/
    │       └── OperationResult.cs
    └── Enums/
```

### Key Architectural Patterns

1. **Clean Architecture**: Dependency flow goes inward (API → Application → Domain ← Infrastructure)
2. **CQRS with MediatR**: Commands and Queries separated for scalability
3. **Repository Pattern**: Generic repository with specialized implementations
4. **OperationResult Pattern**: Consistent error handling across all endpoints
5. **JWT Authentication**: Access tokens (5min) + HttpOnly Refresh tokens (7 days)
6. **RBAC**: Role-based authorization (User, Admin, Manager roles)
7. **Pipeline Behaviors**: Automatic validation and logging via MediatR

---

## Authentication System Details

### Domain Model: User Entity

**Location**: `Domain/Models/Users/User.cs`

```csharp
public class User
{
    public Guid Id { get; set; }                    // Primary key
    public string Username { get; set; }            // Unique username
    public string Email { get; set; }               // Unique email
    public byte[] PasswordHash { get; set; }        // HMACSHA512 hashed password
    public byte[] PasswordSalt { get; set; }        // Salt for password hash

    // Refresh Token Management
    public string? RefreshToken { get; set; }       // Current refresh token
    public DateTime? RefreshTokenExpiresAt { get; set; }

    // Relationships
    public List<UserRole> Roles { get; set; }       // Many-to-many with Role
    public List<BankAccount> BankAccounts { get; set; }
}
```

### Authentication DTOs

**Location**: `Application/Auth/Dtos/UserDtos.cs`

```csharp
// REQUEST: Register new user
public class RegisterUserDto
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}

// REQUEST: Login user
public class LoginUserDto
{
    public string Username { get; set; }
    public string Password { get; set; }
}

// RESPONSE: User information (part of AuthResponseDto)
public class UserDtoResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public List<string> Roles { get; set; }  // Role names (e.g., ["User", "Admin"])
}

// RESPONSE: Authentication response
public class AuthResponseDto
{
    public string Token { get; set; }             // JWT access token (expires in 5 minutes)
    public UserDtoResponse? User { get; set; }    // User details
    public string RefreshToken { get; set; }      // Refresh token (also set in HttpOnly cookie)
}

// REQUEST: Refresh token
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; }
}
```

### OperationResult Pattern

**Location**: `Domain/Models/Common/OperationResult.cs`

All API responses are wrapped in `OperationResult<T>`:

```csharp
public class OperationResult<T>
{
    public bool IsSuccess { get; set; }        // true = success, false = error
    public List<string> Errors { get; set; }   // Error messages (empty if successful)
    public T? Data { get; set; }               // Response data (null if error)

    // Static factory methods
    public static OperationResult<T> Success(T data)
        => new() { IsSuccess = true, Data = data };

    public static OperationResult<T> Failure(params string[] errors)
        => new() { IsSuccess = false, Errors = errors.ToList() };
}
```

### AuthController Endpoints

**Location**: `API/Controllers/AuthController.cs`

#### 1. Register User
```http
POST /api/auth/register
Content-Type: application/json

Request Body:
{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "SecurePass123!"
}

Success Response (200 OK):
{
  "isSuccess": true,
  "errors": [],
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "username": "john_doe",
      "email": "john@example.com",
      "roles": ["User"]
    },
    "refreshToken": "random_64_byte_string"
  }
}

Error Response (400 Bad Request):
{
  "isSuccess": false,
  "errors": [
    "Username already exists",
    "Email is already in use"
  ],
  "data": null
}

Notes:
- Refresh token is ALSO set in HttpOnly cookie named "refreshToken"
- User automatically assigned "User" role
- Password is hashed with HMACSHA512 + salt
```

#### 2. Login User
```http
POST /api/auth/login
Content-Type: application/json

Request Body:
{
  "username": "john_doe",
  "password": "SecurePass123!"
}

Success Response (200 OK):
{
  "isSuccess": true,
  "errors": [],
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "username": "john_doe",
      "email": "john@example.com",
      "roles": ["User", "Admin"]
    },
    "refreshToken": "random_64_byte_string"
  }
}

Error Response (401 Unauthorized):
{
  "isSuccess": false,
  "errors": [
    "Invalid username or password"
  ],
  "data": null
}

Notes:
- Refresh token set in HttpOnly cookie
- JWT token expires in 5 minutes (configurable in appsettings.json)
```

#### 3. Refresh Token
```http
POST /api/auth/refresh
Cookie: refreshToken=random_64_byte_string

Success Response (200 OK):
{
  "isSuccess": true,
  "errors": [],
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",  // New JWT
    "user": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "username": "john_doe",
      "email": "john@example.com",
      "roles": ["User"]
    },
    "refreshToken": "new_random_64_byte_string"  // New refresh token
  }
}

Error Response (401 Unauthorized):
{
  "isSuccess": false,
  "errors": [
    "Invalid or expired refresh token"
  ],
  "data": null
}

Notes:
- Refresh token read from HttpOnly cookie (NOT from request body)
- New refresh token generated and set in cookie
- Refresh tokens expire after 7 days
```

### JWT Token Claims

The JWT access token contains the following claims:

```json
{
  "sub": "3fa85f64-5717-4562-b3fc-2c963f66afa6",  // UserId
  "name": "john_doe",                              // Username
  "email": "john@example.com",                     // Email
  "role": ["User", "Admin"],                       // Roles (can be multiple)
  "iss": "CleanArchApp",                           // Issuer
  "aud": "CleanArchAppUser",                       // Audience
  "exp": 1733234567,                               // Expiration (5 minutes from issue)
  "iat": 1733234267                                // Issued at
}
```

### Cookie Configuration

Refresh tokens are set as HttpOnly cookies with the following settings:

```csharp
new CookieOptions
{
    HttpOnly = true,           // Not accessible via JavaScript
    Secure = true,             // HTTPS only
    SameSite = SameSiteMode.Strict,  // CSRF protection
    Expires = DateTime.UtcNow.AddDays(7)  // 7-day expiration
}
```

---

## Required Backend Changes for Frontend Integration

### 1. Add CORS Configuration

**CRITICAL**: Your backend currently has NO CORS configuration. You must add this for the frontend to communicate with the API.

**File**: `API/Program.cs`

Add the following BEFORE `var app = builder.Build();`:

```csharp
// Add CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",  // Vite default dev server
            "http://localhost:3000",  // Alternative React dev server
            "https://yourdomain.com"  // Production domain
        )
        .AllowCredentials()           // REQUIRED for HttpOnly cookies
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});
```

Add this AFTER `app.UseHttpsRedirection();` and BEFORE `app.UseAuthentication();`:

```csharp
app.UseCors("AllowFrontend");
```

### 2. Update appsettings.json

**File**: `API/appsettings.json`

Consider updating JWT settings for better security in production:

```json
{
  "JwtSettings": {
    "Secret": "super_secret_key_for_dev_only_change_this",  // CHANGE IN PRODUCTION!
    "Issuer": "CleanArchApp",
    "Audience": "CleanArchAppUser",
    "ExpireMinutes": 15  // Consider 15-30 minutes instead of 5
  }
}
```

### 3. Verify API Launch Configuration

Your API runs on:
- **HTTPS**: `https://localhost:7030`
- **HTTP**: `http://localhost:5198`

Use HTTPS in development for the Secure cookie flag to work properly.

---

## Frontend TypeScript Types

### Create these types in your React frontend

**File**: `src/types/auth.types.ts`

```typescript
// ============================================
// OPERATION RESULT WRAPPER (All API responses use this)
// ============================================
export interface OperationResult<T> {
  isSuccess: boolean;
  errors: string[];
  data: T | null;
}

// ============================================
// USER TYPES
// ============================================
export interface User {
  id: string;           // Guid converted to string
  username: string;
  email: string;
  roles: string[];      // Array of role names: ["User", "Admin", etc.]
}

// ============================================
// AUTHENTICATION REQUEST TYPES
// ============================================
export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  username: string;
  password: string;
}

// ============================================
// AUTHENTICATION RESPONSE TYPES
// ============================================
export interface AuthResponse {
  token: string;              // JWT access token (expires in 5-15 minutes)
  user: User;                 // User details with roles
  refreshToken: string;       // Refresh token (also in HttpOnly cookie)
}

// Wrapped version (what API actually returns)
export type AuthOperationResult = OperationResult<AuthResponse>;
```

### API Client Configuration

**File**: `src/lib/api-client.ts`

```typescript
import axios, { AxiosError, AxiosInstance } from 'axios';
import { OperationResult, AuthResponse } from '@/types/auth.types';

// API Base Configuration
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7030/api';

// Create axios instance
export const apiClient: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true,  // CRITICAL: Required for HttpOnly cookies
});

// ============================================
// REQUEST INTERCEPTOR: Add JWT to all requests
// ============================================
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('accessToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// ============================================
// RESPONSE INTERCEPTOR: Handle token refresh
// ============================================
apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError<OperationResult<any>>) => {
    const originalRequest = error.config;

    // If 401 and not already retried, try to refresh token
    if (error.response?.status === 401 && originalRequest && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        // Call refresh endpoint (uses HttpOnly cookie automatically)
        const response = await axios.post<OperationResult<AuthResponse>>(
          `${API_BASE_URL}/auth/refresh`,
          {},
          { withCredentials: true }
        );

        if (response.data.isSuccess && response.data.data) {
          const { token } = response.data.data;

          // Store new token
          localStorage.setItem('accessToken', token);

          // Retry original request with new token
          originalRequest.headers.Authorization = `Bearer ${token}`;
          return apiClient(originalRequest);
        }
      } catch (refreshError) {
        // Refresh failed, redirect to login
        localStorage.removeItem('accessToken');
        window.location.href = '/login';
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);
```

### Authentication API Service

**File**: `src/services/auth.service.ts`

```typescript
import { apiClient } from '@/lib/api-client';
import {
  RegisterRequest,
  LoginRequest,
  AuthOperationResult,
  OperationResult,
  AuthResponse,
} from '@/types/auth.types';

export class AuthService {
  /**
   * Register a new user
   */
  static async register(data: RegisterRequest): Promise<AuthOperationResult> {
    const response = await apiClient.post<AuthOperationResult>('/auth/register', data);

    // If successful, store the access token
    if (response.data.isSuccess && response.data.data) {
      localStorage.setItem('accessToken', response.data.data.token);
    }

    return response.data;
  }

  /**
   * Login user
   */
  static async login(data: LoginRequest): Promise<AuthOperationResult> {
    const response = await apiClient.post<AuthOperationResult>('/auth/login', data);

    // If successful, store the access token
    if (response.data.isSuccess && response.data.data) {
      localStorage.setItem('accessToken', response.data.data.token);
    }

    return response.data;
  }

  /**
   * Refresh access token using HttpOnly cookie
   */
  static async refreshToken(): Promise<AuthOperationResult> {
    const response = await apiClient.post<AuthOperationResult>('/auth/refresh', {});

    // If successful, update stored token
    if (response.data.isSuccess && response.data.data) {
      localStorage.setItem('accessToken', response.data.data.token);
    }

    return response.data;
  }

  /**
   * Logout user (client-side only, backend has no logout endpoint)
   */
  static logout(): void {
    localStorage.removeItem('accessToken');
    // Note: HttpOnly cookie will expire naturally
    window.location.href = '/login';
  }

  /**
   * Get current access token
   */
  static getAccessToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  /**
   * Check if user is authenticated
   */
  static isAuthenticated(): boolean {
    return !!this.getAccessToken();
  }
}
```

---

## API Integration Guide for Frontend

### Environment Configuration

**File**: `.env` (Frontend)

```env
# Backend API URL
VITE_API_BASE_URL=https://localhost:7030/api

# Set to false to use real backend
VITE_USE_MOCK_DATA=false
```

### Usage Example in React Components

```typescript
import { useState } from 'react';
import { AuthService } from '@/services/auth.service';
import { LoginRequest } from '@/types/auth.types';

export function LoginPage() {
  const [error, setError] = useState<string | null>(null);

  const handleLogin = async (credentials: LoginRequest) => {
    try {
      const result = await AuthService.login(credentials);

      if (result.isSuccess && result.data) {
        // Success! User is logged in, token is stored
        console.log('Logged in user:', result.data.user);
        // Redirect to dashboard or home
        window.location.href = '/dashboard';
      } else {
        // Handle errors
        setError(result.errors.join(', '));
      }
    } catch (err) {
      setError('Network error occurred');
    }
  };

  return (
    <form onSubmit={(e) => {
      e.preventDefault();
      const formData = new FormData(e.currentTarget);
      handleLogin({
        username: formData.get('username') as string,
        password: formData.get('password') as string,
      });
    }}>
      {error && <div className="error">{error}</div>}
      <input name="username" placeholder="Username" required />
      <input name="password" type="password" placeholder="Password" required />
      <button type="submit">Login</button>
    </form>
  );
}
```

### Error Handling Best Practices

```typescript
import { AxiosError } from 'axios';
import { OperationResult } from '@/types/auth.types';

export function handleApiError(error: unknown): string[] {
  // Check if it's an OperationResult error from backend
  if (error && typeof error === 'object' && 'response' in error) {
    const axiosError = error as AxiosError<OperationResult<any>>;

    if (axiosError.response?.data?.errors) {
      return axiosError.response.data.errors;
    }
  }

  // Network or unknown error
  return ['An unexpected error occurred. Please try again.'];
}

// Usage:
try {
  await AuthService.login(credentials);
} catch (error) {
  const errors = handleApiError(error);
  setErrors(errors);
}
```

---

## Quick Start Checklist

### Backend Setup

- [x] Backend architecture already implements Clean Architecture
- [x] Authentication system is fully implemented
- [x] OperationResult pattern is in place
- [ ] **TODO**: Add CORS configuration to `Program.cs`
- [ ] **TODO**: Verify JWT settings in `appsettings.json`
- [ ] **TODO**: Test endpoints with Swagger (`https://localhost:7030/swagger`)

### Frontend Setup

- [ ] Create TypeScript types from this document
- [ ] Set up axios client with interceptors
- [ ] Create AuthService with login/register/refresh methods
- [ ] Configure `.env` with `VITE_API_BASE_URL=https://localhost:7030/api`
- [ ] Implement token storage in localStorage
- [ ] Handle automatic token refresh on 401 errors
- [ ] Test authentication flow end-to-end

---

## API Endpoints Summary

| Method | Endpoint | Description | Auth Required | Response Type |
|--------|----------|-------------|---------------|---------------|
| POST | `/api/auth/register` | Register new user | No | `OperationResult<AuthResponse>` |
| POST | `/api/auth/login` | Login user | No | `OperationResult<AuthResponse>` |
| POST | `/api/auth/refresh` | Refresh JWT token | Cookie | `OperationResult<AuthResponse>` |

---

## Important Notes

### Security Considerations

1. **HTTPS Required**: The `Secure` cookie flag requires HTTPS in production
2. **CORS**: Must be configured properly or frontend cannot make requests
3. **Credentials**: `withCredentials: true` is REQUIRED in axios for cookies to work
4. **Token Storage**: Access tokens in localStorage, refresh tokens in HttpOnly cookies
5. **JWT Secret**: Change the secret in production (currently uses dev default)

### Token Lifecycle

1. **Access Token**:
   - Lifetime: 5 minutes (configurable)
   - Storage: localStorage
   - Usage: Sent in Authorization header for all authenticated requests

2. **Refresh Token**:
   - Lifetime: 7 days
   - Storage: HttpOnly cookie (not accessible to JavaScript)
   - Usage: Automatically sent with `/api/auth/refresh` requests

### Common Issues & Solutions

**Issue**: CORS error "No 'Access-Control-Allow-Origin' header"
- **Solution**: Add CORS configuration to backend `Program.cs`

**Issue**: Refresh token not sent to backend
- **Solution**: Ensure `withCredentials: true` in axios config

**Issue**: Cookie not being set
- **Solution**: Use HTTPS in development, check SameSite policy

**Issue**: 401 errors after token expires
- **Solution**: Implement axios response interceptor to auto-refresh

---

## Contact & Support

For any issues or questions:
- Check Swagger documentation: `https://localhost:7030/swagger`
- Review backend logs in Console
- Test endpoints with Postman or curl
- Verify CORS headers in browser DevTools Network tab

---

**Document Version**: 1.0
**Last Updated**: December 2024
**Backend API Version**: .NET 8
**Compatible Frontend**: React 18+ with TypeScript
