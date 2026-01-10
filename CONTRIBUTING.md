# Contributing to Double M Educational Platform

## Project Architecture

This project follows **Clean Architecture** principles with three main layers:

- **DAL (Data Access Layer)**: Entities, Repositories, Database Context
- **BLL (Business Logic Layer)**: DTOs, Services, Mappings, Interfaces
- **PL (Presentation Layer)**: API Controllers, Hub (SignalR)

## Coding Standards

All code must follow the `.editorconfig` standards:

- **Indentation**: 4 spaces
- **Line Endings**: CRLF
- **Charset**: UTF-8
- **Namespaces**: File-scoped (`namespace X;`)
- **Properties**: Expression-bodied (`=>`)
- **Methods**: Non-expression bodied (block-scoped `{}`)
- **Braces**: Always required
- **Logging**: Serilog with structured data (use `{Parameter}` syntax)

## Dependency Injection & Service Registration

Services are registered in `Program.cs` as scoped dependencies. Follow this pattern:

```csharp
builder.Services.AddScoped<IServiceInterface, ServiceImplementation>();
```

## Database Access

- Use `IUnitOfWork` to access repositories
- Always use async/await patterns
- Use transactions for multi-step operations:
  ```csharp
  await _unitOfWork.BeginTransactionAsync();
  try { /* operations */ await _unitOfWork.CommitTransactionAsync(); }
  catch { await _unitOfWork.RollbackTransactionAsync(); throw; }
  ```

## Error Handling

- Catch all exceptions at service layer
- Log with Serilog before rethrowing or handling
- Return appropriate HTTP status codes from controllers
- Use `ModelState.IsValid` checks in controllers before processing

## Authentication & Authorization

- Use JWT bearer tokens (configured in `Program.cs`)
- Policies: `StudentOnly`, `TeacherOnly`, `AdminOnly`, `ParentOnly`, `TeacherOrAdmin`, `StudentOrParent`
- Extract user ID: `User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value`
- Always validate user ID is not null/empty before processing

## Caching Strategy

### Redis Cache Requirements

- **Teachers**: Cache `GetAll()` results for 1 hour (invalidate on create/update/delete)
- **Courses**: Cache `GetAll()` and `GetById(id)` for 30 minutes (invalidate on changes)
- **Cache Key Format**: `teachers:all`, `courses:all`, `courses:{id}`

### Cache Implementation Pattern

```csharp
// Get from cache
var key = "courses:all";
var cached = await _cacheService.GetAsync<List<CourseDto>>(key);
if (cached != null) return cached;

// If not in cache, fetch from database
var data = await _unitOfWork.Courses.GetAllAsync();
var dtos = _mapper.Map<List<CourseDto>>(data);

// Store in cache (TTL: 30 minutes)
await _cacheService.SetAsync(key, dtos, TimeSpan.FromMinutes(30));
return dtos;
```

## Real-Time Notifications (SignalR)

- Use `INotificationService.CreateNotificationAsync()` for database persistence
- SignalR hub broadcasts to connected clients automatically
- Notification types: `Enrollment`, `NewContent`, `NewQuiz`, `QuizCompleted`, `NewSection`, `NewLesson`
- Priority levels: `Low`, `Medium`, `High`

## Course Access Codes

- **Expiration**: 32 days from generation (FIXED - NOT 7 days)
- **Generation**: Use `ICourseAccessCodeService.GenerateCodeAsync()` or `BulkGenerateCodesAsync()`
- **Format**: 12-character alphanumeric (A-Z, 0-9)
- **Usage**: One-time use, validates expiration, course enrollment, and duplicate checks
- **Bulk Operations**: Optimized for 1000+ codes with in-memory deduplication

## Lesson Completion & Progress

- `ProgressService.MarkLessonCompleteAsync()` creates `LessonProgress` record
- Automatically updates `CourseProgress.CompletionPercentage`
- Formula: `(CompletedLessons / TotalLessons) * 100`
- Prevent duplicate completions: Check before creating

## Quiz Management

- **Attempts**: Single attempt by default (`AttemptsAllowed = 1`)
- **Score Calculation**: Sum of correct answer points
- **Passing**: Checked against `Quiz.PassingScore` percentage
- **Reset**: Admin can delete attempts to allow retakes
- **Notifications**: Created on completion with score and percentage

## Parent-Student Linking

- **Link Codes**: 6-digit, 24-hour expiration
- **One-Way**: Parents link to students (not vice versa)
- **Permissions**: Parents see only linked students' progress and quiz marks
- **Verification**: Always check `IParentService.IsLinkedAsync()` before returning data

## DTOs & Mappings

- Keep DTOs in `BLL.DTOs.*` folders organized by feature
- AutoMapper profiles in `BLL.Mappings.MappingProfile`
- DTO names should reflect operations: `Create*`, `Update*`, `*Detail`, `*Summary`

## Testing & Validation

- DTOs use `[Required]`, `[StringLength]`, `[Range]` attributes
- Controllers validate `ModelState.IsValid` before processing
- Services perform business logic validation (e.g., enrollment checks)

## Logging Best Practices

- Use structured logging: `_logger.Information("Message with {Parameter1} and {Parameter2}", param1, param2);`
- Log levels: `Debug` (detailed info), `Information` (general flow), `Warning` (issues), `Error` (exceptions)
- Always log exceptions with stack trace: `_logger.Error(ex, "Operation failed")`
- Log IDs for traceability: StudentId, CourseId, QuizId, etc.

## File Paths & Assets

- **Videos/PDFs**: Stored on BunnyCDN
- **URLs**: Retrieved from `Lesson.VideoUrl` and `Lesson.MaterialUrl`
- **Media Service**: Use `IMediaService` for CDN interactions

## Device Sessions (Mobile/Web)

- Managed by `IDeviceSessionService`
- 30-day session expiration
- Tracks device info, IP address, client type
- Admin can revoke all user sessions

## Comments & Documentation

- Use `/// <summary>` for public methods in services
- Document complex business logic inline
- Keep comments up-to-date with code changes

## Code Review Checklist

Before committing code:

- [ ] Follows `.editorconfig` formatting
- [ ] Uses async/await properly
- [ ] Includes structured logging
- [ ] Handles exceptions gracefully
- [ ] Validates user permissions
- [ ] Uses transactions where needed
- [ ] Includes proper null checks
- [ ] DTO mappings are complete
- [ ] No hardcoded values (use constants/settings)
- [ ] Cache invalidation logic is present (where applicable)