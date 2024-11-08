# Archive Mode

**This project/template has been merged to a new modern template and no longer updated. Please continue your support at the new project template home: https://github.com/DDTH/blazor-admin-template**

# .NET WebAPI Template

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![Actions Status](https://github.com/btnguyen2k/dotnet-webapi-template/workflows/ci/badge.svg)](https://github.com/btnguyen2k/dotnet-webapi-template/actions)
[![Release](https://img.shields.io/github/release/btnguyen2k/dotnet-webapi-template.svg?style=flat-square)](RELEASE-NOTES.md)

Template to quickly spin up a .NET Web API project.

## Features

- Skeleton to quickly build RESTful APIs with .NET:
  - JSON for input and output.
  - Authentication and authorization using JWT with ASP.NET Core Identity.
  - Included sample Database access using [Entity Framework](https://learn.microsoft.com/en-us/ef/core/).
- Sample files included: README, LICENSE, RELEASE-NOTES, and `.gitignore`.
- GitHub Actions integrated:
  - `dependabot.yaml`, `automerge-dependabot.yaml`: Automatically update dependencies and merge PRs from Dependabot.
  - `ci.yaml`: Automatically build and run tests with code coverage reports.
  - `release.yaml`: Automatically create new releases.
  - `codeql.yaml`: Automatically run CodeQL analysis.
- Sample Dockerfile files to package the application as Docker images for Linux and Windows.

## Usage

Install (or update) the package from NuGet to make the template available:

```sh
$ dotnet new install Ddth.Templates.WebApi
```

After the package is installed, you can create a new project using the template:

```sh
$ dotnet new dwt -n MyApp
```

The above command will create a new solution named `MyApp` the current directory.

**Happy coding!**

🌟 If you find this project useful, please star it. 🌟

## License

This template is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

## Contributing & Support

Feel free to create [pull requests](https://github.com/btnguyen2k/dotnet-webapi-template/compare/contrib_wait_for_merge...) or [issues](https://github.com/btnguyen2k/dotnet-webapi-template/issues) to report bugs or suggest new features.

> Please create PRs against the `contrib_wait_for_merge` branch.
