from PIL import Image, ImageDraw, ImageFilter
import math, random

OUT = "D:/GAME/AureliaUnity/Assets/Models/Village/KnightFace.png"
W = 1024
X0, X1 = -.105, .105      # world x range mapped to u
Z0, Z1 = 1.62, 1.88       # world z range mapped to v


def px(x, z):
    return ((x - X0) / (X1 - X0) * W, (1 - (z - Z0) / (Z1 - Z0)) * W)


SS = 2
im = Image.new('RGBA', (W * SS, W * SS), (240, 193, 154, 255))
d = ImageDraw.Draw(im, 'RGBA')


def S(p):
    return (p[0] * SS, p[1] * SS)


def poly(points, fill):
    d.polygon([S(p) for p in points], fill=fill)


def ellipse(cx, cz, rx, rz, fill):
    a = px(cx - rx, cz + rz)
    b = px(cx + rx, cz - rz)
    d.ellipse([a[0] * SS, a[1] * SS, b[0] * SS, b[1] * SS], fill=fill)


# soft shading: temples and jaw a touch darker, cheeks warm
shade = Image.new('RGBA', im.size, (0, 0, 0, 0))
sd = ImageDraw.Draw(shade, 'RGBA')
for cx, cz, rx, rz, col in ((0, 1.65, .09, .035, (190, 120, 100, 70)), (-.09, 1.76, .02, .06, (190, 130, 110, 50)), (.09, 1.76, .02, .06, (190, 130, 110, 50))):
    a = px(cx - rx, cz + rz)
    b = px(cx + rx, cz - rz)
    sd.ellipse([a[0] * SS, a[1] * SS, b[0] * SS, b[1] * SS], fill=col)
shade = shade.filter(ImageFilter.GaussianBlur(26 * SS))
im.alpha_composite(shade)
d = ImageDraw.Draw(im, 'RGBA')
# blush
for sx in (-1, 1):
    blush = Image.new('RGBA', im.size, (0, 0, 0, 0))
    bd = ImageDraw.Draw(blush, 'RGBA')
    a = px(sx * .052 - .014, 1.712 + .008)
    b = px(sx * .052 + .014, 1.712 - .008)
    bd.ellipse([a[0] * SS, a[1] * SS, b[0] * SS, b[1] * SS], fill=(238, 120, 120, 95))
    blush = blush.filter(ImageFilter.GaussianBlur(14 * SS))
    im.alpha_composite(blush)
d = ImageDraw.Draw(im, 'RGBA')


def lerp(a, b, t):
    return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(len(a)))


def eye(sx):
    global d
    cx = sx * .039
    cz = 1.748
    w = .030
    h = .025
    outer = 1 if sx > 0 else 0    # t value of the outer corner

    def upper(t):
        tilt = (t if outer == 1 else 1 - t) * .0045
        return cz + math.sin(math.pi * t) ** .72 * h + tilt - .001

    def lower(t):
        tilt = (t if outer == 1 else 1 - t) * .0045
        return cz - math.sin(math.pi * t) ** .95 * h * .62 + tilt - .001

    def X(t):
        return cx - w + 2 * w * t
    top = [px(X(i / 48), upper(i / 48)) for i in range(49)]
    bot = [px(X(1 - i / 48), lower(1 - i / 48)) for i in range(49)]
    mask = Image.new('L', im.size, 0)
    md = ImageDraw.Draw(mask)
    md.polygon([S(p) for p in top + bot], fill=255)
    layer = Image.new('RGBA', im.size, (250, 248, 246, 255))
    ld = ImageDraw.Draw(layer, 'RGBA')
    # sclera shading near the lid
    for i in range(24):
        t = i / 23
        ld.rectangle([0, int(px(0, cz + h)[1] * SS) + int(i * 2.2 * SS) - int(30 * SS), im.size[0], int(px(0, cz + h)[1] * SS) + int(i * 2.2 * SS) - int(30 * SS) + int(2.4 * SS)], fill=(205, 196, 196, int(110 * (1 - t))))
    # iris: vertical gradient built from 1px rows
    icx = cx + (-.0012 if sx > 0 else .0012)
    icz = cz + .0006
    ir_w, ir_h = .0192, .0265
    y_top = px(0, icz + ir_h)[1]
    y_bot = px(0, icz - ir_h)[1]
    iris = Image.new('RGBA', im.size, (0, 0, 0, 0))
    idr = ImageDraw.Draw(iris, 'RGBA')
    n = int((y_bot - y_top) * SS)
    for r in range(n):
        t = r / max(1, n - 1)
        if t < .35:
            col = lerp((20, 78, 44), (46, 142, 62), t / .35)
        elif t < .7:
            col = lerp((46, 142, 62), (120, 214, 84), (t - .35) / .35)
        else:
            col = lerp((120, 214, 84), (206, 246, 128), (t - .7) / .3)
        idr.line([(0, int(y_top * SS) + r), (im.size[0], int(y_top * SS) + r)], fill=col + (255,), width=1)
    imask = Image.new('L', im.size, 0)
    imd = ImageDraw.Draw(imask)
    a = px(icx - ir_w, icz + ir_h)
    b = px(icx + ir_w, icz - ir_h)
    imd.ellipse([a[0] * SS, a[1] * SS, b[0] * SS, b[1] * SS], fill=255)
    # dark limbal ring
    ring = Image.new('RGBA', im.size, (0, 0, 0, 0))
    rd = ImageDraw.Draw(ring, 'RGBA')
    rd.ellipse([a[0] * SS, a[1] * SS, b[0] * SS, b[1] * SS], outline=(18, 60, 34, 255), width=int(5 * SS))
    layer.paste(iris, (0, 0), Image.composite(iris.split()[3], Image.new('L', im.size, 0), imask))
    layer.alpha_composite(ring)
    ld = ImageDraw.Draw(layer, 'RGBA')
    # pupil
    pp1 = px(icx - ir_w * .34, icz + ir_h * .46)
    pp2 = px(icx + ir_w * .34, icz - ir_h * .36)
    ld.ellipse([pp1[0] * SS, pp1[1] * SS, pp2[0] * SS, pp2[1] * SS], fill=(10, 30, 20, 255))
    # starburst glints
    for fx, fz, fr in ((-.5, .52, .27), (.4, -.4, .15)):
        c1 = px(icx + ir_w * fx - ir_w * fr, icz + ir_h * fz + ir_h * fr)
        c2 = px(icx + ir_w * fx + ir_w * fr, icz + ir_h * fz - ir_h * fr)
        ld.ellipse([c1[0] * SS, c1[1] * SS, c2[0] * SS, c2[1] * SS], fill=(255, 255, 255, 255))
    # lid shadow over the iris top
    sh = Image.new('RGBA', im.size, (0, 0, 0, 0))
    shd = ImageDraw.Draw(sh, 'RGBA')
    shd.polygon([S(p) for p in top] + [S(px(X(1), upper(1) - .012)), S(px(X(0), upper(0) - .012))][::1], fill=(30, 20, 20, 0))
    layer.putalpha(mask)
    im.alpha_composite(layer)
    d = ImageDraw.Draw(im, 'RGBA')
    # thick upper lash line + flick, tapered
    for i in range(48):
        t = i / 48
        t2 = (i + 1) / 48
        wd = (6 + 14 * math.sin(math.pi * min(1, t * 1.1)) ** .6) * SS
        d.line([S(px(X(t), upper(t) + .0008)), S(px(X(t2), upper(t2) + .0008))], fill=(34, 22, 18, 255), width=int(wd))
    ox = X(outer)
    sgn = 1 if outer == 1 else -1
    d.line([S(px(ox, upper(outer) - .001)), S(px(ox + sgn * .008, upper(outer) + .009)), S(px(ox + sgn * .0125, upper(outer) + .013))], fill=(34, 22, 18, 255), width=int(8 * SS), joint='curve')
    d.line([S(px(ox - sgn * .002, upper(outer) - .002)), S(px(ox + sgn * .006, upper(outer) + .002))], fill=(34, 22, 18, 255), width=int(8 * SS))
    # lower lid
    for i in range(48):
        t = i / 48
        t2 = (i + 1) / 48
        d.line([S(px(X(t), lower(t))), S(px(X(t2), lower(t2)))], fill=(150, 98, 86, 160), width=int(3 * SS))
    # soft crease
    for i in range(32):
        t = .1 + .8 * i / 32
        t2 = .1 + .8 * (i + 1) / 32
        d.line([S(px(X(t), upper(t) + .0075)), S(px(X(t2), upper(t2) + .0075))], fill=(170, 112, 92, 110), width=int(2.4 * SS))
    # eyebrow as a tapered polygon with a gentle arch
    up = []
    lo = []
    for i in range(25):
        t = i / 24
        x = cx - w * 1.05 + 2 * w * 1.12 * t
        arch = math.sin(math.pi * t) ** .8 * .006
        z = 1.7925 + arch + (t - .5) * (.0035 if outer else -.0035) * 0 + ((t if outer else 1 - t) * .003)
        th = (.0034 * (1 - .75 * ((t if outer else 1 - t)) ** 1.5) + .0008)
        up.append(px(x, z + th))
        lo.append(px(x, z - th))
    d.polygon([S(p) for p in up + lo[::-1]], fill=(100, 60, 36, 255))


eye(1)
eye(-1)
# nose: tiny tick and shadow under it
d.line([S(px(-.0045, 1.718)), S(px(.0, 1.706)), S(px(.0045, 1.718))], fill=(200, 140, 112, 255), width=int(5 * SS), joint='curve')
nose = Image.new('RGBA', im.size, (0, 0, 0, 0))
nd = ImageDraw.Draw(nose, 'RGBA')
a = px(-.009, 1.708)
b = px(.009, 1.698)
nd.ellipse([a[0] * SS, a[1] * SS, b[0] * SS, b[1] * SS], fill=(190, 120, 96, 90))
nose = nose.filter(ImageFilter.GaussianBlur(7 * SS))
im.alpha_composite(nose)
d = ImageDraw.Draw(im, 'RGBA')
# mouth: gentle confident smile with soft lower lip
mpts = []
for i in range(41):
    t = i / 40
    x = -.019 + .038 * t
    z = 1.676 - math.sin(math.pi * t) * .0045 + ((t - .5) ** 2) * .018
    mpts.append(px(x, z))
d.line([S(p) for p in mpts], fill=(150, 70, 70, 255), width=int(5 * SS), joint='curve')
lip = px(-.009, 1.664)
lip2 = px(.009, 1.656)
d.ellipse([lip[0] * SS, lip[1] * SS, lip2[0] * SS, lip2[1] * SS], fill=(214, 128, 124, 150))
d.line([S(px(-.019, 1.679)), S(px(-.0215, 1.683))], fill=(150, 70, 70, 255), width=int(4 * SS))
d.line([S(px(.019, 1.679)), S(px(.0215, 1.683))], fill=(150, 70, 70, 255), width=int(4 * SS))
im = im.resize((W, W), Image.LANCZOS).convert('RGB')
im.save(OUT)
print('face texture saved')
