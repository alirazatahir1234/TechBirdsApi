# 🧪 **TechBirds API - Complete Testing Guide**

## 🔐 **AdminAuthController Tests**

### **1. Admin Registration** `/api/admin/auth/register` 
**Method:** `POST`

#### **Request Body (All Parameters):**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "admin@techbirds.com",
  "password": "AdminPass123!",
  "bio": "Senior System Administrator with 10 years of experience in web development and DevOps",
  "avatar": "https://example.com/avatars/john-doe.jpg",
  "website": "https://johndoe.dev",
  "twitter": "https://twitter.com/johndoe",
  "linkedin": "https://linkedin.com/in/johndoe",
  "specialization": "Full Stack Development & System Architecture"
}
```

#### **Expected Response:**
```json
{
  "message": "Admin user created successfully"
}
```

---

### **2. Admin Login** `/api/admin/auth/login`
**Method:** `POST`

#### **Request Body:**
```json
{
  "email": "admin@techbirds.com",
  "password": "AdminPass123!"
}
```

#### **Expected Response (Full User Data):**
```json
{
  "user": {
    "id": "12345678-1234-1234-1234-123456789abc",
    "firstName": "John",
    "lastName": "Doe",
    "name": "John Doe",
    "email": "admin@techbirds.com",
    "role": "Admin",
    "bio": "Senior System Administrator with 10 years of experience in web development and DevOps",
    "avatar": "https://example.com/avatars/john-doe.jpg",
    "website": "https://johndoe.dev",
    "twitter": "https://twitter.com/johndoe",
    "linkedin": "https://linkedin.com/in/johndoe",
    "specialization": "Full Stack Development & System Architecture",
    "articleCount": 0,
    "postsCount": 0,
    "totalViews": 0,
    "createdAt": "2025-08-09T10:30:00Z",
    "joinedAt": "2025-08-09T10:30:00Z",
    "lastActive": "2025-08-09T10:30:00Z"
  },
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-08-10T10:30:00Z"
}
```

---

### **3. Get Current Admin** `/api/admin/auth/me`
**Method:** `GET`
**Headers:** `Authorization: Bearer {token}`

#### **Expected Response:**
```json
{
  "id": "12345678-1234-1234-1234-123456789abc",
  "firstName": "John",
  "lastName": "Doe",
  "name": "John Doe",
  "email": "admin@techbirds.com",
  "role": "Admin",
  "bio": "Senior System Administrator with 10 years of experience in web development and DevOps",
  "avatar": "https://example.com/avatars/john-doe.jpg",
  "website": "https://johndoe.dev",
  "twitter": "https://twitter.com/johndoe",
  "linkedin": "https://linkedin.com/in/johndoe",
  "specialization": "Full Stack Development & System Architecture",
  "articleCount": 0,
  "postsCount": 0,
  "totalViews": 0,
  "createdAt": "2025-08-09T10:30:00Z",
  "joinedAt": "2025-08-09T10:30:00Z",
  "lastActive": "2025-08-09T10:30:00Z"
}
```

---

### **4. Admin Logout** `/api/admin/auth/logout`
**Method:** `POST`
**Headers:** `Authorization: Bearer {token}`

#### **Expected Response:**
```json
{
  "message": "Logged out successfully"
}
```

---

## 🔐 **Regular AuthController Tests**

> **⚠️ NOTE:** User registration is handled exclusively by administrators through the AdminAuthController. Regular users cannot self-register.

### **5. User Login** `/api/auth/login`
**Method:** `POST`

#### **Request Body:**
```json
{
  "email": "user@techbirds.com",
  "password": "UserPass123!"
}
```

#### **Expected Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "role": "Subscriber"
}
```

---

## 🧪 **Testing Commands (cURL)**

### **Test 1: Admin Registration (Only way to create users)**
```bash
curl -X POST http://localhost:5001/api/admin/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Doe", 
    "email": "admin@techbirds.com",
    "password": "AdminPass123!",
    "bio": "Senior System Administrator with 10 years of experience",
    "avatar": "https://example.com/avatars/john-doe.jpg",
    "website": "https://johndoe.dev",
    "twitter": "https://twitter.com/johndoe",
    "linkedin": "https://linkedin.com/in/johndoe",
    "specialization": "Full Stack Development & System Architecture"
  }'
```

### **Test 2: Admin Login**
```bash
curl -X POST http://localhost:5001/api/admin/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@techbirds.com",
    "password": "AdminPass123!"
  }'
```

### **Test 3: Get Admin Profile** 
```bash
curl -X GET http://localhost:5001/api/admin/auth/me \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### **Test 4: User Login (Users created by admin only)**
```bash
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@techbirds.com",
    "password": "UserPass123!"
  }'
```

---

## 📊 **Complete Field Mapping**

### **AdminRegisterRequest Fields:**
| Field | Type | Required | Description | Example |
|-------|------|----------|-------------|---------|
| `firstName` | string | ✅ | First name | "John" |
| `lastName` | string | ✅ | Last name | "Doe" |
| `email` | string | ✅ | Email address | "admin@techbirds.com" |
| `password` | string | ✅ | Password | "AdminPass123!" |
| `bio` | string? | ❌ | Biography | "System Administrator..." |
| `avatar` | string? | ❌ | Profile image URL | "https://example.com/avatar.jpg" |
| `website` | string? | ❌ | Personal website | "https://johndoe.dev" |
| `twitter` | string? | ❌ | Twitter URL | "https://twitter.com/johndoe" |
| `linkedin` | string? | ❌ | LinkedIn URL | "https://linkedin.com/in/johndoe" |
| `specialization` | string? | ❌ | Area of expertise | "Full Stack Development" |

### **AdminUserResponse Fields:**
| Field | Type | Description | Auto-Generated |
|-------|------|-------------|----------------|
| `id` | string | User GUID | ✅ |
| `firstName` | string | First name | From request |
| `lastName` | string | Last name | From request |
| `name` | string | Full name | ✅ Combined |
| `email` | string | Email address | From request |
| `role` | string | User role | ✅ "Admin" |
| `bio` | string | Biography | From request |
| `avatar` | string? | Profile image | From request |
| `website` | string? | Website URL | From request |
| `twitter` | string? | Twitter URL | From request |
| `linkedin` | string? | LinkedIn URL | From request |
| `specialization` | string? | Specialization | From request |
| `articleCount` | int | Article count | ✅ 0 |
| `postsCount` | int | Posts count | ✅ 0 |
| `totalViews` | int | Total views | ✅ 0 |
| `createdAt` | DateTime | Creation time | ✅ UTC Now |
| `joinedAt` | DateTime | Join time | ✅ UTC Now |
| `lastActive` | DateTime? | Last activity | ✅ UTC Now |

---

## 🚀 **Testing Results Expected:**

1. ✅ **Admin Registration**: Creates admin with all 15+ fields
2. ✅ **Admin Login**: Returns full user profile + JWT token
3. ✅ **Get Profile**: Shows complete admin data
4. ✅ **User Login**: Only users created by admins can login
5. ✅ **Admin-Only Registration**: Regular users cannot self-register
6. ✅ **No Authors Table**: System uses only AspNetUsers table

**Your API now supports admin-controlled user management with 23 fields per admin user!** 🎯
