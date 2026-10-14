using AysuOgel.Business.Models;

namespace AysuOgel.Business.Abstract;

public interface IAboutPageService
{
    Task<AboutPageContent> GetAsync();
}