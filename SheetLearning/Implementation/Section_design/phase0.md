# Phase 0 — Setup Design System & Shared Layout

**Date:** 2024-05-15

## Summary

Removed Bootstrap 5 and jQuery from the frontend. Set up the "Sonic Scholastic" design system using Tailwind CSS via CDN, configured with the full color palette, typography, and spacing tokens from the DESIGN.md specification.

## Files Created

| File | Purpose |
|---|---|
| `Pages/Shared/_TailwindConfigPartial.cshtml` | Tailwind config with all Sonic Scholastic colors, fonts, spacing, borderRadius |
| `Pages/Shared/_SidebarPartial.cshtml` | 280px fixed sidebar with navy background (#1B1F3B), navigation links with Material Symbols icons |
| `Pages/Shared/_TopBarPartial.cshtml` | Sticky top app bar with search, notifications, settings, help, user avatar |
| `Pages/Shared/_Layout.cshtml` | Main app layout: sidebar + topbar + main content + footer |
| `Pages/Shared/_LayoutPublic.cshtml` | Public layout for landing/login pages: simple top header + main content + footer |
| `wwwroot/css/site.css` | Custom CSS: Material Symbols font, glassmorphism classes (.glass-overlay, .glass-panel), piano key styles (.piano-key-white, .piano-key-black, .piano-key-active), custom scrollbar |

## Files Modified

| File | Change |
|---|---|
| `Pages/Shared/_Layout.cshtml` | Complete rewrite: removed Bootstrap navbar, added Tailwind CDN, Google Fonts, app layout structure |
| `wwwroot/css/site.css` | Replaced Bootstrap-dependent styles with Sonic Scholastic custom classes |

## Design System Tokens Applied

- **Colors:** 40+ semantic tokens (surface, primary, secondary, tertiary, error variants)
- **Typography:** Plus Jakarta Sans (headings), Inter (body), material symbols (icons)
- **Spacing:** 8px unit base, 24px gutter, 40px desktop margin, 16px mobile margin
- **Border Radius:** 0.25rem default, 0.5rem lg, 0.75rem xl, 9999px full
- **Sidebar:** 280px fixed, #1B1F3B background

## Not Yet Removed

- Bootstrap library files in `wwwroot/lib/` still exist on disk but are no longer referenced
- Old `_Layout.cshtml.css` still exists but is orphaned (no longer imported)
- jQuery still in `wwwroot/lib/` but no longer imported in layouts
