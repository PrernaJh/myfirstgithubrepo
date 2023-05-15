using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PackageTracker.Domain.Utilities;

#if RELEASE
using Microsoft.AspNetCore.Authorization;
using PackageTracker.Identity.Data.Constants;
#endif

namespace PackageTracker.API.Controllers
{
    public class EncryptRequest
    {
        public AESKey Crypto { get; set; } 
        public string PlainText { get; set; } 
        public string Salt { get; set; } 
    }
    public class DecryptRequest
    {
        public AESKey Crypto { get; set; } 
        public string EncryptedText { get; set; } 
        public string Salt { get; set; } 
    }
    public class EncryptReply
    {
        public AESKey Crypto { get; set; } 
        public string PlainText { get; set; } 
        public string Salt { get; set; } 
        public string EncryptedText { get; set; } 
    }

#if RELEASE
    [Authorize(Roles = IdentityDataConstants.SystemAdministrator)]
#endif
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CryptoController : ControllerBase
    {
        private readonly ILogger<CryptoController> logger;

        public CryptoController(ILogger<CryptoController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        public AESKey CreateKey()
        {
            return CryptoUtility.CreateKey();
        }

        [HttpPost]
        public EncryptReply Encrypt([FromBody] EncryptRequest request)
        {
            return new EncryptReply
            {
                Crypto = request.Crypto,
                PlainText = request.PlainText,
                Salt = request.Salt,
                EncryptedText = CryptoUtility.Encrypt(request.Crypto, request.PlainText, request.Salt)
            };
        }
#if DEBUG
        [HttpPost]
        public EncryptReply Decrypt([FromBody] DecryptRequest request)
        {
            return new EncryptReply
            {
                Crypto = request.Crypto,
                EncryptedText = request.EncryptedText,
                Salt = request.Salt,
                PlainText = CryptoUtility.Decrypt(request.Crypto, request.EncryptedText, request.Salt)
            };
        }
#endif
    }
}
