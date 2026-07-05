using MediatR;
using Microsoft.EntityFrameworkCore;
using TillApp.Infrastructure;
using TillApp.Models;

namespace TillApp.Features
{
    public record GetStoreSettingsQuery : IRequest<StoreSettings>;

    public class GetStoreSettingHandler : IRequestHandler<GetStoreSettingsQuery, StoreSettings?>
    {
        private readonly AppDbContext _db;

        public GetStoreSettingHandler(AppDbContext db) => _db = db;

        public Task<StoreSettings?> Handle(GetStoreSettingsQuery q, CancellationToken ct)
        {
            return _db.StoreSettings.FirstOrDefaultAsync(ct);
        }
    }
}
