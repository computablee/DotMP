# DotMP

[![Nuget](https://img.shields.io/nuget/v/DotMP.svg?style=flat-square)](https://www.nuget.org/packages/DotMP)
![Build](https://github.com/computablee/DotMP/actions/workflows/compile.yml/badge.svg)
![Tests](https://github.com/computablee/DotMP/actions/workflows/integration.yml/badge.svg)
[![Quality](https://github.com/computablee/DotMP/actions/workflows/lint.yml/badge.svg)](https://github.com/marketplace/actions/super-linter)
[![Codecov](https://codecov.io/gh/computablee/DotMP/graph/badge.svg?token=MHAKXKRV1K)](https://codecov.io/gh/computablee/DotMP)
[![All Contributors](https://img.shields.io/github/all-contributors/computablee/DotMP?color=ee8449&style=flat-square)](#contributors)

![DotMP logo](https://raw.githubusercontent.com/computablee/DotMP/main/dotmp_logo.png)

A library for writing OpenMP-style parallel code in .NET.
Inspired by the fork-join paradigm of OpenMP, and attempts to replicate the OpenMP programming style as faithfully as possible, though breaking spec at times.

Prior users of OpenMP should find DotMP to be fairly intuitive, though there are some important differences.

The repository and all source code [can be found here](https://github.com/computablee/DotMP/tree/main).
For extensive documentation (including documentation of internal and private methods), an up-to-date copy of the Doxygen docs is hosted [here](https://computablee.github.io/DotMP/).
For a comprehensive tutorial on DotMP, [check out the wiki](https://github.com/computablee/DotMP/wiki).

## Installing DotMP

Check out [DotMP on NuGet](https://www.nuget.org/packages/DotMP).

DotMP can be installed from the NuGet command line interface via the following command:
```
dotnet add package DotMP --version 1.6.1
```
You can also using the following PackageReference:
```xml
<PackageReference Include="DotMP" Version="1.6.1" />
```
Check the [wiki](https://github.com/computablee/DotMP/wiki#building-dotmp-from-source) for instructions on building DotMP from source.

## Contributors

This repository uses [all-contributors](https://github.com/all-contributors/all-contributors) to thank all of the hard-working contributors to this project.

Below is a list of all contributors to DotMP!

<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->
<table>
  <tbody>
    <tr>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/MaurizioPz"><img src="https://avatars.githubusercontent.com/u/455216?v=4?s=100" width="100px;" alt="Maurizio"/><br /><sub><b>Maurizio</b></sub></a><br /><a href="https://github.com/computablee/DotMP/commits?author=MaurizioPz" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://janheres.eu"><img src="https://avatars.githubusercontent.com/u/74781187?v=4?s=100" width="100px;" alt="Jan Hereš"/><br /><sub><b>Jan Hereš</b></sub></a><br /><a href="https://github.com/computablee/DotMP/commits?author=HarryHeres" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/computablee"><img src="https://avatars.githubusercontent.com/u/20172521?v=4?s=100" width="100px;" alt="Phillip Allen Lane"/><br /><sub><b>Phillip Allen Lane</b></sub></a><br /><a href="https://github.com/computablee/DotMP/commits?author=computablee" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/exrol"><img src="https://avatars.githubusercontent.com/u/86170495?v=4?s=100" width="100px;" alt="exrol"/><br /><sub><b>exrol</b></sub></a><br /><a href="https://github.com/computablee/DotMP/commits?author=exrol" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/jestes15"><img src="https://avatars.githubusercontent.com/u/51448244?v=4?s=100" width="100px;" alt="Joshua Estes"/><br /><sub><b>Joshua Estes</b></sub></a><br /><a href="https://github.com/computablee/DotMP/commits?author=jestes15" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://www.bayoosoft.com/"><img src="https://avatars.githubusercontent.com/u/45914736?v=4?s=100" width="100px;" alt="blouflashdb"/><br /><sub><b>blouflashdb</b></sub></a><br /><a href="https://github.com/computablee/DotMP/commits?author=blouflashdb" title="Code">💻</a></td>
      <td align="center" valign="top" width="14.28%"><a href="https://github.com/Skenvy"><img src="https://avatars.githubusercontent.com/u/17214791?v=4?s=100" width="100px;" alt="Nathan Levett"/><br /><sub><b>Nathan Levett</b></sub></a><br /><a href="https://github.com/computablee/DotMP/commits?author=Skenvy" title="Code">💻</a></td>
    </tr>
  </tbody>
</table>

<!-- markdownlint-restore -->
<!-- prettier-ignore-end -->

<!-- ALL-CONTRIBUTORS-LIST:END -->
