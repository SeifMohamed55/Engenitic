namespace GraduationProject.Application.Services.Interfaces
{
    public interface IRefreshTokenService
    {
        /// <summary>
        /// Refreshes the access token
        /// </summary>
        /// <param name="oldAccessToken"></param>
        /// <returns>
        /// The new Access Token
        /// </returns>
        Task<ServiceResult<string>> Refresh(string oldAccessToken, string requestRefToken, Guid deviceId);
    }
}
