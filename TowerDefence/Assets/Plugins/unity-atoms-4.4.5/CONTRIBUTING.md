# Contributing

Thanks for considering contributing to Unity Atoms ❤️ Read the guidelines below before you submit an issue or create a PR.

## Project structure

Unity Atoms is a [monorepo](https://en.wikipedia.org/wiki/Monorepo). Basically that means that there are several packages / projects contained in one repository.

-   Packages - contains all the different packages

### UPM doesn't allow...

-   importing a sub folder in a Git repo when depending on a package through a Git URL.
-   excluding files (using property "files" in package.json) when importing locally using the file syntax (eg. "com.unity-atoms.unity-atoms-core": "file:../../Packages/Core").
-   package.json in subdirectories (only root level)

### Current project structure therefore allows for...

-   including an example repo for examples
-   use the local pacakges in the example repo
-   referencing this git repo in another project's manifest file

### Smooth Workflow

The github layout makes it difficult to contribute to Atoms from inside a Unity project. However, there are a few steps one can take to alleviate this issue.

1.  Clone the atoms repository and place it where you want your Unity project to be.
2.  Create a new Unity project (name it whatever, it won't matter) and wait for Unity to load.
3.  Close Unity and drag all the files of your newly created Unity project inside your atoms repository.
4.  Inside the Unity Hub, add the atoms repository to your Projects and open it.
5.  Inside Unity, go to Edit > Preferences > External Tools. Toggle "Embedded Packages" and click the "Regenerate Project Files" button.

You now have the atoms repository inside a Unity project and can easily make changes to it. However, this approach will lead to all kinds of changes in the repository you don't want to push (for example the Assets folder). So we will take a couple of extra steps to supress those.

1.  Open your repository folder and enable hidden items to be shown (this may be dependant on your OS).
2.  Go to .git > info > exclude.
3.  Paste the following:
```
/[Aa]ssets/
/[Ll]ogs/
/[Pp]roject[Ss]ettings/
/[Uu]ser[Ss]ettings/

manifest.json
packages-lock.json
```

Now your Atoms contributing experience will be as smooth as butter on ice.

If you are still confused, you can also [watch this step-by-step](https://youtu.be/BV22qR921Sw) guide on how to set it up.

## Style Guide

### Language Usage

-   Mark closed types as sealed to enable proper devirtualization (see [here](https://blogs.unity3d.com/2016/07/26/il2cpp-optimizations-devirtualization/) for more info).
-   Avoid LINQ usage for runtime usage except where absolutely possible (`ToList` or `ToArray` for example) and avoid using `ForEach`. Using these methods creates easily avoidable garbage. Editor usage is another story as performance is not as generally important.

### Layout

There is an `.editorconfig` at the root of the repository located [here](/.editorconfig) that can be used by most IDEs to help ensure these settings are automatically applied.

-   **Indentation:** 1 tab = 4 spaces (no tab character)
-   **Desired width:** 120-130 characters max per line
-   **Line Endings:** Unix (LF), with a new-line at the end of each file.
-   **White Space:** Trim empty whitespace from the ends of lines.

### Naming and Formatting

| Object Name      | Notation    | Example                   |
| ---------------- | ----------- | ------------------------- |
| Namespaces       | PascalCase  | UnityAtoms                |
| Classes          | PascalCase  | UATags                    |
| Methods          | PascalCase  | GetTags                   |
| Method arguments | camelCase   | oldValue                  |
| Properties       | PascalCase  | Value                     |
| Public fields    | PascalCase  | Value                     |
| Private fields   | \_camelCase | \_value                   |
| Constants        | SNAKE_CASE  | NETWORK_ACCESS_TOKEN_SIZE |
| Inline variables | camelCase   | value                     |

### Structure

-   Follow good encapsulation principles and try to limit exposing fields directly as public; unless necessary everything should be marked as private/protected unless necessary. Where public access to a field is needed, use a public property instead.
-   Where classes or methods are not intended for use by a user, mark these as `internal`.
-   Order class structure like so:
    -   Namespace
        -   Internal classes
        -   Properties (Public/Private)
        -   Fields (Public/Private)
        -   Unity Methods
        -   Primary Methods
        -   Helper Methods
-   Lines of code that are generally longer than 120-130 characters should generally be broken out into multiple lines. For example, instead of:

`public bool SomeMethodWithManyParams(int param1, float param2, List<int> param3, out int param4, out int param5)...`

do

```
public bool SomeMethodWithManyParams(
     int param1,
     float param2,
     List<int> param3,
     out int param4,
     out int param5)...
```

### Example Formatting

```
using System;
using UnityEngine;

namespace Example
{
    public class Foo : MonoBehavior
    {
        public int SomeValue { get { return _someValue; } }

        [SerializeField]
        private int _someValue;

        private const string Warning = "Somethings wrong!";

        private void Update()
        {
            // Do work
            Debug.Log(Warning);
        }
    }
}
```

## Documentation

All new features added to the project should be documented using [C# XML comments](https://docs.microsoft.com/en-us/dotnet/csharp/codedoc). There is a node.js script included in this project to auto generate documentation for the website based on the C# XML comments. This is something needed for a PR to be accepted.

**Prerequisites for running documentation generation script:**

-   Install [Node.js](https://nodejs.org/en/).
-   Install [NPM CLI](https://docs.npmjs.com/cli/npm).
-   Install [csc](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/command-line-building-with-csc-exe) (C# compiler) included in for example [.NET Framework](https://dotnet.microsoft.com/download/dotnet-framework).
-   Run `npm install` in the root of the project to install necessary dependencies.

When you are all setup you simply run `npm run generate:docs` in the root of the project and voila, fresh documentation is generated for you!

## Generator

Before submitting a PR please check and see if the change requires you to update the Generator and the templates.

### Pro tip

If you are doing updates that requires you to update all existing Atoms you can use `Unity Atoms/Tools/Regenerate all Atoms` from the top menu bar.

## Pull requests

Pull requests should be made to the [canary branch](https://github.com/unity-atoms/unity-atoms/tree/canary).

### Checklist before submitting a PR

-   [ ] A PR should always reference an issue - create one if there is none.
-   [ ] Document your code using C# XML comments.
-   [ ] Add documentation to the `docs` folder if needed.
-   [ ] Run `npm run generate:docs` to generate new docs.
-   [ ] Update templates in generator if needed.
-   [ ] Add what you changed in the `CHANGELOG.md`.
-   [ ] Make sure you follow the styleguide listed above.
