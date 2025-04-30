namespace QMS.Middleware
{
    public static class GlobalErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalErrorHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalErrorHandlingMiddleware>();
        }
    }
}
