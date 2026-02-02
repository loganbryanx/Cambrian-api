# Backend Updates - Matching Frontend Features

## Summary
Updated backend API to support all new frontend features including password management, billing invoices, and audio streaming.

## Changes Made

### 1. Password Change Endpoint
**Endpoint:** `POST /auth/password`

**Request:**
```json
{
  "currentPassword": "string",
  "newPassword": "string"
}
```

**Response:**
```json
{
  "message": "Password changed successfully."
}
```

**Features:**
- Validates current password
- Requires minimum 8 characters for new password
- Returns appropriate error messages
- Updates in-memory users dictionary

### 2. Billing Invoices Endpoint
**Endpoint:** `GET /billing/invoices`

**Response:**
```json
[
  {
    "id": "inv_001",
    "date": "2024-01-15",
    "amount": 9.99,
    "status": "Paid",
    "downloadUrl": "https://invoice.stripe.com/inv_001"
  }
]
```

**Features:**
- Returns invoice history for authenticated user
- Automatically generates sample invoices based on subscription
- Includes download URLs for Stripe-hosted invoices

### 3. Audio Streaming Support
**Updated Models:**

**CatalogTrack:**
```csharp
record CatalogTrack(
  string Id, 
  string Title, 
  string Artist, 
  string Genre, 
  decimal Price, 
  string Rights, 
  string? AudioUrl = null
);
```

**LicenseRecord:**
```csharp
record LicenseRecord(
  string Id, 
  string Title, 
  string Status, 
  DateOnly PurchasedOn, 
  string? Artist = null, 
  string? Genre = null, 
  string? AudioUrl = null
);
```

**Features:**
- All catalog tracks now include audio URLs
- Library tracks preserve audio URLs when saved
- Uses demo audio from SoundHelix for testing
- Real audio URLs: 
  - SoundHelix-Song-1.mp3 through SoundHelix-Song-6.mp3

### 4. Data Model Additions
**New Records:**
```csharp
record PasswordChangeRequest(string CurrentPassword, string NewPassword);
record InvoiceRecord(string Id, DateOnly Date, decimal Amount, string Status, string? DownloadUrl = null);
```

**New Dictionary:**
```csharp
var invoicesByEmail = new Dictionary<string, List<InvoiceRecord>>(StringComparer.OrdinalIgnoreCase);
```

## Files Updated
1. `src/auth/Cambrian.Api/Program.cs` - Auth-focused API
2. `src/Cambrian.Api/Program.cs` - Main production API

## Testing
All endpoints can be tested with the frontend at:
- Production: https://cambrian-blush.vercel.app
- Backend: https://cambrian-api.onrender.com

### Test Credentials
Use any email/password combination - accounts are created in-memory on registration.

## Deployment
Push to GitHub main branch - Render will auto-deploy the backend.

## Compatibility
✅ Settings page password change
✅ Invoices page billing history  
✅ Audio player track streaming
✅ Library and catalog with audio URLs
