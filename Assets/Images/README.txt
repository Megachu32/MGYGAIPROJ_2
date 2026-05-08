RULES REGARDING IMAGES

Images Should be in one of these formats
1. .PNG
2. .JPEG / .JPG
3. .PSD

the size needs to be 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096
example: 128x64, 512x512, 32x256

when you put the image into the Images folder press the image and see the inspector. you need to change the setting

1. Sprite Mode: if the image only contain one image for example Board.png then set the Sprite Mode to single,  but if the image is like Example_Spire_Image.png then set it to multiply

2. Mesh Type (Currently: Tight)
What it is: This determines the shape of the invisible 3D geometry Unity draws behind your 2D image.

The Best Setting:

Tight is best for irregularly shaped pieces (like a chess knight or a meeple). Unity will draw a complex boundary that hugs the artwork closely, ignoring the transparent space, which saves performance.

Full Rect is best for perfect rectangles, like a standard playing card, a square board tile, or UI buttons.

3. Wrap Mode (Currently: Clamp)
What it is: This controls what the image does if it is stretched or if the camera views it at a weird angle.

The Best Setting: Leave it as Clamp. This is the absolute best setting for individual 2D assets. It stops the edges of your images from "bleeding" or creating weird visual artifacts. (You would only use Repeat if you were making a seamless tiling background pattern).

4. Filter Mode (Currently: Bilinear)
What it is: This setting is the number one culprit for "goofy" looking 2D games! It dictates how Unity blends the pixels together when the image is scaled up or zoomed in on.

The Best Setting:

If your board game uses High-Resolution, smooth art (like digital paintings or sleek vector graphics): Leave it on Bilinear. It applies a very slight blur to keep edges smooth.

If your game uses Pixel Art OR if you ever notice your high-res cards are looking slightly fuzzy and you want crisp, razor-sharp edges: Change this to Point (no filter).