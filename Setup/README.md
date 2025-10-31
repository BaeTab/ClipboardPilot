# Setup Script Usage Guide

## 📦 Available Setup Scripts

This project includes three Inno Setup scripts with different complexity levels:

### 1. ClipboardPilot-Simple.iss ⭐ **RECOMMENDED**
- **Ultra-simple, guaranteed to work**
- No dependencies on external files
- Creates basic installer with desktop icon
- Best for quick testing

```powershell
# Compile
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" Setup\ClipboardPilot-Simple.iss
```

### 2. ClipboardPilot-Minimal.iss
- Minimal configuration
- Includes desktop icon option
- No fancy features

### 3. ClipboardPilot-Full.iss
- Complete installer with all features
- Multi-language support (English/Korean)
- Previous version detection
- User data cleanup on uninstall
- Startup option

## 🚀 Quick Start

### Method 1: Use Build Script (Recommended)

```powershell
# Build everything (project + installer)
.\Build-Release.ps1 -Version "1.0.0"

# Skip installer creation
.\Build-Release.ps1 -SkipSetup

# Clean build
.\Build-Release.ps1 -Clean
```

### Method 2: Manual Compilation

```powershell
# 1. Check available scripts
.\Test-SetupScripts.ps1

# 2. Compile manually
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" Setup\ClipboardPilot-Simple.iss
```

### Method 3: Inno Setup GUI

```
1. Open Inno Setup Compiler
2. File > Open > Setup\ClipboardPilot-Simple.iss
3. Build > Compile (F9)
```

## 📋 Prerequisites

### Required:
- ✅ ClipboardPilot.exe built and located in `Output\ClipboardPilot\`
- ✅ Inno Setup 6 installed

### Optional:
- LICENSE.txt (for Full version)
- README.md (for Full version)

## 🔧 Troubleshooting

### Error: "Cannot find source file"

**Solution:** Make sure you've built the project first:
```powershell
.\Build-Simple.ps1
```

### Error: "Icon file not found"

**Solution:** Use `ClipboardPilot-Simple.iss` which doesn't require icon files.

### Error: "License file not found"

**Solution:** Either:
1. Use `ClipboardPilot-Simple.iss` (no license required)
2. Create `LICENSE.txt` in root folder

### Compilation aborted without error

**Cause:** Missing files referenced in the script.

**Solution:** Use the simplest script first:
```powershell
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" Setup\ClipboardPilot-Simple.iss
```

## 📁 File Structure

```
ClipboardPilot/
├── Output/
│   └── ClipboardPilot/
│       └── ClipboardPilot.exe      ← Must exist before building installer
├── Setup/
│   ├── ClipboardPilot-Simple.iss   ← Use this one!
│   ├── ClipboardPilot-Minimal.iss
│   └── ClipboardPilot-Full.iss
├── Build-Simple.ps1
├── Build-Release.ps1
└── Test-SetupScripts.ps1
```

## ✅ Step-by-Step: First Time Setup

1. **Build the project:**
   ```powershell
   .\Build-Simple.ps1
   ```

2. **Verify output:**
   ```powershell
   Test-Path "Output\ClipboardPilot\ClipboardPilot.exe"
   # Should return: True
   ```

3. **Test setup scripts:**
   ```powershell
   .\Test-SetupScripts.ps1
   ```

4. **Create installer:**
   ```powershell
   # If Inno Setup is installed
   .\Build-Release.ps1
   
   # Or manually
   & "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" Setup\ClipboardPilot-Simple.iss
   ```

5. **Check output:**
   ```powershell
   Get-ChildItem Output\*.exe
   ```

## 🎯 Expected Output

After successful build, you should have:

```
Output/
├── ClipboardPilot-Setup.exe         (~265 MB)
├── ClipboardPilot-v1.0.0-win-x64.zip
└── checksums.txt
```

## 💡 Tips

- Always build the project before creating installer
- Use `ClipboardPilot-Simple.iss` for testing
- Use `ClipboardPilot-Full.iss` for distribution
- Check `Test-SetupScripts.ps1` output for issues

## 📞 Support

If you encounter issues:
1. Run `.\Test-SetupScripts.ps1` to diagnose
2. Check that `Output\ClipboardPilot\ClipboardPilot.exe` exists
3. Try the simplest script first: `ClipboardPilot-Simple.iss`
