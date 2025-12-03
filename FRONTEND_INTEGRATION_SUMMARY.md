# Frontend Integration Summary - Quick Reference

## Backend API Configuration

**Base URL**: `https://localhost:7030/api`
**CORS**: Configured for `localhost:5173` (Vite) and `localhost:3000`
**Authentication**: JWT (5min access token) + HttpOnly Refresh Token (7 days)

---

## Key Differences from Booklyfy Spec

### 1. User Model Differences

**Backend Uses**:
```typescript
interface User {
  id: string;           // Guid
  username: string;     // NOT firstName/lastName
  email: string;
  roles: string[];      // ["User", "Admin", "Manager"]
  // NO: phoneNumber, profileImageUrl, isEmailVerified, lastLoginAt
}
```

**Booklyfy Spec Had**:
- `firstName` + `lastName` instead of `username`
- Additional fields like `phoneNumber`, `profileImageUrl`, etc.

### 2. OperationResult Wrapper

**ALL API responses are wrapped**:
```typescript
interface OperationResult<T> {
  isSuccess: boolean;
  errors: string[];    // Array of error messages
  data: T | null;      // Actual data (null if error)
}
```

**Booklyfy Spec Had**:
```typescript
// Different error structure:
interface ApiError {
  code: string;
  message: string;
  details?: string;
}
```

### 3. Authentication Endpoints

| Endpoint | Backend | Booklyfy Spec | Difference |
|----------|---------|---------------|------------|
| Register | `POST /api/auth/register` | Same | ✓ Compatible |
| Login | `POST /api/auth/login` | Same | ✓ Compatible |
| Refresh | `POST /api/auth/refresh` | Same | ✓ Compatible |
| Logout | ❌ Not implemented | `POST /api/auth/logout` | Client-side only |
| Get Me | ❌ Not implemented | `GET /api/auth/me` | Decode JWT instead |
| Forgot Password | ❌ Not implemented | `POST /api/auth/forgot-password` | TODO |
| Reset Password | ❌ Not implemented | `POST /api/auth/reset-password` | TODO |

---

## Authentication Flow

### Register
```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "john_doe",        // NOT firstName/lastName
  "email": "john@example.com",
  "password": "SecurePass123!"
}

Response (200 OK):
{
  "isSuccess": true,
  "errors": [],
  "data": {
    "token": "eyJhbGci...",
    "user": {
      "id": "guid",
      "username": "john_doe",
      "email": "john@example.com",
      "roles": ["User"]
    },
    "refreshToken": "64_byte_string"
  }
}
```

### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "john_doe",        // NOT email
  "password": "SecurePass123!"
}

Response: Same as Register
```

### Refresh Token
```http
POST /api/auth/refresh
Cookie: refreshToken=xxx           // Automatically sent by browser

Response: Same as Register/Login (new tokens)
```

---

## Required Frontend Changes

### 1. Update TypeScript Types

**Create**: `src/types/backend.types.ts`
```typescript
// Backend uses this wrapper for ALL responses
export interface OperationResult<T> {
  isSuccess: boolean;
  errors: string[];
  data: T | null;
}

// Backend User model
export interface User {
  id: string;
  username: string;      // NOT firstName/lastName
  email: string;
  roles: string[];
}

// Auth Request DTOs
export interface RegisterRequest {
  username: string;      // NOT firstName/lastName
  email: string;
  password: string;
}

export interface LoginRequest {
  username: string;      // Can also accept email
  password: string;
}

// Auth Response
export interface AuthResponse {
  token: string;
  user: User;
  refreshToken: string;
}

export type AuthResult = OperationResult<AuthResponse>;
```

### 2. Update API Client

**File**: `src/lib/api-client.ts`
```typescript
import axios from 'axios';

export const apiClient = axios.create({
  baseURL: 'https://localhost:7030/api',
  withCredentials: true,  // CRITICAL: Required for cookies
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add JWT token to requests
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Auto-refresh on 401
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401 && !error.config._retry) {
      error.config._retry = true;
      try {
        const { data } = await axios.post(
          'https://localhost:7030/api/auth/refresh',
          {},
          { withCredentials: true }
        );
        if (data.isSuccess) {
          localStorage.setItem('accessToken', data.data.token);
          error.config.headers.Authorization = `Bearer ${data.data.token}`;
          return apiClient(error.config);
        }
      } catch {
        localStorage.removeItem('accessToken');
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  }
);
```

### 3. Update Auth Service

**File**: `src/services/auth.service.ts`
```typescript
import { apiClient } from '@/lib/api-client';
import {
  RegisterRequest,
  LoginRequest,
  AuthResult,
} from '@/types/backend.types';

export class AuthService {
  static async register(data: RegisterRequest): Promise<AuthResult> {
    const response = await apiClient.post<AuthResult>('/auth/register', data);
    if (response.data.isSuccess && response.data.data) {
      localStorage.setItem('accessToken', response.data.data.token);
    }
    return response.data;
  }

  static async login(data: LoginRequest): Promise<AuthResult> {
    const response = await apiClient.post<AuthResult>('/auth/login', data);
    if (response.data.isSuccess && response.data.data) {
      localStorage.setItem('accessToken', response.data.data.token);
    }
    return response.data;
  }

  static async refresh(): Promise<AuthResult> {
    const response = await apiClient.post<AuthResult>('/auth/refresh');
    if (response.data.isSuccess && response.data.data) {
      localStorage.setItem('accessToken', response.data.data.token);
    }
    return response.data;
  }

  static logout(): void {
    localStorage.removeItem('accessToken');
    window.location.href = '/login';
  }
}
```

### 4. Handle OperationResult in Components

```typescript
// OLD (Booklyfy spec):
const response = await authService.login(credentials);
if (response.user) { /* success */ }

// NEW (Backend):
const result = await AuthService.login(credentials);
if (result.isSuccess && result.data) {
  // Success: result.data.user, result.data.token
  console.log('User:', result.data.user);
} else {
  // Error: result.errors is an array of error messages
  console.error('Errors:', result.errors.join(', '));
}
```

### 5. Error Handling

```typescript
try {
  const result = await AuthService.login(credentials);

  if (result.isSuccess && result.data) {
    // Success
    setUser(result.data.user);
    navigate('/dashboard');
  } else {
    // Backend validation errors
    setErrors(result.errors);  // Array of strings
  }
} catch (error) {
  // Network error
  setErrors(['Network error occurred']);
}
```

---

## Environment Setup

**Frontend `.env`**:
```env
VITE_API_BASE_URL=https://localhost:7030/api
VITE_USE_MOCK_DATA=false
```

---

## Testing Checklist

1. **Start Backend**:
   ```bash
   cd API
   dotnet run
   ```
   Backend runs on `https://localhost:7030`

2. **Test with Swagger**:
   Visit `https://localhost:7030/swagger`

3. **Start Frontend**:
   ```bash
   npm run dev
   ```
   Frontend runs on `http://localhost:5173`

4. **Test Auth Flow**:
   - [ ] Register new user (use username, not firstName/lastName)
   - [ ] Login with username/password
   - [ ] Check localStorage for `accessToken`
   - [ ] Check browser cookies for `refreshToken`
   - [ ] Make authenticated request (JWT in Authorization header)
   - [ ] Test auto-refresh when token expires

---

## Common Issues

**CORS Error**:
- Solution: Backend already configured for `localhost:5173`

**"Username" field error**:
- Solution: Frontend forms should use `username` NOT `firstName`/`lastName`

**Cookie not sent**:
- Solution: Ensure `withCredentials: true` in axios

**"isSuccess is undefined"**:
- Solution: All responses are wrapped in `OperationResult<T>`, check `.isSuccess` and `.data`

---

## Quick Command Reference

```bash
# Backend
cd API
dotnet run                          # Start API on https://localhost:7030
dotnet ef migrations add <Name>     # Create migration
dotnet ef database update           # Apply migrations

# Frontend
npm run dev                         # Start Vite dev server
```

---

## What's NOT Implemented Yet

These endpoints from the Booklyfy spec don't exist in the backend:

- `GET /api/auth/me` - Decode JWT client-side instead
- `POST /api/auth/logout` - Client-side only (clear localStorage)
- `POST /api/auth/forgot-password` - TODO
- `POST /api/auth/reset-password` - TODO
- `POST /api/auth/verify-email` - TODO
- All Business/Service/Booking endpoints - TODO

**For now, focus on User Authentication only.**

---

## Summary for AI Agent

Dear Frontend AI Agent,

The backend uses a **different User model** and **different response format** than the Booklyfy spec:

1. **User has `username` instead of `firstName`/`lastName`**
2. **All responses wrapped in `OperationResult<T>`** with `{ isSuccess, errors, data }`
3. **Only 3 auth endpoints**: register, login, refresh
4. **Refresh token in HttpOnly cookie**, access token in localStorage
5. **CORS already configured** for localhost:5173

**Action Items**:
1. Replace all `firstName`/`lastName` with `username` in forms
2. Wrap all API calls to expect `OperationResult<T>`
3. Check `result.isSuccess` before accessing `result.data`
4. Display `result.errors` array on failures
5. Use `withCredentials: true` in axios for cookies

Good luck!
