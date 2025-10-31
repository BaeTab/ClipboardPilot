# ClipboardPilot 릴리스 빌드 및 설치 파일 생성 스크립트
# PowerShell 7+ 권장

param(
    [string]$Version = "1.0.0",
    [switch]$SkipBuild,
    [switch]$SkipSetup,
    [switch]$Clean
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

# 색상 출력 함수
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

function Write-Step {
    param([string]$Message)
    Write-ColorOutput "`n==> $Message" "Cyan"
}

function Write-Success {
    param([string]$Message)
    Write-ColorOutput "✓ $Message" "Green"
}

function Write-Error {
    param([string]$Message)
    Write-ColorOutput "✗ $Message" "Red"
}

# 시작
Write-ColorOutput @"
╔══════════════════════════════════════════════════════════╗
║                                                          ║
║     📋 클립보드 파일럿 - 릴리스 빌드 스크립트              ║
║                                                          ║
║     Version: $Version                                    ║
║                                                          ║
╚══════════════════════════════════════════════════════════╝
"@ "Yellow"

# 경로 설정
$RootPath = Split-Path -Parent $PSScriptRoot
$ProjectPath = Join-Path $RootPath "ClipboardPilot"
$ProjectFile = Join-Path $ProjectPath "ClipboardPilot.csproj"
$PublishPath = Join-Path $ProjectPath "bin\Release\net8.0-windows\win-x64\publish"
$OutputPath = Join-Path $RootPath "Output"
$SetupPath = Join-Path $RootPath "Setup"
$SetupScript = Join-Path $SetupPath "ClipboardPilot.iss"

# 경로 확인
if (-not (Test-Path $ProjectFile)) {
    Write-Error "프로젝트 파일을 찾을 수 없습니다: $ProjectFile"
    exit 1
}

# Clean
if ($Clean) {
    Write-Step "이전 빌드 정리 중..."
    
    if (Test-Path (Join-Path $ProjectPath "bin")) {
        Remove-Item (Join-Path $ProjectPath "bin") -Recurse -Force
        Write-Success "bin 폴더 삭제"
    }
    
    if (Test-Path (Join-Path $ProjectPath "obj")) {
        Remove-Item (Join-Path $ProjectPath "obj") -Recurse -Force
        Write-Success "obj 폴더 삭제"
    }
    
    if (Test-Path $OutputPath) {
        Remove-Item $OutputPath -Recurse -Force
        Write-Success "Output 폴더 삭제"
    }
}

# 출력 폴더 생성
if (-not (Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath | Out-Null
}

# 1. 빌드
if (-not $SkipBuild) {
    Write-Step "프로젝트 빌드 중..."
    
    # Restore
    Write-ColorOutput "패키지 복원 중..." "Gray"
    dotnet restore $ProjectFile
    if ($LASTEXITCODE -ne 0) {
        Write-Error "패키지 복원 실패"
        exit 1
    }
    Write-Success "패키지 복원 완료"
    
    # Build
    Write-ColorOutput "Release 빌드 중..." "Gray"
    dotnet build $ProjectFile -c Release --no-restore
    if ($LASTEXITCODE -ne 0) {
        Write-Error "빌드 실패"
        exit 1
    }
    Write-Success "빌드 완료"
    
    # Publish
    Write-ColorOutput "게시 중 (Self-contained, Single-file)..." "Gray"
    dotnet publish $ProjectFile `
        -c Release `
        -r win-x64 `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:PublishTrimmed=false `
        -p:PublishReadyToRun=false `
        -p:Version=$Version
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "게시 실패"
        exit 1
    }
    Write-Success "게시 완료"
    
    # 파일 크기 확인
    $ExePath = Join-Path $PublishPath "ClipboardPilot.exe"
    if (Test-Path $ExePath) {
        $FileSize = (Get-Item $ExePath).Length / 1MB
        Write-Success ("실행 파일 크기: {0:N2} MB" -f $FileSize)
    }
}

# 2. ZIP 파일 생성
Write-Step "ZIP 파일 생성 중..."

$ZipFileName = "ClipboardPilot-v$Version-win-x64.zip"
$ZipPath = Join-Path $OutputPath $ZipFileName

if (Test-Path $ZipPath) {
    Remove-Item $ZipPath -Force
}

# ZIP 내용 준비
$TempZipDir = Join-Path $env:TEMP "ClipboardPilot-Temp"
if (Test-Path $TempZipDir) {
    Remove-Item $TempZipDir -Recurse -Force
}
New-Item -ItemType Directory -Path $TempZipDir | Out-Null

# 파일 복사
Copy-Item (Join-Path $PublishPath "ClipboardPilot.exe") $TempZipDir
Copy-Item (Join-Path $RootPath "README.md") $TempZipDir -ErrorAction SilentlyContinue
Copy-Item (Join-Path $RootPath "LICENSE.txt") $TempZipDir -ErrorAction SilentlyContinue
Copy-Item (Join-Path $ProjectPath "clipboard-pilot.settings.json") $TempZipDir -ErrorAction SilentlyContinue

# README 생성
$ReadmeContent = @"
클립보드 파일럿 v$Version
========================

설치 방법
--------
1. ClipboardPilot.exe를 원하는 폴더에 복사하세요.
2. 실행하면 자동으로 데이터베이스가 생성됩니다.

시스템 요구사항
--------------
- Windows 10 (1903 이상) 또는 Windows 11
- .NET 8.0 Runtime (포함됨)

사용법
------
1. 프로그램을 실행하세요.
2. 평소처럼 Ctrl+C로 복사하면 자동으로 저장됩니다.
3. Ctrl+Shift+V로 미니 패널을 열 수 있습니다.

단축키
------
- Ctrl+Shift+V : 미니 패널
- Ctrl+Alt+V : 이전 항목 붙여넣기
- Ctrl+Alt+1~9 : 즐겨찾기

데이터 위치
-----------
%APPDATA%\ClipboardPilot\

문의
----
https://github.com/clipboardpilot/clipboardpilot
"@

Set-Content -Path (Join-Path $TempZipDir "사용설명서.txt") -Value $ReadmeContent -Encoding UTF8

# ZIP 생성
Compress-Archive -Path "$TempZipDir\*" -DestinationPath $ZipPath -Force

Remove-Item $TempZipDir -Recurse -Force

Write-Success "ZIP 파일 생성: $ZipFileName"

# 3. Inno Setup으로 설치 파일 생성
if (-not $SkipSetup) {
    Write-Step "설치 파일 생성 중..."
    
    # Inno Setup 경로 찾기
    $InnoSetupPaths = @(
        "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
        "C:\Program Files\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
    )
    
    $InnoSetupExe = $null
    foreach ($path in $InnoSetupPaths) {
        if (Test-Path $path) {
            $InnoSetupExe = $path
            break
        }
    }
    
    if ($null -eq $InnoSetupExe) {
        Write-ColorOutput "⚠ Inno Setup이 설치되지 않았습니다." "Yellow"
        Write-ColorOutput "  다운로드: https://jrsoftware.org/isdl.php" "Gray"
        Write-ColorOutput "  설치 후 다시 실행하세요." "Gray"
    }
    else {
        Write-ColorOutput "Inno Setup 발견: $InnoSetupExe" "Gray"
        
        # Setup 스크립트 찾기 (우선순위: Simple > Full > Minimal)
        $SetupScripts = @(
            (Join-Path $SetupPath "ClipboardPilot-Simple.iss"),
            (Join-Path $SetupPath "ClipboardPilot-Full.iss"),
            (Join-Path $SetupPath "ClipboardPilot-Minimal.iss"),
            $SetupScript
        )
        
        $FoundScript = $null
        foreach ($script in $SetupScripts) {
            if (Test-Path $script) {
                $FoundScript = $script
                Write-ColorOutput "Setup 스크립트 사용: $(Split-Path $script -Leaf)" "Gray"
                break
            }
        }
        
        if ($null -eq $FoundScript) {
            Write-ColorOutput "⚠ Setup 스크립트를 찾을 수 없습니다." "Yellow"
            Write-ColorOutput "  다음 위치에 스크립트를 생성하세요: $SetupPath" "Gray"
        }
        else {
            # LICENSE 파일 생성 (없으면)
            $LicensePath = Join-Path $RootPath "LICENSE.txt"
            if (-not (Test-Path $LicensePath)) {
                $LicenseContent = @"
MIT License

Copyright (c) 2024 Clipboard Pilot

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
"@
                Set-Content -Path $LicensePath -Value $LicenseContent -Encoding UTF8
            }
            
            # Inno Setup 실행
            Write-ColorOutput "컴파일 중..." "Gray"
            & $InnoSetupExe $FoundScript /Q
            
            if ($LASTEXITCODE -eq 0) {
                Write-Success "설치 파일 생성 완료"
                
                # 생성된 설치 파일 확인
                $SetupFiles = Get-ChildItem $OutputPath -Filter "ClipboardPilot-Setup*.exe"
                foreach ($file in $SetupFiles) {
                    $FileSize = $file.Length / 1MB
                    Write-ColorOutput ("  - {0} ({1:N2} MB)" -f $file.Name, $FileSize) "Gray"
                }
            }
            else {
                Write-ColorOutput "⚠ 설치 파일 생성 실패 (Exit Code: $LASTEXITCODE)" "Yellow"
                Write-ColorOutput "  수동으로 Inno Setup Compiler를 실행하여 확인하세요." "Gray"
            }
        }
    }
}

# 4. 체크섬 생성
Write-Step "체크섬 생성 중..."

$ChecksumFile = Join-Path $OutputPath "checksums.txt"
$ChecksumContent = @()

$Files = Get-ChildItem $OutputPath -File
foreach ($file in $Files) {
    if ($file.Extension -ne ".txt") {
        $Hash = Get-FileHash $file.FullName -Algorithm SHA256
        $ChecksumContent += "$($Hash.Hash)  $($file.Name)"
    }
}

$ChecksumContent | Set-Content -Path $ChecksumFile -Encoding UTF8
Write-Success "체크섬 파일 생성: checksums.txt"

# 완료
Write-Step "완료!"

Write-ColorOutput @"

생성된 파일:
-------------
"@ "Green"

Get-ChildItem $OutputPath -File | ForEach-Object {
    $Size = $_.Length / 1MB
    Write-ColorOutput ("  📦 {0,-40} ({1,8:N2} MB)" -f $_.Name, $Size) "White"
}

Write-ColorOutput @"

다음 단계:
---------
1. Output 폴더의 파일을 확인하세요.
2. 설치 파일(*.exe)을 테스트하세요.
3. GitHub Release에 업로드하세요.

"@ "Cyan"

# 파일 탐색기로 출력 폴더 열기
if ($Host.Name -eq "ConsoleHost") {
    $Response = Read-Host "출력 폴더를 여시겠습니까? (Y/N)"
    if ($Response -eq "Y" -or $Response -eq "y") {
        Start-Process explorer.exe $OutputPath
    }
}
