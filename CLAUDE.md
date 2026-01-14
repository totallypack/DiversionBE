# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Diversion** is a full-stack social networking application for connecting people based on shared interests and hobbies. It features user profiles with multiple account types (Regular, Business, Caregiver), event management, friend connections, communities, messaging, and caregiver relationship management.

**Tech Stack:**
- **Backend**: ASP.NET Core 9.0 (Web API), Entity Framework Core 9.0.11, SQL Server (LocalDB), ASP.NET Core Identity
- **Frontend**: React 18.2.0, React Router v6, Vite 5.0.12, Reactstrap (Bootstrap 5)

**Repositories:**
- Backend: `C:\Users\total\workspace\csharp\Diversion`
- Frontend: `C:\Users\total\workspace\csharp\DiversionFE`

## Build & Development Commands

### Backend (.NET)

```bash
# Navigate to backend
cd C:\Users\total\workspace\csharp\Diversion

# Restore packages
dotnet restore

# Build
dotnet build

# Run application (listens on configured ports)
dotnet run

# Database migrations
dotnet ef migrations add <MigrationName>
dotnet ef database update
dotnet ef migrations remove  # Rollback last migration

# Check Entity Framework tools version
dotnet ef --version
```

### Frontend (React + Vite)

```bash
# Navigate to frontend
cd C:\Users\total\workspace\csharp\DiversionFE

# Install dependencies
npm install

# Development server (http://localhost:5173)
npm run dev
# or
npm start

# Production build
npm run build

# Preview production build
npm run serve
```

### Running Full Stack

1. Start backend: `cd Diversion && dotnet run`
2. Start frontend: `cd DiversionFE && npm run dev`
3. Frontend connects to backend via CORS (configured for `http://localhost:5173`)

## Architecture & Data Flow

### Backend Architecture

**Authentication & Authorization:**
- Cookie-based authentication using ASP.NET Core Identity
- `IdentityUser` for user accounts, extended by `UserProfile` model
- Password requirements: 8+ chars, upper/lower/digit, no special chars required
- Cookie expires after 7 days with sliding expiration
- Returns 401 (not redirects) on unauthorized API calls

**Database Context (`DiversionDbContext`):**
- Inherits from `IdentityDbContext`
- Connection: SQL Server LocalDB
- Contains 17 DbSets (models listed below)
- Extensive relationship configuration with indexes for performance
- Delete behaviors: Cascade for dependent entities, Restrict for foreign keys to prevent orphaned data

**Controller Pattern:**
- All controllers use `[Authorize]` attribute (except Auth endpoints)
- Constructor injection with `DiversionDbContext context` parameter
- User ID retrieved via `User.FindFirstValue(ClaimTypes.NameIdentifier)`
- DTOs for all API requests/responses (in `DTOs/` folder)

**Key Models & Relationships:**

1. **Users & Profiles:**
   - `IdentityUser` → `UserProfile` (1:1, cascade delete)
   - `UserProfile.UserType` enum: Regular (0), Business (1), Caregiver (2)
   - Business fields: BusinessName, BusinessWebsite, BusinessHours, BusinessCategory, IsVerified
   - Caregiver fields: CaregiverCredentials, YearsOfExperience, Certifications, CareTypes, Specializations, IsBackgroundChecked, LicenseNumber, LicenseExpiry, EmploymentStatus

2. **Interests System:**
   - `Interest` (top-level categories)
   - `SubInterest` → `Interest` (many:1, restrict delete)
   - `UserInterest` links users to sub-interests (many:many through join table)
   - Extensive seed data in `DiversionDbContext.OnModelCreating`

3. **Events:**
   - `Event.OrganizerId` → `IdentityUser`
   - `Event.InterestTagId` → `SubInterest`
   - `EventAttendee` links users to events with status ("Going", "Maybe", "NotGoing")
   - `EventType`: "Online" (requires MeetingUrl) or "InPerson" (requires City, State)
   - Organizer automatically added as "Going" attendee on event creation (transaction-based)

4. **Friendships (Bidirectional):**
   - `Friendship` creates two records for each friendship (userId → friendId AND friendId → userId)
   - `FriendRequest` for request/approval workflow with status (Pending, Accepted, Rejected)
   - Accept operation creates both Friendship records in a transaction
   - Unique constraint on (UserId, FriendId) prevents duplicates

5. **Communities & Messaging:**
   - `Community` with `IsPrivate` flag
   - `CommunityMembership` with roles: "Owner", "Moderator", "Member"
   - `CommunityMessage` with optional `ReplyToMessageId` for threading
   - `DirectMessage` between friends with read tracking (IsRead, ReadAt)
   - Unique constraint: one membership per user per community

6. **Caregiver Relationships (One-to-Many):**
   - `CaregiverRequest` → `CareRelationship` (request/approval workflow)
   - **Granular permissions:** CanManageEvents, CanManageProfile, CanManageFriendships
   - Soft delete: `IsActive` flag, `RevokedAt` timestamp
   - **"Acting On Behalf Of" Pattern:** Caregiver creates events/friend requests using recipient's ID
   - `CaregiverAuthHelper.ValidateAndAuthorize()` validates permissions and returns effective user ID
   - Unique constraint on (CaregiverId, RecipientId)

**Constants Pattern:**
Constants defined in `Constants/` folder:
- `EventTypeConstants`: "Online", "InPerson"
- `AttendeeStatusConstants`: "Going", "Maybe", "NotGoing"
- `FriendRequestStatus` enum: Pending (0), Accepted (1), Rejected (2)
- `CaregiverRequestStatus` enum: Pending (0), Accepted (1), Rejected (2)

**Authorization Helpers:**
- `CaregiverAuthHelper.ValidateAndAuthorize()`: Validates caregiver relationships and permissions
  - Returns `CaregiverAuthResult` with `IsAuthorized`, `ErrorMessage`, `EffectiveUserId`
  - Used in EventsController.CreateEvent, FriendRequestController.SendFriendRequest
  - Pattern: Check ActingOnBehalfOf parameter → Validate relationship → Return effective user ID

### Frontend Architecture

**Routing:**
- React Router v6 configured in `App.jsx`
- Protected routes assume authentication (no auth guard implemented client-side)
- Routes organized by feature: `/events`, `/communities`, `/messages`, `/friends`, `/profile`, `/caregiver`

**Component Organization:**
```
src/
  components/
    common/           # Reusable UI components (FullWidthSection, etc.)
    communities/      # Community browsing, creation, forum
    events/           # Event CRUD, attendee management
    friends/          # Friend search, requests, list
    interests/        # Interest selection, browsing
    messages/         # Direct messaging, conversation list
    profiles/         # User profiles, profile setup
  managers/           # API client wrappers (one per controller)
  utils/
    apiClient.js      # Centralized fetch wrapper with credentials
  constants/          # Frontend constants matching backend
```

**API Client Pattern (`utils/apiClient.js`):**
- `apiGet(url)`, `apiPost(url, data)`, `apiPut(url, data)`, `apiDelete(url)`
- All requests include `credentials: "include"` for cookie-based auth
- Base URL: `http://localhost:5000` (hardcoded in apiClient.js)
- Managers wrap apiClient calls with typed function names

**Manager Pattern (`managers/`):**
Each manager corresponds to a backend controller:
- `authManager.js` → AuthController (login, register, logout)
- `profileManager.js` → UserProfileController
- `eventManager.js` → EventsController
- `friendManager.js` → FriendRequestController, FriendshipsController
- `communityManager.js` → CommunitiesController
- `messageManager.js` → DirectMessagesController, CommunityMessagesController
- `caregiverManager.js` → CaregiverRequestController, CareRelationshipController

**Real-Time Updates:**
Polling-based (no WebSockets):
- Direct messages: Poll every 5 seconds (active thread)
- Community messages: Poll every 10 seconds (active forum)
- Conversations list: Poll every 30 seconds
- Friend requests: Poll every 30 seconds
- Unread message count: Poll every 30 seconds

**State Management:**
- Local component state with React hooks (useState, useEffect)
- No global state management library (Redux, Context, etc.)
- User info stored in localStorage: "username", "token" (if applicable)

## Common Patterns & Conventions

### Adding a New Feature with Database Changes

1. **Create Models** in `Models/` folder with navigation properties
2. **Add DbSet** to `DiversionDbContext.cs`
3. **Configure relationships** in `DiversionDbContext.OnModelCreating()` with indexes
4. **Create migration**: `dotnet ef migrations add <FeatureName>`
5. **Apply migration**: `dotnet ef database update`
6. **Create DTOs** in `DTOs/` folder (request/response objects)
7. **Create Controller** with CRUD endpoints
8. **Test backend** with `dotnet build`

### Adding "Acting On Behalf Of" to a New Controller

1. Add `using Diversion.Helpers;`
2. Add `ActingOnBehalfOf` property to request DTO (nullable string)
3. In controller method:
   ```csharp
   var authResult = await CaregiverAuthHelper.ValidateAndAuthorize(
       _context, userId, dto.ActingOnBehalfOf, CaregiverPermission.<Permission>);

   if (!authResult.IsAuthorized)
       return BadRequest(authResult.ErrorMessage);

   var effectiveUserId = authResult.EffectiveUserId;
   ```
4. Use `effectiveUserId` instead of `userId` for all database operations

### Frontend: Adding a New Feature

1. **Create manager** in `src/managers/` wrapping API calls
2. **Create components** in appropriate `src/components/` subfolder
3. **Add routes** to `App.jsx`
4. **Add navigation** to `NavBar.jsx` if needed
5. **Test frontend** with `npm run build`

## Database Seeding

Extensive seed data for Interests and SubInterests in `DiversionDbContext.OnModelCreating()`:
- 8 main interests: Outdoors, Gaming, Models/Collecting, Fitness, Arts/Crafts, Music, Food/Cooking, Tech
- 100+ sub-interests across all categories
- Uses fixed GUIDs for consistent seeding

## Important Notes

**Transaction Usage:**
- Use transactions for multi-step operations that must be atomic
- Examples: Creating event + attendee, accepting friend request (creates 2 friendships)
- Pattern:
  ```csharp
  using var transaction = await _context.Database.BeginTransactionAsync();
  try {
      // operations
      await _context.SaveChangesAsync();
      await transaction.CommitAsync();
  } catch {
      await transaction.RollbackAsync();
      throw;
  }
  ```

**CORS Configuration:**
Backend allows credentials from `http://localhost:5173` only. Update `Program.cs` if frontend port changes.

**Identity Password Requirements:**
Defined in `Program.cs`: 8+ chars, digit, uppercase, lowercase, no special char requirement.

**Polling Intervals:**
Keep existing polling intervals for consistency:
- Active interactions: 5-10 seconds
- Background updates: 30 seconds

**User Type Validation:**
When creating caregiver-only features, validate `UserProfile.UserType == UserType.Caregiver` in controller.

## Migration Strategy

When making breaking changes to DTOs or models:
1. All new fields should be **nullable** to avoid breaking existing data
2. Test migration rollback: `dotnet ef migrations remove`
3. Existing data patterns: Business and Caregiver fields are nullable and only populated for respective user types
