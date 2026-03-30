# Sprint 2 completion notes — Course Catalog & Content Management

**Status:** Backend course catalog + content management is implemented (courses, sections, lessons, content assets).

## Delivered (backend)

### Domain

- **Course aggregate**: `Course` → `Section` → `Lesson` → `ContentAsset`
- **Publishing**: `Course.Publish()` requires at least one lesson
- **Soft delete**: `Course`, `Section`, `Lesson`, `ContentAsset` include `IsDeleted`/`DeletedAt` and are filtered by EF query filters

### Database

- **Migration**: `Sprint2_CoursesAndContent`
- Tables:
  - `courses`
  - `course_sections`
  - `course_lessons`
  - `content_assets`

### Web API (v1)

All endpoints are under `api/v1`.

| Method | Route | Permission |
|---|---|---|
| `GET` | `/courses?page=&pageSize=&search=&status=` | `Courses.Read` |
| `GET` | `/courses/{id}` | `Courses.Read` |
| `POST` | `/courses` | `Courses.Create` |
| `PUT` | `/courses/{id}` | `Courses.Update` |
| `DELETE` | `/courses/{id}` | `Courses.Delete` |
| `POST` | `/courses/{id}/publish` | `Courses.Publish` |
| `POST` | `/courses/{id}/sections` | `Courses.Update` |
| `POST` | `/courses/{courseId}/sections/{sectionId}/lessons` | `Courses.Update` |
| `POST` | `/courses/{courseId}/sections/{sectionId}/lessons/{lessonId}/assets` | `Courses.Update` |
| `GET` | `/assets/{storageKey}` | Anonymous |

### Request examples

Create course:

```json
POST /api/v1/courses
{
  "title": "Intro to C#",
  "description": "Basics"
}
```

Upload asset (multipart/form-data):

- `assetType`: `Video` | `Pdf` | `Scorm` | `Other`
- `file`: the uploaded file

## Notes

- Local file storage is implemented via `Storage:Local:BasePath` and served via `GET /api/v1/assets/{storageKey}`.
- For production, the storage abstraction (`IFileStorage`) is ready to be backed by S3/Azure Blob.

