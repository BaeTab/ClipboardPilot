# 클립보드 파일럿 - 설치 파일 생성 가이드

## 📦 개요

이 가이드는 클립보드 파일럿의 릴리스 설치 파일을 생성하는 방법을 설명합니다.

## 🎯 생성 가능한 설치 형식

1. **단일 실행 파일** (.exe) - Self-contained
2. **ZIP 아카이브** (.zip) - Portable 버전
3. **Inno Setup 설치 파일** (.exe) - 완전한 설치 프로그램
4. **WiX MSI 설치 파일** (.msi) - Windows Installer

---

## 방법 1: 간단한 빌드 (가장 쉬움)

### 필요 도구
- .NET 8.0 SDK

### 실행 방법

```powershell
# PowerShell에서 실행
.\Build-Simple.ps1 -Version "1.0.0"
```

### 결과
- `Output\ClipboardPilot\ClipboardPilot.exe` - 단일 실행 파일 생성
- 크기: 약 150-200MB (모든 의존성 포함)

---

## 방법 2: 완전한 릴리스 빌드 (권장)

### 필요 도구
- .NET 8.0 SDK
- Inno Setup 6 (선택사항)
  - 다운로드: https://jrsoftware.org/isdl.php

### 실행 방법

```powershell
# 전체 빌드 (설치 파일 포함)
.\Build-Release.ps1 -Version "1.0.0"

# 빌드만 (설치 파일 제외)
.\Build-Release.ps1 -Version "1.0.0" -SkipSetup

# 이전 빌드 정리 후 빌드
.\Build-Release.ps1 -Version "1.0.0" -Clean
```

### 결과
- `Output\ClipboardPilot-v1.0.0-win-x64.zip` - ZIP 파일
- `Output\ClipboardPilot-Setup-v1.0.0.exe` - 설치 프로그램
- `Output\checksums.txt` - SHA256 체크섬

---

## 방법 3: Visual Studio에서 빌드

### 단계

1. **솔루션 열기**
   - Visual Studio 2022에서 `ClipboardPilot.sln` 열기

2. **빌드 구성 선택**
   - 상단 도구 모음: `Release` | `Any CPU`

3. **게시**
   ```
   프로젝트 우클릭 > 게시 > 
   대상: 폴더 >
   폴더 위치: bin\Release\net8.0-windows\win-x64\publish
   ```

4. **프로필 설정**
   - 대상 런타임: `win-x64`
   - 배포 모드: `자체 포함`
   - 단일 파일 생성: `체크`
   - ReadyToRun 컴파일: `체크 해제`
   - 트리밍: `체크 해제`

5. **게시 실행**
   - `게시` 버튼 클릭

---

## 방법 4: 명령줄에서 직접 빌드

### 단일 파일 생성

```bash
dotnet publish ClipboardPilot/ClipboardPilot.csproj \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -p:PublishTrimmed=false \
  -p:Version=1.0.0
```

### 프레임워크 의존 (작은 크기)

```bash
dotnet publish ClipboardPilot/ClipboardPilot.csproj \
  -c Release \
  -r win-x64 \
  --self-contained false \
  -p:PublishSingleFile=true \
  -p:Version=1.0.0
```

---

## 🔧 Inno Setup 설치 파일 생성

### 1. Inno Setup 설치
- https://jrsoftware.org/isdl.php 에서 다운로드
- 기본 설정으로 설치

### 2. 스크립트 편집 (선택사항)
```
Setup\ClipboardPilot.iss 파일 열기
버전, 경로 등 수정
```

### 3. 컴파일

#### GUI 방식
```
1. Inno Setup Compiler 실행
2. File > Open > Setup\ClipboardPilot.iss
3. Build > Compile
```

#### 명령줄 방식
```powershell
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" Setup\ClipboardPilot.iss
```

### 4. 결과 확인
- `Output\ClipboardPilot-Setup-v1.0.0.exe` 생성

---

## 🎁 WiX MSI 생성 (고급)

### 필요 도구
- WiX Toolset v4
  - 다운로드: https://wixtoolset.org/

### 빌드 방법

```bash
# WiX 확장 설치
dotnet tool install --global wix

# MSI 빌드
wix build Setup\ClipboardPilot.wxs -out Output\ClipboardPilot.msi
```

---

## 📊 파일 크기 비교

| 형식 | 크기 | 장점 | 단점 |
|------|------|------|------|
| Self-contained Single | ~180MB | 설치 불필요, .NET 불필요 | 큰 크기 |
| Framework-dependent | ~5MB | 작은 크기 | .NET 8 필요 |
| ZIP | ~180MB | Portable | 압축 필요 |
| Inno Setup | ~180MB | 설치 프로그램, 제거 지원 | - |
| MSI | ~180MB | 기업 환경 적합 | 복잡함 |

---

## ✅ 릴리스 체크리스트

### 빌드 전
- [ ] 버전 번호 확인
- [ ] 변경 로그 업데이트
- [ ] README.md 업데이트
- [ ] 모든 테스트 통과
- [ ] 경고 없이 빌드 성공

### 빌드 후
- [ ] 단일 파일 생성 확인
- [ ] ZIP 파일 생성 확인
- [ ] 설치 파일 생성 확인
- [ ] 체크섬 파일 생성 확인

### 테스트
- [ ] 깨끗한 Windows 10에서 테스트
- [ ] 깨끗한 Windows 11에서 테스트
- [ ] 설치 프로그램 실행 테스트
- [ ] 업그레이드 테스트 (이전 버전에서)
- [ ] 제거 테스트

### 배포
- [ ] GitHub Release 생성
- [ ] 릴리스 노트 작성
- [ ] 파일 업로드
- [ ] 체크섬 확인
- [ ] 다운로드 링크 테스트

---

## 🐛 문제 해결

### "DevExpress 라이선스 오류"
```
문제: 빌드 시 DevExpress 라이선스 오류
해결: licenses.licx 파일 확인 또는 평가판 라이선스 갱신
```

### "파일 크기가 너무 큼"
```
문제: 단일 파일이 200MB 초과
해결: 
1. PublishTrimmed=true 시도 (주의: 테스트 필요)
2. Framework-dependent 버전 제공
3. 압축 도구 사용
```

### "설치 파일 생성 실패"
```
문제: Inno Setup 컴파일 오류
해결:
1. 경로 확인 (공백, 한글 포함 여부)
2. LICENSE.txt 파일 존재 확인
3. 빌드된 .exe 파일 존재 확인
```

### ".NET 런타임 오류"
```
문제: 사용자 PC에서 실행 안 됨
해결: Self-contained 빌드 확인
```

---

## 📝 GitHub Release 배포 방법

### 1. 태그 생성
```bash
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0
```

### 2. GitHub Release 생성
```
1. GitHub 저장소 > Releases > New release
2. 태그 선택: v1.0.0
3. 제목: 클립보드 파일럿 v1.0.0
4. 설명: 릴리스 노트 작성
5. 파일 첨부:
   - ClipboardPilot-Setup-v1.0.0.exe
   - ClipboardPilot-v1.0.0-win-x64.zip
   - checksums.txt
6. Publish release
```

### 3. 릴리스 노트 템플릿
```markdown
## 클립보드 파일럿 v1.0.0

### 🎉 새로운 기능
- 자동 클립보드 수집
- 강력한 검색 및 필터링
- 즐겨찾기 시스템

### 🐛 버그 수정
- (해당사항 없음 - 초기 릴리스)

### 📥 다운로드
- [설치 프로그램](링크) - 권장
- [ZIP 파일](링크) - Portable 버전

### 📋 시스템 요구사항
- Windows 10 (1903 이상) 또는 Windows 11
- .NET 8.0 Runtime (포함됨)

### 🔒 체크섬
SHA256 체크섬은 `checksums.txt` 파일을 참조하세요.
```

---

## 🚀 자동화 (CI/CD)

### GitHub Actions 예시

```yaml
name: Release Build

on:
  push:
    tags:
      - 'v*'

jobs:
  build:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build -c Release
    
    - name: Publish
      run: dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
    
    - name: Create Release
      uses: actions/create-release@v1
      with:
        tag_name: ${{ github.ref }}
        release_name: Release ${{ github.ref }}
        draft: false
        prerelease: false
```

---

## 📞 지원

문제가 발생하면 GitHub Issues에 등록해주세요:
https://github.com/clipboardpilot/clipboardpilot/issues
