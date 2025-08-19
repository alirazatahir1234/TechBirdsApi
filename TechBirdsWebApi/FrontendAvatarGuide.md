# TechBirds API – Avatar Upload & Display Guide

## 1. Uploading an Avatar (Registration/Update)

- **Frontend should send the avatar image as a base64-encoded string.**
- The request payload for registration (or profile update) should look like:

```json
{
  "FirstName": "John",
  "LastName": "Doe",
  "Email": "john@example.com",
  "Password": "yourpassword",
  "Avatar": "iVBORw0KGgoAAAANSUhEUgAA...", // base64 string of image
  // ...other fields
}
```

**How to convert an image to base64 in JavaScript:**
```js
function toBase64(file) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => resolve(reader.result.split(',')[1]);
    reader.onerror = reject;
    reader.readAsDataURL(file);
  });
}
// Usage: let base64 = await toBase64(fileInput.files[0]);
```

---

## 2. Receiving Avatar in API Responses

- The backend returns the avatar as a base64 string in the `Avatar` field of the user object.

**Example response:**
```json
{
  "User": {
    "Id": "123",
    "FirstName": "John",
    "Avatar": "iVBORw0KGgoAAAANSUhEUgAA...", // base64 string
    // ...other fields
  },
  "Token": "jwt-token",
  "ExpiresAt": "2025-08-15T12:00:00Z"
}
```

---

## 3. Displaying Avatar in the Frontend

- To display the avatar, use the base64 string as a data URL:

```js
const avatarBase64 = user.Avatar; // from API response
const avatarUrl = avatarBase64
  ? `data:image/png;base64,${avatarBase64}`
  : '/default-avatar.png'; // fallback image

// In HTML/React:
<img src={avatarUrl} alt="User Avatar" />
```

**Note:** The MIME type (`image/png`, `image/jpeg`, etc.) should match the uploaded image type.

---

## Summary

- **Send:** Avatar as base64 string in requests.
- **Receive:** Avatar as base64 string in responses.
- **Display:** Use `data:image/png;base64,${base64}` in `<img>` tags.

This ensures seamless avatar handling between frontend and backend.
