# 간단한 릴리스 빌드 스크립트 (Inno Setup 불필요)
# 단일 실행 파일만 생성

param(
    [string]$Version = "1.0.0"
)

Write-Host "=== 클립보드 파일럿 빌드 시작 ===" -ForegroundColor Cyan
Write-Host "버전: $Version" -ForegroundColor Yellow
Write-Host ""

# 경로 설정
$ProjectPath = ".\ClipboardPilot\ClipboardPilot.csproj"
$OutputPath = ".\Output"

# 출력 폴더 생성
if (Test-Path $OutputPath) {
    Remove-Item $OutputPath -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputPath | Out-Null

Write-Host "1. 패키지 복원 중..." -ForegroundColor Green
dotnet restore $ProjectPath

Write-Host "2. Release 빌드 중..." -ForegroundColor Green
dotnet build $ProjectPath -c Release

Write-Host "3. 게시 중 (Self-contained Single-file)..." -ForegroundColor Green
dotnet publish $ProjectPath `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:Version=$Version `
    -o "$OutputPath\ClipboardPilot"

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "✓ 빌드 성공!" -ForegroundColor Green
    Write-Host ""
    Write-Host "생성된 파일:" -ForegroundColor Yellow
    Get-ChildItem "$OutputPath\ClipboardPilot" -Filter "*.exe" | ForEach-Object {
        $Size = $_.Length / 1MB
        Write-Host ("  - {0} ({1:N2} MB)" -f $_.Name, $Size) -ForegroundColor White
    }
    Write-Host ""
    Write-Host "위치: $OutputPath\ClipboardPilot" -ForegroundColor Cyan
}
else {
    Write-Host "✗ 빌드 실패!" -ForegroundColor Red
    exit 1
}
