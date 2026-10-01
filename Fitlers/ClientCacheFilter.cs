namespace AI_Based_Pickleball_Tournament_Management.Fitlers
{
    public class ClientCacheFilter : IEndpointFilter
    {
        private readonly int duration;

        public ClientCacheFilter(int duration)
        {
            this.duration = duration;
        }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var result = await next(context);
            context.HttpContext.Response.Headers.CacheControl = $"public,max-age={duration}";
            return result;
        }
    }
}