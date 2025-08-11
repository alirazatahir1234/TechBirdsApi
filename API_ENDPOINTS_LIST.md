# 🔌 **TechBirds API - Complete Endpoints List**

## 📋 **ALL EXPOSED API ENDPOINTS**

---

## 🔐 **AUTHENTICATION ENDPOINTS**

### **Regular User Authentication** (`/api/auth`)
```http
POST   /api/auth/register        # User registration
POST   /api/auth/login           # User login
POST   /api/auth/assign-role     # Assign role to user (Admin only)
```

### **Admin Authentication** (`/api/admin/auth`)
```http
POST   /api/admin/auth/register  # Admin registration
POST   /api/admin/auth/login     # Admin login
GET    /api/admin/auth/me        # Get current admin info
POST   /api/admin/auth/logout    # Admin logout
```

---

## 👥 **USER MANAGEMENT ENDPOINTS**

### **Public User Endpoints** (`/api/users`)
```http
GET    /api/users               # Get all users (public data only)
GET    /api/users/{id}          # Get specific user profile (public)
```

### **Authenticated User Endpoints** (`/api/users`)
```http
GET    /api/users/profile       # Get current user's full profile (Auth Required)
PUT    /api/users/profile       # Update current user's profile (Auth Required)
```

### **Admin User Management** (`/api/users/admin`)
```http
GET    /api/users/admin/activities        # View user activities (SuperAdmin/Admin)
GET    /api/users/admin/exceptions        # View system exceptions (SuperAdmin/Admin)
PUT    /api/users/admin/exceptions/{id}/resolve  # Mark exception as resolved (SuperAdmin/Admin)
```

---

## 📰 **ARTICLES ENDPOINTS**

### **Public Article Endpoints** (`/api/articles`)
```http
GET    /api/articles            # Get all articles
GET    /api/articles/{id}       # Get specific article
```

### **Authenticated Article Endpoints** (`/api/articles`)
```http
POST   /api/articles            # Create new article (Author+ role required)
PUT    /api/articles/{id}       # Update article (Owner or Editor+ required)
DELETE /api/articles/{id}       # Delete article (Owner or Editor+ required)
```

---

## 💬 **COMMENTS ENDPOINTS**

### **Public Comment Endpoints** (`/api/comments`)
```http
GET    /api/comments/article/{articleId}  # Get comments for article
GET    /api/comments/post/{postId}        # Get comments for post
GET    /api/comments/{id}                 # Get specific comment
```

### **Authenticated Comment Endpoints** (`/api/comments`)
```http
POST   /api/comments            # Create comment (Auth Required)
PUT    /api/comments/{id}       # Update comment (Owner or Admin+ required)
DELETE /api/comments/{id}       # Delete comment (Owner or Admin+ required)
```

### **Admin Comment Management** (`/api/comments/admin`)
```http
GET    /api/comments/admin/pending     # Get pending comments (Admin+ required)
PUT    /api/comments/{id}/approve      # Approve comment (Admin+ required)
```

---

## 📂 **CATEGORIES ENDPOINTS**

### **Public Category Endpoints** (`/api/categories`)
```http
GET    /api/categories          # Get all categories
GET    /api/categories/{id}     # Get specific category
```

### **Admin Category Management** (`/api/categories`)
```http
POST   /api/categories          # Create category (Admin+ required)
PUT    /api/categories/{id}     # Update category (Admin+ required)
DELETE /api/categories/{id}     # Delete category (Admin+ required)
```

---

## 📝 **POSTS ENDPOINTS** (if implemented)

### **Public Post Endpoints** (`/api/posts`)
```http
GET    /api/posts               # Get all posts
GET    /api/posts/{id}          # Get specific post
```

### **Authenticated Post Endpoints** (`/api/posts`)
```http
POST   /api/posts               # Create post (Author+ required)
PUT    /api/posts/{id}          # Update post (Owner or Editor+ required)
DELETE /api/posts/{id}          # Delete post (Owner or Editor+ required)
```

---

## 🔧 **SYSTEM ENDPOINTS**

### **API Documentation**
```http
GET    /swagger                 # Swagger UI documentation
GET    /swagger/v1/swagger.json # OpenAPI specification
```

### **Health Check** (if implemented)
```http
GET    /health                  # API health status
```

---

## 🔒 **AUTHENTICATION & AUTHORIZATION LEVELS**

### **Public Access** (No Auth Required)
- GET endpoints for articles, categories, comments (viewing)
- User registration and login
- Swagger documentation

### **Authenticated Users** (JWT Token Required)
- Create comments
- Update own profile
- View own full profile data

### **Content Creators** (Author+ Role)
- Create articles/posts
- Edit own content

### **Moderators** (Editor+ Role)
- Edit any articles/posts
- Moderate comments

### **Administrators** (Admin+ Role)
- User management
- Category management
- Comment moderation
- View user activities and system exceptions

### **Super Administrators** (SuperAdmin Role)
- Full system access
- Advanced logging and monitoring
- System configuration

---

## 📊 **ENDPOINT SUMMARY BY CATEGORY**

| Category | Public | Auth Required | Admin Only | Total |
|----------|---------|---------------|------------|-------|
| **Authentication** | 2 | 0 | 5 | **7** |
| **Users** | 2 | 2 | 3 | **7** |
| **Articles** | 2 | 3 | 0 | **5** |
| **Comments** | 3 | 3 | 2 | **8** |
| **Categories** | 2 | 0 | 3 | **5** |
| **System** | 2 | 0 | 0 | **2** |
| **TOTAL** | **13** | **8** | **13** | **34** |

---

## 🌐 **BASE URL**
```
http://localhost:5001
```

## 📱 **API Testing**
- **Swagger UI**: `http://localhost:5001/swagger`
- **OpenAPI Spec**: `http://localhost:5001/swagger/v1/swagger.json`

---

## 🔑 **JWT Token Usage**
For authenticated endpoints, include in headers:
```http
Authorization: Bearer {your-jwt-token}
```

**Your TechBirds API exposes 34+ comprehensive endpoints covering authentication, user management, content management, comments system, and admin operations!** 🚀
