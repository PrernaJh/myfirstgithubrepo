using PackageTracker.Domain.Models;
using PackageTracker.Identity.Service.Models;
using System.Threading.Tasks;

namespace PackageTracker.Identity.Service.Interfaces
{
	public interface IIdentityService
	{
		Task<GenericResponse<SignInResponse>> SignInAsync(SignInRequest signInRequest);
		Task<GenericResponse<PasswordResetResponse>> ResetPassword(PasswordResetRequest passwordResetRequest);
		Task<GenericResponse<ForgotPasswordResponse>> ForgotPassword(ForgotPasswordRequest forgotPasswordRequest);
		Task<GenericResponse<PasswordResetResponse>> SetPassword(string securityCode, string password);
		Task<GenericResponse<string>> GetUsernameByTemporaryPasswordAsync(string temporaryPassword);
	}
}
