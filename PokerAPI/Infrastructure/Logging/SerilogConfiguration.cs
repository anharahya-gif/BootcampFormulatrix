// using Serilog;
// using Microsoft.Extensions.Configuration;

// namespace PokerAPI.Infrastructure.Logging
// {
//     public static class SerilogConfiguration
//     {
//         public static Serilog.ILogger CreateLogger(IConfiguration configuration)
//         {
//             return new LoggerConfiguration()
//                 .ReadFrom.Configuration(configuration)
//                 .Enrich.FromLogContext()
//                 .CreateLogger();
//         }
//     }
// }
