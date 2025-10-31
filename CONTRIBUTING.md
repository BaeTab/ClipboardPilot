# 기여 가이드

클립보드 파일럿에 기여해주셔서 감사합니다! 🎉

## 목차

- [행동 강령](#행동-강령)
- [시작하기](#시작하기)
- [개발 환경 설정](#개발-환경-설정)
- [기여 방법](#기여-방법)
- [코딩 규칙](#코딩-규칙)
- [커밋 메시지](#커밋-메시지)
- [Pull Request 프로세스](#pull-request-프로세스)

## 행동 강령

이 프로젝트에 참여하는 모든 분들은 상호 존중하고 배려하는 태도를 유지해주세요.

### 우리가 약속하는 것

- 🤝 모든 기여자를 환영합니다
- 🎯 건설적인 피드백을 제공합니다
- 💬 명확하고 친절하게 소통합니다
- 🌈 다양성을 존중합니다

### 금지 행동

- ❌ 욕설, 비방, 차별적 발언
- ❌ 개인 정보 공개
- ❌ 괴롭힘이나 트롤링
- ❌ 부적절한 콘텐츠 게시

## 시작하기

### 이슈 찾기

기여할 이슈를 찾고 있다면:

- 🏷️ [`good first issue`](../../labels/good%20first%20issue) - 처음 기여하기 좋은 이슈
- 🆘 [`help wanted`](../../labels/help%20wanted) - 도움이 필요한 이슈
- 🐛 [`bug`](../../labels/bug) - 버그 수정
- ✨ [`enhancement`](../../labels/enhancement) - 새로운 기능

### 질문하기

- 💬 질문이 있으면 [Discussions](../../discussions)에 올려주세요
- 🐛 버그는 [Issues](../../issues)에 리포트해주세요

## 개발 환경 설정

### 필수 도구

```powershell
# .NET 8.0 SDK
winget install Microsoft.DotNet.SDK.8

# Visual Studio 2022 (Community)
winget install Microsoft.VisualStudio.2022.Community

# Git
winget install Git.Git
```

### 프로젝트 설정

```bash
# 1. Fork & Clone
git clone https://github.com/YOUR_USERNAME/ClipboardPilot.git
cd ClipboardPilot

# 2. Upstream 추가
git remote add upstream https://github.com/ORIGINAL_OWNER/ClipboardPilot.git

# 3. 패키지 복원
dotnet restore

# 4. 빌드
dotnet build

# 5. 실행
dotnet run --project ClipboardPilot/ClipboardPilot.csproj
```

### DevExpress

소스를 빌드하려면 DevExpress WPF 라이선스 또는 평가판이 필요합니다:
- [DevExpress 평가판 다운로드](https://www.devexpress.com/products/try/)

## 기여 방법

### 1. 이슈 확인

먼저 작업할 이슈가 있는지 확인하세요.

```bash
# 이슈가 없다면 먼저 생성
# Issues 탭에서 새 이슈 작성
```

### 2. 브랜치 생성

```bash
# upstream에서 최신 코드 가져오기
git fetch upstream
git checkout main
git merge upstream/main

# 새 브랜치 생성
git checkout -b feature/your-feature-name
# 또는
git checkout -b fix/your-bug-fix
```

### 3. 코드 작성

```bash
# 코드 변경
# 테스트 작성 (가능하면)
# 빌드 확인
dotnet build

# 로컬 테스트
dotnet run --project ClipboardPilot/ClipboardPilot.csproj
```

### 4. 커밋

```bash
git add .
git commit -m "feat: Add awesome feature"
```

### 5. Push & Pull Request

```bash
# 브랜치 Push
git push origin feature/your-feature-name

# GitHub에서 Pull Request 생성
# https://github.com/YOUR_USERNAME/ClipboardPilot/pull/new
```

## 코딩 규칙

### C# 스타일

프로젝트는 `.editorconfig`를 사용합니다. Visual Studio가 자동으로 적용합니다.

#### 명명 규칙

```csharp
// ✅ 올바른 예
public class ClipboardItem { }              // PascalCase: 클래스
public void SaveItem() { }                  // PascalCase: 메서드
public string ItemName { get; set; }        // PascalCase: 프로퍼티
private string _itemName;                   // _camelCase: private 필드
int itemCount = 0;                          // camelCase: 로컬 변수

// ❌ 잘못된 예
public class clipboardItem { }              // 소문자 시작
public void save_item() { }                 // 언더스코어 사용
private string itemName;                    // 언더스코어 없음
```

#### 코드 구조

```csharp
// ✅ 올바른 예
public class MyClass
{
    // 1. 필드
    private readonly ILogger _logger;
    private string _data;
    
    // 2. 생성자
    public MyClass(ILogger logger)
    {
        _logger = logger;
    }
    
    // 3. 프로퍼티
    public string Data 
    { 
        get => _data;
        set => _data = value;
    }
    
    // 4. 메서드
    public void DoSomething()
    {
        // ...
    }
}
```

#### 주석

```csharp
// ✅ 주석은 한글로 (코드 이해를 위해)
/// <summary>
/// 클립보드 항목을 저장합니다.
/// </summary>
/// <param name="item">저장할 항목</param>
/// <returns>저장 성공 여부</returns>
public async Task<bool> SaveItemAsync(ClipboardItem item)
{
    // 중복 확인
    if (await IsDuplicateAsync(item))
    {
        _logger.Warning("Duplicate item detected");
        return false;
    }
    
    // 저장 수행
    await _repository.AddAsync(item);
    return true;
}
```

### XAML 스타일

```xaml
<!-- ✅ 올바른 예 -->
<Button 
    Content="저장"
    Command="{Binding SaveCommand}"
    Width="100"
    Height="30"
    Margin="10,5"/>

<!-- ❌ 한 줄로 쓰지 마세요 (가독성 저하) -->
<Button Content="저장" Command="{Binding SaveCommand}" Width="100" Height="30" Margin="10,5"/>
```

## 커밋 메시지

### 형식

```
<type>: <subject>

<body>

<footer>
```

### Type

- `feat`: 새로운 기능
- `fix`: 버그 수정
- `docs`: 문서 수정
- `style`: 코드 포맷팅 (기능 변경 없음)
- `refactor`: 코드 리팩토링
- `test`: 테스트 추가/수정
- `chore`: 빌드/설정 관련

### 예시

```bash
# 좋은 예
feat: Add search history feature
fix: Fix crash when opening mini panel
docs: Update README with new hotkeys
refactor: Simplify clipboard watcher logic

# 나쁜 예
update
fix bug
WIP
```

### 상세한 커밋 메시지

```bash
feat: Add cloud sync feature

- Add SyncService class
- Implement OneDrive integration
- Add sync settings UI

Closes #123
```

## Pull Request 프로세스

### 1. PR 생성

- 📝 명확한 제목 작성
- 📋 템플릿에 따라 설명 작성
- 🏷️ 적절한 라벨 추가

### 2. 체크리스트

PR을 생성하기 전에 확인하세요:

```markdown
- [ ] 코드가 빌드됨
- [ ] 기존 기능이 정상 작동함
- [ ] 새 기능이 의도대로 작동함
- [ ] 코딩 규칙을 준수함
- [ ] 문서를 업데이트함 (필요시)
- [ ] CHANGELOG를 업데이트함 (주요 변경사항)
```

### 3. 리뷰 대기

- 🔍 리뷰어가 코드를 검토합니다
- 💬 피드백에 답변하고 필요시 수정합니다
- ✅ 승인되면 머지됩니다

### 4. PR 템플릿

```markdown
## 변경 사항
이 PR이 수정/추가하는 내용을 간략히 설명해주세요.

## 관련 이슈
Closes #(이슈 번호)

## 변경 유형
- [ ] 🐛 버그 수정
- [ ] ✨ 새로운 기능
- [ ] 📝 문서 업데이트
- [ ] 🔨 리팩토링
- [ ] ⚡ 성능 개선

## 테스트 방법
1. 프로그램 실행
2. XXX 기능 테스트
3. 결과 확인

## 스크린샷
(UI 변경이 있다면 스크린샷 첨부)

## 체크리스트
- [ ] 로컬에서 빌드 성공
- [ ] 기능 테스트 완료
- [ ] 코딩 규칙 준수
- [ ] 문서 업데이트 (필요시)
```

## 테스트

### 수동 테스트

```bash
# 1. 빌드
dotnet build -c Release

# 2. 실행
.\Output\ClipboardPilot\ClipboardPilot.exe

# 3. 주요 기능 테스트
- [ ] 클립보드 수집
- [ ] 검색
- [ ] 즐겨찾기
- [ ] 단축키
- [ ] 설정 변경
```

### 자동 테스트 (향후)

```bash
# 단위 테스트
dotnet test

# 커버리지
dotnet test --collect:"XPlat Code Coverage"
```

## 문서 작성

### README 업데이트

새로운 기능을 추가했다면 README.md를 업데이트하세요:

```markdown
## 새로운 기능 이름

설명...

### 사용 방법

1. ...
2. ...
```

### 코드 문서화

```csharp
/// <summary>
/// 메서드가 하는 일을 설명합니다.
/// </summary>
/// <param name="parameter">매개변수 설명</param>
/// <returns>반환값 설명</returns>
public ReturnType MethodName(ParameterType parameter)
{
    // ...
}
```

## 도움말

### 질문이 있나요?

- 💬 [Discussions](../../discussions)에 질문하세요
- 📧 이메일: b_h_woo@naver.com

### 참고 자료

- [.NET 코딩 규칙](https://docs.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [C# 명명 규칙](https://docs.microsoft.com/dotnet/standard/design-guidelines/naming-guidelines)
- [Git 커밋 메시지 가이드](https://www.conventionalcommits.org/)

---

## 감사합니다! 🙏

모든 기여에 감사드립니다. 여러분 덕분에 클립보드 파일럿이 더 나아집니다! ❤️
