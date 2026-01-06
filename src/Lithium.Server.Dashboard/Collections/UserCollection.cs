using Lithium.Server.Dashboard.Models;

namespace Lithium.Server.Dashboard.Collections;

public sealed class UserCollection(WebDbContext dbFactory)
    : DbRepository<WebDbContext, User>(dbFactory);