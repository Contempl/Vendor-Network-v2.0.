# CLAUDE.md

## Project Overview
B2B platform with 3 roles: **Vendor** (service provider), **Operator** (service consumer), **Administrator**.
Invite-based registration system with JWT authentication.

## Architecture

### Backend (ASP.NET Core Web API)
```
Product.WebApi/          # Controllers, Program.cs
Product.Application/     # Services, DTOs, Interfaces
Product.Domain/          # Entities, Repository Interfaces
Product.Infrastructure/  # EF Core, Repositories, JWT, Email
Product.Tests/           # Unit Tests
Product.IntegrationTests/# Integration Tests
Product.Seeder/          # DB Seeding
```

### Frontend (Next.js App Router)
```
client/src/
├── app/
│   ├── login/page.tsx
│   ├── register/[inviteId]/page.tsx
│   ├── vendor/
│   │   ├── layout.tsx         # Sidebar navigation
│   │   ├── page.tsx           # Vendor profile
│   │   ├── facilities/page.tsx
│   │   ├── operators/page.tsx
│   │   └── invite/page.tsx
│   ├── admin/
│   │   ├── layout.tsx
│   │   ├── login/page.tsx
│   │   └── dashboard/page.tsx
│   └── operator/              # WIP
├── entities/
│   ├── auth/
│   │   ├── auth.types.ts      # TokenDto, UserRegistrationByInviteDto, etc.
│   │   ├── auth.api.ts        # login, registerByInvite, getInvite
│   │   └── auth-utils.ts      # getBusinessIdFromToken (jwt-decode)
│   ├── vendor/
│   │   ├── vendor-types.ts    # BusinessFrontEndDto, UpdateVendorDto, etc.
│   │   ├── vendor-facility-types.ts # VendorFacility, VendorFacilityService, etc.
│   │   ├── vendor.api.ts      # getVendor, updateVendor, facilities CRUD, etc.
│   │   └── vendor-store.ts    # Zustand: vendor, facilities
│   ├── operator/
│   │   ├── operator.types.ts  # UpdateOperatorDto, OpIndustryFrontEndDto, etc.
│   │   └── operator.api.ts    # getOperator, industries CRUD, search vendors
│   └── admin/
│       └── admin-api.ts       # inviteBusiness, inviteUser, removeVendor/Operator
└── shared/
    └── api/
        └── axiosInstance.ts   # Axios with JWT interceptors + auto-refresh
```

## Tech Stack

### Frontend
- **Next.js** 16.1.6 (App Router)
- **React** 19.2.3
- **TypeScript** 5
- **MUI** 7.3.9 (Material UI)
- **Zustand** 5.0.11 (state management)
- **Axios** 1.13.6 (HTTP client)
- **jwt-decode** 4.0.0

### Backend
- **ASP.NET Core** Web API
- **Entity Framework Core**
- **JWT** authentication with refresh tokens
- **OneOf** for discriminated unions in service returns

## Key Conventions

### Frontend
- All pages use `"use client"` directive
- Token storage: `localStorage` keys `"tkn-tko"` (access) and `"refreshToken"`
- `businessId` extracted from JWT payload via `getBusinessIdFromToken()`
- FSD-inspired structure: `entities/` for types+API+stores, `app/` for pages
- MUI dark theme colors: background `#0d0d0d`, cards `#161616`, accent `#e94560`
- `next/navigation` (NOT `next/router`) for routing

### Backend
- Controllers return `OneOf<T1, T2, ...>` matched with `.Match()`
- Custom filters: `[EnsureUserExists]`, `[EnsureVendorExists]`, `[EnsureBusinessAccess]`
- Auth policies: `"Admin"`, `"VendorUser"`, `"OperatorUser"`, `"All"`
- JWT claims: `userId`, `email`, `role` (ClaimTypes.Role), `businessId`

## API Routes

### Auth (AccountController)
- `GET /Account/Register/User/{inviteId}` — validate invite
- `POST /Account/Register/User/{inviteId}` — register by invite
- `POST /Account/Login` — login
- `POST /refresh` — refresh token

### Admin (AdminController) — requires Admin policy
- `POST /Admin/Login`
- `POST /Admin/inviteBusiness`
- `POST /inviteVendorUser`
- `POST /inviteOperatorUser`
- `DELETE /Admin/Vendor/{vendorId}`
- `DELETE /Admin/Operator/{operatorId}`

### Vendor (VendorController) — requires VendorUser policy
- `GET /Vendor/{vendorId}`
- `PUT /Vendor`
- `POST /Vendor/Search/Operators`
- `POST /Vendor/invite`
- `GET /Vendor/facilities`

### Vendor Facilities (VendorFacilityController)
- `GET /facility/{facilityId}`
- `POST /facility`
- `PUT /vendor/facility/{facilityId}`
- `DELETE /{vendorId}/facilities/{facilityId}`
- `GET /facility/{facilityId}/services`

### Operator (OperatorController) — requires OperatorUser policy
- `GET /Operator`
- `PUT /Operator`
- `POST /Operator/search/vendors`
- `POST /Operator/search/vendor`
- `POST /Operator/invite`

### Operator Industries (OperatorIndustryController)
- `GET /operator/industry/{industryId}`
- `GET /operator/industries`
- `POST /operator/{operatorId}/industry`
- `PUT /operator/industry/{industryId}`
- `DELETE /operator/industry/{industryId}`

## Current Status
- ✅ Vendor cabinet (profile, facilities CRUD, operator search, invite user)
- ✅ Admin dashboard (invite business/users, remove vendor/operator)
- ✅ Registration by invite
- ✅ Operator cabinet (layout, profile, industries CRUD, vendor search, invite)

## Frontend TODO
### Broken
- `app/register/[inviteId]/page.tsx` — wrong router import + calls `router.redirect()` (doesn't exist on useRouter)
- `app/vendor/page.tsx` — duplicate Edit button (one in header, one separate)

### Missing pages
- `app/vendor/facilities/[id]/page.tsx` — facility detail page (empty stub)
- `app/page.tsx` — still Next.js boilerplate, needs redirect to /login or role-based landing

### Dead files (delete)
- `client/store/auth.store.ts` — empty, auth lives in localStorage + entities/auth/
- `client/store/vendor.store.ts` — empty, real store is entities/vendor/vendor-store.ts
- `client/shared/api/auth.api.ts` — empty, real auth API is entities/auth/auth-api.ts

### Auth guards (missing)
- `/vendor/*` layout — no token check
- `/operator/*` layout — no token check

## Running the Project

### Backend
```bash
cd Product.WebApi
dotnet run
# Runs on https://localhost:5227
```

### Frontend
```bash
cd client
npm run dev
# Runs on http://localhost:3000
```

### Environment
Create `client/.env.local`:
```
NEXT_PUBLIC_API_URL=http://localhost:5227
```
