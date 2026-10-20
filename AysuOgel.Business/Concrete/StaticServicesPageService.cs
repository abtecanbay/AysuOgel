using AysuOgel.Business.Abstract;
using AysuOgel.Business.Models;

namespace AysuOgel.Business.Concrete;

public class StaticServicesPageService : IServicesPageService
{
    // Görsel eklemek için: BeforeImageUrl = "uye1-once.jpg" (wwwroot/uploads/uye1-once.jpg)
    // veya tam CDN adresi. Boş bırakılırsa dikey boş çerçeve görünür.

    public Task<ServicesPageContent> GetAsync()
    {
        var content = new ServicesPageContent
        {
            Title = "Hizmetlerimiz",
            Intro = "Her hizmetimiz size özel hazırlanan bir programla uygulanır. Bu alan örnek metindir.",

            StepsTitle = "Size özel program",
            Steps =
            {
                new ProgramStep
                {
                    Title = "Tanışma ve değerlendirme",
                    Text = "Bu alan örnek metindir. İlk görüşmede hedefleriniz ve ihtiyaçlarınız belirlenir."
                },
                new ProgramStep
                {
                    Title = "Kişiye özel program",
                    Text = "Bu alan örnek metindir. Programınız size göre hazırlanır."
                },
                new ProgramStep
                {
                    Title = "Düzenli takip ve kilo kontrolü",
                    Text = "Bu alan örnek metindir. Seanslar boyunca kilo kontrolü yapılır ve program gerekirse güncellenir."
                }
            },

            Comparison = new ComparisonSection
            {
                Title = "Normal koşu bandından farkı nedir?",
                Intro = "Bu bölümdeki maddeleri kendi bilgilerinle güncelle.",
                LeftTitle = "Normal koşu bandı",
                RightTitle = "Vaccu Active",
                Rows =
                {
                    new ComparisonRow { Label = "Program", Left = "Genel kullanım", Right = "Kişiye özel hazırlanır" },
                    new ComparisonRow { Label = "Takip", Left = "Bu alan örnek metindir.", Right = "Kilo kontrolü yapılır" },
                    new ComparisonRow { Label = "Uygulama", Left = "Bu alan örnek metindir.", Right = "Bu alan örnek metindir." }
                }
            },

            TransformationsTitle = "Üyelerimizin değişimi",
            TransformationsNote = "Sonuçlar kişiden kişiye farklılık gösterebilir. Görseller üyelerimizin izniyle paylaşılmaktadır.",
        };

        return Task.FromResult(content);
    }
}