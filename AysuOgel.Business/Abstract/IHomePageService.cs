using AysuOgel.Business.Models;

namespace AysuOgel.Business.Abstract;

public interface IHomePageService
{
    Task<HomePageContent> GetAsync();
}