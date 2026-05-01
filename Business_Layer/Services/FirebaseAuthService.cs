using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Layer.Services
{
    using Google.Apis.Auth;

    public class FirebaseAuthService
    {
        public async Task<GoogleJsonWebSignature.Payload> VerifyToken(string idToken)
        {
            return await GoogleJsonWebSignature.ValidateAsync(idToken);
        }
    }
}
