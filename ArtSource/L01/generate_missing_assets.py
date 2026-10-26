from pathlib import Path
from PIL import Image, ImageDraw


# L01 缺失美术资源的确定性生成脚本：只组合现有素材与简单 UI 图形，避免空间和风格漂移。
ROOT = Path(__file__).resolve().parent
INK = (55, 78, 88, 255)
BLUE = (109, 137, 148, 255)
BLUE_DARK = (75, 103, 116, 255)
PAPER = (235, 218, 184, 238)
PAPER_LIGHT = (247, 237, 215, 242)
SAND = (198, 151, 92, 255)
ACCENT = (192, 126, 66, 255)
MUTED = (159, 151, 133, 210)
TRANSPARENT = (0, 0, 0, 0)


def canvas(size):
    return Image.new("RGBA", size, TRANSPARENT)


def save(im, name):
    im.save(ROOT / name)


def panel(name, size, radius=28, fill=PAPER_LIGHT, outline=INK, width=5, inset=8):
    im = canvas(size)
    d = ImageDraw.Draw(im)
    d.rounded_rectangle((inset, inset, size[0] - inset - 1, size[1] - inset - 1), radius, fill, outline, width)
    d.rounded_rectangle((inset + 9, inset + 9, size[0] - inset - 10, size[1] - inset - 10), max(4, radius - 10), None, (*BLUE[:3], 150), 2)
    save(im, name)
    return im


def icon_button(name, kind, selected=False):
    im = panel(name, (160, 160), 34, PAPER if not selected else (224, 197, 146, 245), INK, 5, 10)
    d = ImageDraw.Draw(im)
    if kind == "phone":
        d.rounded_rectangle((55, 31, 106, 128), 9, None, INK, 7)
        d.line((68, 42, 94, 42), fill=INK, width=4)
        d.ellipse((77, 112, 85, 120), fill=INK)
    elif kind == "camera":
        d.rounded_rectangle((31, 55, 129, 115), 10, None, INK, 7)
        d.polygon([(49, 55), (61, 39), (94, 39), (105, 55)], outline=INK)
        d.ellipse((64, 65, 99, 100), None, INK, 7)
    elif kind == "pause":
        d.rounded_rectangle((50, 42, 68, 118), 5, fill=INK)
        d.rounded_rectangle((91, 42, 109, 118), 5, fill=INK)
    elif kind in ("left", "right", "back"):
        pts = [(105, 37), (58, 80), (105, 123)] if kind != "right" else [(55, 37), (102, 80), (55, 123)]
        d.line(pts, fill=INK, width=12, joint="curve")
    elif kind == "close":
        d.line((48, 48, 112, 112), fill=INK, width=9)
        d.line((112, 48, 48, 112), fill=INK, width=9)
    elif kind == "shutter":
        d.ellipse((34, 34, 126, 126), fill=PAPER_LIGHT, outline=INK, width=7)
        d.ellipse((54, 54, 106, 106), fill=ACCENT, outline=INK, width=4)
    save(im, name)
    return im


def check_icon(name, checked):
    im = canvas((96, 96)); d = ImageDraw.Draw(im)
    d.rounded_rectangle((13, 13, 82, 82), 14, PAPER_LIGHT, INK, 5)
    if checked:
        d.line((28, 50, 43, 66, 71, 31), fill=ACCENT, width=9, joint="curve")
    save(im, name)


def slot(name, selected=False):
    fill = (224, 197, 146, 230) if selected else (247, 237, 215, 215)
    return panel(name, (180, 180), 24, fill, ACCENT if selected else INK, 6, 8)


def composite_on_paper(src, size=(1280, 720), padding=60):
    out = Image.new("RGBA", size, (240, 229, 207, 255))
    src = src.convert("RGBA")
    box = src.getbbox()
    if box:
        src = src.crop(box)
    scale = min((size[0] - padding * 2) / src.width, (size[1] - padding * 2) / src.height)
    src = src.resize((int(src.width * scale), int(src.height * scale)), Image.Resampling.LANCZOS)
    out.alpha_composite(src, ((size[0] - src.width) // 2, (size[1] - src.height) // 2))
    d = ImageDraw.Draw(out)
    d.rectangle((18, 18, size[0] - 19, size[1] - 19), None, INK, 6)
    d.rectangle((31, 31, size[0] - 32, size[1] - 32), None, (109, 137, 148, 180), 2)
    return out


def thumbnail(src, name):
    im = src.copy().convert("RGB")
    im.thumbnail((640, 360), Image.Resampling.LANCZOS)
    bg = Image.new("RGB", (640, 360), (240, 229, 207))
    bg.paste(im, ((640 - im.width) // 2, (360 - im.height) // 2))
    bg.save(ROOT / name)


def crop_alpha(src, box, out_size, name):
    obj = src.crop(box)
    obj.thumbnail(out_size, Image.Resampling.LANCZOS)
    out = canvas(out_size)
    out.alpha_composite(obj, ((out_size[0] - obj.width) // 2, (out_size[1] - obj.height) // 2))
    save(out, name)
    return out


def make_hud():
    im = canvas((1920, 1088)); d = ImageDraw.Draw(im)
    d.rounded_rectangle((34, 28, 850, 146), 24, PAPER, INK, 5)
    d.rounded_rectangle((681, 914, 1239, 1061), 28, PAPER, INK, 5)
    d.line((70, 87, 795, 87), fill=(109, 137, 148, 110), width=3)
    save(im, "UI_HUD_Frame.png")
    panel("UI_Objective_Panel.png", (900, 180), 28)
    panel("UI_ObjectiveUpdated_Panel.png", (900, 220), 30, (232, 207, 158, 245), ACCENT)
    panel("UI_Inventory_Bar.png", (620, 190), 34)
    for name, kind in [("UI_Btn_Phone.png", "phone"), ("UI_Btn_Camera.png", "camera"), ("UI_Btn_Back.png", "back"), ("UI_Btn_Pause.png", "pause"), ("UI_Camera_Shutter.png", "shutter"), ("UI_Camera_Exit.png", "close"), ("UI_Phone_Close.png", "close"), ("UI_Arrow_Left.png", "left"), ("UI_Arrow_Right.png", "right")]:
        icon_button(name, kind)
    slot("UI_Inventory_Slot_Normal.png")
    slot("UI_Inventory_Slot_Selected.png", True)


def make_phone_and_camera():
    phone = panel("UI_Phone_Task_BG.png", (720, 1088), 56, (232, 218, 190, 250), INK, 7, 20)
    d = ImageDraw.Draw(phone)
    d.rounded_rectangle((86, 128, 634, 914), 28, (247, 237, 215, 245), BLUE_DARK, 5)
    for y in (264, 464, 664):
        d.line((150, y, 570, y), fill=(109, 137, 148, 130), width=4)
        d.rounded_rectangle((125, y - 52, 205, y + 28), 13, None, INK, 4)
    d.ellipse((335, 970, 385, 1020), None, INK, 5)
    save(phone, "UI_Phone_Task_BG.png")
    check_icon("UI_Task_Check_Off.png", False); check_icon("UI_Task_Check_On.png", True)
    view = canvas((1920, 1088)); d = ImageDraw.Draw(view)
    c = (55, 78, 88, 230); w = 7; m = 72; l = 180
    for x1, y1, x2, y2 in [(m,m,m+l,m),(m,m,m,m+l),(1920-m-l,m,1920-m,m),(1920-m,m,1920-m,m+l),(m,1088-m,m+l,1088-m),(m,1088-m-l,m,1088-m),(1920-m-l,1088-m,1920-m,1088-m),(1920-m,1088-m-l,1920-m,1088-m)]:
        d.line((x1,y1,x2,y2), fill=c, width=w)
    d.ellipse((930, 524, 990, 584), None, (198,151,92,210), 4)
    save(view, "UI_Camera_Viewfinder.png")
    flash = Image.new("RGBA", (1920,1088), (255,248,226,205)); save(flash, "FX_Camera_Flash.png")
    for name, active in [("UI_Hotspot_Default.png",False),("UI_Hotspot_Hover.png",True)]:
        im=canvas((128,128)); dr=ImageDraw.Draw(im); col=ACCENT if active else (*BLUE[:3],190)
        dr.ellipse((28,28,100,100), None, col, 6); dr.ellipse((52,52,76,76), fill=col); save(im,name)


def make_photos():
    drink = Image.open(ROOT / "O02_LemonDrink.png").convert("RGBA")
    table = Image.open(ROOT / "O03_TableClueGroup.png").convert("RGBA")
    work = Image.open(ROOT / "O07_DrinkStation_Active.png").convert("RGBA")
    p1 = composite_on_paper(drink); p2 = composite_on_paper(table); p3 = composite_on_paper(work)
    save(p1, "SHOT01_Drink_Photo.png"); save(p2, "SHOT02_Table_Photo.png"); save(p3, "SHOT03_Work_Photo.png")
    thumbnail(p1,"TH01_Drink.png"); thumbnail(p2,"TH02_Table.png"); thumbnail(p3,"TH03_Work.png")
    panel("UI_ShotSaved_Panel.png", (900,220), 28)
    panel("UI_ItemInspect_Frame.png", (1400,900), 38, (247,237,215,225))
    blocked=canvas((256,256)); d=ImageDraw.Draw(blocked); d.rounded_rectangle((44,80,212,178),18,None,INK,10); d.ellipse((96,96,160,160),None,INK,9); d.line((50,205,207,48),fill=ACCENT,width=16); save(blocked,"UI_CaptureBlocked.png")
    consent=canvas((256,256)); d=ImageDraw.Draw(consent); d.ellipse((31,31,225,225),fill=(232,218,190,245),outline=INK,width=8); d.line((72,132,111,171,188,82),fill=ACCENT,width=17,joint="curve"); save(consent,"UI_ConsentConfirmed.png")


def make_dialogue():
    panel("UI_Dialogue_Box.png",(1500,300),36,(247,237,215,245))
    panel("UI_Nameplate.png",(360,110),24,(224,197,146,245),INK,5)
    im=canvas((64,64)); d=ImageDraw.Draw(im); d.polygon([(14,18),(50,32),(14,46)],fill=ACCENT,outline=INK); save(im,"UI_Dialogue_Next.png")
    panel("UI_WrongItem_Prompt.png",(900,180),28,(247,237,215,245),ACCENT)
    panel("UI_ItemAcquired_Panel.png",(900,220),30,(232,218,190,245),INK)


def make_object_states():
    kitchen=Image.open(ROOT/"S02_Kitchen_BG.png").convert("RGBA")
    crop_alpha(kitchen,(330,380,760,730),(900,900),"O04_Faucet_Dry_Closeup.png")
    storage=Image.open(ROOT/"S03_WaterStorage_BG.png").convert("RGBA")
    crop_alpha(storage,(1410,550,1860,970),(700,600),"O05_PartsBox_Closed.png")
    kit=Image.open(ROOT/"I01_HoseRepairKit.png").convert("RGBA")
    open_box=crop_alpha(kit,(600,150,1350,760),(700,600),"O05_PartsBox_Open.png")
    base=Image.open(ROOT/"O06_WaterTank_Disconnected.png").convert("RGBA")
    repaired=Image.open(ROOT/"O06_WaterTank_Repaired.png").convert("RGBA")
    # 谜题界面只放大接口区域，避免玩家在整只水桶上寻找过小的安装点。
    base_focus=base.crop((760,430,1660,1000))
    repaired_focus=repaired.crop((760,430,1660,1000))
    base_fit=composite_fit(base_focus,(900,900)); repaired_fit=composite_fit(repaired_focus,(900,900))
    save(base_fit,"O06_Repair_Closeup_Base.png")
    # 单独零件从现有套件和断开软管中提取，确保造型一致。
    ring=crop_alpha(kit,(600,610,1010,930),(400,400),"I01A_SealRing.png")
    clamp=crop_alpha(kit,(960,560,1370,990),(400,400),"I01C_Clamp.png")
    hose=crop_alpha(base,(1100,640,1580,900),(400,400),"I01B_Hose.png")
    step1=base_fit.copy(); dr=ImageDraw.Draw(step1); dr.ellipse((482,426,558,502),None,(67,67,63,255),16); save(step1,"O06_Repair_Step1_Seal.png")
    step2=repaired_fit.copy(); save(step2,"O06_Repair_Step2_Hose.png")
    step3=repaired_fit.copy(); dr=ImageDraw.Draw(step3); dr.ellipse((510,404,595,520),None,(198,151,92,230),10); save(step3,"O06_Repair_Step3_Clamp.png")
    target=canvas((300,300)); dr=ImageDraw.Draw(target); dr.ellipse((25,25,275,275),None,(192,126,66,210),9); dr.ellipse((65,65,235,235),None,(192,126,66,140),4); save(target,"UI_DragTarget.png")
    fx=canvas((512,512)); dr=ImageDraw.Draw(fx)
    for a,b in [((256,45),(256,150)),((256,362),(256,467)),((45,256),(150,256)),((362,256),(467,256)),((108,108),(178,178)),((334,334),(404,404)),((404,108),(334,178)),((178,334),(108,404))]: dr.line((*a,*b),fill=(198,151,92,220),width=11)
    dr.ellipse((184,184,328,328),None,(192,126,66,220),10); save(fx,"FX_Repair_Success.png")


def composite_fit(src, size):
    src=src.copy(); box=src.getbbox(); src=src.crop(box) if box else src
    src.thumbnail((size[0]-40,size[1]-40),Image.Resampling.LANCZOS)
    out=canvas(size); out.alpha_composite(src,((size[0]-src.width)//2,(size[1]-src.height)//2)); return out


def make_editor_social():
    editor=Image.new("RGBA",(1920,1088),(226,211,184,248)); d=ImageDraw.Draw(editor)
    d.rounded_rectangle((80,70,1840,1018),40,(247,237,215,245),INK,6)
    for x in (150,720,1290): d.rounded_rectangle((x,240,x+480,510),24,(235,218,184,230),BLUE_DARK,5)
    d.rounded_rectangle((210,665,1710,845),28,(235,218,184,230),INK,5); save(editor,"UI_Editor_BG.png")
    panel("UI_Clip_Slot_Empty.png",(480,270),24,(235,218,184,220),BLUE_DARK)
    panel("UI_Clip_Slot_Filled.png",(480,270),24,(224,197,146,235),ACCENT)
    panel("UI_CopyChoice_Normal.png",(700,130),24,(247,237,215,240),INK)
    panel("UI_CopyChoice_Selected.png",(700,130),24,(224,197,146,245),ACCENT)
    panel("UI_Publish_Button.png",(300,120),28,(198,151,92,245),INK)
    social=Image.new("RGBA",(1920,1088),(226,211,184,248)); d=ImageDraw.Draw(social)
    d.rounded_rectangle((90,60,1830,1028),38,(247,237,215,245),INK,6)
    d.rounded_rectangle((150,125,1040,900),30,(235,218,184,225),BLUE_DARK,5)
    for y in (150,360,570): d.rounded_rectangle((1120,y,1750,y+150),26,(247,237,215,240),INK,4)
    save(social,"UI_Social_BG.png")
    panel("UI_Post_Card.png",(900,760),32,(247,237,215,245),BLUE_DARK)
    panel("UI_Comment_Bubble.png",(720,180),28,(247,237,215,245),INK)
    panel("UI_Brand_DM.png",(720,220),28,(224,197,146,245),ACCENT)
    panel("UI_PrivateNote.png",(900,260),28,(235,218,184,245),BLUE_DARK)
    panel("UI_Continue_Button.png",(320,120),28,(198,151,92,245),INK)


def main():
    make_hud(); make_phone_and_camera(); make_photos(); make_dialogue(); make_object_states(); make_editor_social()
    # 当前同意状态稿先作为可运行资源保留，后续若重绘只需覆盖同名文件。
    consent=Image.open(ROOT/"C01_Hostess_Consent_draft.png").convert("RGBA")
    save(consent,"C01_Hostess_Consent.png")


if __name__ == "__main__":
    main()
