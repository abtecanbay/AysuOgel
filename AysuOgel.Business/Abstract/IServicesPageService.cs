using AysuOgel.Business.Models;

namespace AysuOgel.Business.Abstract;

public interface IServicesPageService
{
    Task<ServicesPageContent> GetAsync();
}