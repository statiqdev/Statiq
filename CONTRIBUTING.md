# Contributing

## Where To Start

Here are a few ways you could get involved:

- Report a bug by opening an issue.
- Suggest or request a feature by opening an issue.
- Enhance or make corrections to the Statiq documentation.
- Write a new module and open a pull request for it.
- Write a blog post about your experiences and let us know about it.

If you need help with any of these things, **please don't hesitate to ask**. Statiq is a beginner-friendly project and we want to help anyone contribute who would like to.

Build instructions are in the `BUILDING.md` file at the root of the repository.

## Submitting Code

If you would like to submit code to Statiq, please follow the guidelines below. If your change is small, go ahead and submit a pull request and any questions can be discussed in the request. If it's a larger change, you should probably open an issue first so that it can be discussed before you start spending time coding.

### Making Changes

[Fork](http://help.github.com/forking/) the Statiq repository on GitHub. The Statiq repository uses a single `main` branch and you can submit PRs against that.

### Handling Updates from Upstream/Master

While you're working away in your branch it's quite possible that your upstream master (most likely the canonical Statiq repository) may be updated. If this happens you should rebase your local branch to pull in the changes. If you're working on a long running feature then you may want to do this quite often, rather than run the risk of potential merge issues further down the line.

### Sending a Pull Request

While working on your feature you may well create several branches, which is fine, but before you send a pull request you should ensure that you have rebased back to a single feature branch. When you're ready to go you should confirm that you are up to date and rebased with upstream/master (see above).

### Style Guidelines

Statiq generally follows accepted .NET coding styles (see the [Framework Design Guidelines](https://msdn.microsoft.com/en-us/library/ms229042%28v=vs.110%29.aspx)). Please note the following differences or points:

- Indent with 4 spaces, not tabs.
- Prefix member names with an underscore (`_`).
- Try to use explicit type names unless the type is extremely long (as can happen with generics) or overly complex, in which case the use of `var` is acceptable.
- Use the C# type aliases for types that have them, e.g. `int` instead of `Int32`, `string` instead of `String` etc.
- Use meaningful names (regardless of length) and try not to use abbreviations in your type names.
- Wrap `if`, `else` and `using` blocks (or blocks in general, really) in curly braces, even if it's a single line. The open and closing braces should go on their own line.
- Pay attention to whitespace and extra blank lines.
- Be explicit with access modifiers. If a class member is private, add the `private` access modifier even though it's implied.
- Avoid `#region`. There is debate on whether regions are valuable, but one of the perks of being a benevolent dictator is that I can restrict their use in this code.
- Constants should be TitleCase and should be placed at the top of their containing class.
- Favor default interface members over extension methods for common interface-based functionality. Please default interface members in a separate file as a partial interface with a name like `IFoo.Defaults.cs`.

### Dependencies

Generally, you should only ever have to take a dependency on `Statiq.Common` if developing a module library. If you find yourself needing to depend on other Statiq libraries like `Statiq.Core`, please open an issue so the needed functionality can be moved into the common assembly. This advise doesn't apply if you're actually working on core functionality.

### New Project Checklist

If you need to make a new project, there are a number of things that need to be done:

- Create the project in the appropriate location
- Edit the AssemblyInfo.cs file to remove everything but `AssemblyTitle`, `AssemblyDescription`, `ComVisible`, and `Guid`
- This list used to be longer, but most project configuration is now handled by `Directory.Build.props`

### Unit Tests

Make sure to run all unit tests before creating a pull request. You code should also have reasonable unit test coverage.

The tests in Statiq follow a very specific pattern, please attempt to follow the same pattern in the tests for your code:

- Tests use [NUnit](https://github.com/nunit).
- Instead of mocks, use stub classes from `Statiq.Testing`. Add new stub behavior there if needed to support your tests.
- All tests are placed into a fixture class with the name `ObjectNameFixture` where `ObjectName` is the name of the object under test.
- The fixture class should be placed in the appropriate test project at the same relative path as the primary source file for the object being tested.
- The fixture class should inherit from `BaseFixture` in the `Statiq.Testing` library.
- Within the fixture class, tests for each method, property, or other symbol should be placed inside a nested class with the name `SymbolNameTests` where `SymbolName` is the name of the symbol (method, property, etc.) being tested. Tests for overloaded methods can all go in the same test class.
- The nested symbol test class should inherit from the outer containing fixture class.
- Test methods for the symbol should be placed in the nested symbol test class and should have explanatory names that follow standard naming conventions. The names of test methods don't have to follow any specific guideline as long as it's reasonably clear what the purpose of the test is.
- Each test method should contain the comments `// Given`, `// When`, and `// Then` to separate the test code into three clear sections (though these can be combined for simple tests so you might have a comment like `// When, Then`).
- Use [Shouldly](https://github.com/shouldly/shouldly) for assertions in new test code.

If there are any questions about how to format test code please take a look at the existing test code or ask.

## Updating Documentation

Making updates to the Statiq documentation is as helpful as writing code (if not more helpful).

## Contributor License Agreement

Because of the non-commercial clause of the Prosperity Public License that Statiq uses as well as the availability of a private license for commercial use, contributing to Statiq requires signing a contributor license agreement to ensure proper rights assignments. Basically, you have to agree to let your contribution be relicensed under the terms of the Statiq license by assigning copyrights over to the project. A GitHub bot will prompt you to sign the CLA on your first pull request, or you can [sign the CLA ahead of time](https://cla-assistant.io/statiqdev/Statiq.Framework).
