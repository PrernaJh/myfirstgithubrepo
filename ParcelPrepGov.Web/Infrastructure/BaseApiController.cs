using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ParcelPrepGov.Web.Infrastructure
{
	[Route("[controller]/[action]")]
	[Authorize]
	[ApiController]
	public class BaseApiController : ControllerBase
	{

	}
}
