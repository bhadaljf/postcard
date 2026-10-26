from pathlib import Path
from PIL import Image, ImageDraw
import shutil


# 将 L01 源素材整理成可直接交给 Unity 的正式交付包。
ROOT = Path(__file__).resolve().parent
OUT = ROOT / "Delivery"
INK = (55, 78, 88, 255)
PAPER = (239, 228, 207, 255)
ACCENT = (192, 126, 66, 255)


DIRS = {
    "bg": OUT / "Backgrounds",
    "char": OUT / "Characters",
    "obj": OUT / "Objects",
    "repair": OUT / "RepairPuzzle",
    "shot": OUT / "CaptureShots",
    "hud": OUT / "UI" / "HUD",
    "camera": OUT / "UI" / "Camera",
    "dialogue": OUT / "UI" / "Dialogue",
    "editor": OUT / "UI" / "Editor",
    "social": OUT / "UI" / "Social",
}


def ensure_dirs():
    for p in DIRS.values():
        p.mkdir(parents=True, exist_ok=True)


def open_rgba(name):
    return Image.open(ROOT / name).convert("RGBA")


def crop_fullscreen(im):
    """将 1920×1088 统一为项目参考分辨率 1920×1080。"""
    if im.size == (1920, 1088):
        return im.crop((0, 4, 1920, 1084))
    return im


def copy_image(name, folder, out_name=None, crop_screen=False):
    im = open_rgba(name)
    if crop_screen:
        im = crop_fullscreen(im)
    im.save(folder / (out_name or name))


def alpha_crop(im, box):
    out = im.crop(box)
    bbox = out.getbbox()
    return out.crop(bbox) if bbox else out


def fit_transparent(im, size, margin=30):
    bbox = im.getbbox()
    if bbox:
        im = im.crop(bbox)
    im.thumbnail((size[0] - margin * 2, size[1] - margin * 2), Image.Resampling.LANCZOS)
    out = Image.new("RGBA", size, (0, 0, 0, 0))
    out.alpha_composite(im, ((size[0] - im.width) // 2, (size[1] - im.height) // 2))
    return out


def build_backgrounds_characters_objects():
    for n in ["S01_FrontHall_BG.png", "S02_Kitchen_BG.png", "S03_WaterStorage_BG.png"]:
        copy_image(n, DIRS["bg"], crop_screen=True)

    copy_image("C01_Hostess_Busy.png", DIRS["char"], crop_screen=True)

    for n in [
        "O02_LemonDrink.png", "O03_TableClueGroup.png", "I02_OldPostcard.png",
        "I01_HoseRepairKit.png", "O04_Faucet_Flowing.png", "O07_DrinkStation_Active.png",
        "O04_Faucet_Dry_Closeup.png", "O05_PartsBox_Closed.png", "O05_PartsBox_Open.png",
    ]:
        copy_image(n, DIRS["obj"], crop_screen=open_rgba(n).size == (1920, 1088))


def build_aligned_repair_states():
    """所有修复状态共用 900×900 画布、同一水桶接口和同一软管位置。"""
    src = open_rgba("O06_WaterTank_Disconnected.png")
    tank = alpha_crop(src, (40, 20, 1165, 1068))
    hose = alpha_crop(src, (1135, 690, 1900, 965))

    # 固定画布坐标。所有步骤都从同一个 base 复制，切换时主体绝不跳动。
    tank.thumbnail((590, 720), Image.Resampling.LANCZOS)
    hose.thumbnail((330, 180), Image.Resampling.LANCZOS)
    tank_xy = (80, 105)
    hose_detached_xy = (555, 545)
    hose_connected_xy = (618, 438)

    base = Image.new("RGBA", (900, 900), (0, 0, 0, 0))
    base.alpha_composite(tank, tank_xy)
    base.alpha_composite(hose, hose_detached_xy)
    base.save(DIRS["repair"] / "O06_Repair_00_Disconnected.png")

    step1 = base.copy()
    d = ImageDraw.Draw(step1)
    d.ellipse((616, 432, 664, 480), outline=(66, 65, 61, 255), width=12)
    step1.save(DIRS["repair"] / "O06_Repair_01_SealInstalled.png")

    step2 = Image.new("RGBA", (900, 900), (0, 0, 0, 0))
    step2.alpha_composite(tank, tank_xy)
    step2.alpha_composite(hose, hose_connected_xy)
    step2.save(DIRS["repair"] / "O06_Repair_02_HoseConnected.png")

    step3 = step2.copy()
    d = ImageDraw.Draw(step3)
    d.rounded_rectangle((616, 430, 650, 492), 8, fill=(112, 142, 153, 245), outline=INK, width=4)
    d.line((622, 444, 644, 444), fill=(231, 219, 192, 255), width=4)
    step3.save(DIRS["repair"] / "O06_Repair_03_ClampLocked.png")

    # 三个拖拽零件保留统一 400×400 画布。
    for n in ["I01A_SealRing.png", "I01B_Hose.png", "I01C_Clamp.png", "UI_DragTarget.png", "FX_Repair_Success.png"]:
        copy_image(n, DIRS["repair"])


def flatten_scene(im):
    out = Image.new("RGBA", im.size, PAPER)
    out.alpha_composite(im)
    return out


def crop_to_16_9(scene, box, size=(1280, 720)):
    crop = scene.crop(box).resize(size, Image.Resampling.LANCZOS)
    d = ImageDraw.Draw(crop)
    d.rectangle((12, 12, size[0] - 13, size[1] - 13), outline=INK, width=5)
    return crop


def build_capture_shots():
    s01 = flatten_scene(crop_fullscreen(open_rgba("S01_FrontHall_BG.png")))
    s02 = flatten_scene(crop_fullscreen(open_rgba("S02_Kitchen_BG.png")))

    # 两张前厅照片来自同一 S01，不再把独立物件摆在虚构纸面上。
    shot1 = crop_to_16_9(s01, (390, 390, 1110, 795))
    shot2 = crop_to_16_9(s01, (320, 450, 1438, 1079))

    # 第三张聚焦真实厨房右侧配饮台，避免使用外貌不一致的人物草稿。
    shot3 = crop_to_16_9(s02, (950, 300, 1730, 739))
    d = ImageDraw.Draw(shot3)
    # 用三条短水流和杯中浅蓝色面表示供水恢复，不改变原空间和设备比例。
    for x in (495, 635, 775):
        d.line((x, 322, x, 365), fill=(116, 163, 178, 220), width=7)

    for i, (name, im) in enumerate([
        ("SHOT01_Drink_Photo.png", shot1),
        ("SHOT02_Table_Photo.png", shot2),
        ("SHOT03_Work_Photo.png", shot3),
    ], 1):
        im.convert("RGB").save(DIRS["shot"] / name)
        thumb = im.resize((640, 360), Image.Resampling.LANCZOS).convert("RGB")
        thumb.save(DIRS["shot"] / f"TH0{i}_{['Drink','Table','Work'][i-1]}.png")


def build_ui():
    hud = [
        "UI_HUD_Frame.png", "UI_Objective_Panel.png", "UI_ObjectiveUpdated_Panel.png",
        "UI_Inventory_Bar.png", "UI_Inventory_Slot_Normal.png", "UI_Inventory_Slot_Selected.png",
        "UI_Btn_Phone.png", "UI_Btn_Camera.png", "UI_Btn_Back.png", "UI_Btn_Pause.png",
        "UI_Arrow_Left.png", "UI_Arrow_Right.png", "UI_Hotspot_Default.png", "UI_Hotspot_Hover.png",
        "UI_Phone_Task_BG.png", "UI_Phone_Close.png", "UI_Task_Check_Off.png", "UI_Task_Check_On.png",
    ]
    camera = [
        "UI_Camera_Viewfinder.png", "UI_Camera_Shutter.png", "UI_Camera_Exit.png",
        "UI_CaptureBlocked.png", "UI_ConsentConfirmed.png", "UI_ShotSaved_Panel.png", "FX_Camera_Flash.png",
    ]
    dialogue = [
        "UI_Dialogue_Box.png", "UI_Nameplate.png", "UI_Dialogue_Next.png", "UI_ItemInspect_Frame.png",
        "UI_ItemAcquired_Panel.png", "UI_WrongItem_Prompt.png",
    ]
    editor = [
        "UI_Editor_BG.png", "UI_Clip_Slot_Empty.png", "UI_Clip_Slot_Filled.png",
        "UI_CopyChoice_Normal.png", "UI_CopyChoice_Selected.png", "UI_Publish_Button.png",
    ]
    social = [
        "UI_Social_BG.png", "UI_Post_Card.png", "UI_Comment_Bubble.png", "UI_Brand_DM.png",
        "UI_PrivateNote.png", "UI_Continue_Button.png",
    ]
    for names, folder in [(hud, DIRS["hud"]), (camera, DIRS["camera"]), (dialogue, DIRS["dialogue"]), (editor, DIRS["editor"]), (social, DIRS["social"])]:
        for n in names:
            copy_image(n, folder, crop_screen=open_rgba(n).size == (1920, 1088))


def main():
    ensure_dirs()
    build_backgrounds_characters_objects()
    build_aligned_repair_states()
    build_capture_shots()
    build_ui()


if __name__ == "__main__":
    main()
