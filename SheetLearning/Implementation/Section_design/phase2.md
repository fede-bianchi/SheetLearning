# Phase 2 — Dashboard, Le Mie Lezioni, Profile, Chat

**Date:** 2024-05-15

## Summary

Created the 4 main student-area pages, all using `_Layout.cshtml` (sidebar + topbar layout). These are the core authenticated pages of the platform.

## Files Created

| File | Purpose |
|---|---|
| `Pages/Dashboard.cshtml` + `.cs` | Student dashboard with KPI cards, bento grid, exercise suggestion, weekly stats chart, recent activity |
| `Pages/Lessons.cshtml` + `.cs` | Lesson management with filter pills (Tutte/Programmate/Completate/Annullate), lesson cards with status badges and date/time info |
| `Pages/Profile.cshtml` + `.cs` | Profile settings with avatar upload, personal info form, password change, language selection, email notifications toggle |
| `Pages/Chat.cshtml` + `.cs` | Real-time chat with conversation list (left pane), chat window (right pane), messages with audio attachment, typing indicator, input area |

## Pages Implemented

### Dashboard (`/Dashboard`)
- **KPI Row:** Progressione Settimanale (+15%), Best Score (98/100), Lezioni Prenotate (2)
- **Bento Grid:** Purple CTA card (Lettura Note suggestion), bar chart (weekly stats), activity list with 3 items
- Cards have hover shadows and interactive states

### Le Mie Lezioni (`/Lessons`)
- **Filter Pills:** Active/selected state with purple fill, unselected with outline border
- **3 Card States:** Programmata (left accent bar, green status), Completata (reduced opacity), Annullata (strikethrough, red status badge)
- Each card has teacher initials avatar, date/time block, and contextual action buttons

### Gestione Profilo (`/Profile`)
- **Avatar Column:** 128px circle with initials, hover overlay with camera icon, upload button
- **Personal Info Form:** 2-column grid (Nome, Nickname) + full-width Email, Save button
- **Security Card:** Current/new password fields, update button
- **Preferences Card:** Language dropdown (IT/EN/FR), email notifications toggle switch

### Chat Studentesca (`/Chat`)
- **Left Pane:** Conversation list with search, active conversation (purple left border + unread badge "2"), online status indicator (green dot)
- **Right Pane:** Glassmorphism sticky header with teacher info + videocam/more buttons, date separator ("Today"), received messages (white bubbles), sent messages (purple bubbles), audio file attachment, typing indicator ("Elena sta scrivendo..."), compose bar with attach/mic/send buttons
- Uses `custom-scrollbar` CSS class for styled scrollbar

## Design Tokens Used
- All pages use sidebar nav + top bar from `_Layout.cshtml`
- Status badges: `primary-fixed` (programmata), `surface-variant` (completata), `error-container` (annullata)
- Chat bubbles: received = white + border, sent = purple container with white text
- Glassmorphism: `glass-panel` class on chat header
