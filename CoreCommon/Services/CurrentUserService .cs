using CoreCommon.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace CoreCommon.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int UserId =>
            int.TryParse(
                _httpContextAccessor.HttpContext?.User
                    .FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id)
                ? id : 0;

        public int CompanyId =>
            int.TryParse(
                _httpContextAccessor.HttpContext?.User
                    .FindFirst("CompanyId")?.Value, out var id)
                ? id : 0;

        public int DepartmentId =>
            int.TryParse(
                _httpContextAccessor.HttpContext?.User
                    .FindFirst("DepartmentId")?.Value, out var id)
                ? id : 0;

        public string? Username =>
            _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.Name)?.Value;

        public string? Role =>
            _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.Role)?.Value;
    }
}
