using NetArchTest.Rules;

namespace FisherTournament.ArchitectureTests;

public class LayerDependenciesTests : TestBase
{
    [Fact]
    public void Domain_ShouldNot_DependOnOtherLayers()
    {
        var otherProjects = new[]
        {
            ApplicationNamespace,
            InfrastructureNamespace,
            WebApiNamespace,
            WebServerNamespace
        };

        var result = Types.InAssemblies(PresentationAssemblies)
            .That().ResideInNamespace(DomainNamespace)
            .Should().NotHaveDependencyOnAny(otherProjects)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Contracts_ShouldNot_DependOnOtherLayers()
    {
        var otherProjects = new[]
        {
            DomainNamespace,
            ApplicationNamespace,
            InfrastructureNamespace,
            WebApiNamespace,
            WebServerNamespace
        };

        var result = Types.InAssemblies(PresentationAssemblies)
            .That().ResideInNamespace(ContractsNamespace)
            .Should().NotHaveDependencyOnAny(otherProjects)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_ShouldNot_DependOnOtherLayers()
    {
        var otherProjects = new[]
        {
            InfrastructureNamespace,
            WebApiNamespace,
            WebServerNamespace
        };

        var result = Types.InAssemblies(PresentationAssemblies)
            .That().ResideInNamespace(ApplicationNamespace)
            .Should().NotHaveDependencyOnAny(otherProjects)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Infrastructure_ShouldNot_DependOnOtherLayers()
    {
        var otherProjects = new[]
        {
            WebApiNamespace,
            WebServerNamespace
        };

        var result = Types.InAssemblies(PresentationAssemblies)
            .That().ResideInNamespace(InfrastructureNamespace)
            .Should().NotHaveDependencyOnAny(otherProjects)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Handlers_Should_DependOnDomain()
    {
        var result = Types.InAssemblies(PresentationAssemblies)
            .That().ResideInNamespace(ApplicationNamespace)
            // .And().Inherit(typeof(IRequestHandler<,>)) // or:
            .And().HaveNameEndingWith("Handler")
            .Should().HaveDependencyOn(DomainNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}