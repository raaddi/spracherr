using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Spracher.Persistence;

public static class IdentityBuilderExtensions
{
    public static IdentityBuilder AddSpracherIdentityStores(this IdentityBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddEntityFrameworkStores<SpracherDbContext>();
    }
}
