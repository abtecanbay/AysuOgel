using AysuOgel.Business.Abstract;
using AysuOgel.Business.Models;

namespace AysuOgel.Business.Concrete;

public class StaticHomePageService : IHomePageService
{
    private static string Img(string seed, int w = 800, int h = 600) =>
        $"https://picsum.photos/seed/{seed}/{w}/{h}";

    public Task<HomePageContent> GetAsync()
    {
        var content = new HomePageContent
        {
            Hero = new HeroSection
            {
                Title = "Hoş geldiniz",
                Subtitle = "Bu alan örnek metindir. Slogan ve kısa tanıtım yazısı buraya gelecek.",
                ImageUrl = Img("hero", 1600, 700),
                ButtonText = "Hizmetlerimiz",
                ButtonUrl = "/hizmetler"
            },
            Blocks =
            {
                new ContentBlock
                {
                    Title = "Hakkımızda",
                    Text = "Bu alan örnek metindir.\nİkinci paragraf için yeni satıra geçebilirsin.",
                    Layout = BlockLayout.ImageLeft,
                    Images = { new ImageItem { Url = Img("about"), Alt = "Örnek görsel" } }
                },
                new ContentBlock
                {
                    Title = "Neler yapıyoruz?",
                    Text = "Bu alan örnek metindir. Hizmetlerin kısa tanıtımı burada olabilir.",
                    Layout = BlockLayout.ImageRight,
                    Images = { new ImageItem { Url = Img("work"), Alt = "Örnek görsel" } }
                },
                new ContentBlock
                {
                    Title = "Galeri",
                    Text = "Örnek görseller.",
                    Layout = BlockLayout.Gallery,
                    Images =
                    {
                        new ImageItem { Url = Img("g1"), Alt = "Örnek 1", Caption = "Örnek 1" },
                        new ImageItem { Url = Img("g2"), Alt = "Örnek 2", Caption = "Örnek 2" },
                        new ImageItem { Url = Img("g3"), Alt = "Örnek 3", Caption = "Örnek 3" },
                        new ImageItem { Url = Img("g4"), Alt = "Örnek 4", Caption = "Örnek 4" },
                        new ImageItem { Url = Img("g5"), Alt = "Örnek 5", Caption = "Örnek 5" },
                        new ImageItem { Url = Img("g6"), Alt = "Örnek 6", Caption = "Örnek 6" }
                    }
                }
            }
        };

        return Task.FromResult(content);
    }
}