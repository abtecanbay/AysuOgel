using AysuOgel.Business.Models;
using AysuOgel.Entities.Entities;

namespace AysuOgel.Business.Abstract;

public interface IAppointmentService
{
    /// <summary>Mesajı ve randevuyu tek işlemde kaydeder. Saat doluysa false döner.</summary>
    Task<bool> CreateRequestAsync(ContactMessage message, DateTime slotStart);

    Task<List<Appointment>> GetAllAsync(AppointmentFilter filter);
    Task<bool> SetStatusAsync(int id, AppointmentStatus status);
}