using GraduationProject.Application.Services.Interfaces;
using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data.Interfaces;
using Microsoft.AspNetCore.Identity;
using Org.BouncyCastle.Security;

namespace GraduationProject.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly RoleManager<Role> _roleManager;

        public AdminService(IUnitOfWork unitOfWork, RoleManager<Role> roleManager)
        {
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
        }

        public async Task BanUser(int id)
        {
            var user = await _unitOfWork.UserRepo.GetByIdAsync(id);
            if (user == null)
                throw new Exception("User not found");
            user.Banned = true;
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UnbanUser(int id)
        {
            var user = await _unitOfWork.UserRepo.GetByIdAsync(id);
            if (user == null)
                throw new Exception("User not found");
            user.Banned = false;
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<PaginatedList<AppUserDTO>> GetUsersPage(int index, string? role)
        {
            if (role == null)
                return await _unitOfWork.UserRepo.GetUsersInRolePage(index, null);

            var roleEntity = await _roleManager.FindByNameAsync(role);
            if (roleEntity == null)
                throw new InvalidParameterException("Role not found");

            return await _unitOfWork.UserRepo.GetUsersInRolePage(index, roleEntity);
        }

    }
}
