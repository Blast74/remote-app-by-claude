using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RemoteDesktopServer.Data;

public class ServerDbContextFactory : IDesignTimeDbContextFactory<ServerDbContext>
{
    public ServerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ServerDbContext>();
        optionsBuilder.UseSqlite("Data Source=rdpserver.db");
        
        return new ServerDbContext(optionsBuilder.Options);
    }
}