# CodeMusic - 在线音乐播放器

一个支持在线播放和下载音乐的C#桌面应用程序，基于ASP.NET Core后端 + Avalonia跨平台客户端。

## 项目概览

### 技术架构

| 组件 | 技术 |
|------|------|
| 后端 | ASP.NET Core 8.0 Web API |
| 客户端 | Avalonia 11.3 (跨平台桌面UI框架) |
| 播放器 | LibVLCSharp |
| MVVM | CommunityToolkit.Mvvm |
| 音乐API | aa1.cn (QQ音乐 + 网易云) |

### 功能实现

**后端 API** (`/api/songs/*`):
- `GET /search?q={keyword}` - 搜索歌曲
- `GET /{source}/{songId}` - 获取歌曲详情(含播放URL)
- `GET /download?url={url}` - 代理下载歌曲
- `GET /hot` - 获取热门歌曲

**客户端功能**:
- ✅ 歌曲搜索 (支持QQ音乐和网易云)
- ✅ 在线播放
- ✅ 下载歌曲到本地
- ✅ 播放控制 (播放/暂停/停止/音量调节)

## 项目结构

```
CodeMusic/
├── CodeMusic.sln              # 解决方案文件
├── src/
│   ├── CodeMusic.Server/      # ASP.NET Core 后端
│   │   ├── Controllers/       # API控制器
│   │   ├── Services/          # 业务逻辑服务
│   │   ├── Models/            # 数据模型
│   │   └── Program.cs         # 入口文件
│   │
│   └── CodeMusic.Client/      # Avalonia 桌面客户端
│       ├── Views/              # UI视图
│       ├── ViewModels/         # MVVM视图模型
│       ├── Services/          # API和播放器服务
│       └── Models/            # 数据模型
└── README.md
```

## 运行方式

### 1. 启动后端服务

```bash
cd src/CodeMusic.Server
dotnet run
```

后端将在 `http://localhost:5000` 启动。

### 2. 启动客户端

```bash
cd src/CodeMusic.Client
dotnet run
```

客户端支持 Windows、macOS、Linux 桌面系统。

## 使用说明

1. 启动后端服务
2. 运行客户端应用
3. 在搜索框输入歌曲名，点击搜索
4. 从列表中选择歌曲，双击或点击播放按钮
5. 点击"下载当前歌曲"按钮保存到本地

## 注意事项

1. 音乐API来自aa1.cn免费公共接口，可能不稳定
2. 仅供学习研究使用，请勿用于商业用途
3. 下载的音乐仅供个人聆听

## 许可证

MIT License