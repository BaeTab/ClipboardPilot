# 간단한 릴리스 빌드 스크립트 (Inno Setup 불필요)
# 단일 실행 파일만 생성

param(
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"

Write-Host "=== 클립보드 파일럿 빌드 시작 ===" -ForegroundColor Cyan
Write-Host "버전: $Version" -ForegroundColor Yellow
Write-Host ""

# 경로 설정
$RootPath = $PSScriptRoot
if ([string]::IsNullOrEmpty($RootPath)) {
    $RootPath = Get-Location
}

Write-Host "루트 경로: $RootPath" -ForegroundColor Gray

$ProjectPath = Join-Path $RootPath "ClipboardPilot\ClipboardPilot.csproj"
$OutputPath = Join-Path $RootPath "Output"

Write-Host "프로젝트: $ProjectPath" -ForegroundColor Gray
Write-Host "출력: $OutputPath" -ForegroundColor Gray
Write-Host ""

# 프로젝트 파일 확인
if (-not (Test-Path $ProjectPath)) {
    Write-Host "✗ 프로젝트 파일을 찾을 수 없습니다: $ProjectPath" -ForegroundColor Red
    Write-Host "현재 디렉토리: $(Get-Location)" -ForegroundColor Gray
    exit 1
}

Write-Host "✓ 프로젝트 파일 확인" -ForegroundColor Green

# 출력 폴더 생성
if (Test-Path $OutputPath) {
    Write-Host "기존 Output 폴더 삭제 중..." -ForegroundColor Yellow
    Remove-Item $OutputPath -Recurse -Force
}

New-Item -ItemType Directory -Path $OutputPath | Out-Null
Write-Host "✓ Output 폴더 생성: $OutputPath" -ForegroundColor Green
Write-Host ""

try {
    Write-Host "1. 패키지 복원 중..." -ForegroundColor Green
    dotnet restore $ProjectPath
    if ($LASTEXITCODE -ne 0) {
        throw "패키지 복원 실패"
    }
    
    Write-Host "2. Release 빌드 중..." -ForegroundColor Green
    dotnet build $ProjectPath -c Release --no-restore
    if ($LASTEXITCODE -ne 0) {
        throw "빌드 실패"
    }
    
    Write-Host "3. 게시 중 (Self-contained Single-file)..." -ForegroundColor Green
    $PublishOutput = Join-Path $OutputPath "ClipboardPilot"
    
    dotnet publish $ProjectPath `
        -c Release `
        -r win-x64 `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:PublishTrimmed=false `
        -p:Version=$Version `
        -o $PublishOutput
    
    if ($LASTEXITCODE -ne 0) {
        throw "게시 실패"
    }
    
    Write-Host ""
    Write-Host "✓ 빌드 성공!" -ForegroundColor Green
    Write-Host ""
    Write-Host "생성된 파일:" -ForegroundColor Yellow
    
    $ExeFiles = Get-ChildItem $PublishOutput -Filter "*.exe"
    if ($ExeFiles) {
        foreach ($file in $ExeFiles) {
            $Size = $file.Length / 1MB
            Write-Host ("  - {0} ({1:N2} MB)" -f $file.Name, $Size) -ForegroundColor White
        }
    }
    else {
        Write-Host "  (실행 파일을 찾을 수 없습니다)" -ForegroundColor Red
    }
    
    Write-Host ""
    Write-Host "위치: $PublishOutput" -ForegroundColor Cyan
    Write-Host ""
    
    # 실행 여부 확인
    if ($Host.Name -eq "ConsoleHost") {
        $Response = Read-Host "출력 폴더를 여시겠습니까? (Y/N)"
        if ($Response -eq "Y" -or $Response -eq "y") {
            Start-Process explorer.exe $OutputPath
        }
    }
}
catch {
    Write-Host ""
    Write-Host "✗ 빌드 실패!" -ForegroundColor Red
    Write-Host "오류: $_" -ForegroundColor Red
    exit 1
}
