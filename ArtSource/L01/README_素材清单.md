# L01《沙漠中的饭店》美术素材清单

> 本目录是源素材与生成过程目录。给 Unity 对接同学的正式资源位于 [`Delivery`](./Delivery/README_Unity接入.md)，请勿将本目录整体复制进项目。

## 使用原则

- 三个房间使用固定视角，不制作角色行走动画。
- 悬停状态由 Unity 统一增加描边或亮度，不额外制作重复图片。
- 场景中的热点使用透明点击区；交互后通过对象图或局部状态图覆盖背景。
- `_draft` 文件仅供构图参考，不进入正式资源引用。

## 场景背景

| 文件 | 用途 | 状态 |
|---|---|---|
| `S01_FrontHall_BG.png` | 饭店前厅固定视图 | 可用，1920×1088，透明背景；旧版备份为 `S01_FrontHall_BG_original.png` |
| `S02_Kitchen_BG.png` | 半开放厨房固定视图 | 可用，1920×1088，透明背景 |
| `S03_WaterStorage_BG.png` | 储水与杂物间固定视图 | 可用，1920×1088，透明背景 |

## 人物

| 文件 | 用途 | 状态 |
|---|---|---|
| `C01_Hostess_Busy.png` | 女主人忙碌、拒绝拍摄 | 可用 |
| `C01_Hostess_Consent.png` | 女主人同意记录 | 占位，不进入正式交付；MVP复用忙碌态并通过对话和授权图标表现状态 |
| `C01_Hostess_Consent_draft.png` | 女主人同意记录 | 草稿；脸型与忙碌态不一致，禁止正式接入 |

## 交互对象与道具

| 文件 | 用途 | 状态 |
|---|---|---|
| `O02_LemonDrink.png` | 柠檬饮料拍摄对象、视频片段缩略图 | 可用 |
| `O03_TableClueGroup.png` | 菜单、空水壶、订单牌组合特写 | 可用 |
| `I02_OldPostcard.png` | 旧明信片道具查看 | 可用 |
| `I01_HoseRepairKit.png` | 软管修复套件道具栏图标 | 可用 |
| `O04_Faucet_Flowing.png` | 供水恢复后的水槽局部覆盖图 | 可用 |
| `O06_WaterTank_Disconnected.png` | 水桶接口未修复状态 | 可用 |
| `O06_WaterTank_Repaired.png` | 水桶接口修复完成状态 | 可用 |
| `O07_DrinkStation_Active.png` | 供水恢复后的配饮台与餐盘 | 可用 |

## 本轮补齐资源

- `SHOT01` 至 `SHOT03`：三次拍摄结果图；`TH01` 至 `TH03`：对应 640×360 缩略图。
- `O05_*`：备件盒关闭与打开状态。
- `O06_Repair_*`：接口特写、密封圈、软管和卡箍三步安装状态。
- `I01A_*`、`I01B_*`、`I01C_*`：修复特写中的三个独立拖拽零件。
- `UI_HUD_*`、`UI_Btn_*`、`UI_Inventory_*`：常驻 HUD、按钮与道具栏。
- `UI_Camera_*`、`UI_Hotspot_*`、`FX_Camera_Flash.png`：拍摄模式与热点反馈。
- `UI_Phone_*`、`UI_Task_*`：品牌任务手机与清单状态。
- `UI_Dialogue_*`、`UI_Objective*`、`UI_*Prompt*`：对话、目标与无字提示底框。
- `UI_Editor_*`、`UI_Clip_*`、`UI_CopyChoice_*`：动态明信片编辑界面。
- `UI_Social_*`、`UI_Post_*`、`UI_Comment_*`、`UI_Brand_DM.png`、`UI_PrivateNote.png`：发布及社交反馈界面。
- 所有文字继续由 Unity TextMeshPro 叠加，图片中不烘焙中文。

## 草稿

| 文件 | 原因 |
|---|---|
| `O06_WaterTank_Repaired_draft.png` | 构图错误地恢复为完整储水间，不能与局部状态直接切换 |

## Unity 接入建议

- Unity只接入 `Delivery` 目录；导入参数、锚点、层级、状态 ID 和九宫格数值见其接入说明。
- 女主人同意态在重做前复用忙碌态并更换对话、拒拍图标和授权图标，避免角色外貌突变。
- 三张结果图与三张缩略图已经从正式场景视图派生，不再使用孤立物件展示图。
