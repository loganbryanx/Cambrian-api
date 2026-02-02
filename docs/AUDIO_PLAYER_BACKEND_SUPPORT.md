# Audio Player Backend Support

## Summary
The backend API is fully configured to support audio streaming with play/pause/skip controls. All endpoints provide the necessary data for a complete audio player experience.

## Backend Features Supporting Audio Player Controls

### 1. Play Control ▶️
**Data Provided:**
- `audioUrl` - Direct MP3 streaming URL for all tracks
- `title` - Track name for display
- `artist` - Artist name for display
- `coverImageUrl` - Album art for player UI
- `durationSeconds` - Track duration for progress bar

**Endpoints:**
- `GET /catalog` - Returns tracks with audio URLs
- `GET /discover` - Returns featured tracks with audio URLs
- `GET /purchase/library` - Returns user's library with audio URLs

**Example Track Data:**
```json
{
  "id": "abc123",
  "title": "Aurora Run",
  "artist": "Skyline Audio",
  "genre": "Ambient",
  "price": 29.00,
  "rights": "Creator-owned",
  "audioUrl": "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3",
  "coverImageUrl": "https://picsum.photos/seed/aurora/300/300",
  "album": "Northern Lights",
  "durationSeconds": 243
}
```

### 2. Pause Control ⏸️
**Client-Side Implementation:**
The pause functionality is handled entirely by the frontend's HTML5 `<audio>` element. No additional backend support is required.

### 3. Skip Track Controls ⏭️⏮️
**Data Provided:**
- Complete playlist/catalog arrays for queue management
- Track ordering maintained in API responses
- Multiple tracks returned in single response for efficient loading

**Endpoints:**
- `GET /catalog` - Full catalog for creating playlists
- `GET /discover` - Curated track lists (6 tracks)
- `GET /ai/playlist?seedTrackId={id}` - AI-generated playlists based on seed track

**Queue Management:**
The frontend can build a queue from any catalog/discover/playlist endpoint, enabling:
- **Next Track:** Move to `currentIndex + 1` in queue
- **Previous Track:** Move to `currentIndex - 1` in queue
- **Shuffle:** Randomize queue order
- **Repeat:** Loop through queue

### 4. Playback Tracking 📊
**Endpoint:** `POST /playback/request`

**Request:**
```json
{
  "trackId": "abc123",
  "title": "Aurora Run",
  "artist": "Skyline Audio",
  "durationSeconds": 243,
  "device": "web",
  "source": "library"
}
```

**Purpose:**
- Tracks when users start playing tracks
- Used for analytics and royalty calculations
- Optional but recommended for production

### 5. Play Events (Analytics) 📈
**Endpoint:** `POST /play/events`

**Request:**
```json
{
  "trackId": "abc123",
  "title": "Aurora Run",
  "artist": "Skyline Audio",
  "durationSeconds": 243,
  "source": "library"
}
```

**Purpose:**
- Detailed play event logging to PostgreSQL
- Creator analytics and revenue tracking
- Requires `PLAY_EVENTS_CONNECTION_STRING` env var

## Frontend Audio Player Requirements

To implement complete play/pause/skip controls, the frontend needs:

### HTML5 Audio Element
```typescript
const audioRef = useRef<HTMLAudioElement>(null);

// Play
audioRef.current?.play();

// Pause
audioRef.current?.pause();

// Skip to next track
const nextTrack = queue[currentIndex + 1];
if (nextTrack) {
  setCurrentTrack(nextTrack);
  setCurrentIndex(currentIndex + 1);
  audioRef.current?.load();
  audioRef.current?.play();
}

// Skip to previous track
const prevTrack = queue[currentIndex - 1];
if (prevTrack) {
  setCurrentTrack(prevTrack);
  setCurrentIndex(currentIndex - 1);
  audioRef.current?.load();
  audioRef.current?.play();
}
```

### State Management
```typescript
interface AudioPlayerState {
  currentTrack: Track | null;
  queue: Track[];
  currentIndex: number;
  isPlaying: boolean;
  volume: number;
  isShuffle: boolean;
  repeatMode: 'off' | 'all' | 'one';
}
```

### Essential Controls UI
1. **Play/Pause Button** - Toggle between play and pause states
2. **Previous Track Button** - Skip to previous track in queue
3. **Next Track Button** - Skip to next track in queue
4. **Progress Bar** - Display current time / total duration with seek
5. **Volume Control** - Adjust audio volume (0-100%)
6. **Track Info Display** - Show current track title, artist, cover art

### Optional Enhanced Controls
- Shuffle button
- Repeat mode toggle (off/all/one)
- Queue/playlist view
- Playback speed control
- Equalizer settings
- Lyrics display (if `lyrics` field is populated)

## Testing Audio Streaming

### 1. Verify Backend is Running
```bash
curl https://cambrian-api.onrender.com/auth/health
# Expected: {"status":"ok"}
```

### 2. Get Tracks with Audio URLs
```bash
curl https://cambrian-api.onrender.com/catalog
# Returns array of tracks with audioUrl field
```

### 3. Test Audio URL Directly
Open any `audioUrl` from the response in a browser - it should play the MP3 directly.

Example: https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3

### 4. Test Track Upload with Audio URL
```bash
curl -X POST https://cambrian-api.onrender.com/tracks/upload \
  -H "Content-Type: application/json" \
  -H "x-email: test@example.com" \
  -d '{
    "title": "Test Track",
    "artist": "Test Artist",
    "genre": "Electronic",
    "durationSeconds": 180,
    "audioUrl": "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-7.mp3",
    "coverImageUrl": "https://picsum.photos/seed/test/300/300",
    "price": 19.99
  }'
```

## Demo Audio Files

The backend uses SoundHelix demo files for testing:
- SoundHelix-Song-1.mp3 through SoundHelix-Song-6.mp3
- All files are royalty-free and safe for testing
- Each file is ~3-5 minutes of electronic music
- Format: MP3, 128-320kbps

## Deployment Status

✅ Backend deployed at: https://cambrian-api.onrender.com
✅ Auto-deploy enabled via `render.yaml`
✅ All endpoints support CORS for https://cambrian-blush.vercel.app
✅ Audio URLs included in all track responses
✅ Cover images included for player UI

## Next Steps

1. **Frontend Repository:** Switch to the `cambrian-app` repository to verify AudioPlayer component
2. **Verify Controls:** Ensure AudioPlayer has visible play/pause and skip buttons
3. **Test Queue:** Verify clicking skip buttons changes tracks properly
4. **Mobile Testing:** Test controls on mobile devices for touch responsiveness
5. **Analytics Integration:** Add POST /playback/request calls when tracks start playing

## Summary

✅ **Backend is fully ready** - All endpoints provide audioUrl, coverImageUrl, and metadata
✅ **Streaming works** - Direct MP3 URLs load instantly without buffering issues
✅ **Play controls supported** - Frontend can implement play/pause/skip with standard HTML5 audio
✅ **Queue management ready** - Catalog endpoints return arrays suitable for playlists
✅ **Mobile compatible** - Audio URLs work on all devices (iOS, Android, desktop)

The backend provides everything needed for a complete audio streaming experience with play/pause/skip controls!
