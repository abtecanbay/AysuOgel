using AysuOgel.Business.Abstract;
using AysuOgel.Business.Models;

namespace AysuOgel.Business.Concrete;

public class StaticAboutPageService : IAboutPageService
{
    // Örnek görsel istersen bir bloğa şunu ekle:
    // Images = { new ImageItem { Url = "https://picsum.photos/seed/x/800/600", Alt = "Örnek" } }
    // Kendi görselini ekleyince Url = "mekan-1.jpg" (wwwroot/uploads/mekan-1.jpg) yeterli.

    public Task<AboutPageContent> GetAsync()
    {
        var content = new AboutPageContent
        {
            Title = "Hakkımızda",
            Intro = "Çankaya, Ankara'da 6 yıldır hizmet veriyoruz. Bu alan örnek metindir; işletmenin kısa tanıtımı buraya gelecek.",
            Blocks =
            {
                new ContentBlock
                {
                    Slug = "mekanimiz",
                    Title = "Mekanımız",
                    Text = "Bu alan örnek metindir.\nMekanın genel tanıtımı, yaklaşımınız ve hizmet anlayışınız buraya yazılabilir.",
                    Layout = BlockLayout.ImageLeft
                },
                new ContentBlock
                {
                    Slug = "vaccu-active",
                    Title = "Vaccu Active",
                    Text = "Bu alan örnek metindir.\nUygulamanın ne olduğu, nasıl yapıldığı ve seans bilgileri buraya yazılabilir.",
                    Layout = BlockLayout.ImageRight
                },
                new ContentBlock
                {
                    Slug = "roll-shape",
                    Title = "Roll Shape",
                    Text = "Bu alan örnek metindir.\nUygulamanın tanıtımı ve seans bilgileri buraya yazılabilir.",
                    Layout = BlockLayout.ImageLeft
                },
                new ContentBlock
                {
                    Slug = "cilt-bakimi",
                    Title = "Profesyonel Cilt Bakımı",
                    Text = "Bu alan örnek metindir.\nCilt bakımı hizmetleri ve kullanılan ürünler buraya yazılabilir.",
                    Layout = BlockLayout.ImageRight
                }
            }
        };

        return Task.FromResult(content);
    }
}