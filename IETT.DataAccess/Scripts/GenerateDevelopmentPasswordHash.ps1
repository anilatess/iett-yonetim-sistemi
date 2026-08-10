[CmdletBinding()]
param(
    [switch]$ValidateOnly
)

$ErrorActionPreference = "Stop"

$entityProject = (Resolve-Path (Join-Path $PSScriptRoot "..\..\IETT.Entity\IETT.Entity.csproj")).Path
$temporaryProject = Join-Path ([System.IO.Path]::GetTempPath()) (
    "iett-password-hash-" + [Guid]::NewGuid().ToString("N"))

New-Item -ItemType Directory -Path $temporaryProject | Out-Null

try {
    $escapedEntityProject = [System.Security.SecurityElement]::Escape($entityProject)
    $projectContent = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
    <ProjectReference Include="$escapedEntityProject" />
  </ItemGroup>
</Project>
"@

    $programContent = @'
using IETT.Entity.Entities;
using Microsoft.AspNetCore.Identity;
using System.Text;

static string ReadPassword(string prompt)
{
    Console.Error.Write(prompt);
    var password = new StringBuilder();

    while (true)
    {
        var key = Console.ReadKey(intercept: true);
        if (key.Key == ConsoleKey.Enter)
        {
            Console.Error.WriteLine();
            return password.ToString();
        }

        if (key.Key == ConsoleKey.Backspace)
        {
            if (password.Length > 0)
            {
                password.Length--;
            }
            continue;
        }

        if (!char.IsControl(key.KeyChar))
        {
            password.Append(key.KeyChar);
        }
    }
}

var password = ReadPassword("Development password: ");
var confirmation = ReadPassword("Confirm password: ");

if (string.IsNullOrWhiteSpace(password))
{
    Console.Error.WriteLine("Password cannot be empty.");
    return 1;
}

if (!string.Equals(password, confirmation, StringComparison.Ordinal))
{
    Console.Error.WriteLine("Passwords do not match.");
    return 1;
}

var hasher = new PasswordHasher<User>();
var hash = hasher.HashPassword(new User(), password);
Console.Out.WriteLine(hash);
return 0;
'@

    Set-Content -LiteralPath (Join-Path $temporaryProject "HashTool.csproj") `
        -Value $projectContent -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $temporaryProject "Program.cs") `
        -Value $programContent -Encoding UTF8

    if ($ValidateOnly) {
        dotnet build (Join-Path $temporaryProject "HashTool.csproj") `
            --configuration Release --nologo --verbosity quiet

        if ($LASTEXITCODE -ne 0) {
            throw "Password hash helper validation failed."
        }

        return
    }

    dotnet run --project (Join-Path $temporaryProject "HashTool.csproj") `
        --configuration Release --nologo --verbosity quiet

    if ($LASTEXITCODE -ne 0) {
        throw "Password hash generation failed."
    }
}
finally {
    if (Test-Path -LiteralPath $temporaryProject) {
        Remove-Item -LiteralPath $temporaryProject -Recurse -Force
    }
}
