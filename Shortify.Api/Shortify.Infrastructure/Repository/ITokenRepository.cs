using Shortify.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shortify.Infrastructure.Repository
{
    public interface ITokenRepository: IRepository<UserToken>
    {
        Task<IExecutionResponse> GetTokensByUserAsync(string userId, bool includeRevoked = false);
        Task<bool> IsTokenRevokedAsync(string token);
        Task<IExecutionResponse> RevokeTokenAsync(string token);
        Task<IExecutionResponse> RevokeAllTokensAsync(string userId);
    }
}
