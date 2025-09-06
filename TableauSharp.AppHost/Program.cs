var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.TableauSharp_Sample>("tableausharp-sample");

builder.Build().Run();
