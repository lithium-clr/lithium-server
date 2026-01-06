using Lithium.Server.Dashboard.Models;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;

namespace Lithium.Server.Dashboard;

public sealed class WebDbContext(DbContextOptions<WebDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>().ToCollection("users");
    }
}