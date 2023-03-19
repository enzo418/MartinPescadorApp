using System.Reflection;

namespace FisherTournament.ArchitectureTests;

public class TestBase
{
    protected static Assembly ApiAssembly => FisherTournament.API.AssemblyReference.Assembly;

    protected static string DomainNamespace => "FisherTournament.Domain";
    protected static string ContractsNamespace => "FisherTournament.Contracts";
    protected static string ApplicationNamespace => "FisherTournament.Application";
    protected static string InfrastructureNamespace => "FisherTournament.Infrastructure";
    protected static string WebApiNamespace => "FisherTournament.API";
}