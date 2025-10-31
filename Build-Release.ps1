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

# 경로 설정 (스크립트가 루트 디렉토리에 있음)
$RootPath = $PSScriptRoot
if ([string]::IsNullOrEmpty($RootPath)) {
    $RootPath = Get-Location
}

Write-ColorOutput "루트 경로: $RootPath" "Gray"

$ProjectPath = Join-Path $RootPath "ClipboardPilot"
$ProjectFile = Join-Path $ProjectPath "ClipboardPilot.csproj"
$PublishPath = Join-Path $ProjectPath "bin\Release\net8.0-windows\win-x64\publish"
$OutputPath = Join-Path $RootPath "Output"
$SetupPath = Join-Path $RootPath "Setup"

Write-ColorOutput "프로젝트 경로: $ProjectPath" "Gray"
Write-ColorOutput "출력 경로: $OutputPath" "Gray"

# 경로 확인
if (-not (Test-Path $ProjectFile)) {
    Write-Error "프로젝트 파일을 찾을 수 없습니다: $ProjectFile"
    Write-ColorOutput "현재 디렉토리: $(Get-Location)" "Gray"
    exit 1
}

Write-Success "프로젝트 파일 확인: $ProjectFile"

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
Write-Step "출력 폴더 준비 중..."
if (-not (Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath | Out-Null
    Write-Success "Output 폴더 생성: $OutputPath"
}
else {
    Write-Success "Output 폴더 확인: $OutputPath"
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
    else {
        Write-Error "실행 파일을 찾을 수 없습니다: $ExePath"
        exit 1
    }
}
else {
    Write-ColorOutput "빌드 건너뜀 (SkipBuild 옵션)" "Yellow"
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
$ExePath = Join-Path $PublishPath "ClipboardPilot.exe"
if (Test-Path $ExePath) {
    Copy-Item $ExePath $TempZipDir
    Write-ColorOutput "실행 파일 복사됨" "Gray"
}
else {
    Write-Error "실행 파일을 찾을 수 없습니다: $ExePath"
    exit 1
}

Copy-Item (Join-Path $RootPath "README.md") $TempZipDir -ErrorAction SilentlyContinue
Copy-Item (Join-Path $RootPath "LICENSE.txt") $TempZipDir -ErrorAction SilentlyContinue
Copy-Item (Join-Path $ProjectPath "clipboard-pilot.settings.json") $TempZipDir -ErrorAction SilentlyContinue

# 사용설명서 생성
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
https://github.com/BaeTab/ClipboardPilot
"@

Set-Content -Path (Join-Path $TempZipDir "사용설명서.txt") -Value $ReadmeContent -Encoding UTF8

# ZIP 생성
Compress-Archive -Path "$TempZipDir\*" -DestinationPath $ZipPath -Force

Remove-Item $TempZipDir -Recurse -Force

$ZipSize = (Get-Item $ZipPath).Length / 1MB
Write-Success ("ZIP 파일 생성: $ZipFileName ({0:N2} MB)" -f $ZipSize)

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
            (Join-Path $SetupPath "ClipboardPilot-Minimal.iss")
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
            # Inno Setup 실행
            Write-ColorOutput "컴파일 중..." "Gray"
            $InnoArgs = @(
                $FoundScript
                "/Q"
                "/DMyAppVersion=$Version"
            )
            
            & $InnoSetupExe $InnoArgs
            
            if ($LASTEXITCODE -eq 0) {
                Write-Success "설치 파일 생성 완료"
                
                # 생성된 설치 파일 확인
                $SetupFiles = Get-ChildItem $OutputPath -Filter "ClipboardPilot-Setup*.exe" -ErrorAction SilentlyContinue
                if ($SetupFiles) {
                    foreach ($file in $SetupFiles) {
                        $FileSize = $file.Length / 1MB
                        Write-ColorOutput ("  - {0} ({1:N2} MB)" -f $file.Name, $FileSize) "Gray"
                    }
                }
                else {
                    Write-ColorOutput "  설치 파일이 Output 폴더에 없습니다. Setup 폴더를 확인하세요." "Yellow"
                }
            }
            else {
                Write-ColorOutput "⚠ 설치 파일 생성 실패 (Exit Code: $LASTEXITCODE)" "Yellow"
                Write-ColorOutput "  수동으로 Inno Setup Compiler를 실행하여 확인하세요." "Gray"
            }
        }
    }
}
else {
    Write-ColorOutput "설치 파일 생성 건너뜀 (SkipSetup 옵션)" "Yellow"
}

# 4. 체크섬 생성
Write-Step "체크섬 생성 중..."

$ChecksumFile = Join-Path $OutputPath "checksums.txt"
$ChecksumContent = @()

$Files = Get-ChildItem $OutputPath -File -ErrorAction SilentlyContinue
if ($Files) {
    foreach ($file in $Files) {
        if ($file.Extension -ne ".txt") {
            try {
                $Hash = Get-FileHash $file.FullName -Algorithm SHA256
                $ChecksumContent += "$($Hash.Hash)  $($file.Name)"
            }
            catch {
                Write-ColorOutput "경고: $($file.Name) 체크섬 생성 실패" "Yellow"
            }
        }
    }
    
    if ($ChecksumContent.Count -gt 0) {
        $ChecksumContent | Set-Content -Path $ChecksumFile -Encoding UTF8
        Write-Success "체크섬 파일 생성: checksums.txt"
    }
}
else {
    Write-ColorOutput "⚠ Output 폴더에 파일이 없습니다." "Yellow"
}

# 완료
Write-Step "완료!"

Write-ColorOutput @"

생성된 파일:
-------------
"@ "Green"

$OutputFiles = Get-ChildItem $OutputPath -File -ErrorAction SilentlyContinue
if ($OutputFiles) {
    foreach ($file in $OutputFiles) {
        $Size = $file.Length / 1MB
        Write-ColorOutput ("  📦 {0,-40} ({1,8:N2} MB)" -f $file.Name, $Size) "White"
    }
}
else {
    Write-ColorOutput "  (파일 없음)" "Gray"
}

Write-ColorOutput @"

다음 단계:
---------
1. Output 폴더의 파일을 확인하세요: $OutputPath
2. 설치 파일(*.exe)을 테스트하세요.
3. GitHub Release에 업로드하세요.

"@ "Cyan"

# 파일 탐색기로 출력 폴더 열기
if ($Host.Name -eq "ConsoleHost") {
    $Response = Read-Host "출력 폴더를 여시겠습니까? (Y/N)"
    if ($Response -eq "Y" -or $Response -eq "y") {
        if (Test-Path $OutputPath) {
            Start-Process explorer.exe $OutputPath
        }
    }
}
