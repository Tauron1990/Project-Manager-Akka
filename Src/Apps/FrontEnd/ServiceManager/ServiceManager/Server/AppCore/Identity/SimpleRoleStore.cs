using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ServiceManager.Server.AppCore.Identity
{ 
    public sealed class SingleRoleStore : RoleStoreBase<>
    {
    }
}