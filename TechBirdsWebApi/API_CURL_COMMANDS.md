# TechBirds Web API - Complete cURL Commands Collection

## Base URLs
- HTTP: `http://localhost:5001`
- HTTPS: `https://localhost:7001`

## Authentication Token
After login, use the returned JWT token in the Authorization header:
```
Authorization: Bearer YOUR_JWT_TOKEN_HERE
```

---

## 🔐 AUTHENTICATION

### Admin Login
```bash
curl -X POST "http://localhost:5001/api/admin/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@techbirds.com",
    "password": "Admin123!"
  }'
```

### Admin Register
```bash
curl -X POST "http://localhost:5001/api/admin/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "password": "Password123!",
    "bio": "Software Developer",
    "avatar": "https://example.com/avatar.jpg",
    "website": "https://johndoe.com",
    "twitter": "@johndoe",
    "linkedIn": "https://linkedin.com/in/johndoe",
    "specialization": "Full Stack Development",
    "role": "Admin"
  }'
```

---

## 👥 USERS MANAGEMENT

### Get All Users (Public)
```bash
curl -X GET "http://localhost:5001/api/users?page=1&limit=10&search=&role=&specialization=&sortBy=joined&sortOrder=desc"
```

### Create User (Admin Only)
```bash
curl -X POST "http://localhost:5001/api/users" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "firstName": "Ali",
    "lastName": "Raza",
    "email": "ali@gmail.com",
    "password": "Alisheikh@123",
    "bio": "Item boomb",
    "avatar": "https://google.com",
    "website": "https://google.com",
    "twitter": "https://google.com",
    "linkedIn": "https://google.com",
    "specialization": "Item",
    "role": "Admin",
    "isActive": true
  }'
```

### Get User by ID
```bash
curl -X GET "http://localhost:5001/api/users/USER_ID_HERE"
```

### Get Current User Profile
```bash
curl -X GET "http://localhost:5001/api/users/profile" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Update User Profile
```bash
curl -X PUT "http://localhost:5001/api/users/profile" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "name": "John Doe Updated",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.updated@example.com",
    "password": "NewPassword123!",
    "bio": "Updated bio",
    "avatar": "https://example.com/new-avatar.jpg",
    "website": "https://johndoe-updated.com",
    "twitter": "@johndoe_updated",
    "linkedIn": "https://linkedin.com/in/johndoe-updated",
    "specialization": "Senior Full Stack Developer",
    "role": "Editor",
    "isActive": true
  }'
```

### Get User Activities
```bash
curl -X GET "http://localhost:5001/api/users/activities?page=1&limit=50" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

---

## 🔧 ADMIN - USER MANAGEMENT

### Get All User Activities (Admin)
```bash
curl -X GET "http://localhost:5001/api/users/admin/activities?userId=&page=1&limit=50" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Get System Exceptions (Admin)
```bash
curl -X GET "http://localhost:5001/api/users/admin/exceptions?userId=&unResolvedOnly=false&page=1&limit=50" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Resolve Exception (Admin)
```bash
curl -X PUT "http://localhost:5001/api/users/admin/exceptions/1/resolve" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "resolution": "Issue resolved by updating the database schema"
  }'
```

---

## 📝 POSTS MANAGEMENT

### Get All Posts (Public)
```bash
curl -X GET "http://localhost:5001/api/posts?page=1&limit=10&category=&search=&sortBy=created&sortOrder=desc"
```

### Get Post by ID
```bash
curl -X GET "http://localhost:5001/api/posts/1"
```

### Get Post by Slug
```bash
curl -X GET "http://localhost:5001/api/posts/slug/sample-post-slug"
```

---

## 🔧 ADMIN - POSTS MANAGEMENT

### Get All Posts (Admin)
```bash
curl -X GET "http://localhost:5001/api/admin/posts?page=1&limit=10&status=&category=&search=&sortBy=created&sortOrder=desc" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Create Post (Admin)
```bash
curl -X POST "http://localhost:5001/api/admin/posts" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "title": "Sample Blog Post",
    "slug": "sample-blog-post",
    "content": "This is the content of the blog post...",
    "excerpt": "Brief description of the post",
    "featuredImage": "https://example.com/featured-image.jpg",
    "categoryId": 1,
    "tags": ["technology", "programming", "web development"],
    "status": "published",
    "metaTitle": "Sample Blog Post - TechBirds",
    "metaDescription": "A sample blog post about technology"
  }'
```

### Get Post by ID (Admin)
```bash
curl -X GET "http://localhost:5001/api/admin/posts/1" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Update Post (Admin)
```bash
curl -X PUT "http://localhost:5001/api/admin/posts/1" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "title": "Updated Blog Post Title",
    "slug": "updated-blog-post",
    "content": "Updated content of the blog post...",
    "excerpt": "Updated brief description",
    "featuredImage": "https://example.com/updated-image.jpg",
    "categoryId": 1,
    "tags": ["technology", "programming", "updated"],
    "status": "published",
    "metaTitle": "Updated Blog Post - TechBirds",
    "metaDescription": "An updated blog post about technology"
  }'
```

### Delete Post (Admin)
```bash
curl -X DELETE "http://localhost:5001/api/admin/posts/1" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

---

## 📂 CATEGORIES

### Get All Categories
```bash
curl -X GET "http://localhost:5001/api/categories"
```

### Get Category by ID
```bash
curl -X GET "http://localhost:5001/api/categories/1"
```

---

## 🔧 ADMIN - CATEGORIES

### Get All Categories (Admin)
```bash
curl -X GET "http://localhost:5001/api/admin/categories" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Create Category (Admin)
```bash
curl -X POST "http://localhost:5001/api/admin/categories" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "name": "Technology",
    "slug": "technology",
    "description": "Posts about technology and innovation",
    "color": "#007bff",
    "isActive": true
  }'
```

### Update Category (Admin)
```bash
curl -X PUT "http://localhost:5001/api/admin/categories/1" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "name": "Updated Technology",
    "slug": "updated-technology",
    "description": "Updated description for technology category",
    "color": "#28a745",
    "isActive": true
  }'
```

### Delete Category (Admin)
```bash
curl -X DELETE "http://localhost:5001/api/admin/categories/1" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

---

## 💬 COMMENTS

### Get Post Comments
```bash
curl -X GET "http://localhost:5001/api/comments/post/1?page=1&limit=10"
```

### Create Comment
```bash
curl -X POST "http://localhost:5001/api/comments" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "postId": 1,
    "content": "This is a great blog post! Thanks for sharing.",
    "parentId": null
  }'
```

### Update Comment
```bash
curl -X PUT "http://localhost:5001/api/comments/1" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "content": "Updated comment content with more details."
  }'
```

### Delete Comment
```bash
curl -X DELETE "http://localhost:5001/api/comments/1" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

---

## 🔧 ADMIN - COMMENTS

### Get All Comments (Admin)
```bash
curl -X GET "http://localhost:5001/api/admin/comments?page=1&limit=10&status=&postId=" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Approve Comment (Admin)
```bash
curl -X PUT "http://localhost:5001/api/admin/comments/1/approve" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Reject Comment (Admin)
```bash
curl -X PUT "http://localhost:5001/api/admin/comments/1/reject" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

---

## 🔍 SEARCH

### Search Posts
```bash
curl -X GET "http://localhost:5001/api/search?q=technology&type=posts&page=1&limit=10"
```

### Search All Content
```bash
curl -X GET "http://localhost:5001/api/search?q=technology&page=1&limit=10"
```

---

## 📧 NEWSLETTER

### Subscribe to Newsletter
```bash
curl -X POST "http://localhost:5001/api/newsletter/subscribe" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "subscriber@example.com",
    "firstName": "John",
    "lastName": "Doe"
  }'
```

### Unsubscribe from Newsletter
```bash
curl -X POST "http://localhost:5001/api/newsletter/unsubscribe" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "subscriber@example.com"
  }'
```

---

## 🔧 ADMIN - NEWSLETTER

### Get All Subscribers (Admin)
```bash
curl -X GET "http://localhost:5001/api/admin/newsletter/subscribers?page=1&limit=10&status=active" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Send Newsletter (Admin)
```bash
curl -X POST "http://localhost:5001/api/admin/newsletter/send" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "subject": "Weekly Tech Newsletter",
    "content": "<h1>This week in technology...</h1><p>Content goes here...</p>",
    "scheduledFor": null
  }'
```

---

## 📊 ADMIN - DASHBOARD

### Get Dashboard Stats
```bash
curl -X GET "http://localhost:5001/api/admin/dashboard/stats" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Get Recent Activity
```bash
curl -X GET "http://localhost:5001/api/admin/dashboard/activity?limit=10" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

---

## ❤️ HEALTH CHECK

### Health Check
```bash
curl -X GET "http://localhost:5001/api/health"
```

### Database Health Check
```bash
curl -X GET "http://localhost:5001/api/health/database"
```

---

## 📱 USAGE EXAMPLES

### Complete User Creation Flow
```bash
# 1. Login as Admin
TOKEN=$(curl -s -X POST "http://localhost:5001/api/admin/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email": "admin@techbirds.com", "password": "Admin123!"}' | \
  jq -r '.token')

# 2. Create a new user
curl -X POST "http://localhost:5001/api/users" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "firstName": "Ali",
    "lastName": "Raza",
    "email": "ali@gmail.com",
    "password": "Alisheikh@123",
    "bio": "Item boomb",
    "avatar": "https://google.com",
    "website": "https://google.com",
    "twitter": "https://google.com",
    "linkedIn": "https://google.com",
    "specialization": "Item",
    "role": "Admin",
    "isActive": true
  }'
```

### Testing Complete Post Management
```bash
# 1. Get admin token (as above)
TOKEN=$(curl -s -X POST "http://localhost:5001/api/admin/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email": "admin@techbirds.com", "password": "Admin123!"}' | \
  jq -r '.token')

# 2. Create a post
curl -X POST "http://localhost:5001/api/admin/posts" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "title": "Sample Blog Post",
    "slug": "sample-blog-post",
    "content": "This is the content of the blog post...",
    "excerpt": "Brief description of the post",
    "featuredImage": "https://example.com/featured-image.jpg",
    "categoryId": 1,
    "tags": ["technology", "programming", "web development"],
    "status": "published",
    "metaTitle": "Sample Blog Post - TechBirds",
    "metaDescription": "A sample blog post about technology"
  }'

# 3. Get all posts
curl -X GET "http://localhost:5001/api/posts"
```

---

## 📋 IMPORTANT NOTES

1. **Authentication**: Most admin endpoints require a valid JWT token
2. **Base URL**: Change `localhost:5001` to your actual server URL in production
3. **HTTPS**: Use `https://localhost:7001` for HTTPS endpoints
4. **Rate Limiting**: Some endpoints may have rate limiting in production
5. **CORS**: Make sure CORS is properly configured for your frontend domain

## 🔄 Environment Variables

For different environments, update the base URLs:
- **Development**: `http://localhost:5001`
- **Staging**: `https://staging-api.techbirds.com`
- **Production**: `https://api.techbirds.com`
