using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shottify.Application.Abstraction.DTOs
{
    public class AuthResponeseDTO
    {
        public string AccessToken { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public string UserId { get; set; }
    }
}
