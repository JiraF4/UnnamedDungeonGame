using Godot;
using System;

[Tool]
public partial class ItemView : SubViewport
{

    [Export]
    public bool BakeItem
    {
        set
        {
            Console.WriteLine("!!");
            SaveTex();
        }
        get => false;
    }
    
    public async void SaveTex()
    {
        await ToSignal(RenderingServer.Singleton, "frame_post_draw");
        
        
        var texture = new ImageTexture();
        var image = GetTexture().GetImage();
        image.FlipY();
        image.Convert(Image.Format.Rgba8);

        var width = image.GetWidth();
        var height = image.GetHeight();

        var maxX = 0;
        var maxY = 0;
        var minX = width;
        var minY = height;
        
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                var pixel = image.GetPixel(x, y);
                if (pixel.A < 1) continue;
                if (maxX < x) maxX = x;
                if (maxY < y) maxY = y;
                if (minX > x) minX = x;
                if (minY > y) minY = y;
            }
        }

        

        width = maxX - minX;
        height = maxY - minY;
        if (width  % 24 != 0) width  += (24 - (width  % 24))/2;
        if (height % 24 != 0) height += (24 - (height % 24))/2;
        if (width  % 24 != 0) minX   -= 24 - (width % 24);
        if (height % 24 != 0) minY   -= 24 - (height % 24);
        if (width  % 24 != 0) width  += 24 - (width % 24);
        if (height % 24 != 0) height += 24 - (height % 24);
        
        var rect = new Rect2I(minX, minY, width, height);
        image = image.GetRegion(rect);
        
        var borderColor = new Color(128, 128, 0);
        for (var x = 0; x < image.GetWidth(); x++)
        {
            image.SetPixel(x, 0, borderColor);
            image.SetPixel(x, image.GetHeight()-1, borderColor);
        }
        for (var y = 0; y < image.GetHeight(); y++)
        {
            image.SetPixel(0, y, borderColor);
            image.SetPixel(image.GetWidth()-1, y, borderColor);
        }

        image.SavePng("res://Temp/ItemViewExport.png");
    }
}
