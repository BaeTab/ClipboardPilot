# 🚀 GitHub 릴리스 빠른 가이드

## 📋 체크리스트

### 1️⃣ 빌드 생성 (5분)

```powershell
cd D:\mySource\ClipboardPilot
.\Build-Release.ps1 -Version "1.0.0"
```

**확인**: `Output/` 폴더에 3개 파일 생성
- ✅ `ClipboardPilot-Setup-v1.0.0.exe`
- ✅ `ClipboardPilot-v1.0.0-win-x64.zip`
- ✅ `checksums.txt`

---

### 2️⃣ Git 태그 생성 (1분)

```bash
git add .
git commit -m "chore: Prepare for v1.0.0 release"
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin main
git push origin v1.0.0
```

---

### 3️⃣ GitHub 릴리스 생성 (5분)

#### 웹사이트에서:

1. **저장소 이동**: https://github.com/BaeTab/ClipboardPilot
2. **Releases 클릭** → **"Create a new release"**
3. **태그 선택**: `v1.0.0` (이미 생성됨)
4. **제목 입력**:
   ```
   📋 Clipboard Pilot v1.0.0 - Initial Release
   ```
5. **설명 복사**: `release-notes-v1.0.0.md` 내용 붙여넣기
6. **파일 업로드**: 
   - 드래그 앤 드롭으로 3개 파일 업로드
7. **옵션 설정**:
   - ✅ Set as the latest release
8. **"Publish release"** 클릭! 🎉

---

## 🎯 릴리스 URL

릴리스 후 다음 URL로 접근 가능:

```
https://github.com/BaeTab/ClipboardPilot/releases/tag/v1.0.0
https://github.com/BaeTab/ClipboardPilot/releases/latest
```

---

## 📝 릴리스 제목 & 태그 규칙

### 태그 형식
```
v{Major}.{Minor}.{Patch}
```

**예시**:
- `v1.0.0` - 초기 릴리스
- `v1.0.1` - 버그 수정
- `v1.1.0` - 새 기능
- `v2.0.0` - 메이저 업데이트

### 제목 형식
```
📋 Clipboard Pilot v{version} - {description}
```

**예시**:
- `📋 Clipboard Pilot v1.0.0 - Initial Release`
- `📋 Clipboard Pilot v1.0.1 - Bug Fixes`
- `📋 Clipboard Pilot v1.1.0 - New Features`

---

## 🔧 CLI로 빠르게 릴리스

### GitHub CLI 사용

```bash
# 릴리스 생성 (파일 자동 업로드)
gh release create v1.0.0 `
  --title "📋 Clipboard Pilot v1.0.0 - Initial Release" `
  --notes-file release-notes-v1.0.0.md `
  Output/ClipboardPilot-Setup-v1.0.0.exe `
  Output/ClipboardPilot-v1.0.0-win-x64.zip `
  Output/checksums.txt
```

---

## 🆘 문제 해결

### 태그 삭제/재생성

```bash
# 로컬 태그 삭제
git tag -d v1.0.0

# 원격 태그 삭제
git push origin :refs/tags/v1.0.0

# 다시 생성
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0
```

### 릴리스 수정

1. GitHub 릴리스 페이지 → **"Edit release"**
2. 내용 수정
3. **"Update release"** 클릭

---

## 📊 릴리스 후 확인

- [ ] 릴리스 페이지 확인
- [ ] 다운로드 링크 테스트
- [ ] README의 릴리스 링크 업데이트
- [ ] 소셜 미디어 홍보
- [ ] CHANGELOG.md 업데이트

---

## 🎉 완료!

**다음**: [SOCIAL_MEDIA_TEMPLATES.md](SOCIAL_MEDIA_TEMPLATES.md)에서 홍보 템플릿 참조

---

자세한 가이드: [GITHUB_RELEASE_GUIDE.md](GITHUB_RELEASE_GUIDE.md)
