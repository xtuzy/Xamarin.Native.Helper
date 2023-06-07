using Topten.RichTextKit.Editor;
using Yang.Maui.Helper.Graphics;
using Yang.Maui.Helper.Image;
using Font = Microsoft.Maui.Font;

namespace Yang.Maui.Helper.Maui.Test.Pages;

public partial class FontTestPage : ContentPage
{
	public FontTestPage()
	{
		InitializeComponent();
        LoadFontFileAsync();
        var showYouYuan = new JustifyParagraphLabelNormal()
        {
            
            Paragraph = "��ϼ�������ɣ���ˮ������һɫ"
        };
        showYouYuan.Font = new MauiFont(showYouYuan, Font.OfSize("YouYuan", 0));
        var showIconFont = new JustifyParagraphLabelNormal()
        {
            Paragraph = $"{FontAwesomeIcons.ArrowLeft} {FontAwesomeIcons.ArrowRight}"
        };
        showIconFont.Font = new MauiFont(showIconFont, Font.OfSize("Font Awesome 6 Free-Solid-900", 0));
        layout.Add(showYouYuan);
        layout.Add(showIconFont);     
    }

    async Task LoadFontFileAsync()
    {
        Stream stream = null;
        try
        {
            stream = await FileSystem.OpenAppPackageFileAsync("YouYuan.ttf");
        }
        catch (FileNotFoundException e)
        {
            
        }
        finally
        {
            if (stream != null)
            {
                var typeface = SkiaSharp.SKTypeface.FromStream(stream);
            }
            Console.WriteLine($"Find YouYuan.ttf:{stream != null}");
        }
    }
}