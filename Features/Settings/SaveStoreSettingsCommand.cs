using MediatR;
using Microsoft.EntityFrameworkCore;
using TillApp.Infrastructure;
using TillApp.Models;

namespace TillApp.Features
{
    public record SaveStoreSettingsCommand(string Name, string Address, string Phone, string TaxCode, string? LogoPath) : IRequest<Unit>;

    public class SaveStoreSettingHandler : IRequestHandler<SaveStoreSettingsCommand, Unit>
    {
        private readonly AppDbContext _db;

        public SaveStoreSettingHandler(AppDbContext db) => _db = db;

        public async Task<Unit> Handle(SaveStoreSettingsCommand cmd, CancellationToken ct)
        {
            var settings = await _db.StoreSettings.FirstOrDefaultAsync(ct);

            if (settings == null)
            {
                settings = new StoreSettings();
                _db.StoreSettings.Add(settings);
            }

            settings.Name = cmd.Name;
            settings.Address = cmd.Address;
            settings.Phone = cmd.Phone;
            settings.TaxCode = cmd.TaxCode;
            settings.LogoPath = cmd.LogoPath;

            await _db.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}
