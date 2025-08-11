# 💬 **COMMENTS API - Frontend Migration Guide**

## 🚨 **BREAKING CHANGES - Comments Only**

This guide focuses **only** on the changes needed for the Comments system in your frontend.

---

## 📊 **COMMENT DATA MODEL CHANGES**

### **❌ OLD Comment Structure (Remove This)**
```typescript
// OLD - No longer works
interface OldComment {
  id: number;
  articleId: number;
  authorName: string;    // ❌ REMOVED - was just a string
  content: string;
  createdAt: string;
}
```

### **✅ NEW Comment Structure (Use This)**
```typescript
// NEW - Current structure
interface Comment {
  id: number;
  articleId?: number;    // Optional - for article comments
  postId?: number;       // Optional - for post comments
  content: string;       // Max 2000 characters
  createdAt: string;
  updatedAt?: string;    // NEW - tracks edits
  isApproved: boolean;   // NEW - moderation support
  user: {                // NEW - full user object instead of authorName
    id: string;
    name: string;
    avatar?: string;
    specialization?: string;
  }
}
```

---

## 🔄 **API ENDPOINTS CHANGES**

### **✅ Updated Comment Endpoints**

#### **Get Comments (No Auth Required)**
```http
GET /api/comments/article/{articleId}    # Get article comments
GET /api/comments/post/{postId}          # Get post comments
```

**Response Example:**
```json
[
  {
    "id": 1,
    "articleId": 123,
    "content": "Great article!",
    "createdAt": "2025-08-09T10:30:00Z",
    "isApproved": true,
    "user": {
      "id": "user-guid",
      "name": "John Doe",
      "avatar": "https://example.com/avatar.jpg",
      "specialization": "Web Development"
    }
  }
]
```

#### **Create Comment (Auth Required)**
```http
POST /api/comments
Authorization: Bearer {jwt-token}
Content-Type: application/json

{
  "articleId": 123,    // OR "postId": 456 (not both)
  "content": "This is my comment"
}
```

#### **Update/Delete Comment (Auth Required)**
```http
PUT /api/comments/{id}
DELETE /api/comments/{id}
Authorization: Bearer {jwt-token}
```

---

## 🔒 **AUTHENTICATION REQUIREMENTS**

### **What Changed:**
- **Anonymous Users**: Can only **VIEW** comments
- **Authenticated Users**: Can **CREATE** comments
- **Comment Owners**: Can **EDIT/DELETE** their own comments
- **Admins/Editors**: Can **MANAGE** any comments

### **Frontend Auth Check:**
```typescript
const canEditComment = (comment: Comment, currentUser?: User): boolean => {
  if (!currentUser) return false;
  
  // User owns the comment OR user is admin/editor
  return comment.user.id === currentUser.id || 
         currentUser.roles?.some(r => ['Admin', 'Editor', 'SuperAdmin'].includes(r));
};
```

---

## 💻 **FRONTEND CODE CHANGES**

### **1. Update Your Comment Service**

```typescript
class CommentsService {
  private baseUrl = 'http://localhost:5001/api/comments';

  // GET comments (no auth needed)
  async getArticleComments(articleId: number): Promise<Comment[]> {
    const response = await fetch(`${this.baseUrl}/article/${articleId}`);
    if (!response.ok) throw new Error('Failed to fetch comments');
    return response.json();
  }

  // CREATE comment (auth required)
  async createComment(data: {articleId?: number, postId?: number, content: string}): Promise<Comment> {
    const token = localStorage.getItem('jwt_token'); // Your token storage
    
    const response = await fetch(this.baseUrl, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify(data)
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Failed to create comment');
    }
    return response.json();
  }

  // UPDATE comment (auth required)
  async updateComment(commentId: number, content: string): Promise<void> {
    const token = localStorage.getItem('jwt_token');
    
    const response = await fetch(`${this.baseUrl}/${commentId}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({ content })
    });

    if (!response.ok) throw new Error('Failed to update comment');
  }

  // DELETE comment (auth required)
  async deleteComment(commentId: number): Promise<void> {
    const token = localStorage.getItem('jwt_token');
    
    const response = await fetch(`${this.baseUrl}/${commentId}`, {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${token}`
      }
    });

    if (!response.ok) throw new Error('Failed to delete comment');
  }
}
```

### **2. Update Your Comment Component**

```tsx
const CommentSection = ({ articleId, currentUser }) => {
  const [comments, setComments] = useState([]);
  const [newComment, setNewComment] = useState('');

  // Load comments
  useEffect(() => {
    loadComments();
  }, [articleId]);

  const loadComments = async () => {
    try {
      const data = await commentsService.getArticleComments(articleId);
      setComments(data);
    } catch (error) {
      console.error('Failed to load comments:', error);
    }
  };

  // Create comment
  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!currentUser || !newComment.trim()) return;

    try {
      const comment = await commentsService.createComment({
        articleId,
        content: newComment.trim()
      });
      
      setComments([...comments, comment]);
      setNewComment('');
    } catch (error) {
      alert(`Error: ${error.message}`);
    }
  };

  // Check if user can edit comment
  const canEdit = (comment) => {
    if (!currentUser) return false;
    return comment.user.id === currentUser.id || 
           currentUser.roles?.includes('Admin');
  };

  return (
    <div className="comments-section">
      <h3>Comments ({comments.length})</h3>
      
      {/* Comment Form - Only show if user is logged in */}
      {currentUser && (
        <form onSubmit={handleSubmit}>
          <textarea
            value={newComment}
            onChange={(e) => setNewComment(e.target.value)}
            placeholder="Write your comment..."
            maxLength={2000}
            required
          />
          <div>
            <span>{newComment.length}/2000</span>
            <button type="submit">Post Comment</button>
          </div>
        </form>
      )}

      {/* Comments List */}
      {comments.map(comment => (
        <div key={comment.id} className="comment">
          <div className="comment-header">
            {/* Show user avatar and name instead of authorName */}
            <img 
              src={comment.user.avatar || '/default-avatar.png'} 
              alt={comment.user.name}
            />
            <div>
              <strong>{comment.user.name}</strong>
              {comment.user.specialization && (
                <span className="specialization">{comment.user.specialization}</span>
              )}
              <span className="date">{new Date(comment.createdAt).toLocaleDateString()}</span>
            </div>
            
            {/* Edit/Delete buttons - only for comment owner or admin */}
            {canEdit(comment) && (
              <div className="comment-actions">
                <button onClick={() => handleEdit(comment.id)}>Edit</button>
                <button onClick={() => handleDelete(comment.id)}>Delete</button>
              </div>
            )}
          </div>
          
          <div className="comment-content">
            {comment.content}
          </div>
        </div>
      ))}
    </div>
  );
};
```

---

## ⚠️ **VALIDATION & ERROR HANDLING**

### **Content Validation:**
```typescript
const validateComment = (content: string): string | null => {
  if (!content.trim()) {
    return "Comment content is required";
  }
  if (content.length > 2000) {
    return "Comment cannot exceed 2000 characters";
  }
  return null;
};
```

### **Error Messages You'll See:**
```json
// Content too long
{ "message": "Content cannot exceed 2000 characters" }

// Not authenticated
{ "message": "Unauthorized" }

// Can't edit others' comments
{ "message": "Access denied" }

// Comment not found
{ "message": "Comment not found" }
```

---

## ✅ **MIGRATION CHECKLIST**

### **Required Changes:**
- [ ] Update comment data structure (remove `authorName`, add `user` object)
- [ ] Add authentication headers for POST/PUT/DELETE operations
- [ ] Show user avatar and name instead of just author name
- [ ] Add character count validation (2000 max)
- [ ] Show edit/delete buttons only for comment owners or admins
- [ ] Handle authentication errors gracefully
- [ ] Update comment form to require login

### **Optional Enhancements:**
- [ ] Add edit comment functionality (inline editing)
- [ ] Show "edited" indicator for updated comments
- [ ] Add comment moderation for admins
- [ ] Implement real-time comment updates

---

## 🎯 **QUICK SUMMARY**

**What you need to change in your frontend:**

1. **Data Structure**: Replace `authorName` with `user` object containing `id`, `name`, `avatar`, `specialization`
2. **Authentication**: Add JWT token to POST, PUT, DELETE requests
3. **UI Changes**: Show user avatar and full profile info instead of just name
4. **Permissions**: Only show edit/delete for comment owners or admins
5. **Validation**: Add 2000 character limit and required content validation

**That's it!** These are the only changes needed for your comment system to work with the new backend. 🚀
