using Microsoft.Extensions.DependencyInjection;
using Shortify.Infrastructure.PostgreSQL.Repository;
using Shortify.Infrastructure.Repository;
using Shottify.Application.Abstraction.Service;
using Shottify.Application.Service;


namespace Shortify.IoC
{
    public static class IoCContainer
    {
        public static void RegisterServices(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ILinkService, LinkService>();
            services.AddScoped<ILinkShortenerService, LinkShortenerService>();            
            services.AddScoped<ITokenService, TokenService>();


            services.AddScoped<ILinkRepository, LinkRepository>();
            services.AddScoped<ITokenRepository, TokenRepository>();
        }
    }
}
