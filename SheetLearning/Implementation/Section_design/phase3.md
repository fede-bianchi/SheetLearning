# Phase 3 — Exercises & Forum

**Date:** 2024-05-15

## Summary

Created the 4 exercise and forum pages, plus a new immersive layout for full-screen exercise mode.

## Files Created

| File | Purpose |
|---|---|
| `Pages/Shared/_LayoutImmersive.cshtml` | Minimal full-screen layout (no sidebar, no header) for immersive exercises |
| `Pages/ExerciseNoteReading.cshtml` + `.cs` | Note reading exercise with staff area, virtual piano keyboard, stats bar, sidebar layout |
| `Pages/ExerciseNoteReadingImmersive.cshtml` + `.cs` | Full-screen version of the note reading exercise, uses `_LayoutImmersive` |
| `Pages/Forum.cshtml` + `.cs` | Community forum with post feed, vote widgets, filter pills, trending sidebar, glassmorphism promo card |
| `Pages/NewPost.cshtml` + `.cs` | Rich post editor with toolbar (bold/italic/underline/lists/music-note/link), category chips, attachment drop zone, preview/publish actions |

## Pages Implemented

### Esercizio Lettura Note (`/ExerciseNoteReading`)
- **Stats Row:** Timer (1.2s), Streak (14 🔥), Accuracy (98%) with separator dividers
- **Staff Area:** Simulated 5-line musical staff with treble clef (𝄞) and a note, decorative grid background
- **Virtual Piano:** 2-octave keyboard with white/black keys, one active key highlighted in purple
- **Action Buttons:** Pause (muted) + Next Exercise (purple CTA)
- Uses standard `_Layout.cshtml` with sidebar

### Esercizio Lettura Note Immersivo (`/ExerciseNoteReadingImmersive`)
- Same content as above but without sidebar or footer
- Uses `_LayoutImmersive.cshtml` — full viewport, no sidebar, no topbar
- Back button uses `chevron_left` icon

### Forum Community (`/Forum`)
- **Filter Pills Row:** Recent (active purple), Most Voted, Theory, Practice, Gear
- **3 Post Cards:** Each with vote widget (up/down arrows + count), author avatar, timestamp, category badge, title (hover turns purple), excerpt, comment count, share button
- **Load More button** at bottom
- **Right Sidebar:** Glassmorphism promo card (Masterclass Weekend) + Trending Tags (#Chopin, #SightReading, etc.)
- 12-column grid layout (8 for feed, 4 for sidebar)

### Nuovo Post Forum (`/Forum/New`)
- **Title Input:** Large text with headline styling
- **Category Chips:** Selected (purple with check icon), unselected (gray), dashed "Nuova Tag" button
- **Rich Text Editor:** Toolbar with bold/italic/underline, dividers, bullet/numbered lists, music note insert, link insert, markdown toggle
- **Textarea:** 250px min-height with placeholder text
- **Attachment Zone:** Dashed border drop zone for PDF/MP3
- **Action Footer:** Save Draft (left), Anteprima + Pubblica (right, purple CTA)

## New Layout
`_LayoutImmersive.cshtml` provides a distraction-free fullscreen canvas with no sidebar or topbar, used for exercises requiring maximum focus.
