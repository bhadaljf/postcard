# L01 Unity 美术资源接入说明

> 本目录是给程序与 UI 对接同学使用的唯一正式资源入口。上一级目录保存源图、生成脚本、草稿和旧版本，不应直接拖入 Unity 的 `Assets`。

## 1. 项目统一规格

| 项目 | 统一值 |
|---|---|
| 参考分辨率 | 1920×1080，16:9 |
| Canvas Scaler | Scale With Screen Size |
| Match | 0.5 |
| 图片格式 | PNG |
| 色彩空间 | sRGB |
| Mip Maps | UI 与固定场景均关闭 |
| Filter Mode | Bilinear |
| Wrap Mode | Clamp |
| UI Compression | None |
| 场景图 Max Size | 2048 |
| 小图标 Max Size | 256 或 512，按原图尺寸向上取整 |

全屏资源已经统一裁为 1920×1080。不要再对单张背景进行额外裁切，否则场景之间的门洞和 UI 坐标会偏移。

## 2. 目录职责

| 目录 | Unity 用途 | 默认层级 |
|---|---|---:|
| `Backgrounds` | 三个固定房间背景，使用全屏 `Image` | 0 |
| `Objects` | 调查物、完成状态图和道具查看图 | 10 |
| `Characters` | 女主人透明人物层 | 20 |
| `RepairPuzzle` | 修复特写、三个零件和成功特效 | 40 |
| `CaptureShots` | 拍摄结果与编辑页缩略图 | 50 |
| `UI/HUD` | 常驻目标、道具栏、手机、按钮、场景切换 | 100 |
| `UI/Camera` | 取景框、快门、拒拍与授权反馈 | 150 |
| `UI/Dialogue` | 对话、目标更新、获得道具和错误提示 | 200 |
| `UI/Editor` | 动态明信片编辑界面 | 200 |
| `UI/Social` | 发布结果与社交反馈界面 | 200 |

## 3. 场景与人物接入

| 场景 ID | 背景文件 | 初始叠加对象 | 出口 |
|---|---|---|---|
| `S01` | `Backgrounds/S01_FrontHall_BG.png` | 无额外叠图；背景中的饮料和餐桌使用透明热点 | 右侧厨房门 → `S02` |
| `S02` | `Backgrounds/S02_Kitchen_BG.png` | `Characters/C01_Hostess_Busy.png` | 左门 → `S01`；右门 → `S03` |
| `S03` | `Backgrounds/S03_WaterStorage_BG.png` | 无额外叠图；水桶点击后打开修复特写 | 左门 → `S02` |

人物图使用全屏 RectTransform：锚点 Stretch、Left/Right/Top/Bottom 均为0。不要根据透明区域自动裁剪，否则人物在状态切换时会移动。

MVP 不接入外貌不一致的 `C01_Hostess_Consent_draft.png`。获得授权后继续保留 `C01_Hostess_Busy.png`，移除拒拍图标并更新对话；第三次照片聚焦配饮台和工作过程，不拍人物正脸。后续重绘同意态时再增加同画布替换图。

## 4. 修复谜题状态机

四张状态图均为900×900，主体位置和缩放一致。使用同一个 RectTransform，锚点和 Pivot 均为 `(0.5,0.5)`，只切换 Sprite。

| 状态 ID | 显示图片 | 允许操作 | 完成后状态 |
|---|---|---|---|
| `RepairOpened` | `RepairPuzzle/O06_Repair_00_Disconnected.png` | 将密封圈拖至接口 | `SealInstalled` |
| `SealInstalled` | `RepairPuzzle/O06_Repair_01_SealInstalled.png` | 将软管拖至接口 | `HoseConnected` |
| `HoseConnected` | `RepairPuzzle/O06_Repair_02_HoseConnected.png` | 将卡箍拖至连接处 | `ClampLocked` |
| `ClampLocked` | `RepairPuzzle/O06_Repair_03_ClampLocked.png` | 播放成功反馈并关闭特写 | `WaterRestored` |

拖拽零件：

- `I01A_SealRing.png`
- `I01B_Hose.png`
- `I01C_Clamp.png`

零件顺序错误时不消耗道具，回弹至原槽并触发 `TIP_WRONG_TARGET`。

## 5. 拍摄素材

| 片段 ID | 结果图 | 缩略图 | 解锁条件 |
|---|---|---|---|
| `ShotDrink` | `CaptureShots/SHOT01_Drink_Photo.png` | `CaptureShots/TH01_Drink.png` | 在 S01 拍摄柠檬饮料 |
| `ShotTable` | `CaptureShots/SHOT02_Table_Photo.png` | `CaptureShots/TH02_Table.png` | 在 S01 拍摄餐桌 |
| `ShotWork` | `CaptureShots/SHOT03_Work_Photo.png` | `CaptureShots/TH03_Work.png` | `WaterRestored` 且完成授权对话 |

结果图为1280×720，缩略图为640×360。两者均是RGB图片，不需要勾选 Alpha Is Transparency。

## 6. UI 锚点

| UI | 锚点 | 建议边距或位置 |
|---|---|---|
| 当前目标 | 左上 | X=34，Y=-28 |
| 暂停 | 右上 | X=-32，Y=-28 |
| 手机、相机 | 右侧居中 | X=-32，垂直间隔20 |
| 道具栏 | 底部居中 | Y=26 |
| 左右场景箭头 | 左/右居中 | 距屏幕边缘24 |
| 对话框 | 底部居中 | Y=36 |
| 模态界面 | 四向 Stretch | 边距0 |

`UI_HUD_Frame.png` 只提供整体构图参考。实际接入时优先分别使用目标条、道具栏和按钮，避免把交互按钮合并成一张不可点击的大图。

## 7. 九宫格 Border

| 资源类别 | Border（Left/Bottom/Right/Top） |
|---|---|
| 目标条、提示条、文案选项 | 28/28/28/28 |
| 对话框、道具查看框、卡槽 | 36/36/36/36 |
| 手机、编辑页、社交页 | 不切片，按参考分辨率整体显示 |
| 160×160按钮 | 不切片，保持等比缩放 |

## 8. 文本与字体

所有中文均由 TextMeshPro 动态填写，图片内没有烘焙文字。

- 正文：项目内统一的可商用中文无衬线字体，Regular。
- 标题与明信片文案：同字体 Medium；MVP 不另外引入手写字体，避免授权和缺字风险。
- 正文颜色：`#374E58`。
- 强调颜色：`#C07E42`。
- 目标与提示最小字号：参考分辨率下32px。
- 按钮最小字号：28px。

## 9. 交付状态

- 本目录文件均可作为 L01 MVP 接入。
- 源目录中的 `_draft`、`_original` 和 `C01_Hostess_Consent.png` 不属于正式交付。
- 如果重新生成资源，必须先运行 `build_unity_delivery.py`，再执行尺寸、透明通道和链接检查。
