# GoDingtalk UI

一个可双击使用的钉钉直播回放下载工具。

## 下载即用

仓库已经集成运行所需文件：

- `GoDingtalk.exe`：钉钉直播下载核心程序
- `GoDingtalk-UI.exe`：图形界面
- `tools/ffmpeg/bin/ffmpeg.exe`：视频合并和转换依赖

使用方式：

1. 双击 `GoDingtalk-UI.exe`，或双击 `Start-GoDingtalk-UI.bat`。
2. 首次使用先点击 `登录/刷新`，按提示完成钉钉登录。
3. 粘贴钉钉直播/回放链接。
4. 选择保存目录。
5. 点击 `开始下载`。

下载完成后，视频会保存为 mp4 文件。

## 注意

- `.goDingtalkConfig/` 会在本机自动生成，用于保存登录状态，不会提交到仓库。
- `video/` 是默认下载目录，不会提交到仓库。
- 如果登录状态过期，重新点击 `登录/刷新` 即可。

## 开发

UI 源码在 `GoDingtalk-UI.cs`。在 Windows 上可使用系统自带 .NET Framework C# 编译器重新编译：

```powershell
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:winexe /platform:anycpu /codepage:65001 /r:System.Windows.Forms.dll /r:System.Drawing.dll /out:GoDingtalk-UI.exe GoDingtalk-UI.cs
```
