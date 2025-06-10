using GraduationProject.Application.Services.Interfaces;
using GraduationProject.Common.Constants;
using GraduationProject.Domain.DTOs;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data.Interfaces;
using Microsoft.AspNetCore.Identity;
using Org.BouncyCastle.Security;
using System.Net;

namespace GraduationProject.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<AppUser> _userManager;

        public AdminService(IUnitOfWork unitOfWork,
            RoleManager<Role> roleManager,
            UserManager<AppUser> userManager,
            ICoursesService coursesService)
        {
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<ServiceResult<bool>> BanUser(int id)
        {
            try
            {

                var user = await _unitOfWork.UserRepo.GetByIdAsync(id);
                if (user == null)
                    return ServiceResult<bool>.Failure("User not found", HttpStatusCode.NotFound);

                user.Banned = true;
                await _unitOfWork.CourseRepo.HideAllInstructorCourses(user.Id);

                await _unitOfWork.SaveChangesAsync();
                return ServiceResult<bool>.Success(true, "User banned successfully", HttpStatusCode.OK);
            }
            catch
            {
                return ServiceResult<bool>
                    .Failure("An error occurred while banning the user", HttpStatusCode.ServiceUnavailable);
            }

        }

        public async Task<ServiceResult<bool>> UnbanUser(int id)
        {
            try
            {
                var user = await _unitOfWork.UserRepo.GetByIdAsync(id);
                if (user == null)
                    return ServiceResult<bool>.Failure("User not found", HttpStatusCode.NotFound);

                user.Banned = false;
                await _unitOfWork.CourseRepo.UnHideAllInstructorCourses(user.Id);

                await _unitOfWork.SaveChangesAsync();
                return ServiceResult<bool>.Success(true, "User unbanned successfully", HttpStatusCode.OK);
            }
            catch
            {
                return ServiceResult<bool>
                    .Failure("An error occurred while unbanning the user", HttpStatusCode.ServiceUnavailable);
            }
        }

        public async Task<ServiceResult<PaginatedList<AppUserDTO>>> GetUsersPage(int index, string? role)
        {
            try
            {
                PaginatedList<AppUserDTO> users;
                if (role == null)
                {
                     users = await _unitOfWork.UserRepo.GetUsersInRolePage(index, null);
                }
                else
                {
                    var roleEntity = await _roleManager.FindByNameAsync(role);
                    if (roleEntity == null)
                        return ServiceResult<PaginatedList<AppUserDTO>>
                            .Failure("Role not found");

                    users = await _unitOfWork.UserRepo.GetUsersInRolePage(index, roleEntity);
                }

                    
                if (index > users.TotalPages && users.TotalPages != 0)
                    return ServiceResult<PaginatedList<AppUserDTO>>
                        .Failure("Invalid Page Number", HttpStatusCode.BadRequest);

                return ServiceResult<PaginatedList<AppUserDTO>>
                    .Success(users, "List Retrieved successfully");
            }
            catch
            {
                return ServiceResult<PaginatedList<AppUserDTO>>
                    .Failure("An error occurred while retrieving the users", HttpStatusCode.ServiceUnavailable);
            }
            
        }

        public async Task<ServiceResult<bool>> VerifyInstructor(int id)
        {
            try
            {
                var user = await _unitOfWork.UserRepo.GetUserWithRoles(id);
                if (user == null)
                    return ServiceResult<bool>.Failure("User not found", HttpStatusCode.NotFound);
                var roles = user.Roles.Select(x => x.NormalizedName);

                // Check if user is instructor
                if (!roles.Contains(Roles.Instructor.ToUpper()))
                    return ServiceResult<bool>.Failure("User is not an instructor", HttpStatusCode.BadRequest);

                // Check if already verified
                if (roles.Contains(Roles.VerifiedInstructor.ToUpper()))
                    return ServiceResult<bool>.Failure("User is already a verified instructor", HttpStatusCode.BadRequest);

                // Proceed to verify
                var res = await _userManager.AddToRoleAsync(user, Roles.VerifiedInstructor);
                if (!res.Succeeded)
                    return ServiceResult<bool>.Failure("Failed to add user to role", HttpStatusCode.InternalServerError);
                return ServiceResult<bool>.Success(true, "Instructor verified successfully", HttpStatusCode.OK);
            }
            catch
            {
                return ServiceResult<bool>
                    .Failure("An error occurred while verifying the instructor", HttpStatusCode.ServiceUnavailable);
            }
        }

    }
}
