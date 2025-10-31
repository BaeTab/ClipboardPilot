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

- 상세한 README.md
- CONTRIBUTING.md (기여 가이드)
- INSTALLATION.md (설치 가이드)
- LICENSE.txt (MIT 라이선스)
- 코드 내 XML 문서화 주석
- 사용자 가이드 (README 포함)

### 🔨 Build (빌드)

- PowerShell 빌드 스크립트
  - `Build-Simple.ps1` - 간단한 빌드
  - `Build-Release.ps1` - 완전한 릴리스 빌드
- Inno Setup 설치 스크립트
  - `ClipboardPilot-Simple.iss`
  - `ClipboardPilot-Full.iss`
  - `ClipboardPilot-Minimal.iss`
- Self-contained 단일 실행 파일 생성
- ZIP 아카이브 생성
- 체크섬 파일 생성 (SHA256)

### 📝 Known Issues (알려진 문제)

- DevExpress 라이선스 필요 (소스 빌드 시)
- ARM64 프로세서 미지원 (x64만 지원)
- 대량의 이미지 수집 시 메모리 사용량 증가 가능

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

[Unreleased]: https://github.com/yourusername/ClipboardPilot/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/yourusername/ClipboardPilot/releases/tag/v1.0.0
