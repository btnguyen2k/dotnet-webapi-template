# .NET WebAPI Template release notes

## 2024-09-01 - v1.0.0

### Changes

- Redesigned bootstrapping.

## 2024-08-15 - v0.4.0

### Added/Refactoring

- Refactor: bootstrap flow redesigned.
- Refactor: JWT authorization redesigned.

### Fixed/Improvement

- Fixed CA1820, CA1823, ASP0015, IDE0007 warnings.

## 2024-08-09 - v0.3.0

### Added/Refactoring

- Added .editorconfig and .gitattributes files.
- Refactored to move EF repositories to a separate project.

### Fixed/Improvement

- Improved .gitignore rules.
- Improvement: fixed CA2254 warning.

## 2024-08-08 - v0.2.1

### Fixed/Improvement

- Fix: register repositories with AddDbContext/AddDbContextPool.
- Fix: user roles mismatched causing JwtAuthorizeAttribute failed to validate user roles.
- Fix: use services.AddExceptionHandler() to handle exception globally.

## 2024-08-08 - v0.2.0

### Added/Refactoring

- Feature: Pack and Publish as NuGet package.

## 2024-07-31 - v0.1.0

### Added/Refactoring

- Feature: RESTful API skeleton, using JSON for input and output.
- Feature: Authentication and authorization using JWT.
- Feature: Sample database access using Entity Framework.
- Feature: GitHub Actions integrated.
