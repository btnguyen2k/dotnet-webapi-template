# dotnet-webapi-template

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![Actions Status](https://github.com/btnguyen2k/dotnet-webapi-template/workflows/ci/badge.svg)](https://github.com/btnguyen2k/dotnet-webapi-template/actions)
[![Release](https://img.shields.io/github/release/btnguyen2k/dotnet-webapi-template.svg?style=flat-square)](RELEASE-NOTES.md)

Template to quickly spin up a .NET Web API project.

## Features

- Skeleton to quickly build RESTful APIs with .NET:
  - Supports authentication and authorization using JWT.
  - Utilizes JSON for input and output.
  - Included sample Database access using [Entity Framework](https://learn.microsoft.com/en-us/ef/core/).
- Sample files included: README, LICENSE, RELEASE-NOTES, and `.gitignore`.
- GitHub Actions integrated:
  - `dependabot.yaml`, `automerge-dependabot.yaml`: Automatically update dependencies and merge PRs from Dependabot.
  - `ci.yaml`: Automatically run tests and generate code coverage reports.
  - `release.yaml`: Automatically create new releases.
  - `codeql.yaml`: Automatically run CodeQL analysis.

## Usage

**Create new project from this template**

(preferred method) Utilizing either the [Use this template](https://docs.github.com/en/repositories/creating-and-managing-repositories/creating-a-repository-from-a-template#creating-a-repository-from-a-template) feature or the `gh` command line tool:

```sh
$ gh repo create my-new-project --template btnguyen2k/dotnet-webapi-template
```

(less preferred method) Or simply clone/fork this repository.

**(optional) Rename the project/namespace**

Use the `Rename` feature from Visual Studio (or other IDEs that you are using) to rename the project/namespace to match your brand.

**Update sample files**

`LICENSE.tpl.md`, `README.tpl.md` and `RELEASE-NOTES.tpl.md` are sample license, readme and release-notes files. Update them to
reflect your application's name, license and functionality; then rename them to `LICENSE.md`, `README.md` and `RELEASE-NOTES.md`.

**Happy coding!**

## License

This template is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

## Contributing & Support

Feel free to create [pull requests](https://github.com/btnguyen2k/dotnet-webapi-template/pulls) or [issues](https://github.com/btnguyen2k/dotnet-webapi-template/issues) to report bugs or suggest new features. If you find this project useful, please star it.
