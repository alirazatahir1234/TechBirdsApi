# TechBirds API - Frontend Integration Guide

## 🚨 **BREAKING CHANGES SUMMARY**

This document outlines all backend changes and provides implementation guidance for frontend integration.

---

## 📋 **TABLE OF CONTENTS**
1. [Architecture Changes](#architecture-changes)
2. [Authentication & User Management](#authentication--user-management)
3. [API Endpoints Changes](#api-endpoints-changes)
4. [Comments API - Enhanced with UserId](#comments-api---enhanced-with-userid)
5. [Data Models & DTOs](#data-models--dtos)
6. [Error Handling](#error-handling)
7. [Logging & Activity Tracking](#logging--activity-tracking)
8. [Frontend Implementation Examples](#frontend-implementation-examples)
9. [Migration Checklist](#migration-checklist)

---

## 🏗️ **ARCHITECTURE CHANGES**

### ❌ **REMOVED SYSTEMS**
- **Authors API**: Completely removed `/api/authors` endpoints
- **Duplicate User System**: No more separate authors table
- **Old Role System**: Replaced with ASP.NET Core Identity roles

### ✅ **NEW UNIFIED SYSTEM**
- **Single User System**: Only `AspNetUsers` table (ApplicationUser)
- **Identity-Based Authentication**: Full ASP.NET Core Identity integration
- **Enhanced User Profiles**: Rich user data with social links and statistics
- **Comprehensive Logging**: Activity tracking and exception management
- **Enhanced Comments System**: Full user integration with userId tracking

---

## 🔐 **AUTHENTICATION & USER MANAGEMENT**

### **New Role Hierarchy** (6-Tier System)
```javascript
const ROLES = {
  SUBSCRIBER: 'Subscriber',     // Level 1 - Basic users
  CONTRIBUTOR: 'Contributor',   // Level 2 - Can contribute content
  AUTHOR: 'Author',            // Level 3 - Can publish articles
  ADMIN: 'Admin',              // Level 4 - Site administration
  EDITOR: 'Editor',            // Level 5 - Editorial control
  SUPERADMIN: 'SuperAdmin'     // Level 6 - Full system access
};
```

### **Authentication Endpoints**

#### **Register User**
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "name": "John Doe",
  "bio": "Content creator and tech enthusiast"
}
```

**Response:**
```json
{
  "message": "User created successfully"
}
```

#### **Login User**
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "role": "Author"
}
```

---

## 🔄 **API ENDPOINTS CHANGES**

### **❌ REMOVED ENDPOINTS**
```
DELETE /api/authors/*           # All author endpoints removed
DELETE /api/authors/{id}        # No longer exists
PUT /api/authors/{id}           # No longer exists  
GET /api/authors                # No longer exists
POST /api/authors               # No longer exists
```

### **✅ NEW USER ENDPOINTS**

#### **Public User Endpoints**
```http
GET /api/users                  # Get all users (public data only)
GET /api/users/{id}             # Get specific user profile (public)
```

#### **Authenticated User Endpoints**
```http
GET /api/users/profile          # Get current user's full profile
PUT /api/users/profile          # Update current user's profile
```

#### **Admin Endpoints** (SuperAdmin/Admin only)
```http
GET /api/users/admin/activities        # View user activities
GET /api/users/admin/exceptions        # View system exceptions
PUT /api/users/admin/exceptions/{id}/resolve  # Mark exception as resolved
```

---

## 💬 **COMMENTS API - Enhanced with UserId**

The Comments API has been completely refactored to use `userId` instead of `authorName` and now includes full authentication, authorization, and user data.

### **✅ NEW ENHANCED COMMENTS ENDPOINTS**

#### **Get Comments for Article**
```http
GET /api/comments/article/{articleId}
```

**Response:**
```json
[
  {
    "id": 1,
    "articleId": 123,
    "content": "Great article! Very informative.",
    "createdAt": "2025-08-09T10:30:00Z",
    "updatedAt": null,
    "isApproved": true,
    "user": {
      "id": "user-guid-here",
      "name": "John Doe",
      "avatar": "https://example.com/avatar.jpg",
      "specialization": "Web Development"
    }
  }
]
```

#### **Get Comments for Post**
```http
GET /api/comments/post/{postId}
```

**Response:** Same structure as article comments

#### **Create Comment** (Authenticated Users Only)
```http
POST /api/comments
Authorization: Bearer {jwt-token}
Content-Type: application/json

{
  "articleId": 123,    // OR "postId": 456 (not both)
  "content": "This is my comment content"
}
```

**Response:**
```json
{
  "id": 1,
  "articleId": 123,
  "postId": null,
  "content": "This is my comment content",
  "createdAt": "2025-08-09T10:30:00Z",
  "isApproved": true,
  "user": {
    "id": "current-user-guid",
    "name": "Current User",
    "avatar": "https://example.com/avatar.jpg",
    "specialization": "Content Creator"
  }
}
```

#### **Update Comment** (Owner or Admin/Editor only)
```http
PUT /api/comments/{id}
Authorization: Bearer {jwt-token}
Content-Type: application/json

{
  "content": "Updated comment content"
}
```

#### **Delete Comment** (Owner or Admin/Editor only)
```http
DELETE /api/comments/{id}
Authorization: Bearer {jwt-token}
```

#### **Admin Endpoints** (Admin/Editor/SuperAdmin only)
```http
GET /api/comments/admin/pending         # Get pending comments for approval
PUT /api/comments/{id}/approve          # Approve a comment
```

### **🔒 COMMENTS SECURITY FEATURES**
- **Authentication Required**: POST, PUT, DELETE require valid JWT token
- **User Ownership**: Users can only edit/delete their own comments
- **Role-Based Permissions**: Admin/Editor/SuperAdmin can manage any comments
- **Content Validation**: 2000 character limit, required content
- **Activity Logging**: All comment actions are logged
- **Moderation Support**: Comments can be approved/pending

### **📊 COMMENTS DATA CHANGES**
```typescript
// OLD Comment Model (REMOVED)
interface OldComment {
  id: number;
  articleId: number;
  authorName: string;    // ❌ REMOVED - was just a string
  content: string;
  createdAt: string;
}

// NEW Comment Model (CURRENT)
interface NewComment {
  id: number;
  articleId?: number;    // Optional - for article comments
  postId?: number;       // Optional - for post comments
  content: string;       // Max 2000 characters
  createdAt: string;
  updatedAt?: string;    // NEW - tracks edits
  isApproved: boolean;   // NEW - moderation support
  user: {                // NEW - full user object
    id: string;
    name: string;
    avatar?: string;
    specialization?: string;
  }
}
```

---

## 📊 **DATA MODELS & DTOs**

### **Enhanced User Model**
```typescript
interface ApplicationUser {
  // Identity Fields
  id: string;
  userName: string;
  email: string;
  emailConfirmed: boolean;
  
  // Profile Fields
  name: string;              // Display name (max 100 chars)
  firstName: string;         // NEW FIELD
  lastName: string;          // NEW FIELD
  bio: string;              // Biography (max 1000 chars)
  avatar?: string;          // Profile image URL (max 500 chars)
  
  // Social Links
  website?: string;         // Website URL (max 200 chars)
  twitter?: string;         // Twitter handle (max 100 chars)
  linkedin?: string;        // LinkedIn URL (max 200 chars)
  
  // Professional Info
  specialization?: string;  // Area of expertise (max 200 chars)
  
  // Statistics (Read-only)
  articleCount: number;     // Number of published articles
  postsCount: number;       // NEW FIELD - Total posts count
  totalViews: number;       // Total article views
  
  // Timestamps
  createdAt: string;        // Account creation date
  joinedAt: string;         // When user joined platform
  lastActive?: string;      // Last activity timestamp
  updatedAt?: string;       // Profile last updated
}
```

### **Article Model Changes**
```typescript
interface Article {
  id: number;
  title: string;
  content: string;
  userId: string;          // CHANGED: Now references AspNetUsers.Id
  user?: PublicUserDto;    // CHANGED: Now contains user object instead of author
  categoryId: number;
  category?: Category;
  
  // Enhanced fields
  excerpt?: string;
  slug?: string;
  featuredImage?: string;
  imageUrl?: string;
  tags?: string;
  status: string;
  featured: boolean;
  allowComments: boolean;
  viewCount: number;
  readTime: number;
  
  // SEO fields
  metaDescription?: string;
  metaKeywords?: string;
  
  // Timestamps
  createdAt: string;
  updatedAt?: string;
  publishedAt?: string;
}
```

---

## 💻 **FRONTEND IMPLEMENTATION EXAMPLES**

### **1. Enhanced Comments Service**
```typescript
class CommentsService {
  private baseUrl = 'http://localhost:5001/api/comments';
  private authService = new AuthService();

  async getArticleComments(articleId: number): Promise<Comment[]> {
    const response = await fetch(`${this.baseUrl}/article/${articleId}`);
    if (!response.ok) throw new Error('Failed to fetch comments');
    return response.json();
  }

  async getPostComments(postId: number): Promise<Comment[]> {
    const response = await fetch(`${this.baseUrl}/post/${postId}`);
    if (!response.ok) throw new Error('Failed to fetch comments');
    return response.json();
  }

  async createComment(commentData: CreateCommentRequest): Promise<Comment> {
    const response = await fetch(this.baseUrl, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...this.authService.getAuthHeaders()
      },
      body: JSON.stringify(commentData)
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Failed to create comment');
    }

    return response.json();
  }

  async updateComment(commentId: number, content: string): Promise<void> {
    const response = await fetch(`${this.baseUrl}/${commentId}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        ...this.authService.getAuthHeaders()
      },
      body: JSON.stringify({ content })
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Failed to update comment');
    }
  }

  async deleteComment(commentId: number): Promise<void> {
    const response = await fetch(`${this.baseUrl}/${commentId}`, {
      method: 'DELETE',
      headers: this.authService.getAuthHeaders()
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Failed to delete comment');
    }
  }

  // Admin functions
  async getPendingComments(): Promise<Comment[]> {
    const response = await fetch(`${this.baseUrl}/admin/pending`, {
      headers: this.authService.getAuthHeaders()
    });
    if (!response.ok) throw new Error('Failed to fetch pending comments');
    return response.json();
  }

  async approveComment(commentId: number): Promise<void> {
    const response = await fetch(`${this.baseUrl}/${commentId}/approve`, {
      method: 'PUT',
      headers: this.authService.getAuthHeaders()
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Failed to approve comment');
    }
  }
}

interface CreateCommentRequest {
  articleId?: number;
  postId?: number;
  content: string;
}

interface Comment {
  id: number;
  articleId?: number;
  postId?: number;
  content: string;
  createdAt: string;
  updatedAt?: string;
  isApproved: boolean;
  user: {
    id: string;
    name: string;
    avatar?: string;
    specialization?: string;
  };
}
```

### **2. Comment Component (React Example)**
```tsx
interface CommentComponentProps {
  articleId?: number;
  postId?: number;
  currentUser?: ApplicationUser;
}

const CommentComponent: React.FC<CommentComponentProps> = ({ 
  articleId, 
  postId, 
  currentUser 
}) => {
  const [comments, setComments] = useState<Comment[]>([]);
  const [newComment, setNewComment] = useState('');
  const [loading, setLoading] = useState(false);
  const commentsService = new CommentsService();

  useEffect(() => {
    loadComments();
  }, [articleId, postId]);

  const loadComments = async () => {
    try {
      const data = articleId 
        ? await commentsService.getArticleComments(articleId)
        : await commentsService.getPostComments(postId!);
      setComments(data);
    } catch (error) {
      console.error('Failed to load comments:', error);
    }
  };

  const handleSubmitComment = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newComment.trim() || !currentUser) return;

    setLoading(true);
    try {
      const comment = await commentsService.createComment({
        articleId,
        postId,
        content: newComment.trim()
      });
      
      setComments([...comments, comment]);
      setNewComment('');
    } catch (error) {
      alert(`Failed to post comment: ${error.message}`);
    } finally {
      setLoading(false);
    }
  };

  const handleEditComment = async (commentId: number, newContent: string) => {
    try {
      await commentsService.updateComment(commentId, newContent);
      await loadComments(); // Reload to show updated comment
    } catch (error) {
      alert(`Failed to edit comment: ${error.message}`);
    }
  };

  const handleDeleteComment = async (commentId: number) => {
    if (!confirm('Are you sure you want to delete this comment?')) return;

    try {
      await commentsService.deleteComment(commentId);
      setComments(comments.filter(c => c.id !== commentId));
    } catch (error) {
      alert(`Failed to delete comment: ${error.message}`);
    }
  };

  const canEditComment = (comment: Comment): boolean => {
    if (!currentUser) return false;
    return comment.user.id === currentUser.id || 
           currentUser.roles?.some(r => ['Admin', 'Editor', 'SuperAdmin'].includes(r));
  };

  return (
    <div className="comments-section">
      <h3>Comments ({comments.length})</h3>
      
      {/* Comment Form */}
      {currentUser && (
        <form onSubmit={handleSubmitComment} className="comment-form">
          <div className="user-info">
            <img src={currentUser.avatar || '/default-avatar.png'} alt="Avatar" />
            <span>{currentUser.name}</span>
          </div>
          <textarea
            value={newComment}
            onChange={(e) => setNewComment(e.target.value)}
            placeholder="Write your comment..."
            maxLength={2000}
            rows={3}
            required
          />
          <div className="form-actions">
            <span className="char-count">{newComment.length}/2000</span>
            <button type="submit" disabled={loading || !newComment.trim()}>
              {loading ? 'Posting...' : 'Post Comment'}
            </button>
          </div>
        </form>
      )}

      {/* Comments List */}
      <div className="comments-list">
        {comments.map(comment => (
          <div key={comment.id} className="comment">
            <div className="comment-header">
              <img 
                src={comment.user.avatar || '/default-avatar.png'} 
                alt={comment.user.name}
                className="user-avatar"
              />
              <div className="user-details">
                <span className="user-name">{comment.user.name}</span>
                {comment.user.specialization && (
                  <span className="user-specialization">
                    {comment.user.specialization}
                  </span>
                )}
                <span className="comment-date">
                  {new Date(comment.createdAt).toLocaleDateString()}
                  {comment.updatedAt && ' (edited)'}
                </span>
              </div>
              
              {canEditComment(comment) && (
                <div className="comment-actions">
                  <button onClick={() => handleEditComment(comment.id, prompt('Edit comment:', comment.content) || comment.content)}>
                    Edit
                  </button>
                  <button onClick={() => handleDeleteComment(comment.id)}>
                    Delete
                  </button>
                </div>
              )}
            </div>
            
            <div className="comment-content">
              {comment.content}
            </div>
          </div>
        ))}
        
        {comments.length === 0 && (
          <p className="no-comments">No comments yet. Be the first to comment!</p>
        )}
      </div>
    </div>
  );
};
```

---

## ⚠️ **ERROR HANDLING**

### **Standardized Error Responses**
```typescript
interface ApiErrorResponse {
  message: string;
  errors?: string[];      // Validation errors
  statusCode: number;
}
```

### **Comments API Error Examples**
```json
// Validation Error (400)
{
  "message": "Content is required",
  "statusCode": 400
}

// Content too long (400)
{
  "message": "Content cannot exceed 2000 characters",
  "statusCode": 400
}

// Unauthorized (401)
{
  "message": "Unauthorized",
  "statusCode": 401
}

// Forbidden - can't edit others' comments (403)
{
  "message": "Access denied",
  "statusCode": 403
}

// Comment not found (404)
{
  "message": "Comment not found",
  "statusCode": 404
}
```

---

## ✅ **MIGRATION CHECKLIST**

### **Comments System Migration**

#### **1. Update Comments Service** ✅
```typescript
// Replace old comment structure:
// - Remove authorName field
// - Add user object with id, name, avatar, specialization
// - Add authentication headers for POST/PUT/DELETE
// - Handle both articleId and postId
// - Add error handling for all operations
```

#### **2. Update Comment Components** ✅
```typescript
// Changes required:
// - Replace authorName with user.name
// - Add user avatar display
// - Add authentication checks for creating comments
// - Add edit/delete permissions based on ownership
// - Add character count validation (2000 max)
// - Handle both article and post comments
```

#### **3. Add Authentication Integration** ✅
```typescript
// Implement:
// - JWT token handling for comment operations
// - User permission checks (edit own comments only)
// - Admin role checks for comment management
// - Login requirement for posting comments
```

#### **4. Testing Checklist** ✅
- [ ] Anonymous users can view comments
- [ ] Authenticated users can create comments
- [ ] Users can edit their own comments
- [ ] Users can delete their own comments
- [ ] Admins/Editors can manage any comments
- [ ] Content validation works (max 2000 chars)
- [ ] User avatars and info display correctly
- [ ] Error messages are user-friendly
- [ ] Both article and post comments work

---

## 📞 **SUPPORT & QUESTIONS**

If you encounter issues during migration:

1. **Check Authentication**: Ensure JWT tokens are properly included in comment operations
2. **Verify User Data**: Comments now include full user objects instead of just names
3. **Test Permissions**: Verify role-based access control for comment management
4. **Monitor API Responses**: Look for proper userId in comment data

---

**Last Updated**: August 9, 2025  
**API Version**: v2.0  
**Breaking Changes**: Yes - Complete author system removal, user system refactoring, and enhanced comments with userId integration
