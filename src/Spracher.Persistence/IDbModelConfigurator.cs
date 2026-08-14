using Microsoft.EntityFrameworkCore;

namespace Spracher.Persistence;

public interface IDbModelConfigurator
{
    void Configure(ModelBuilder modelBuilder);
}
