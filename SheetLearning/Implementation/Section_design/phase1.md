# Phase 1 — Landing Page & Login

**Date:** 2024-05-15

## Summary

Created the two public-facing pages: the **Landing Page** (home) and the **Login** page, both based on the design mockups in `Design/`. Both pages use `_LayoutPublic.cshtml` which provides the public header (no sidebar) and footer.

## Files Created

| File | Purpose |
|---|---|
| `Pages/Login.cshtml` | Login page with social login buttons (Google, Apple), email/password form, password visibility toggle, register link |
| `Pages/Login.cshtml.cs` | Code-behind PageModel for login form binding |

## Files Modified

| File | Change |
|---|---|
| `Pages/Index.cshtml` | Complete rewrite: replaces Bootstrap "Welcome" page with full landing page (hero section + exercise preview cards) |

## Pages Implemented

### Landing Page (`/`)
- **Hero Section:** Background image overlay (musical staves), tagline badge, main heading, CTA button ("Inizia Gratis")
- **Exercises Preview:** 3 cards in a responsive grid (Lettura Note, Accordi, Ritmo) with icon containers, hover shadow effects, and color transitions
- Uses `_LayoutPublic` (public header with navigation + footer)

### Login Page (`/Login`)
- **Social Login:** Google + Apple buttons with icons
- **Divider:** "Or" with horizontal lines
- **Email Form:** Email input, password input with show/hide toggle, forgot password link
- **Register Link:** "Don't have an account? Register"
- Uses `_LayoutPublic`

## Design Tokens Used
- All colors, typography, and spacing from Tailwind config (no Bootstrap classes)
- Cards: `rounded-xl`, white background, 1px `outline-variant` border, hover shadow
- Buttons: `bg-primary-container text-on-primary-container rounded-lg`
- Inputs: border `outline-variant`, focus ring `primary/20`
