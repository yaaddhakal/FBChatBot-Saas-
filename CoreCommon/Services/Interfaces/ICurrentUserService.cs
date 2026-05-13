using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCommon.Services.Interfaces
{
    public interface ICurrentUserService
    {
        int UserId { get; }
        int CompanyId { get; }
        int DepartmentId { get; }
        string? Username { get; }
        string? Role { get; }
    }
}
