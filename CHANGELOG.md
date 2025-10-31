# 변경 로그

이 파일은 클립보드 파일럿의 모든 주목할 만한 변경사항을 기록합니다.

형식은 [Keep a Changelog](https://keepachangelog.com/ko/1.0.0/)를 기반으로 하며,
이 프로젝트는 [Semantic Versioning](https://semver.org/lang/ko/)을 따릅니다.

## [Unreleased]

### 계획된 기능
- 클립보드 히스토리 내보내기/가져오기 (JSON, CSV)
- 텍스트 변환 도구 (대소문자, 인코딩 등)
- OCR 지원 (이미지 → 텍스트)
- 클라우드 동기화 (선택사항)

---

## [1.0.0] - 2024-10-31

### 🎉 초기 릴리스

클립보드 파일럿의 첫 공개 버전입니다!

### ✨ Added (추가)

#### 핵심 기능
- 자동 클립보드 히스토리 수집
  - 텍스트 (Text)
  - HTML 콘텐츠
  - RTF (Rich Text Format)
  - 이미지 (PNG, JPG, BMP 등)
  - 파일 및 폴더 목록
- 실시간 클립보드 감시 및 자동 저장
- SQLite 기반 로컬 데이터베이스
- 최대 1000개 항목 보관 (설정 가능)

#### 검색 & 필터링
- 전체 텍스트 검색 (Full-text search)
- 빠른 필터 버튼
  - 오늘
  - 최근 24시간
  - 이미지만
  - 텍스트만
  - 고정된 항목
- 고급 필터
  - 유형별 필터링
  - 날짜 범위 검색
  - 크기별 필터링
  - 라벨별 필터링

#### 즐겨찾기 시스템
- 1~9 숫자로 즐겨찾기 순위 지정
- `Ctrl+Alt+1~9` 전역 단축키로 즉시 붙여넣기
- 즐겨찾기 뱃지 표시

#### 조직화 기능
- 핀 고정 (Pin) 기능
- 7가지 컬러 라벨
  - 빨강, 주황, 노랑, 초록, 파랑, 보라, 회색
- 항목별 메모 추가 (선택사항)

#### 단축키
- **전역 단축키** (모든 앱에서 작동)
  - `Ctrl+Shift+V`: 미니 패널 열기
  - `Ctrl+Alt+V`: 이전 항목 붙여넣기
  - `Ctrl+Alt+1~9`: 즐겨찾기 붙여넣기
- **프로그램 내 단축키**
  - `Ctrl+F`: 검색
  - `Ctrl+D`: 삭제
  - `Ctrl+P`: 핀 고정/해제
  - `1~9`: 즐겨찾기 설정
  - `F5`: 새로고침
  - `Esc`: 필터 초기화

#### 이미지 기능
- 이미지 자동 저장
- 썸네일 자동 생성 (200x200px)
- 이미지 미리보기
- 파일 시스템 또는 데이터베이스 저장 선택

#### UI/UX
- DevExpress 기반 모던 UI
- 다크/라이트 테마 (시스템 설정 따름)
- 사용자 정의 타이틀바
  - 최소화, 최대화, 닫기 버튼
  - Segoe MDL2 Assets 아이콘 사용
  - 드래그로 창 이동
  - 더블클릭으로 최대화/복원
- 드래그 앤 드롭 지원
- 반응형 레이아웃
- 미니 패널 (빠른 접근)

#### 설정
- 자동 정리 설정 (30일 기본)
- 최대 항목 수 설정
- 이미지 저장 옵션
- 민감정보 필터링 옵션
- 단축키 커스터마이징
- 시작 프로그램 등록

### 🐛 Fixed (수정)

#### UI 버그 수정
- 타이틀바 버튼이 표시되지 않던 문제 수정
  - `WindowStyle`을 `None`으로 변경
  - 버튼 아이콘을 Segoe MDL2 Assets 폰트로 변경
  - 최대화/복원 버튼이 상태에 따라 아이콘 변경되도록 수정
  - 타이틀바 더블클릭으로 최대화/복원 기능 추가

#### 빌드 스크립트 수정
- `Build-Release.ps1` 경로 설정 오류 수정
  - `$RootPath` 계산 로직 수정
  - Output 폴더 자동 생성 기능 추가
  - 경로 확인 로직 추가
- `Build-Simple.ps1` 경로 설정 오류 수정
  - 동일한 경로 수정 적용

#### Inno Setup 스크립트 수정
- `ClipboardPilot-Simple.iss` 파일 경로 수정
  - publish 폴더 경로로 변경
  - `AppPublisher URL` → `AppPublisherURL` (공백 제거)
- `ClipboardPilot-Full.iss` 파일 경로 수정
  - publish 폴더 경로로 변경
  - `SetupIconFile` 주석 처리 (아이콘 파일 없을 때)
  - 선택적 파일에 `skipifsourcedoesntexist` 플래그 추가
- `ClipboardPilot-Minimal.iss` 파일 경로 수정
  - publish 폴더 경로로 변경

### 🔧 Changed (변경)

#### 라이선스 변경
- MIT License → CC BY-NC-SA 4.0 (Creative Commons)
  - 개인/교육/연구: 무료 사용
  - 상업적 사용: 별도 라이선스 필요
  - 라이선스 문의: b_h_woo@naver.com

#### FAQ 업데이트
- 라이선스 관련 FAQ 추가
- 기업 사용 관련 안내 추가
- 상업적 라이센스 문의 방법 추가

### 🛡️ Security (보안)

#### 프라이버시
- 완전 오프라인 작동 (인터넷 불필요)
- 로컬 SQLite 데이터베이스만 사용
- 클라우드 업로드 없음

#### 민감정보 보호
- 비밀번호 패턴 자동 감지 및 필터링
  - 8-72자 복잡한 비밀번호 패턴
  - 'password', 'pwd', 'secret' 등 키워드 포함 시 제외
- 자기 자신의 복사 작업 필터링 (무한 루프 방지)

### 🔧 Technical (기술)

#### 기술 스택
- **.NET 8.0** - 최신 .NET 프레임워크
- **WPF** - Windows Presentation Foundation
- **DevExpress WPF 23.2+** - UI 컴포넌트 라이브러리
- **Entity Framework Core 8** - ORM
- **SQLite** - 로컬 데이터베이스
- **CommunityToolkit.Mvvm 8.2+** - MVVM 프레임워크
- **Serilog 3.1+** - 구조화된 로깅

#### 아키텍처
- Clean Architecture 기반
- MVVM 패턴
- Repository 패턴
- Unit of Work 패턴
- Dependency Injection

#### 성능
- 비동기 프로그래밍 (async/await)
- 데이터베이스 인덱싱
- 이미지 썸네일 캐싱
- 메모리 효율적인 스트림 처리

### 📚 Documentation (문서)

#### 사용자 문서
- 상세한 README.md
- INSTALLATION.md (설치 가이드)
- FAQ 섹션 (README 포함)
- 사용자 가이드 (README 포함)

#### 개발자 문서
- CONTRIBUTING.md (기여 가이드)
- CHANGELOG.md (변경 로그)
- 코드 내 XML 문서화 주석
- GitHub Issue/PR 템플릿

#### 빌드 가이드 (내부 문서)
- NEW_BUILD_RELEASE_GUIDE.md - 빌드 파일 GitHub 릴리스 업로드 가이드
- QUICK_BUILD_UPLOAD.md - 빠른 빌드 업로드 참조
- INNO_SETUP_FIX_GUIDE.md - Inno Setup 오류 해결 가이드
- BUILD_SCRIPTS_FIX_REPORT.md - 빌드 스크립트 수정 보고서
- RELEASE_COMMIT_GUIDE.md - GitHub 릴리스 커밋 가이드
- RELEASE_QUICK_REF.md - 릴리스 빠른 참조 카드

### 🔨 Build (빌드)

#### PowerShell 빌드 스크립트
- `Build-Simple.ps1` - 간단한 빌드
  - 단일 EXE 파일만 생성
  - 가장 빠른 빌드
  - 테스트용 권장
- `Build-Release.ps1` - 완전한 릴리스 빌드
  - Self-contained 단일 실행 파일
  - ZIP 아카이브 생성
  - Inno Setup 설치 파일 생성
  - SHA256 체크섬 파일 생성
  - 경로 자동 감지 및 확인
  - 상세한 로그 출력

#### Inno Setup 설치 스크립트
- `ClipboardPilot-Simple.iss` (권장)
  - 기본 설치
  - 빠른 빌드
  - 데스크톱 아이콘 옵션
- `ClipboardPilot-Minimal.iss`
  - 최소 설치
  - 가장 빠른 빌드
  - 옵션 없음
- `ClipboardPilot-Full.iss`
  - 완전한 설치
  - 문서 포함 (README, LICENSE, CHANGELOG)
  - 다국어 지원 (English, Korean)
  - Quick Launch 아이콘

#### 빌드 출력
- Self-contained 단일 실행 파일 (~265 MB)
- ZIP 아카이브 (Portable 버전)
- 설치 프로그램 (Inno Setup)
- SHA256 체크섬 파일

### 📦 Distribution (배포)

#### 릴리스 파일
- `ClipboardPilot-Setup-v1.0.0.exe` - 설치 프로그램 (권장)
- `ClipboardPilot-v1.0.0-win-x64.zip` - Portable 버전
- `checksums.txt` - SHA256 체크섬

#### GitHub Release
- 태그: `v1.0.0`
- 제목: "📋 Clipboard Pilot v1.0.0 - Initial Release"
- 상세한 Release Notes 포함
- 다운로드 파일 3개 제공

### 📝 Known Issues (알려진 문제)

#### 빌드 관련
- DevExpress 라이선스 필요 (소스 빌드 시)
- Inno Setup 6 필요 (설치 파일 생성 시)
  - 다운로드: https://jrsoftware.org/isdl.php

#### 호환성
- ARM64 프로세서 미지원 (x64만 지원)
- Windows 10 (1903) 이상 필요

#### 성능
- 대량의 이미지 수집 시 메모리 사용량 증가 가능
- 1000개 이상 항목 시 검색 속도 저하 가능

---

## 버전 형식

```
[Major].[Minor].[Patch]

Major: 호환되지 않는 API 변경
Minor: 이전 버전과 호환되는 기능 추가
Patch: 이전 버전과 호환되는 버그 수정
```

## 변경 유형

- **Added**: 새로운 기능
- **Changed**: 기존 기능 변경
- **Deprecated**: 곧 제거될 기능
- **Removed**: 제거된 기능
- **Fixed**: 버그 수정
- **Security**: 보안 관련 수정

---

[Unreleased]: https://github.com/BaeTab/ClipboardPilot/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/BaeTab/ClipboardPilot/releases/tag/v1.0.0
